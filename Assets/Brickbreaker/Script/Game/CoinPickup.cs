using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int Value { get; private set; }

    public void Initialize(int value)
    {
        Value = value;
    }
}
