using UnityEngine;
using UnityEngine.SceneManagement;
using RTLTMPro;
using TMPro;

// این اسکریپت کل صفحه‌ی پایان بازی (باخت/پیروزی) رو موقع اجرا با کد می‌سازه،
// چون امکان ساختن دستی آبجکت‌های UI تو خودِ Unity Editor نبود.
// کافیه این اسکریپت رو به یه آبجکت تو صحنه (مثلاً GameManager) اضافه کنی و
// فیلدهای Persian Font، Target Canvas، و Card Swipe رو تو Inspector پر کنی (پایین توضیح داده شده).
public class GameOverUI : MonoBehaviour
{
    [Header("فونت فارسی (همون NotoNaskhArabic-Regular SDF که برای کارت‌ها استفاده می‌شه)")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن، نه Canvas داخل کارت)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("آبجکت کارت (برای ادامه‌ی بازی بعد از دیدن تبلیغ)")]
    [SerializeField] private CardSwipe cardSwipe;

    [Header("تصویر دکمه‌ی بازگشت (اختیاری — اگه خالی بمونه از رنگ ساده استفاده می‌شه)")]
    [SerializeField] private Sprite backButtonSprite;

    private GameObject panel;
    private GameObject adButton;
    private RTLTextMeshPro messageText;
    private RTLTextMeshPro scoreText;

    private GameStats.StatType lastLossType;
    private bool lastLossHitMax;
    private bool wasLoss;

    void Start()
    {
        BuildUI();
        GameStats.Instance.OnGameLost += HandleGameLost;
        GameStats.Instance.OnGameWon += HandleGameWon;
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
        {
            GameStats.Instance.OnGameLost -= HandleGameLost;
            GameStats.Instance.OnGameWon -= HandleGameWon;
        }
    }

    void HandleGameLost(GameStats.StatType type, bool hitMax)
    {
        lastLossType = type;
        lastLossHitMax = hitMax;
        wasLoss = true;

        messageText.text = GetLossMessage(type, hitMax);
        adButton.SetActive(!GameStats.Instance.AdUsedThisRun);
        ShowScore();
        panel.transform.SetAsLastSibling(); // روی نوارهای وضعیت
        panel.SetActive(true);
        RuntimeUIHelper.PlayFadeIn(this, panel); // باز شدنِ نرم
    }

    void HandleGameWon()
    {
        wasLoss = false;
        messageText.text = "چهار سال ریاست‌جمهوری‌ت با موفقیت به پایان رسید!";
        adButton.SetActive(false);
        ShowScore();
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
        RuntimeUIHelper.PlayFadeIn(this, panel); // باز شدنِ نرم
    }

    // ریالِ به‌دست‌آمده از این دور (به‌جای امتیاز) — به موجودی اضافه می‌شه
    void ShowScore()
    {
        var g = GameStats.Instance;
        int earned = RialSystem.RewardRun(g.Budget, g.Popularity, g.Security, g.Diplomacy);
        scoreText.text = $"ریالِ به‌دست‌آمده: +{ToPersian(earned)}      موجودی: {ToPersian(RialSystem.Total)}";
    }

    static string ToPersian(int n)
    {
        string r = "";
        foreach (char c in n.ToString())
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }

    // «بازیِ دوباره» — فوری یه بازیِ جدید شروع می‌کنه (بدونِ برگشت به منو)
    void RetryGame()
    {
        panel.SetActive(false);
        if (cardSwipe != null) cardSwipe.RestartNewGame();
        else BackToMenu();
    }

    // پیام‌های ساده‌ی هر ۸ نوع باخت — بعداً موقع نوشتن محتوای اصلی می‌شه با متن بهتر جایگزین‌شون کرد
    string GetLossMessage(GameStats.StatType type, bool hitMax)
    {
        switch (type)
        {
            case GameStats.StatType.Budget:
                return hitMax
                    ? "ابرتورم! چاپ بی‌رویه‌ی پول اقتصاد کشور رو نابود کرد."
                    : "ورشکستگی! خزانه‌ی دولت کاملاً خالی شد.";
            case GameStats.StatType.Popularity:
                return hitMax
                    ? "هرج‌ومرج پوپولیستی! وعده‌های بیش‌ازحد، اوضاع رو از کنترل خارج کرد."
                    : "استعفای اجباری! مردم دیگه بهت اعتماد ندارن.";
            case GameStats.StatType.Security:
                return hitMax
                    ? "دولت پلیسی! سرکوب بیش‌ازحد، نظم اجتماعی رو از بین برد."
                    : "هرج‌ومرج! امنیت داخلی کشور کاملاً فروپاشید.";
            default: // Diplomacy
                return hitMax
                    ? "وابستگی کامل به خارج! کشور استقلالش رو از دست داد."
                    : "انزوای بین‌المللی! کشور از همه‌ی متحدانش بریده شد.";
        }
    }

    // فعلاً بدون SDK واقعی تبلیغ — فقط شبیه‌سازی می‌کنه: طبق سند طراحی،
    // شاخص بحرانی رو به ۳۰ برمی‌گردونه و بازی همون‌جا ادامه پیدا می‌کنه (حداکثر ۱ بار در هر بازی)
    void WatchAdAndContinue()
    {
        if (!wasLoss) return;

        GameStats.Instance.RecoverStatViaAd(lastLossType, lastLossHitMax);
        panel.SetActive(false);
        cardSwipe.BeginGame();
    }

    // چون بعد از این دکمه به منوی اصلی برمی‌گردیم (نه مستقیم بازی جدید)، سیو رو نگه می‌داریم
    // مگر اینکه از قبل به‌خاطر باخت پاک شده باشه (تو CardSwipe انجام می‌شه)
    void BackToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // کل ساختار صفحه‌ی پایان بازی (پس‌زمینه‌ی تیره + متن + دکمه‌ها) رو با کد می‌سازه
    void BuildUI()
    {
        // اول از Prefab (قابلِ ویرایش تو ادیتور) امتحان می‌کنیم؛ اگه نبود به روشِ کدیِ قبلی برمی‌گردیم
        if (BuildFromPrefab()) return;

        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.85f));

        messageText = RuntimeUIHelper.CreateRTLText(panel.transform, "Message", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.85f), 36, persianFont);

        // خطِ امتیاز + رکورد
        scoreText = RuntimeUIHelper.CreateRTLText(panel.transform, "Score", new Vector2(0.1f, 0.46f), new Vector2(0.9f, 0.54f), 30, persianFont);
        scoreText.color = new Color(0.98f, 0.86f, 0.5f);
        scoreText.fontStyle = TMPro.FontStyles.Bold;

        adButton = RuntimeUIHelper.CreateButton(panel.transform, "WatchAdButton", new Vector2(0.28f, 0.33f), new Vector2(0.72f, 0.42f), "دیدن تبلیغ و ادامه", persianFont, new Color(0.6f, 0.5f, 0.15f, 1f), WatchAdAndContinue);

        // «بازیِ دوباره» — فوری یه بازیِ جدید (حلقه‌ی «یه بار دیگه»)
        RuntimeUIHelper.CreateButton(panel.transform, "RetryButton", new Vector2(0.28f, 0.22f), new Vector2(0.72f, 0.31f), "بازیِ دوباره", persianFont, new Color(0.18f, 0.5f, 0.24f, 1f), RetryGame);

        // دکمه‌ی «بازگشت» — از پکِ ButtonAssets (اگه تو صحنه وصل نشده، از Resources لود می‌شه)
        Sprite back = backButtonSprite != null ? backButtonSprite : Resources.Load<Sprite>("UI/Btn_Return");
        if (back != null)
            RuntimeUIHelper.CreateImageButton(panel.transform, "BackToMenuButton", new Vector2(0.34f, 0.10f), new Vector2(0.66f, 0.19f), back, BackToMenu);
        else
            RuntimeUIHelper.CreateButton(panel.transform, "BackToMenuButton", new Vector2(0.3f, 0.10f), new Vector2(0.7f, 0.19f), "بازگشت به منو", persianFont, new Color(0.2f, 0.45f, 0.55f, 1f), BackToMenu);

        panel.SetActive(false);
    }

    // صفحه‌ی پایان بازی رو از Resources/Prefabs/GameOverPanel می‌سازه (چیدمان تو خودِ Prefab).
    // متن‌ها/فونت‌ها و کلیکِ دکمه‌ها رو با کد وصل می‌کنیم. اگه Prefab نبود false برمی‌گردونه.
    bool BuildFromPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/GameOverPanel");
        if (prefab == null) return false;

        panel = Instantiate(prefab, targetCanvas.transform);
        panel.name = "GameOverPanel";

        messageText = FindText("Message");
        scoreText = FindText("Score");

        // فونتِ فارسی رو روی همه‌ی متن‌ها (از جمله برچسبِ دکمه‌ها) ست می‌کنیم
        if (persianFont != null)
            foreach (var t in panel.GetComponentsInChildren<RTLTextMeshPro>(true))
                t.font = persianFont;

        adButton = FindChild("WatchAdButton");
        WireButton("WatchAdButton", WatchAdAndContinue);
        WireButton("RetryButton", RetryGame);
        WireButton("BackToMenuButton", BackToMenu);

        panel.SetActive(false);
        return true;
    }

    RTLTextMeshPro FindText(string name)
    {
        Transform t = FindDeep(panel.transform, name);
        if (t == null) return null;
        var r = t.GetComponent<RTLTextMeshPro>();
        if (r != null && persianFont != null) r.font = persianFont;
        return r;
    }

    GameObject FindChild(string name)
    {
        Transform t = FindDeep(panel.transform, name);
        return t != null ? t.gameObject : null;
    }

    void WireButton(string name, UnityEngine.Events.UnityAction action)
    {
        Transform t = FindDeep(panel.transform, name);
        if (t == null) return;
        var b = t.GetComponent<UnityEngine.UI.Button>();
        if (b == null) b = t.gameObject.AddComponent<UnityEngine.UI.Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(action);
    }

    static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform c in parent)
        {
            Transform r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
