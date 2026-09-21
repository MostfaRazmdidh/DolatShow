using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// نوارِ وضعیتِ هر شاخص. طرحِ جدید: به‌جای اسلایدرِ چرخیده‌ی قدیمی، یه «بج» (badge) کاملِ آماده
// (تصویر توی Assets/Resources/Bars/) نشون داده می‌شه که آیکون و اسمِ شاخص داخلش پخته شده. این اسکریپت
// فقط «پُرشدنِ طلایی» رو توی پنلِ پایینیِ بج (Fill Area) و عددِ شاخص رو مدیریت می‌کنه.
public class StatBarUI : MonoBehaviour
{
    [SerializeField] private GameStats.StatType statType;
    [SerializeField] private Slider slider; // اسلایدرِ قدیمی — دیگه دیده نمی‌شه (visualش مخفی می‌شه)

    [Header("پیش‌نمایش/نتیجه‌ی اثر (از CardSwipe کنترل می‌شه)")]
    [SerializeField] private TMP_Text hintText; // قدیمی — دیگه استفاده نمی‌شه (به‌جاش hint رو کد می‌سازه)

    [Header("فونت فارسی (برای عدد و +/-)")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("چیدمانِ بجِ نوار")]
    [Tooltip("ارتفاعِ بج (پیکسل). عرض خودکار از نسبتِ تصویر حساب می‌شه.")]
    [SerializeField] private float barHeight = 430f;
    [Tooltip("فاصله‌ی بج از بالای صفحه (پیکسل)")]
    [SerializeField] private float topMargin = 30f;

    [Header("محدوده‌ی پُرشدنِ طلایی داخلِ پنلِ مستطیلیِ پایین (کسری از خودِ تصویرِ بج)")]
    [Tooltip("لبه‌ی چپِ پنلِ پایینی (۰ تا ۱) — اندازه‌گیری‌شده از روی تصویرِ Sagsa تا کلِ باکس پر بشه")]
    [SerializeField] private float fillLeftFrac = 0.125f;
    [SerializeField] private float fillRightFrac = 0.85f;
    [Tooltip("کفِ پنلِ پایینی از پایینِ تصویر (۰ تا ۱)")]
    [SerializeField] private float fillBottomFrac = 0.03f;
    [Tooltip("سقفِ پنلِ پایینی از پایینِ تصویر (۰ تا ۱)")]
    [SerializeField] private float fillTopFrac = 0.44f;

    [Header("رنگ‌ها و اندازه")]
    [SerializeField] private Color fillColor = new Color(0.90f, 0.66f, 0.20f, 1f); // طلاییِ گرم
    [SerializeField] private float valueFontSize = 34f;

    [Header("Juice — انیمیشن و حسِ تغییرِ شاخص")]
    [Tooltip("چند ثانیه طول بکشه تا عدد و نوار به مقدارِ جدید برسن (پرشِ نرم)")]
    [SerializeField] private float animDuration = 0.45f;
    [Tooltip("چقدر بج موقعِ تغییر «پرش» کنه (۰.۱۲ = ۱۲٪ بزرگ‌تر و برگشت)")]
    [SerializeField] private float popScale = 0.12f;
    [Tooltip("زیرِ این مقدار یا بالای (۱۰۰ منهای این)، بج نبضِ قرمزِ هشدار می‌زنه")]
    [SerializeField] private int dangerThreshold = 15;
    [SerializeField] private Color flashUpColor = new Color(0.35f, 0.95f, 0.45f);
    [SerializeField] private Color flashDownColor = new Color(0.98f, 0.4f, 0.35f);

    [Header("تنظیمِ دقیقِ Fill داخلِ باکس (پیکسل — همون اندازه‌هایی که تو Inspector فرستادی)")]
    [Tooltip("فاصله‌ی طلایی از لبه‌ی چپِ باکس")]
    [SerializeField] private float fillPadLeft = 10.77f;
    [Tooltip("فاصله‌ی طلایی از لبه‌ی راستِ باکس")]
    [SerializeField] private float fillPadRight = 7.18f;
    [Tooltip("فاصله از بالای باکس (وقتی پُرِ کامل)")]
    [SerializeField] private float fillPadTop = 0f;
    [Tooltip("فاصله از کفِ باکس")]
    [SerializeField] private float fillPadBottom = 0f;

