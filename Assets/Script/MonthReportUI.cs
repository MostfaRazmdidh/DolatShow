using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RTLTMPro;
using TMPro;

// کارنامه‌ی پایانِ ماهِ داستانی، روی تصویرِ «روزنامه» (Assets/Resources/UI/Newspaper) نوشته می‌شه.
// موقعِ اجرا با کد ساخته می‌شه (CardSwipe.BeginGame با MonthReportUI.Create می‌سازتش) — بدونِ وایرینگِ صحنه.
public class MonthReportUI : MonoBehaviour
{
    private TMP_FontAsset persianFont;
    private Canvas targetCanvas;
    private CardSwipe cardSwipe; // برای دکمه‌ی «دوباره» (شروعِ فوریِ بازیِ جدید)

    private GameObject panel;
    private RTLTextMeshPro mastheadText;   // «کارنامه‌ی فروردین» تو بنرِ مشکیِ بالای روزنامه
    private RTLTextMeshPro endingTitleText; // اسمِ پایان (تیترِ درشت)
    private RTLTextMeshPro endingBodyText;  // متنِ طنز
    private RTLTextMeshPro headlineText;     // تیترِ روزنامه
    private RTLTextMeshPro statsText;        // اعدادِ نهایی
    private RTLTextMeshPro scoreText;        // امتیاز + رکورد

    private static readonly Color parchmentInk = new Color(0.24f, 0.14f, 0.05f); // قهوه‌ای تیره روی کاغذِ کاهی

