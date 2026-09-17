using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

// این اسکریپت رو روی هر کدوم از ۴ آبجکت نوار وضعیت می‌ذاریم
// و تو Inspector مشخص می‌کنیم این نوار مال کدوم شاخصه
public class StatBarUI : MonoBehaviour
{
    [SerializeField] private GameStats.StatType statType;
    [SerializeField] private Slider slider; // یه UI Slider معمولی، فقط برای نمایش (تعاملی نیست)

    [Header("پیش‌نمایش اثر تصمیم (اختیاری — موقع کشیدن کارت نشون داده می‌شه)")]
    [SerializeField] private TMP_Text hintText; // یه متن کوچیک کنار نوار، مثلاً +۱۵ یا -۲۰ (نیازی به RTL نداره چون فقط عدد و علامته)

    [Header("اسم شاخص (زیرِ نوار نوشته می‌شه)")]
    [Tooltip("فونت فارسی — همون NotoNaskhArabic که بقیه‌ی متن‌ها ازش استفاده می‌کنن")]
    [SerializeField] private TMP_FontAsset persianFont;
    [Tooltip("اندازه‌ی فونتِ اسم شاخص")]
    [SerializeField] private float nameFontSize = 50f;
    [Tooltip("رنگ اسم شاخص")]
    [SerializeField] private Color nameColor = new Color(0.96f, 0.90f, 0.78f);
    [Tooltip("فاصله‌ی اسم از بالای نوار (پیکسل). چون بالای همه‌ی نوارها هم‌تراز است، با این مقدارِ ثابت " +
        "اسمِ همه‌ی نوارها هم‌ترازِ هم می‌شه — حتی اگه طولِ نوارها فرق داشته باشه.")]
    [SerializeField] private float nameDropFromTop = 380f;
    [Tooltip("اندازه‌ی جعبه‌ی متنِ اسم شاخص")]
    [SerializeField] private Vector2 nameBoxSize = new Vector2(260f, 80f);

    [Header("ظاهر نوار (رنگِ بخشِ پرشده)")]
    [SerializeField] private Color fillColor = new Color(0.92f, 0.71f, 0.28f, 1f);      // طلایی گرم و روشن

    public GameStats.StatType StatType => statType;

    private TMP_Text valueText; // عدد فعلی شاخص، روی خودِ اسلایدر — با کد ساخته می‌شه، نیازی به وایرینگ دستی نداره

    // اسم فارسیِ هر شاخص برای نمایش زیرِ نوار
    private static string GetStatName(GameStats.StatType type)
    {
        switch (type)
        {
            case GameStats.StatType.Budget:     return "بودجه";
            case GameStats.StatType.Popularity: return "محبوبیت";
            case GameStats.StatType.Security:   return "امنیت";
            case GameStats.StatType.Diplomacy:  return "دیپلماسی";
            default: return "";
        }
    }

    void Start()
    {
        StyleSlider();
        CreateValueText();
        CreateNameLabel();

        // مقدار اولیه رو بگیر و نمایش بده
        slider.value = GameStats.Instance.GetStat(statType);
        UpdateValueText(slider.value);

        // از این به بعد هر تغییری رخ بده خودکار آپدیت می‌شه
        GameStats.Instance.OnStatChanged += HandleStatChanged;
    }

    // فقط رنگِ بخشِ پرشده (Fill) رو طلایی می‌کنیم و دستگیره رو مخفی می‌کنیم.
    // نکته: پس‌زمینه‌ی نوار (Background) دیگه اینجا رنگ نمی‌شه — چون توسعه‌دهنده براش اسپرایتِ
    // آماده (BG_slidr) گذاشته؛ اگه رنگش کنیم اون اسپرایت تیره و کم‌رنگ («سایه‌مانند») می‌شه.
    void StyleSlider()
    {
        // fill (بخش پر شده)
        if (slider.fillRect != null)
        {
            Image fillImg = slider.fillRect.GetComponent<Image>();
            if (fillImg != null) fillImg.color = fillColor;
        }

        // دستگیره‌ی اسلایدر رو مخفی کن — این نوار فقط نمایشیه
        if (slider.handleRect != null)
            slider.handleRect.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // جلوگیری از خطای احتمالی وقتی صحنه عوض می‌شه
        if (GameStats.Instance != null)
            GameStats.Instance.OnStatChanged -= HandleStatChanged;
    }

