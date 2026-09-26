using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

// سال و ماهِ جاریِ ریاست‌جمهوری رو نشون می‌ده، داخل یه باکس (تصویر History) پایینِ صفحه.
// (سال اول = ماه ۱ تا ۱۲، ...) و ماه رو ماه‌به‌ماه می‌شمره.
public class YearDisplayUI : MonoBehaviour
{
    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("باکس تاریخ (تصویر History) پایینِ صفحه")]
    [SerializeField] private Sprite boxSprite;
    [Tooltip("فاصله‌ی باکس از پایینِ صفحه (پیکسل)")]
    [SerializeField] private float bottomMargin = 120f;
    [Tooltip("اندازه‌ی باکس (پیکسل)")]
    [SerializeField] private Vector2 boxSize = new Vector2(760f, 200f);
    [Tooltip("اندازه‌ی فونتِ متن داخل باکس")]
    [SerializeField] private float fontSize = 34f;
    [Tooltip("رنگ متن")]
    [SerializeField] private Color textColor = new Color(0.96f, 0.90f, 0.78f);

    private static readonly string[] yearNames = { "سال اول", "سال دوم", "سال سوم", "سال چهارم" };
    private static readonly string[] monthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private RTLTextMeshPro yearText;
    private GameObject boxGO; // خودِ باکس — تا شروعِ بازی مخفیه (تو منوی اصلی دیده نشه)

    void Start()
    {
        // اول از Prefab (قابلِ ویرایش تو ادیتور) امتحان می‌کنیم؛ اگه نبود به روشِ کدیِ قبلی برمی‌گردیم
        if (!BuildFromPrefab())
        {
            // باکس، پایین-وسطِ صفحه
            GameObject box = new GameObject("DateBox", typeof(RectTransform));
            boxGO = box;
            box.transform.SetParent(targetCanvas.transform, false);
            RectTransform boxRT = box.GetComponent<RectTransform>();
            boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0f); // پایین-وسط
            boxRT.pivot = new Vector2(0.5f, 0f);
            boxRT.anchoredPosition = new Vector2(0f, bottomMargin);
            boxRT.sizeDelta = boxSize;

            if (boxSprite != null)
            {
                Image img = box.AddComponent<Image>();
                img.sprite = boxSprite;
                img.preserveAspect = true;
            }

            // متن، وسطِ باکس
            yearText = RuntimeUIHelper.CreateRTLText(box.transform, "YearText", Vector2.zero, Vector2.one, fontSize, persianFont);
            yearText.color = textColor;
            yearText.fontStyle = FontStyles.Bold;
        }

        // موقعیتِ نهاییِ باکسِ تاریخ رو مستقیم اینجا قفل می‌کنیم تا حتماً اعمال شه (بدونِ اتکا به Prefab/Instantiate).
        // مختصاتِ دقیقی که توسعه‌دهنده خواست: کنارِ چپش دکمه‌ی توقفه.
        RectTransform brt = boxGO != null ? boxGO.GetComponent<RectTransform>() : null;
        if (brt != null)
        {
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(112f, 45f);
        }

        UpdateDisplay(GameStats.Instance.CurrentMonth);
        GameStats.Instance.OnMonthChanged += UpdateDisplay;

        // باکسِ تاریخ فقط تو گیم‌پلی دیده بشه، نه تو منوی اصلی → تا شروعِ بازی مخفیش کن
        boxGO.SetActive(false);
        CardSwipe.GameStarted += ShowBox;
    }

    // باکسِ تاریخ رو از Resources/Prefabs/DateBox می‌سازه (چیدمان تو خودِ Prefab).
    // فقط فونت و متن رو با کد ست می‌کنیم. اگه Prefab نبود false برمی‌گردونه.
    bool BuildFromPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/DateBox");
        if (prefab == null) return false;

        // مهم: worldPositionStays=false تا مختصاتِ RectTransformِ خودِ Prefab (anchoredPosition/size) عیناً حفظ شه.
        // با نسخه‌ی دوآرگومانی (worldPositionStays=true) یونیتی موقعیت رو از نو حساب می‌کرد و تغییرِ Prefab بی‌اثر می‌شد.
        boxGO = Instantiate(prefab, targetCanvas.transform, false);
        boxGO.name = "DateBox";

        Transform yt = boxGO.transform.Find("YearText");
        if (yt != null)
        {
            yearText = yt.GetComponent<RTLTextMeshPro>();
            if (yearText != null && persianFont != null) yearText.font = persianFont;
        }
        // اگه متن پیدا نشد، نمونه‌ی ساخته‌شده رو پاک می‌کنیم تا باکسِ یتیم رو صفحه نمونه (و کد نسخه‌ی خودش رو بسازه)
        if (yearText == null)
        {
            if (boxGO != null) Destroy(boxGO);
            boxGO = null;
            return false;
        }
        return true;
    }

    void ShowBox()
    {
        if (boxGO != null) boxGO.SetActive(true);
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
            GameStats.Instance.OnMonthChanged -= UpdateDisplay;
        CardSwipe.GameStarted -= ShowBox;
    }

    void UpdateDisplay(int month)
    {
        int yearIndex = Mathf.Clamp((month - 1) / 12, 0, yearNames.Length - 1);
        int monthIndex = (month - 1) % 12; // ماه داخل سال (۰ تا ۱۱)
        // مثال: «سال اول — فروردین»
        yearText.text = yearNames[yearIndex] + " — " + monthNames[monthIndex];
    }
}
