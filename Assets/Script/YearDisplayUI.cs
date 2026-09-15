using UnityEngine;
using RTLTMPro;
using TMPro;

// یه متن ساده که سال فعلی از دوره‌ی ۴ ساله‌ی ریاست‌جمهوری رو نشون می‌ده
// (سال اول = ماه ۱ تا ۱۲، سال دوم = ۱۳ تا ۲۴، و به همین ترتیب)
public class YearDisplayUI : MonoBehaviour
{
    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن)")]
    [SerializeField] private Canvas targetCanvas;

    private static readonly string[] yearNames = { "سال اول", "سال دوم", "سال سوم", "سال چهارم" };

    private RTLTextMeshPro yearText;

    void Start()
    {
        yearText = RuntimeUIHelper.CreateRTLText(targetCanvas.transform, "YearText", new Vector2(0.25f, 0.9f), new Vector2(0.75f, 0.98f), 32, persianFont);
        yearText.color = Color.black;

        UpdateYear(GameStats.Instance.CurrentMonth);
        GameStats.Instance.OnMonthChanged += UpdateYear;
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
            GameStats.Instance.OnMonthChanged -= UpdateYear;
    }

    void UpdateYear(int month)
    {
        int index = Mathf.Clamp((month - 1) / 12, 0, yearNames.Length - 1);
        yearText.text = yearNames[index];
    }
}
