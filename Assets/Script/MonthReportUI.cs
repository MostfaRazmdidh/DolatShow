using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RTLTMPro;
using TMPro;

// کارنامه‌ی پایانِ ماهِ داستانی (مثلاً فروردین/کمپین). موقع اجرا با کد ساخته می‌شه (نیازی به وایرینگ
// تو صحنه نداره — CardSwipe موقعِ BeginGame با MonthReportUI.Create می‌سازتش).
// بر اساسِ وضعیتِ نهاییِ ۴ شاخص، یکی از ۴ «پایانِ» ماه رو انتخاب و نشون می‌ده، به‌همراه گزارشِ اعداد.
public class MonthReportUI : MonoBehaviour
{
    private TMP_FontAsset persianFont;
    private Canvas targetCanvas;

    private GameObject panel;
    private GameObject endingImageGO;
    private RTLTextMeshPro titleText;
    private RTLTextMeshPro endingTitleText;
    private RTLTextMeshPro endingBodyText;
    private RTLTextMeshPro headlineText;
    private RTLTextMeshPro statsText;

    // مسیرِ عکسِ اختیاریِ هر پایان تو Resources — اگه بذاری، بالای کارنامه نشون داده می‌شه
    private const string endingImageResourcePath = "Story/Farvardin/Endings/ending_";

