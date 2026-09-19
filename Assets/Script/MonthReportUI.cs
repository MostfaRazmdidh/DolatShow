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

    private GameObject panel;
    private RTLTextMeshPro mastheadText;   // «کارنامه‌ی فروردین» تو بنرِ مشکیِ بالای روزنامه
    private RTLTextMeshPro endingTitleText; // اسمِ پایان (تیترِ درشت)
    private RTLTextMeshPro endingBodyText;  // متنِ طنز
    private RTLTextMeshPro headlineText;     // تیترِ روزنامه
    private RTLTextMeshPro statsText;        // اعدادِ نهایی

    private static readonly Color parchmentInk = new Color(0.24f, 0.14f, 0.05f); // قهوه‌ای تیره روی کاغذِ کاهی

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
        // پس‌زمینه‌ی تیره‌ی محو پشتِ روزنامه
        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MonthReportPanel", new Color(0f, 0f, 0f, 0.8f));

        // تصویرِ روزنامه، وسطِ صفحه، هم‌اندازه‌ی نسبتِ خودش (بدون کش‌آمدن) تا متن‌ها سرِ جاشون بشینن
        Sprite paper = Resources.Load<Sprite>("UI/Newspaper");
        GameObject paperGO = new GameObject("Newspaper", typeof(RectTransform));
        paperGO.transform.SetParent(panel.transform, false);
        RectTransform paperRT = paperGO.GetComponent<RectTransform>();
        paperRT.anchorMin = paperRT.anchorMax = new Vector2(0.5f, 0.5f);
        paperRT.pivot = new Vector2(0.5f, 0.5f);
        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        float aspect = paper != null ? (paper.rect.width / paper.rect.height) : (1024f / 1536f);
        float ph = canvasSize.y * 0.96f;
        float pw = ph * aspect;
        if (pw > canvasSize.x * 0.98f) { pw = canvasSize.x * 0.98f; ph = pw / aspect; }
        paperRT.sizeDelta = new Vector2(pw, ph);
        paperRT.anchoredPosition = Vector2.zero;
        if (paper != null)
        {
            Image img = paperGO.AddComponent<Image>();
            img.sprite = paper;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        // همه‌ی متن‌ها فرزندِ روزنامه‌ان تا با کسرها دقیق سرِ جاشون بشینن
        mastheadText = MakeText(paperRT, "Masthead", 0.13f, 0.795f, 0.87f, 0.865f, 40, new Color(0.96f, 0.86f, 0.55f));
        mastheadText.fontStyle = FontStyles.Bold;
        mastheadText.text = "کارنامه‌ی فروردین";

        endingTitleText = MakeText(paperRT, "EndingTitle", 0.12f, 0.66f, 0.88f, 0.76f, 46, parchmentInk);
        endingTitleText.fontStyle = FontStyles.Bold;

        endingBodyText = MakeText(paperRT, "EndingBody", 0.13f, 0.45f, 0.87f, 0.64f, 26, parchmentInk);

        headlineText = MakeText(paperRT, "Headline", 0.13f, 0.35f, 0.87f, 0.43f, 24, new Color(0.45f, 0.28f, 0.12f));
        headlineText.fontStyle = FontStyles.Italic;

        statsText = MakeText(paperRT, "Stats", 0.06f, 0.26f, 0.94f, 0.34f, 26, new Color(0.35f, 0.2f, 0.06f));
        statsText.fontStyle = FontStyles.Bold;

        // دکمه‌ی خانه (بازگشت به منوی اصلی) — پایینِ روزنامه
        Sprite home = Resources.Load<Sprite>("UI/Btn_Home");
        if (home != null)
        {
            float bw = 0.42f, bAspect = home.rect.width / home.rect.height;
            // ارتفاعِ دکمه نسبت به پهناش (که کسری از پهنای روزنامه‌ست)
            float bhFrac = (bw * pw / bAspect) / ph;
            float cx = 0.5f, cy = 0.16f;
            var go = RuntimeUIHelper.CreateImageButton(paperRT, "HomeButton",
                new Vector2(cx - bw / 2f, cy - bhFrac / 2f), new Vector2(cx + bw / 2f, cy + bhFrac / 2f), home, BackToMenu);
        }
        else
        {
            RuntimeUIHelper.CreateButton(paperRT, "HomeButton", new Vector2(0.3f, 0.11f), new Vector2(0.7f, 0.2f),
                "بازگشت به منو", persianFont, new Color(0.2f, 0.55f, 0.25f, 1f), BackToMenu);
        }

        panel.SetActive(false);
    }

    RTLTextMeshPro MakeText(Transform parent, string name, float xMin, float yMin, float xMax, float yMax, float size, Color color)
    {
        RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(parent, name, new Vector2(xMin, yMin), new Vector2(xMax, yMax), size, persianFont);
        t.color = color;
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

        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
    }

    void BackToMenu()
    {
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
