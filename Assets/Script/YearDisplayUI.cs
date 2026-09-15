using UnityEngine;
using RTLTMPro;
using TMPro;

// متن بالای صفحه که هم سالِ ریاست‌جمهوری و هم ماهِ جاری رو نشون می‌ده
// (سال اول = ماه ۱ تا ۱۲، سال دوم = ۱۳ تا ۲۴، ...) و ماه رو ماه‌به‌ماه می‌شمره
public class YearDisplayUI : MonoBehaviour
{
    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن)")]
    [SerializeField] private Canvas targetCanvas;

    private static readonly string[] yearNames = { "سال اول", "سال دوم", "سال سوم", "سال چهارم" };
    private static readonly string[] monthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private RTLTextMeshPro yearText;

    void Start()
    {
        yearText = RuntimeUIHelper.CreateRTLText(targetCanvas.transform, "YearText", new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.98f), 30, persianFont);
        yearText.color = Color.white;
        yearText.fontStyle = FontStyles.Bold;
        yearText.outlineWidth = 0.2f;
        yearText.outlineColor = Color.black;

        UpdateDisplay(GameStats.Instance.CurrentMonth);
        GameStats.Instance.OnMonthChanged += UpdateDisplay;
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
            GameStats.Instance.OnMonthChanged -= UpdateDisplay;
    }

    void UpdateDisplay(int month)
    {
        int yearIndex = Mathf.Clamp((month - 1) / 12, 0, yearNames.Length - 1);
        int monthIndex = (month - 1) % 12; // ماه داخل سال (۰ تا ۱۱)
        // مثال: «سال اول — فروردین»
        yearText.text = yearNames[yearIndex] + " — " + monthNames[monthIndex];
    }
}
