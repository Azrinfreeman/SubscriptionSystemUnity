using UnityEngine;

public class GameUIAmbient : MonoBehaviour
{
    [Tooltip("Local UI units; never moves interactive content.")]
    public float amplitude = 8;
    public float period = 6;
    Vector3 rest;
    void OnEnable()
    {
        rest = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = rest + Vector3.up * (GameUIPreferences.ReducedMotion ? 0 : Mathf.Sin(Time.unscaledTime * Mathf.PI * 2 / Mathf.Max(1, period)) * amplitude);
    }

    void OnDisable()
    {
        transform.localPosition = rest;
    }
}
