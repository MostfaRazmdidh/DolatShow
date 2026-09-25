using UnityEngine;

// مدیرِ موسیقیِ پس‌زمینه. اگه فایلی به اسمِ Resources/Sound/Music (wav/mp3/ogg) بذاری،
// به‌صورتِ لوپ پخش می‌شه و از تنظیماتِ موسیقی (روشن/خاموش + بلندی) پیروی می‌کنه.
// اگه فایلِ موسیقی نباشه، بی‌صدا و بی‌خطا کنار می‌مونه (آماده برای وقتی موسیقی اضافه شد).
public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource source;
    private AudioClip musicClip;

    // ساختِ نمونه (اگه نبود). MainMenuUI موقعِ شروع صداش می‌زنه.
    public static void EnsureExists()
    {
        if (instance != null) return;
        GameObject go = new GameObject("MusicManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MusicManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;

        // موسیقی از Resources لود می‌شه (اگه بود)
        musicClip = Resources.Load<AudioClip>("Sound/Music");
        if (musicClip != null)
        {
            source.clip = musicClip;
        }
        ApplyToSource();
    }

    // از بیرون (SettingsSystem) صدا زده می‌شه وقتی تنظیماتِ موسیقی عوض شد.
    public static void ApplySettings()
    {
        if (instance != null) instance.ApplyToSource();
    }

    void ApplyToSource()
    {
        if (source == null) return;
        source.volume = SettingsSystem.MusicVolume;

        bool shouldPlay = SettingsSystem.MusicOn && musicClip != null;
        if (shouldPlay && !source.isPlaying) source.Play();
        else if (!shouldPlay && source.isPlaying) source.Pause();
    }
}