    public GameStats.StatType StatType => statType;

    private RectTransform fillRT;   // بخشِ طلاییِ پرشونده — ارتفاعش با مقدارِ شاخص عوض می‌شه
    private float fillAreaHeight;   // ارتفاعِ باکس (پیکسل) — برای محاسبه‌ی دقیقِ پرشدن
    private TMP_Text valueText;     // عددِ شاخص
    private TMP_Text hintRuntime;   // +/- که بعد از تصمیم نشون داده می‌شه
    private GameObject badgeGO;     // خودِ بج — تا شروعِ بازی مخفیه (تو منو دیده نشه)
    private RectTransform badgeRT;  // برای انیمیشنِ پرش
    private Image dangerOverlay;    // لایه‌ی قرمزِ هشدار که نبض می‌زنه
    private Color valueBaseColor = Color.black; // رنگِ پایه‌ی عدد (برای برگردوندن بعد از فلش)

    private float displayValue;     // مقدارِ فعلیِ نشون‌داده‌شده (برای انیمیشنِ نرم)
    private Coroutine animCo;       // کوروتینِ انیمیشنِ تغییر

    void Start()
    {
        HideOldSliderVisual();
        BuildBadge();

        int val = GameStats.Instance.GetStat(statType);
        displayValue = val;
        SetFillF(val);
        UpdateValueText(val);

        GameStats.Instance.OnStatChanged += HandleStatChanged;
    }

