using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// نوارِ وضعیتِ هر شاخص — طرحِ «نمودارِ ستونی»: هر شاخص یه ستونِ عمودیِ رنگیِ تمیزه که ارتفاعِ پُرشدنش
// = مقدارِ شاخص (۰..۱۰۰)، با اسمِ شاخص زیرش و عدد روش. موقعِ اجرا با کد بالای صفحه ساخته می‌شه
// (بدونِ وایرینگ/تصویر). Juice (پرشِ نرمِ عدد، فلشِ رنگ، نبضِ خطر) حفظ شده.
public class StatBarUI : MonoBehaviour
{
    [SerializeField] private GameStats.StatType statType;
    [SerializeField] private Slider slider; // اسلایدرِ قدیمی — دیگه دیده نمی‌شه (visualش مخفی می‌شه)

    [Header("پیش‌نمایش/نتیجه‌ی اثر (قدیمی — استفاده نمی‌شه)")]
    [SerializeField] private TMP_Text hintText;

    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("چیدمانِ ستون")]
    [Tooltip("پهنای ستون (پیکسل)")]
    [SerializeField] private float columnWidth = 140f;
    [Tooltip("ارتفاعِ ستون (پیکسل)")]
    [SerializeField] private float columnHeight = 230f;
    [Tooltip("فاصله از بالای صفحه (پیکسل)")]
    [SerializeField] private float topMargin = 45f;
    [Tooltip("رنگِ زمینه‌ی ستون (track)")]
    [SerializeField] private Color trackColor = new Color(0.10f, 0.08f, 0.06f, 0.82f);
    [SerializeField] private float valueFontSize = 34f;
    [SerializeField] private float nameFontSize = 26f;

    [Header("Juice — انیمیشن و حسِ تغییرِ شاخص")]
    [SerializeField] private float animDuration = 0.45f;
    [SerializeField] private float popScale = 0.10f;
    [Tooltip("زیرِ این مقدار یا بالای (۱۰۰ منهای این)، ستون نبضِ قرمزِ هشدار می‌زنه")]
    [SerializeField] private int dangerThreshold = 15;
    [SerializeField] private Color flashUpColor = new Color(0.35f, 0.95f, 0.45f);
    [SerializeField] private Color flashDownColor = new Color(0.98f, 0.4f, 0.35f);

    public GameStats.StatType StatType => statType;

    private RectTransform fillRT;   // بخشِ رنگیِ پرشونده — ارتفاعش با مقدارِ شاخص عوض می‌شه
    private TMP_Text valueText;     // عددِ شاخص
    private TMP_Text hintRuntime;   // +/- که بعد از تصمیم نشون داده می‌شه
    private GameObject columnGO;    // خودِ ستون — تا شروعِ بازی مخفیه (تو منو دیده نشه)
    private RectTransform columnRT; // برای انیمیشنِ پرش
    private Image dangerOverlay;    // لایه‌ی قرمزِ هشدار که نبض می‌زنه
    private Color valueBaseColor = Color.white;

    private float displayValue;     // مقدارِ فعلیِ نشون‌داده‌شده (برای انیمیشنِ نرم)
    private Coroutine animCo;

    void Start()
    {
        HideOldSliderVisual();
        BuildColumn();

        int val = GameStats.Instance.GetStat(statType);
        displayValue = val;
        SetFillF(val);
        UpdateValueText(val);

        GameStats.Instance.OnStatChanged += HandleStatChanged;
    }

    // نبضِ قرمزِ هشدار وقتی شاخص به لبه‌ی باخت (۰ یا ۱۰۰) نزدیکه
    void Update()
    {
        if (dangerOverlay == null || columnGO == null || !columnGO.activeInHierarchy) return;
        int v = Mathf.RoundToInt(displayValue);
        bool danger = v <= dangerThreshold || v >= 100 - dangerThreshold;
        float a = danger ? (0.15f + 0.28f * Mathf.Abs(Mathf.Sin(Time.time * 4f))) : 0f;
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

        if (columnGO == null || !columnGO.activeInHierarchy)
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

    // انیمیشنِ نرمِ تغییرِ شاخص: پرشِ عدد + بالا/پایین‌رفتنِ ستون + پرشِ اندازه + فلشِ رنگِ عدد
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

            if (columnRT != null)
                columnRT.localScale = Vector3.one * (1f + popScale * Mathf.Sin(k * Mathf.PI));
            if (valueText != null)
                valueText.color = Color.Lerp(flash, valueBaseColor, k);

            yield return null;
        }

        displayValue = target;
        SetFillF(target);
        UpdateValueText(target);
        if (columnRT != null) columnRT.localScale = Vector3.one;
        if (valueText != null) valueText.color = valueBaseColor;
        animCo = null;
    }

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

    void BuildColumn()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        Color statColor = StatColor(statType);

        // ظرفِ ستون — بالای صفحه، وسطِ اسلاتِ افقیِ خودش
        columnGO = new GameObject("Column_" + statType, typeof(RectTransform));
        columnGO.transform.SetParent(canvas.transform, false);
        columnRT = columnGO.GetComponent<RectTransform>();
        float xFrac = SlotXFraction(statType);
        columnRT.anchorMin = columnRT.anchorMax = new Vector2(xFrac, 1f);
        columnRT.pivot = new Vector2(0.5f, 1f);
        columnRT.sizeDelta = new Vector2(columnWidth, columnHeight);
        columnRT.anchoredPosition = new Vector2(0f, -topMargin);

        // زمینه‌ی ستون (track)
        Image track = columnGO.AddComponent<Image>();
        track.color = trackColor;
        track.raycastTarget = false;

        // پُرشونده (رنگِ شاخص) — از پایین بالا می‌آد
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(columnGO.transform, false);
        fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 0f); // ارتفاع در SetFillF ست می‌شه
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fillRT.pivot = new Vector2(0.5f, 0f);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = statColor;
        fillImg.raycastTarget = false;

        // لایه‌ی قرمزِ هشدار — رو کلِ ستون، اولش نامرئی؛ تو Update نبض می‌زنه
        GameObject dangerGO = new GameObject("DangerOverlay", typeof(RectTransform));
        dangerGO.transform.SetParent(columnGO.transform, false);
        RectTransform dangerRT = dangerGO.GetComponent<RectTransform>();
        dangerRT.anchorMin = Vector2.zero; dangerRT.anchorMax = Vector2.one;
        dangerRT.offsetMin = dangerRT.offsetMax = Vector2.zero;
        dangerOverlay = dangerGO.AddComponent<Image>();
        dangerOverlay.color = new Color(1f, 0.15f, 0.1f, 0f);
        dangerOverlay.raycastTarget = false;

        // عددِ شاخص — روی ستون، سفیدِ درشت با دورخطِ تیره
        valueText = CreatePlainText(columnGO.transform, "Value", valueFontSize, FontStyles.Bold);
        RectTransform valRT = valueText.rectTransform;
        valRT.anchorMin = Vector2.zero; valRT.anchorMax = Vector2.one;
        valRT.offsetMin = valRT.offsetMax = Vector2.zero;
        valueText.alignment = TextAlignmentOptions.Top;
        valueText.margin = new Vector4(0, 8, 0, 0);
        valueText.color = Color.white;
        valueBaseColor = valueText.color;
        valueText.outlineWidth = 0.22f;
        valueText.outlineColor = new Color(0f, 0f, 0f, 1f);

        // اسمِ شاخص — زیرِ ستون
        TMP_Text nameLabel = CreatePlainText(columnGO.transform, "Name", nameFontSize, FontStyles.Bold);
        RectTransform nameRT = nameLabel.rectTransform;
        nameRT.anchorMin = nameRT.anchorMax = new Vector2(0.5f, 0f);
        nameRT.pivot = new Vector2(0.5f, 1f);
        nameRT.sizeDelta = new Vector2(columnWidth + 50f, 44f);
        nameRT.anchoredPosition = new Vector2(0f, -8f);
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.text = StatName(statType);
        nameLabel.color = new Color(0.96f, 0.92f, 0.80f);
        nameLabel.outlineWidth = 0.2f;
        nameLabel.outlineColor = new Color(0f, 0f, 0f, 1f);

        // متنِ +/- (بعد از تصمیم) — بالای ستون
        hintRuntime = CreatePlainText(columnGO.transform, "Hint", valueFontSize * 0.95f, FontStyles.Bold);
        RectTransform hintRT = hintRuntime.rectTransform;
        hintRT.anchorMin = hintRT.anchorMax = new Vector2(0.5f, 1f);
        hintRT.pivot = new Vector2(0.5f, 0f);
        hintRT.sizeDelta = new Vector2(columnWidth + 40f, 42f);
        hintRT.anchoredPosition = new Vector2(0f, 6f);
        hintRuntime.alignment = TextAlignmentOptions.Center;
        hintRuntime.outlineWidth = 0.25f;
        hintRuntime.outlineColor = new Color(0f, 0f, 0f, 1f);
        hintRuntime.gameObject.SetActive(false);

        columnGO.SetActive(false); // تا شروعِ بازی مخفی
    }

    public void SetVisible(bool visible)
    {
        if (columnGO != null) columnGO.SetActive(visible);
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

    // ارتفاعِ پُرشونده رو بر اساسِ مقدارِ شاخص (۰..۱۰۰) تنظیم می‌کنه (اعشاری برای انیمیشنِ نرم)
    void SetFillF(float value)
    {
        if (fillRT == null) return;
        float f = Mathf.Clamp01(value / 100f);
        fillRT.anchorMax = new Vector2(1f, f);
    }

    void UpdateValueText(int value)
    {
        if (valueText != null) valueText.text = ToPersian(value);
    }

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
        // چیدمانِ افقی (چپ→راست): دیپلماسی، بودجه، محبوبیت، امنیت
        switch (type)
        {
            case GameStats.StatType.Diplomacy: return 0.125f;
            case GameStats.StatType.Budget: return 0.375f;
            case GameStats.StatType.Popularity: return 0.625f;
            case GameStats.StatType.Security: return 0.875f;
            default: return 0.5f;
        }
    }

    // رنگِ هر شاخص (برای نمودارِ ستونیِ رنگی)
    static Color StatColor(GameStats.StatType type)
    {
        switch (type)
        {
            case GameStats.StatType.Budget: return new Color(0.95f, 0.72f, 0.20f);      // طلایی
            case GameStats.StatType.Popularity: return new Color(0.36f, 0.80f, 0.45f);  // سبز
            case GameStats.StatType.Security: return new Color(0.88f, 0.38f, 0.34f);     // قرمز
            case GameStats.StatType.Diplomacy: return new Color(0.36f, 0.62f, 0.95f);    // آبی
            default: return Color.gray;
        }
    }

    static string StatName(GameStats.StatType type)
    {
        switch (type)
        {
            case GameStats.StatType.Budget: return "بودجه";
            case GameStats.StatType.Popularity: return "محبوبیت";
            case GameStats.StatType.Security: return "امنیت";
            case GameStats.StatType.Diplomacy: return "دیپلماسی";
            default: return "";
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
