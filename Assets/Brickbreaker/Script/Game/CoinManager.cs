using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Coins are spawned (visual only, no value added yet) when a brick is destroyed, and only
// actually collected -- animated to GameManager.CollectPoint, then their value added -- once
// CollectAllCoins() is called, which BrickManager does after a full shot finishes.
public class CoinManager : MonoBehaviour
{
    public CoinPickup SilverCoinPrefab;
    public CoinPickup GoldCoinPrefab;
    public int SilverCoinValue = 15;
    public int GoldCoinValue = 100;
    public float CoinMoveSpeed = 10f;

    private readonly List<CoinPickup> _activeCoins = new List<CoinPickup>();

    public void SpawnCoin(Vector3 position, bool isGold)
    {
        CoinPickup prefab = isGold ? GoldCoinPrefab : SilverCoinPrefab;
        int value = isGold ? GoldCoinValue : SilverCoinValue;

        CoinPickup coin = Instantiate(prefab, position, Quaternion.identity);
        coin.Initialize(value);

        _activeCoins.Add(coin);
    }

    // Moves every coin spawned since the last collection toward CollectPoint. Each coin plays
    // CollectCoin and gets destroyed the moment IT individually arrives (not once for the whole
    // batch), since coins spawned from different brick positions naturally arrive at different
    // times. Callers should yield on this -- it takes real time (the movement), not just one frame.
    public IEnumerator CollectAllCoins()
    {
        if (_activeCoins.Count == 0) yield break;

        Transform target = GameManager.Instance.CollectPoint;
        List<CoinPickup> stillMoving = new List<CoinPickup>(_activeCoins);
        _activeCoins.Clear();

        int totalValue = 0;

        while (stillMoving.Count > 0)
        {
            for (int i = stillMoving.Count - 1; i >= 0; i--)
            {
                CoinPickup coin = stillMoving[i];
                if (coin == null)
                {
                    stillMoving.RemoveAt(i);
                    continue;
                }

                coin.transform.position = Vector3.MoveTowards(coin.transform.position, target.position, CoinMoveSpeed * Time.deltaTime);

                if (Vector3.Distance(coin.transform.position, target.position) <= 0.01f)
                {
                    totalValue += coin.Value;
                    GameManager.Instance.SoundManager.Play(SoundType.CollectCoin);
                    Destroy(coin.gameObject);
                    stillMoving.RemoveAt(i);
                }
            }

            yield return null;
        }

        GameManager.Instance.AddCoin(totalValue);
    }

    // Destroys any coins spawned but not yet collected -- without this, coins still mid-flight
    // at restart would linger visually and get wrongly collected/counted on the next
    // CollectAllCoins() call in the new run.
    public void ResetCoins()
    {
        foreach (CoinPickup coin in _activeCoins)
        {
            if (coin != null) Destroy(coin.gameObject);
        }
        _activeCoins.Clear();
    }
}
