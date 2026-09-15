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

    public GameStats.StatType StatType => statType;

    private TMP_Text valueText; // عدد فعلی شاخص، روی خودِ اسلایدر — با کد ساخته می‌شه، نیازی به وایرینگ دستی نداره

    void Start()
    {
        CreateValueText();
        CreateIcon();

        // مقدار اولیه رو بگیر و نمایش بده
        slider.value = GameStats.Instance.GetStat(statType);
        UpdateValueText(slider.value);

        // از این به بعد هر تغییری رخ بده خودکار آپدیت می‌شه
        GameStats.Instance.OnStatChanged += HandleStatChanged;
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
        valueText.color = Color.black;
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
        rect.anchoredPosition = new Vector2(-8, 0);
        rect.sizeDelta = new Vector2(48, 48);

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
