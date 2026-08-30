using System.Collections;
using UnityEngine;

// Lives on Main Camera. Shakes the camera's own transform for world content (bricks, ball,
// board), and -- since an Overlay Canvas ignores its own root Transform -- separately nudges
// GameHUD's RectTransform in sync so the HUD shakes too. HUD (the other top-level Canvas child)
// is deliberately left alone.
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    public RectTransform HudRoot;

    // Shake magnitude decays across the shake's duration -- 1 at the start, 0 at the end.
    public AnimationCurve FalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // UI pixels of HUD shake per world unit of camera shake -- lets one magnitude value from
    // callers drive both the (small, world-unit) camera offset and the (much larger, pixel-unit)
    // HUD offset at a sensible relative scale.
    public float HudMagnitudeMultiplier = 40f;

    private Vector3 _cameraBasePosition;
    private Vector2 _hudBasePosition;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _cameraBasePosition = transform.localPosition;
        if (HudRoot != null) _hudBasePosition = HudRoot.anchoredPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        // Restart cleanly rather than stacking -- two overlapping shakes fighting over the same
        // transform would otherwise leave the camera/HUD not quite back at their base position
        // when the shorter one's coroutine finishes.
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
            ResetToBase();
        }

        _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float falloff = FalloffCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            Vector2 offset = Random.insideUnitCircle * magnitude * falloff;

            transform.localPosition = _cameraBasePosition + (Vector3)offset;
            if (HudRoot != null) HudRoot.anchoredPosition = _hudBasePosition + offset * HudMagnitudeMultiplier;

            yield return null;
        }

        ResetToBase();
        _shakeRoutine = null;
    }

    private void ResetToBase()
    {
        transform.localPosition = _cameraBasePosition;
        if (HudRoot != null) HudRoot.anchoredPosition = _hudBasePosition;
    }
}
