using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Shoot,
    WallBounce,
    HitBlock,
    Destroyed,
    BallReturn,
    CollectCoin,
    WaveClear,
}

[System.Serializable]
public class SoundEntry
{
    public SoundType Type;
    public AudioClip Clip;
}

public class SoundManager : MonoBehaviour
{
    public AudioSource AudioSource;
    public List<SoundEntry> Sounds;

    private Dictionary<SoundType, AudioClip> _clipsByType;

    private void Awake()
    {
        _clipsByType = new Dictionary<SoundType, AudioClip>();
        foreach (SoundEntry entry in Sounds)
        {
            _clipsByType[entry.Type] = entry.Clip;
        }
    }

    // PlayOneShot (not swapping AudioSource.clip) so overlapping sounds -- e.g. two balls hitting
    // bricks the same frame -- layer instead of cutting each other off.
    public void Play(SoundType type)
    {
        if (_clipsByType.TryGetValue(type, out AudioClip clip) && clip != null)
        {
            AudioSource.PlayOneShot(clip);
        }
    }
}