    void HandleStatChanged(GameStats.StatType changedType, int newValue)
    {
        if (changedType != statType) return; // این نوار فقط به شاخص خودش واکنش نشون می‌ده
        slider.value = newValue;
        UpdateValueText(newValue);
    }

    // عدد شاخص رو روی خودِ نوار (روی اسلایدر) نشون می‌ده — فقط عدده، نیازی به RTL نداره
    void CreateValueText()
    {
        GameObject textGO = new GameObject("ValueText", typeof(RectTransform));
        textGO.transform.SetParent(slider.transform, false);
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        valueText = textGO.AddComponent<TextMeshProUGUI>();
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.fontSize = 22;
        valueText.color = Color.white;
        valueText.fontStyle = FontStyles.Bold;
        valueText.outlineWidth = 0.2f;
        valueText.outlineColor = Color.black;
        valueText.raycastTarget = false;
    }

    void UpdateValueText(float value)
    {
        if (valueText != null) valueText.text = ((int)value).ToString();
    }

    // اسم فارسیِ شاخص رو وسطِ زیرِ نوار می‌نویسه (به‌جای آیکونِ قبلی).
    // نکته‌ی مهم: خودِ آبجکتِ نوار تو صحنه ۹۰ درجه چرخیده (m_LocalEulerAngles z=90) تا اسلایدرِ
    // افقی به‌صورت عمودی وایسته. چون این متن فرزندِ همون آبجکته، اون چرخش ۹۰ درجه رو به ارث می‌بره
    // و کج/زاویه‌دار دیده می‌شه. برای همین متن رو ۹۰- درجه برعکس می‌چرخونیم تا صاف و افقی بشه،
    // و چون محورهای محلی هم چرخیدن، برای «پایینِ نوار» باید تو محورِ x محلیِ منفی جابه‌جاش کنیم.
    void CreateNameLabel()
    {
        GameObject nameGO = new GameObject("NameLabel", typeof(RectTransform));
        nameGO.transform.SetParent(transform, false);
        RectTransform rect = nameGO.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // وسطِ نوار
        rect.pivot = new Vector2(0.5f, 0.5f);

        // چرخشِ متن رو صاف کن: چون والد ۹۰+ چرخیده، متن رو ۹۰- می‌چرخونیم تا در نهایت افقی بشه
        rect.localRotation = Quaternion.Euler(0f, 0f, -90f);

        // موقعیتِ عمودی: چون نوارها طولِ متفاوتی دارن، اگه اسم رو نسبت به «پایینِ» هر نوار بذاریم،
        // اسمِ نوارهای بلندتر پایین‌تر می‌افته (باگی که «محبوبیت پایینه» رو می‌ساخت). ولی «بالای» همه‌ی
        // نوارها هم‌تراز است؛ پس اسم رو یه فاصله‌ی ثابت (nameDropFromTop) پایین‌ترِ بالای نوار می‌ذاریم
        // تا اسمِ همه‌ی نوارها هم‌ترازِ هم بشه. (والد ۹۰+ چرخیده: محورِ x محلیِ مثبت = بالای نوار.)
        float halfLen = GetComponent<RectTransform>().rect.width * 0.5f;
        rect.anchoredPosition = new Vector2(halfLen - nameDropFromTop, 0f);
        rect.sizeDelta = nameBoxSize;

        RTLTextMeshPro nameText = nameGO.AddComponent<RTLTextMeshPro>();
        nameText.font = persianFont;
        nameText.fontSize = nameFontSize;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = nameColor;
        nameText.fontStyle = FontStyles.Bold;
        nameText.raycastTarget = false;
        nameText.text = GetStatName(statType);
    }

    // موقع کشیدن کارت صدا زده می‌شه تا نشون بده اگه همین الان رها کنی، این شاخص چقدر تغییر می‌کنه
    public void ShowHint(int amount)
    {
        if (hintText == null) return;

        if (amount == 0)
        {
            HideHint();
            return;
        }

        hintText.gameObject.SetActive(true);
        hintText.text = (amount > 0 ? "+" : "") + amount;
        hintText.color = amount > 0 ? Color.green : Color.red;
    }

    public void HideHint()
    {
        if (hintText != null) hintText.gameObject.SetActive(false);
    }
}
