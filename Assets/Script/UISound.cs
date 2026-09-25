using UnityEngine;
using UnityEngine.UI;

// صدای کلیکِ سراسریِ دکمه‌ها. به‌جای اینکه به تک‌تکِ ۷۰+ دکمه‌ی بازی دستی صدا وصل کنیم،
// این سیستم خودکار همه‌ی Buttonهای صحنه رو پیدا می‌کنه و بهشون صدای کلیک وصل می‌کنه —
// حتی دکمه‌هایی که بعداً (تو پنل‌های داینامیک مثل تنظیمات/پروفایل) ساخته می‌شن.
//
// صدا از Resources/Sound/Click لود می‌شه (می‌تونی این فایل رو با هر کلیکِ دلخواهِ خودت جایگزین کنی).
// چون از AudioSource پخش می‌شه، خودکار از تنظیماتِ صدا (AudioListener.volume) پیروی می‌کنه.
public class UISound : MonoBehaviour
{
    private static UISound instance;
    private AudioSource src;
    private AudioClip clickClip;
    private float scanTimer;
    private const float ScanInterval = 0.3f; // هر ۰.۳ ثانیه دنبالِ دکمه‌های جدید می‌گرده

    // ساختِ نمونه (اگه نبود). MainMenuUI موقعِ شروع صداش می‌زنه.
    public static void EnsureExists()
    {
        if (instance != null) return;
        GameObject go = new GameObject("UISound");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<UISound>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        clickClip = Resources.Load<AudioClip>("Sound/Click");

        HookAll(); // دکمه‌های موجود رو همین الان وصل کن
    }

    void Update()
    {
        scanTimer += Time.unscaledDeltaTime;
        if (scanTimer >= ScanInterval)
        {
            scanTimer = 0f;
            HookAll();
        }
    }

    // صدای کلیک رو پخش می‌کنه (از هر جای کد هم می‌شه صداش زد)
    public static void PlayClick()
    {
        if (instance != null && instance.src != null && instance.clickClip != null)
            instance.src.PlayOneShot(instance.clickClip);
    }

    // همه‌ی دکمه‌های صحنه (حتی توی پنل‌های غیرفعال) رو پیدا و — اگه هنوز وصل نشدن — صدای کلیک بهشون وصل می‌کنه.
    // برای اینکه دوبار وصل نشه، به هر دکمه یه نشانگرِ کوچیک (UISoundHooked) اضافه می‌کنیم.
    public static void HookAll()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button b in buttons)
        {
            if (b == null) continue;
            if (b.GetComponent<UISoundHooked>() != null) continue; // قبلاً وصل شده
            b.gameObject.AddComponent<UISoundHooked>();
            b.onClick.AddListener(PlayClick);
        }
    }
}

// نشانگرِ خالی: فقط برای اینکه بدونیم یه دکمه قبلاً صدای کلیک گرفته (تا دوباره اضافه نشه).
public class UISoundHooked : MonoBehaviour { }
