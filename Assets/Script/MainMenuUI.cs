using UnityEngine;
using TMPro;
using System.Collections.Generic;

// منوی اصلی بازی رو موقع اجرا با کد می‌سازه (مثل GameOverUI، چون امکان ساختنش تو Editor نبود).
// چیدمان: پس‌زمینه‌ی دفترِ ریاست‌جمهوری + سه دکمه‌ی وسط‌چین: «شروع بازی»، «تنظیمات»، «خروج».
public class MainMenuUI : MonoBehaviour
{
    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("آبجکت کارت (برای شروع بازی بعد از بستن منو)")]
    [SerializeField] private CardSwipe cardSwipe;

    [Header("تصاویرِ منو")]
    [Tooltip("پس‌زمینه‌ی منو (BG)")]
    [SerializeField] private Sprite backgroundSprite;
    [Tooltip("دکمه‌ی «شروع بازی» (اسلایسِ شروع بازی از ButtonAssets)")]
    [SerializeField] private Sprite startButtonSprite;
    [Tooltip("دکمه‌ی «تنظیمات» (Settings.png)")]
    [SerializeField] private Sprite settingsButtonSprite;
    [Tooltip("دکمه‌ی «خروج» (Exit.png)")]
    [SerializeField] private Sprite exitButtonSprite;

    [Header("چیدمانِ دکمه‌ها (کسری از صفحه؛ x وسط=۰.۵، y: پایین=۰ بالا=۱)")]
    [Tooltip("پهنای هر دکمه نسبت به عرضِ صفحه — ارتفاع خودکار از نسبتِ تصویر حساب می‌شه (بدون کش‌آمدن)")]
    [SerializeField, Range(0.2f, 0.9f)] private float buttonWidthFraction = 0.44f;
    [SerializeField] private Vector2 startButtonCenter = new Vector2(0.5f, 0.66f);
    [SerializeField] private Vector2 settingsButtonCenter = new Vector2(0.5f, 0.505f);
    [SerializeField] private Vector2 exitButtonCenter = new Vector2(0.5f, 0.355f);

    private GameObject panel;

    void Start()
    {
        BuildUI();
    }

    void BuildUI()
    {
        panel = backgroundSprite != null
            ? RuntimeUIHelper.CreateImage(targetCanvas.transform, "MainMenuPanel", Vector2.zero, Vector2.one, backgroundSprite, stretch: true)
            : RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MainMenuPanel", new Color(0.05f, 0.05f, 0.08f, 1f));

        // سه دکمه (اگه اسپرایتشون خالی بمونه، نسخه‌ی رنگ‌ساده‌ی متنی نشون داده می‌شه)
        AddMenuButton("StartButton", startButtonSprite, startButtonCenter, "شروع بازی", StartGame);
        AddMenuButton("SettingsButton", settingsButtonSprite, settingsButtonCenter, "تنظیمات", OpenSettings);
        AddMenuButton("ExitButton", exitButtonSprite, exitButtonCenter, "خروج", QuitGame);

        // منو باید روی همه‌چیز (از جمله نوارهای وضعیت که موقع اجرا روی Canvas ساخته می‌شن) باشه
        panel.transform.SetAsLastSibling();
    }

    // یه دکمه رو وسط‌چینِ نقطه‌ی center می‌سازه؛ پهنا ثابته و ارتفاع از نسبتِ خودِ تصویر حساب می‌شه
    // تا دکمه کش نیاد. اگه اسپرایت نبود، نسخه‌ی رنگ‌ساده‌ی متنی می‌ذاره.
    void AddMenuButton(string name, Sprite sprite, Vector2 center, string fallbackLabel, UnityEngine.Events.UnityAction onClick)
    {
        if (sprite == null)
        {
            Vector2 fMin = new Vector2(center.x - buttonWidthFraction / 2f, center.y - 0.05f);
            Vector2 fMax = new Vector2(center.x + buttonWidthFraction / 2f, center.y + 0.05f);
            RuntimeUIHelper.CreateButton(panel.transform, name, fMin, fMax, fallbackLabel, persianFont, new Color(0.2f, 0.45f, 0.6f, 1f), onClick);
            return;
        }

        // ارتفاعِ ناحیه‌ی دکمه = پهنا ÷ نسبتِ تصویر، تا با preserveAspect دقیقاً پر بشه و کج/کشیده نشه
        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        float spriteAspect = sprite.rect.width / sprite.rect.height; // پهنا/ارتفاعِ تصویر
        float widthPx = buttonWidthFraction * canvasSize.x;
        float heightFrac = (widthPx / spriteAspect) / canvasSize.y;

        Vector2 min = new Vector2(center.x - buttonWidthFraction / 2f, center.y - heightFrac / 2f);
        Vector2 max = new Vector2(center.x + buttonWidthFraction / 2f, center.y + heightFrac / 2f);
        RuntimeUIHelper.CreateImageButton(panel.transform, name, min, max, sprite, onClick);
    }

    // «شروع بازی»: اگه سیوِ قبلی هست، بازی رو ادامه می‌ده؛ وگرنه بازیِ جدید شروع می‌کنه.
    // (تو این چیدمان دکمه‌ی جداگانه‌ی «ادامه» نداریم؛ اگه بعداً خواستی اضافه می‌کنیم.)
    void StartGame()
    {
        if (SaveSystem.HasSave())
            ContinueGame();
        else
            StartNewGame();
    }

    void StartNewGame()
    {
        SaveSystem.DeleteSave();
        GameStats.Instance.ResetState();
        CardDatabase.Instance.ClearFlags();
        panel.SetActive(false);
        cardSwipe.BeginGame();
    }

    void ContinueGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) { StartNewGame(); return; }

        GameStats.Instance.LoadFromSaveData(data);
        CardDatabase.Instance.SetActiveFlags(data.activeFlags ?? new List<string>());
        panel.SetActive(false);
        cardSwipe.BeginGame();
    }

    // فعلاً صفحه‌ی تنظیمات ساخته نشده — این دکمه جای خالیشه. وقتی محتوای تنظیمات (صدا و...) مشخص شد پر می‌شه.
    void OpenSettings()
    {
        Debug.Log("تنظیمات هنوز ساخته نشده — بعداً اضافه می‌شه.");
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