    public static MonthReportUI Create(Canvas canvas, TMP_FontAsset font, CardSwipe cardSwipe = null)
    {
        if (canvas == null)
        {
            Debug.LogWarning("MonthReportUI: Canvas پیدا نشد؛ کارنامه ساخته نشد.");
            return null;
        }
        GameObject go = new GameObject("MonthReportUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        MonthReportUI ui = go.AddComponent<MonthReportUI>();
        ui.persianFont = font;
        ui.targetCanvas = canvas;
        ui.cardSwipe = cardSwipe;
        ui.BuildUI();
        return ui;
    }

    void BuildUI()
    {
        // پس‌زمینه‌ی تیره‌ی محو پشتِ روزنامه
        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MonthReportPanel", new Color(0f, 0f, 0f, 0.8f));

        // تصویرِ روزنامه، بالای صفحه (کوچیک‌تر از قبل تا زیرش برای دکمه جا باشه)، با نسبتِ درست
        Sprite paper = Resources.Load<Sprite>("UI/Newspaper");
        GameObject paperGO = new GameObject("Newspaper", typeof(RectTransform));
        paperGO.transform.SetParent(panel.transform, false);
        RectTransform paperRT = paperGO.GetComponent<RectTransform>();
        paperRT.anchorMin = paperRT.anchorMax = new Vector2(0.5f, 1f); // بالا-وسط
        paperRT.pivot = new Vector2(0.5f, 1f);
        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        float aspect = paper != null ? (paper.rect.width / paper.rect.height) : (1024f / 1536f);
        float ph = canvasSize.y * 0.78f; // کوچیک‌تر از صفحه تا پایینش برای دکمه‌ی خانه جا بمونه
        float pw = ph * aspect;
        if (pw > canvasSize.x * 0.96f) { pw = canvasSize.x * 0.96f; ph = pw / aspect; }
        paperRT.sizeDelta = new Vector2(pw, ph);
        paperRT.anchoredPosition = new Vector2(0f, -canvasSize.y * 0.02f);
        if (paper != null)
        {
            Image img = paperGO.AddComponent<Image>();
            img.sprite = paper;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        // همه‌ی متن‌ها فرزندِ روزنامه‌ان تا با کسرها دقیق سرِ جاشون بشینن (متن‌ها بزرگ‌تر شدن)
        mastheadText = MakeText(paperRT, "Masthead", 0.13f, 0.795f, 0.87f, 0.87f, 46, new Color(0.96f, 0.86f, 0.55f));
        mastheadText.fontStyle = FontStyles.Bold;
        mastheadText.text = "کارنامه‌ی فروردین";

        endingTitleText = MakeText(paperRT, "EndingTitle", 0.12f, 0.65f, 0.88f, 0.77f, 56, parchmentInk);
        endingTitleText.fontStyle = FontStyles.Bold;

        endingBodyText = MakeText(paperRT, "EndingBody", 0.12f, 0.45f, 0.88f, 0.64f, 32, parchmentInk);

        headlineText = MakeText(paperRT, "Headline", 0.12f, 0.35f, 0.88f, 0.44f, 30, new Color(0.45f, 0.28f, 0.12f));
        headlineText.fontStyle = FontStyles.Italic;

        statsText = MakeText(paperRT, "Stats", 0.05f, 0.27f, 0.95f, 0.35f, 32, new Color(0.35f, 0.2f, 0.06f));
        statsText.fontStyle = FontStyles.Bold;

        // خطِ ریالِ به‌دست‌آمده (به‌جای امتیاز)
        scoreText = MakeText(paperRT, "Rial", 0.05f, 0.17f, 0.95f, 0.26f, 34, new Color(0.5f, 0.28f, 0.05f));
        scoreText.fontStyle = FontStyles.Bold;

        // دو دکمه‌ی تصویریِ پایینِ صفحه (بیرونِ روزنامه): «بازیِ دوباره» (چپ) و «خانه» (راست)
        // هر کدوم با نسبتِ تصویرِ خودش ساخته می‌شه تا کامل و بدونِ بریدگی باشه.
        AddImageButton("ReplayButton", "UI/Btn_Replay", 0.30f, 0.085f, 0.035f, RetryGame);
        AddImageButton("HomeButton", "UI/Btn_Home", 0.72f, 0.085f, 0.035f, BackToMenu);

        panel.SetActive(false);
    }

    // یه دکمه‌ی تصویری وسطِ نقطه‌ی cx پایینِ صفحه می‌سازه؛ ارتفاع ثابت و عرض از نسبتِ خودِ تصویر (بدونِ بریدگی).
    void AddImageButton(string name, string resPath, float cx, float hFrac, float bottom, UnityEngine.Events.UnityAction onClick)
    {
        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        Sprite sp = Resources.Load<Sprite>(resPath);
        if (sp != null)
        {
            float aspect = sp.rect.width / sp.rect.height;
            float wFrac = (hFrac * canvasSize.y * aspect) / canvasSize.x;
            RuntimeUIHelper.CreateImageButton(panel.transform, name,
                new Vector2(cx - wFrac / 2f, bottom), new Vector2(cx + wFrac / 2f, bottom + hFrac), sp, onClick);
        }
        else
        {
            RuntimeUIHelper.CreateButton(panel.transform, name,
                new Vector2(cx - 0.18f, bottom), new Vector2(cx + 0.18f, bottom + hFrac),
                name == "HomeButton" ? "بازگشت به منو" : "بازیِ دوباره", persianFont,
                new Color(0.25f, 0.4f, 0.5f, 1f), onClick);
        }
    }

    RTLTextMeshPro MakeText(Transform parent, string name, float xMin, float yMin, float xMax, float yMax, float size, Color color)
    {
        RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(parent, name, new Vector2(xMin, yMin), new Vector2(xMax, yMax), size, persianFont);
        t.color = color;
        t.raycastTarget = false; // متن نباید جلوی کلیکِ دکمه رو بگیره
        return t;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show(int budget, int popularity, int security, int diplomacy)
    {
        int competence = Mathf.RoundToInt((budget + security + diplomacy) / 3f);
        bool popHigh = popularity >= 50;
        bool compHigh = competence >= 50;

        string eTitle, eBody, eHeadline;
        Color titleColor;

        if (popHigh && compHigh)
        {
            eTitle = "قهرمانِ ملی!";
            eBody = "مردم رو دوش‌شون گذاشتنت، اقتصاددان‌ها برات دست زدن، و حتی رقیب‌هات مجبور شدن تبریک بگن. یه شروعِ رؤیایی برای کسی که هنوز عملاً هیچ کاری نکرده.";
            eHeadline = "تیتر روزنامه‌ها: «مسیحِ اقتصاد از راه رسید!»";
            titleColor = new Color(0.15f, 0.45f, 0.2f);
        }
        else if (popHigh && !compHigh)
        {
            eTitle = "پوپولیستِ محبوب";
            eBody = "مردم عاشقتن، چون به همه، همه‌چیز رو قول دادی. فقط یه مشکلِ کوچیک هست: خزانه خالیه و هیچ‌کس نمی‌دونه این وعده‌ها قراره از کجا بیان. ولی فعلاً کی به فکرِ فرداست؟";
            eHeadline = "تیتر روزنامه‌ها: «رئیس‌جمهورِ وعده‌ها سوگند خورد؛ صف‌ها هم قول دادن بمونن.»";
            titleColor = new Color(0.6f, 0.42f, 0.1f);
        }
        else if (!popHigh && compHigh)
        {
            eTitle = "تکنوکراتِ بی‌رأی";
            eBody = "نمودارهات بی‌نقص بودن، بودجه‌ت متوازن بود، و دقیقاً به همین خاطر کسی رأیت نداد. مردم آدمِ حسابگر نمی‌خوان، معجزه می‌خوان.";
            eHeadline = "تیتر روزنامه‌ها: «کاندیدایی که اکسل بلد بود، دلِ ملت رو بلد نبود.»";
            titleColor = new Color(0.15f, 0.3f, 0.55f);
        }
        else
        {
            eTitle = "کاندیدای فراموش‌شده";
            eBody = "نه مردم دوستت داشتن، نه اوضاع رو بهتر کردی. تا آخرِ هفته اسمت هم یادِ کسی نمی‌مونه. شاید سیاست اصلاً کارِ تو نبود.";
            eHeadline = "تیتر روزنامه‌ها: «کدوم کاندیدا؟»";
            titleColor = new Color(0.55f, 0.2f, 0.15f);
        }

        endingTitleText.text = eTitle;
        endingTitleText.color = titleColor;
        endingBodyText.text = eBody;
        headlineText.text = eHeadline;
        statsText.text = $"بودجه {Fa(budget)}    محبوبیت {Fa(popularity)}    امنیت {Fa(security)}    دیپلماسی {Fa(diplomacy)}";

        // ریالِ به‌دست‌آمده از این دور (به‌جای امتیاز) — به موجودی اضافه می‌شه
        int earned = RialSystem.RewardRun(budget, popularity, security, diplomacy);
        scoreText.text = $"ریالِ به‌دست‌آمده: +{Fa(earned)}      موجودی: {Fa(RialSystem.Total)}";

        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
    }

    // «بازیِ دوباره» — بدونِ برگشت به منو، فوری یه بازیِ جدید شروع می‌کنه
    void RetryGame()
    {
        Time.timeScale = 1f;
        Hide();
        if (cardSwipe != null) cardSwipe.RestartNewGame();
        else BackToMenu(); // اگه رفرنس نبود، حداقل به منو برگرد
    }

    void BackToMenu()
    {
        // با ری‌لودِ صحنه به منوی اصلی برمی‌گردیم. (حالا که DontDestroyOnLoad از GameStats حذف شده،
        // GameManagerِ قدیمی با صحنه پاک می‌شه و نسخه‌ی تازه‌ش منوی نو رو درست نشون می‌ده.)
        Time.timeScale = 1f; // محضِ اطمینان اگه جایی pause شده بود
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    static string Fa(int n)
    {
        string s = n.ToString();
        string r = "";
        foreach (char c in s)
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
