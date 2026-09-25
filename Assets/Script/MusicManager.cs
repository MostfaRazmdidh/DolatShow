using UnityEngine;

// مدیرِ آهنگِ اصلیِ بازی. آهنگ (Resources/Sound/OrgenalSund) فقط **تو منوی اصلی** پخش می‌شه؛
// وقتی بازی شروع شد (کارت‌ها/گیم‌پلی) قطع می‌شه، و با برگشت به منو دوباره از اول پخش می‌شه.
// از تنظیماتِ موسیقی (روشن/خاموش + بلندی) هم پیروی می‌کنه. اگه فایلِ آهنگ نباشه بی‌صدا و بی‌خطا کنار می‌مونه.
public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource source;
    private AudioClip musicClip;
    private bool inMenu = true;   // پیش‌فرض: تو منوییم (اولِ بازی)

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

        // آهنگِ اصلی از Resources لود می‌شه (اگه بود)
        musicClip = Resources.Load<AudioClip>("Sound/OrgenalSund");
        if (musicClip != null) source.clip = musicClip;

        // وقتی بازی شروع شد، آهنگِ منو قطع بشه
        CardSwipe.GameStarted += OnGameStarted;

        ApplyToSource();
    }

    void OnDestroy()
    {
        CardSwipe.GameStarted -= OnGameStarted;
    }

    void OnGameStarted()
    {
        SetMenu(false); // رفتیم تو گیم‌پلی → آهنگ قطع
    }

    // MainMenuUI موقعِ نمایشِ منو این رو true می‌زنه (بعد از ری‌لودِ صحنه هم همین‌طور).
    public static void SetMenu(bool value)
    {
        if (instance == null) return;
        instance.inMenu = value;
        instance.ApplyToSource();
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

        // فقط تو منو + اگه موسیقی روشنه + اگه فایلِ آهنگ هست
        bool shouldPlay = inMenu && SettingsSystem.MusicOn && musicClip != null;
        if (shouldPlay && !source.isPlaying) source.Play();
        else if (!shouldPlay && source.isPlaying) source.Stop(); // Stop تا دفعه‌ی بعد از اولِ آهنگ پخش شه
    }
}