    public static MonthReportUI Create(Canvas canvas, TMP_FontAsset font)
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
        ui.BuildUI();
        return ui;
    }

    void BuildUI()
    {
        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MonthReportPanel", new Color(0.03f, 0.03f, 0.05f, 0.92f));

        titleText = RuntimeUIHelper.CreateRTLText(panel.transform, "ReportTitle", new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f), 40, persianFont);
        titleText.text = "کارنامه‌ی فروردین";
        titleText.color = new Color(0.96f, 0.83f, 0.45f);
        titleText.fontStyle = FontStyles.Bold;

        // جای عکسِ پایان (اگه تو Resources باشه پر می‌شه)
        endingImageGO = null;

        endingTitleText = RuntimeUIHelper.CreateRTLText(panel.transform, "EndingTitle", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.72f), 44, persianFont);
        endingTitleText.fontStyle = FontStyles.Bold;

        endingBodyText = RuntimeUIHelper.CreateRTLText(panel.transform, "EndingBody", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.62f), 28, persianFont);
        endingBodyText.color = new Color(0.92f, 0.90f, 0.84f);

        headlineText = RuntimeUIHelper.CreateRTLText(panel.transform, "Headline", new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.41f), 24, persianFont);
        headlineText.color = new Color(0.75f, 0.78f, 0.85f);
        headlineText.fontStyle = FontStyles.Italic;

        statsText = RuntimeUIHelper.CreateRTLText(panel.transform, "Stats", new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.31f), 26, persianFont);
        statsText.color = new Color(0.96f, 0.83f, 0.45f);

        RuntimeUIHelper.CreateButton(panel.transform, "BackToMenuButton", new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.19f),
            "بازگشت به منو", persianFont, new Color(0.2f, 0.55f, 0.25f, 1f), BackToMenu);

        panel.SetActive(false);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    // اعداد نهایی رو می‌گیره، پایان رو انتخاب می‌کنه و کارنامه رو نشون می‌ده
    public void Show(int budget, int popularity, int security, int diplomacy)
    {
        int competence = Mathf.RoundToInt((budget + security + diplomacy) / 3f);
        bool popHigh = popularity >= 50;
        bool compHigh = competence >= 50;

        int endingIndex;
        string eTitle, eBody, eHeadline;

        if (popHigh && compHigh)
        {
            endingIndex = 0;
            eTitle = "قهرمانِ ملی!";
            eBody = "مردم رو دوش‌شون گذاشتنت، اقتصاددان‌ها برات دست زدن، و حتی رقیب‌هات مجبور شدن تبریک بگن. یه شروعِ رؤیایی برای کسی که هنوز عملاً هیچ کاری نکرده.";
            eHeadline = "تیتر روزنامه‌ها: «مسیحِ اقتصاد از راه رسید!»";
        }
        else if (popHigh && !compHigh)
        {
            endingIndex = 1;
            eTitle = "پوپولیستِ محبوب";
            eBody = "مردم عاشقتن، چون به همه، همه‌چیز رو قول دادی. فقط یه مشکلِ کوچیک هست: خزانه خالیه و هیچ‌کس نمی‌دونه این وعده‌ها قراره از کجا بیان. ولی فعلاً کی به فکرِ فرداست؟";
            eHeadline = "تیتر روزنامه‌ها: «رئیس‌جمهورِ وعده‌ها سوگند خورد؛ صف‌ها هم قول دادن بمونن.»";
        }
        else if (!popHigh && compHigh)
        {
            endingIndex = 2;
            eTitle = "تکنوکراتِ بی‌رأی";
            eBody = "نمودارهات بی‌نقص بودن، بودجه‌ت متوازن بود، و دقیقاً به همین خاطر کسی رأیت نداد. مردم آدمِ حسابگر نمی‌خوان، معجزه می‌خوان.";
            eHeadline = "تیتر روزنامه‌ها: «کاندیدایی که اکسل بلد بود، دلِ ملت رو بلد نبود.»";
        }
        else
        {
            endingIndex = 3;
            eTitle = "کاندیدای فراموش‌شده";
            eBody = "نه مردم دوستت داشتن، نه اوضاع رو بهتر کردی. تا آخرِ هفته اسمت هم یادِ کسی نمی‌مونه. شاید سیاست اصلاً کارِ تو نبود.";
            eHeadline = "تیتر روزنامه‌ها: «کدوم کاندیدا؟»";
        }

        endingTitleText.text = eTitle;
        endingTitleText.color = EndingColor(endingIndex);
        endingBodyText.text = eBody;
        headlineText.text = eHeadline;
        statsText.text = $"بودجه {Fa(budget)}    محبوبیت {Fa(popularity)}    امنیت {Fa(security)}    دیپلماسی {Fa(diplomacy)}";

        TrySetEndingImage(endingIndex);

        panel.transform.SetAsLastSibling(); // روی نوارهای وضعیت
        panel.SetActive(true);
    }

    Color EndingColor(int index)
    {
        switch (index)
        {
            case 0: return new Color(0.45f, 0.85f, 0.5f);  // سبز — قهرمان
            case 1: return new Color(0.96f, 0.75f, 0.35f);  // طلایی — پوپولیست
            case 2: return new Color(0.55f, 0.75f, 0.95f);  // آبی — تکنوکرات
            default: return new Color(0.85f, 0.45f, 0.4f);   // قرمز — فراموش‌شده
        }
    }

    // اگه عکسِ پایان تو Resources/Story/Farvardin/Endings/ending_<index> باشه، بالای کارنامه نشونش می‌ده
    void TrySetEndingImage(int index)
    {
        Sprite sprite = Resources.Load<Sprite>(endingImageResourcePath + index);

        if (sprite == null)
        {
            if (endingImageGO != null) endingImageGO.SetActive(false);
            return;
        }

        if (endingImageGO == null)
            endingImageGO = RuntimeUIHelper.CreateImage(panel.transform, "EndingImage", new Vector2(0.25f, 0.72f), new Vector2(0.75f, 0.87f), sprite);
        else
        {
            endingImageGO.SetActive(true);
            Image img = endingImageGO.GetComponent<Image>();
            if (img != null) img.sprite = sprite;
        }
    }

    void BackToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // عددِ انگلیسی رو به رقمِ فارسی تبدیل می‌کنه (مثلاً 52 → ۵۲)
    static string Fa(int n)
    {
        string s = n.ToString();
        string r = "";
        foreach (char c in s)
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
