using UnityEngine;
using UnityEngine.UI;
using TMPro;

// این اسکریپت رو روی هر کدوم از ۴ آبجکت نوار وضعیت می‌ذاریم
// و تو Inspector مشخص می‌کنیم این نوار مال کدوم شاخصه
public class StatBarUI : MonoBehaviour
{
    [SerializeField] private GameStats.StatType statType;
    [SerializeField] private Slider slider; // یه UI Slider معمولی، فقط برای نمایش (تعاملی نیست)

    [Header("پیش‌نمایش اثر تصمیم (اختیاری — موقع کشیدن کارت نشون داده می‌شه)")]
    [SerializeField] private TMP_Text hintText; // یه متن کوچیک کنار نوار، مثلاً +۱۵ یا -۲۰ (نیازی به RTL نداره چون فقط عدد و علامته)

    [Header("آیکون شاخص (اختیاری)")]
    [SerializeField] private Sprite icon;
    [Tooltip("اندازه‌ی آیکون (پیکسل). بزرگ‌ترش کنی آیکون درشت‌تر می‌شه.")]
    [SerializeField] private float iconSize = 90f;
    [Tooltip("جابه‌جایی آیکون نسبت به لبه‌ی چپِ نوار")]
    [SerializeField] private Vector2 iconOffset = new Vector2(-8f, 0f);

    [Header("ظاهر نوار (تم چرمی/طلایی)")]
    [SerializeField] private Color trackColor = new Color(0.18f, 0.12f, 0.07f, 0.95f); // چرم تیره
    [SerializeField] private Color fillColor = new Color(0.79f, 0.62f, 0.24f, 1f);      // طلایی

    public GameStats.StatType StatType => statType;

    private TMP_Text valueText; // عدد فعلی شاخص، روی خودِ اسلایدر — با کد ساخته می‌شه، نیازی به وایرینگ دستی نداره

    void Start()
    {
        StyleSlider();
        CreateValueText();
        CreateIcon();

        // مقدار اولیه رو بگیر و نمایش بده
        slider.value = GameStats.Instance.GetStat(statType);
        UpdateValueText(slider.value);

        // از این به بعد هر تغییری رخ بده خودکار آپدیت می‌شه
        GameStats.Instance.OnStatChanged += HandleStatChanged;
    }

    // اسلایدر پیش‌فرض یونیتی سفید و زشته؛ اینجا با کد رنگ چرمی/طلایی بهش می‌دیم و دستگیره‌ی
    // اضافی رو مخفی می‌کنیم (چون این اسلایدر فقط نمایشیه و تعاملی نیست)
    void StyleSlider()
    {
        // track (پس‌زمینه‌ی خالی نوار)
        Transform bg = slider.transform.Find("Background");
        if (bg != null)
        {
            Image bgImg = bg.GetComponent<Image>();
            if (bgImg != null) bgImg.color = trackColor;
        }

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

    // آیکون شاخص رو کنار (سمت چپ) خودِ نوار می‌ذاره — اگه آیکونی وصل نشده باشه کاری نمی‌کنه
    void CreateIcon()
    {
        if (icon == null) return;

        GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(transform, false);
        RectTransform rect = iconGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.anchoredPosition = iconOffset;
        rect.sizeDelta = new Vector2(iconSize, iconSize);

        Image img = iconGO.AddComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;
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
