using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class AttackFeelManager : MonoBehaviour
{
    public static AttackFeelManager Instance { get; private set; }

    [SerializeField]
    CinemachineBasicMultiChannelPerlin noise;

    [SerializeField]
    float defaultShakeFrequency = 1f;

    Coroutine _shakeCoroutine;
    Coroutine _hitStopCoroutine;
    float _timeScaleBeforeHitStop = 1f;
    bool _hitStopActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (noise == null)
            noise = FindAnyObjectByType<CinemachineBasicMultiChannelPerlin>();
    }

    void OnDisable()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
            ClearShake();
        }

        if (_hitStopActive)
            RestoreTimeScale();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayFeel(HitEntry entry)
    {
        if (entry == null)
            return;

        PlayCameraShake(entry.cameraShakePower, entry.cameraShakeDuration);
        PlayHitStop(entry.hitStopTimeScale, entry.hitStopDuration);
    }

    void PlayCameraShake(float power, float duration)
    {
        if (noise == null || power <= 0f || duration <= 0f)
            return;

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        _shakeCoroutine = StartCoroutine(CameraShakeCo(power, duration));
    }

    IEnumerator CameraShakeCo(float power, float duration)
    {
        noise.AmplitudeGain = power;
        noise.FrequencyGain = defaultShakeFrequency;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float falloff = 1f - t;
            noise.AmplitudeGain = power * falloff;
            noise.FrequencyGain = defaultShakeFrequency * falloff;
            yield return null;
        }

        ClearShake();
        _shakeCoroutine = null;
    }

    void ClearShake()
    {
        if (noise == null)
            return;

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }

    void PlayHitStop(float timeScale, float duration)
    {
        if (duration <= 0f)
            return;

        if (_hitStopCoroutine != null)
        {
            StopCoroutine(_hitStopCoroutine);
            RestoreTimeScale();
        }

        _hitStopCoroutine = StartCoroutine(HitStopCo(timeScale, duration));
    }

    IEnumerator HitStopCo(float timeScale, float duration)
    {
        _timeScaleBeforeHitStop = Time.timeScale;
        _hitStopActive = true;
        Time.timeScale = Mathf.Max(0f, timeScale);

        yield return new WaitForSecondsRealtime(duration);

        RestoreTimeScale();
        _hitStopCoroutine = null;
    }

    void RestoreTimeScale()
    {
        if (!_hitStopActive)
            return;

        Time.timeScale = _timeScaleBeforeHitStop;
        _hitStopActive = false;
    }
}