    // نبضِ قرمزِ هشدار وقتی شاخص به لبه‌ی باخت (۰ یا ۱۰۰) نزدیکه — حسِ خطر/تنش می‌سازه
    void Update()
    {
        if (dangerOverlay == null || badgeGO == null || !badgeGO.activeInHierarchy) return;
        int v = Mathf.RoundToInt(displayValue);
        bool danger = v <= dangerThreshold || v >= 100 - dangerThreshold;
        float a = danger ? (0.15f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 4f))) : 0f;
        Color c = dangerOverlay.color;
        dangerOverlay.color = new Color(c.r, c.g, c.b, a);
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
            GameStats.Instance.OnStatChanged -= HandleStatChanged;
    }

    void HandleStatChanged(GameStats.StatType changedType, int newValue)
    {
        if (changedType != statType) return;

        // اگه بج هنوز فعال نشده (مثلاً موقعِ لودِ سیو قبل از شروع)، بدونِ انیمیشن ست کن
        if (badgeGO == null || !badgeGO.activeInHierarchy)
        {
            displayValue = newValue;
            SetFillF(newValue);
            UpdateValueText(newValue);
            return;
        }

        bool increased = newValue > displayValue;
        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimateChange(newValue, increased));
    }

    // انیمیشنِ نرمِ تغییرِ شاخص: پرشِ عدد + پرشدنِ نوار + پرشِ اندازه‌ی بج + فلشِ رنگِ عدد
    IEnumerator AnimateChange(int target, bool increased)
    {
        float from = displayValue;
        float t = 0f;
        Color flash = increased ? flashUpColor : flashDownColor;
        if (valueText != null) valueText.color = flash;

        while (t < animDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / animDuration);
            displayValue = Mathf.Lerp(from, target, k);
            SetFillF(displayValue);
            UpdateValueText(Mathf.RoundToInt(displayValue));

            // پرشِ اندازه: اول بزرگ، بعد برگشت (نیم‌سینوس)
            if (badgeRT != null)
                badgeRT.localScale = Vector3.one * (1f + popScale * Mathf.Sin(k * Mathf.PI));

            // رنگِ عدد از فلش به رنگِ پایه برمی‌گرده
            if (valueText != null)
                valueText.color = Color.Lerp(flash, valueBaseColor, k);

            yield return null;
        }

        displayValue = target;
        SetFillF(target);
        UpdateValueText(target);
        if (badgeRT != null) badgeRT.localScale = Vector3.one;
        if (valueText != null) valueText.color = valueBaseColor;
        animCo = null;
    }

    // ویژوالِ اسلایدرِ چرخیده‌ی قدیمی رو مخفی می‌کنه (نوارِ جدید یه بجِ صافِ روی Canvas‌ه)
    void HideOldSliderVisual()
    {
        if (slider == null) return;
        DisableChild(slider.transform, "Background");
        DisableChild(slider.transform, "Fill Area");
        DisableChild(slider.transform, "Handle Slide Area");
    }

    void DisableChild(Transform parent, string childName)
    {
        Transform t = parent.Find(childName);
        if (t != null) t.gameObject.SetActive(false);
    }

    void BuildBadge()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        Sprite badge = Resources.Load<Sprite>("Bars/" + FileName(statType));
        if (badge == null)
        {
            Debug.LogWarning($"StatBarUI: تصویرِ بج برای {statType} تو Resources/Bars پیدا نشد.");
            return;
        }

        // خودِ بج — بالای صفحه، وسطِ اسلاتِ افقیِ خودش
        badgeGO = new GameObject("Badge_" + statType, typeof(RectTransform));
        badgeGO.transform.SetParent(canvas.transform, false);
        badgeRT = badgeGO.GetComponent<RectTransform>();
        float xFrac = SlotXFraction(statType);
        badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(xFrac, 1f);
        badgeRT.pivot = new Vector2(0.5f, 1f);
        float aspect = badge.rect.width / badge.rect.height;
        badgeRT.sizeDelta = new Vector2(barHeight * aspect, barHeight);
        badgeRT.anchoredPosition = new Vector2(0f, -topMargin);

        Image badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.sprite = badge;
        badgeImg.raycastTarget = false;

        // لایه‌ی قرمزِ هشدار — رو کلِ بج، اولش نامرئی؛ تو Update وقتی شاخص بحرانی شد نبض می‌زنه
        GameObject dangerGO = new GameObject("DangerOverlay", typeof(RectTransform));
        dangerGO.transform.SetParent(badgeGO.transform, false);
        RectTransform dangerRT = dangerGO.GetComponent<RectTransform>();
        dangerRT.anchorMin = Vector2.zero; dangerRT.anchorMax = Vector2.one;
        dangerRT.offsetMin = dangerRT.offsetMax = Vector2.zero;
        dangerOverlay = dangerGO.AddComponent<Image>();
        if (badge != null) dangerOverlay.sprite = badge; // فرمِ بج رو بگیره تا فقط خودِ نوار قرمز شه، نه یه مستطیل
        dangerOverlay.color = new Color(1f, 0.15f, 0.1f, 0f);
        dangerOverlay.raycastTarget = false;

        // پنلِ پایینی (Fill Area) — محدوده‌ای که طلایی توش پر می‌شه
        GameObject area = new GameObject("FillArea", typeof(RectTransform));
        area.transform.SetParent(badgeGO.transform, false);
        RectTransform areaRT = area.GetComponent<RectTransform>();
        areaRT.anchorMin = new Vector2(fillLeftFrac, fillBottomFrac);
        areaRT.anchorMax = new Vector2(fillRightFrac, fillTopFrac);
        areaRT.offsetMin = areaRT.offsetMax = Vector2.zero;
        // ارتفاعِ باکس (پیکسل) = کسرِ ارتفاعیِ باکس × ارتفاعِ بج — برای محاسبه‌ی دقیقِ پرشدن
        fillAreaHeight = (fillTopFrac - fillBottomFrac) * barHeight;

        // خودِ طلاییِ پرشونده — کاملاً کشیده به باکس، و ارتفاعش با آفستِ بالا (در SetFill) با مقدارِ شاخص کم/زیاد می‌شه
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(area.transform, false);
        fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 1f);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;
        fillImg.raycastTarget = false;

        // عددِ شاخص — وسطِ پنل. مشکی با دورخطِ روشن تا هم رو طلایی هم رو تیره خوب دیده بشه
        valueText = CreatePlainText(area.transform, "Value", valueFontSize, FontStyles.Bold);
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = Color.black;
        valueBaseColor = valueText.color; // رنگِ پایه برای برگردوندن بعد از فلش
        valueText.outlineWidth = 0.22f;
        valueText.outlineColor = new Color(0.98f, 0.92f, 0.72f, 1f); // کرمِ روشن

        // متنِ +/- که بعد از تصمیم بالای پنل نشون داده می‌شه (اول مخفیه)
        hintRuntime = CreatePlainText(badgeGO.transform, "Hint", valueFontSize * 0.95f, FontStyles.Bold);
        RectTransform hintRT = hintRuntime.rectTransform;
        hintRT.anchorMin = hintRT.anchorMax = new Vector2(0.5f, fillTopFrac);
        hintRT.pivot = new Vector2(0.5f, 0f);
        hintRT.sizeDelta = new Vector2(barHeight * aspect * 0.6f, 46f);
        hintRT.anchoredPosition = new Vector2(0f, 4f);
        hintRuntime.alignment = TextAlignmentOptions.Center;
        hintRuntime.outlineWidth = 0.25f;
        hintRuntime.outlineColor = new Color(0f, 0f, 0f, 1f);
        hintRuntime.gameObject.SetActive(false);

        // تا شروعِ بازی بج مخفیه تا تو منوی اصلی دیده نشه (CardSwipe.BeginGame نشونش می‌ده)
        badgeGO.SetActive(false);
    }

    // با شروعِ بازی (CardSwipe.BeginGame) صدا زده می‌شه تا نوارها ظاهر بشن
    public void SetVisible(bool visible)
    {
        if (badgeGO != null) badgeGO.SetActive(visible);
    }

    TMP_Text CreatePlainText(Transform parent, string name, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (persianFont != null) t.font = persianFont;
        t.fontSize = size;
        t.fontStyle = style;
        t.raycastTarget = false;
        return t;
    }

    // ارتفاعِ طلایی رو بر اساسِ مقدارِ شاخص (۰ تا ۱۰۰) تنظیم می‌کنه (مقدارِ اعشاری برای انیمیشنِ نرم).
    // با آفستِ دقیق (Left/Right/Top/Bottom) که طبقِ اندازه‌های فرستاده‌شده ثابته؛ فقط آفستِ بالا با مقدار عوض می‌شه.
    void SetFillF(float value)
    {
        if (fillRT == null) return;
        float f = Mathf.Clamp01(value / 100f);
        fillRT.offsetMin = new Vector2(fillPadLeft, fillPadBottom);
        fillRT.offsetMax = new Vector2(-fillPadRight, -((1f - f) * fillAreaHeight + fillPadTop));
    }

    void UpdateValueText(int value)
    {
        if (valueText != null) valueText.text = ToPersian(value);
    }

    // موقعِ نمایشِ نتیجه‌ی تصمیم (بعد از انتخاب) صدا زده می‌شه
    public void ShowHint(int amount)
    {
        if (hintRuntime == null) return;
        if (amount == 0) { HideHint(); return; }
        hintRuntime.gameObject.SetActive(true);
        hintRuntime.text = (amount > 0 ? "+" : "") + ToPersian(amount);
        hintRuntime.color = amount > 0 ? new Color(0.4f, 0.9f, 0.45f) : new Color(0.95f, 0.4f, 0.35f);
    }

    public void HideHint()
    {
        if (hintRuntime != null) hintRuntime.gameObject.SetActive(false);
    }

    static float SlotXFraction(GameStats.StatType type)
    {
        // چیدمانِ افقی (چپ→راست): دیپلماسی، بودجه، محبوبیت، امنیت — مثلِ قبل، مساوی و بدونِ روی‌هم‌افتادگی
        switch (type)
        {
            case GameStats.StatType.Diplomacy: return 0.125f;
            case GameStats.StatType.Budget: return 0.375f;
            case GameStats.StatType.Popularity: return 0.625f;
            case GameStats.StatType.Security: return 0.875f;
            default: return 0.5f;
        }
    }

    static string FileName(GameStats.StatType type)
    {
        switch (type)
        {
            case GameStats.StatType.Budget: return "Bar_Budget";
            case GameStats.StatType.Popularity: return "Bar_Popularity";
            case GameStats.StatType.Security: return "Bar_Security";
            case GameStats.StatType.Diplomacy: return "Bar_Diplomacy";
            default: return "Bar_Budget";
        }
    }

    static string ToPersian(int n)
    {
        string s = n.ToString();
        string r = "";
        foreach (char c in s)
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
