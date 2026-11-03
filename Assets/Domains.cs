using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class Domains : MonoBehaviour
{
    public static Domains instance;
    public SubscriptionSettings settings;
    // Compatibility for scripts reading the old property. Configuration lives in settings.
    public string Domain => settings != null ? settings.backendBaseUrl.TrimEnd('/') + "/" : "";

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
