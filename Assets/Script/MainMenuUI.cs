using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;
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

    // پنلِ انتخابِ «شروع بازی جدید / ادامه بازی قبلی» که با زدنِ «شروع بازی» باز می‌شه
    private GameObject startPanel;
    private GameObject continueButtonGO;

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

        // باکسِ ریال (بالا-چپ) — موجودیِ ریال رو نشون می‌ده. کوچیک نگهش می‌داریم چون بعداً
        // کنارش چیزهای دیگه هم قراره اضافه شن.
        BuildRialBox();

        // پنلِ انتخابِ شروع/ادامه رو (یه‌بار) بساز و مخفی نگه‌دار
        BuildStartPanel();

        // منو باید روی همه‌چیز (از جمله نوارهای وضعیت که موقع اجرا روی Canvas ساخته می‌شن) باشه
        panel.transform.SetAsLastSibling();
    }

    // باکسِ ریال بالا-چپِ منو: تصویرِ RialBox (سکه + کادر) و عددِ موجودی داخلِ کادرش.
    void BuildRialBox()
    {
        Sprite boxSpr = Resources.Load<Sprite>("UI/RialBox");
        if (boxSpr == null) return;

        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        float h = 0.095f; // بزرگ‌تر شد (قبلاً ۰.۰۶۵ بود، خیلی کوچیک به‌نظر می‌اومد)
        float aspect = boxSpr.rect.width / boxSpr.rect.height;
        float w = (h * canvasSize.y * aspect) / canvasSize.x;
        float left = 0.02f, top = 0.985f;

        GameObject boxGO = RuntimeUIHelper.CreateImage(panel.transform, "RialBox",
            new Vector2(left, top - h), new Vector2(left + w, top), boxSpr);

        // عددِ موجودی — داخلِ کادرِ سمتِ راستِ تصویر (سکه سمتِ چپه)
        RTLTextMeshPro num = RuntimeUIHelper.CreateRTLText(boxGO.transform, "RialAmount",
            new Vector2(0.44f, 0.18f), new Vector2(0.95f, 0.82f), 40, persianFont);
        num.text = RialSystem.TotalPersian();
        num.color = new Color(0.98f, 0.88f, 0.55f); // کرمِ طلایی
        num.fontStyle = FontStyles.Bold;
        num.alignment = TextAlignmentOptions.Center;
        num.raycastTarget = false;
    }

    // پنلِ «شروع بازی جدید / ادامه بازی قبلی» رو با کد می‌سازه (تصاویرش از Resources/UI لود می‌شن،
    // پس نیازی به وایرینگِ دستی تو صحنه نیست). با زدنِ دکمه‌ی «شروع بازی» نشون داده می‌شه.
    void BuildStartPanel()
    {
        // پس‌زمینه‌ی تیره‌ی محو؛ کلیک روی فضای خالیش پنل رو می‌بنده (برگشت به منو)
        startPanel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "StartPanel", new Color(0f, 0f, 0f, 0.75f));
        Button overlayBtn = startPanel.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;
        overlayBtn.onClick.AddListener(HideStartPanel);

        Sprite boxSpr = Resources.Load<Sprite>("UI/StartBox");
        Sprite newSpr = Resources.Load<Sprite>("UI/Btn_NewGame");
        Sprite contSpr = Resources.Load<Sprite>("UI/Btn_Continue");

        // باکسِ تزئینیِ وسطِ صفحه — ارتفاعش از نسبتِ خودِ تصویر حساب می‌شه تا کش نیاد
        GameObject boxGO;
        if (boxSpr != null)
        {
            Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
            float boxWFrac = 0.94f;
            float boxAspect = boxSpr.rect.width / boxSpr.rect.height;
            float boxHFrac = (boxWFrac * canvasSize.x / boxAspect) / canvasSize.y;
            boxGO = RuntimeUIHelper.CreateImage(startPanel.transform, "Box",
                new Vector2(0.5f - boxWFrac / 2f, 0.5f - boxHFrac / 2f),
                new Vector2(0.5f + boxWFrac / 2f, 0.5f + boxHFrac / 2f), boxSpr);
        }
        else
        {
            boxGO = startPanel; // اگه تصویرِ باکس نبود، دکمه‌ها مستقیم روی پنل می‌شینن
        }
        Transform boxParent = boxGO.transform;

        // «شروع بازی جدید» — نیمه‌ی بالاییِ باکس (نسبتِ ناحیه نزدیکِ نسبتِ خودِ دکمه تا کم‌فضای مرده باشه)
        if (newSpr != null)
            RuntimeUIHelper.CreateImageButton(boxParent, "NewGameButton",
                new Vector2(0.27f, 0.52f), new Vector2(0.73f, 0.93f), newSpr, StartNewGame);
        else
            RuntimeUIHelper.CreateButton(boxParent, "NewGameButton",
                new Vector2(0.20f, 0.56f), new Vector2(0.80f, 0.90f), "شروع بازی جدید", persianFont, new Color(0.2f, 0.45f, 0.6f, 1f), StartNewGame);

        // «ادامه بازی قبلی» — نیمه‌ی پایینیِ باکس
        if (contSpr != null)
            continueButtonGO = RuntimeUIHelper.CreateImageButton(boxParent, "ContinueButton",
                new Vector2(0.27f, 0.07f), new Vector2(0.73f, 0.48f), contSpr, ContinueGame);
        else
            continueButtonGO = RuntimeUIHelper.CreateButton(boxParent, "ContinueButton",
                new Vector2(0.20f, 0.10f), new Vector2(0.80f, 0.44f), "ادامه بازی قبلی", persianFont, new Color(0.2f, 0.55f, 0.25f, 1f), ContinueGame);

        startPanel.SetActive(false);
    }

    // «ادامه» فقط وقتی سیوِ قبلی باشه فعاله؛ در غیر این صورت کم‌رنگ و غیرقابل‌کلیک می‌شه
    void SetContinueEnabled(bool enabled)
    {
        if (continueButtonGO == null) return;
        Button btn = continueButtonGO.GetComponent<Button>();
        if (btn != null) btn.interactable = enabled;
        Image img = continueButtonGO.GetComponent<Image>();
        if (img != null) img.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }

    void HideStartPanel()
    {
        if (startPanel != null) startPanel.SetActive(false);
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

    // «شروع بازی»: پنلِ انتخاب رو باز می‌کنه («شروع بازی جدید» یا «ادامه بازی قبلی»).
    // اگه سیوِ قبلی نباشه، دکمه‌ی «ادامه» کم‌رنگ و غیرفعال نشون داده می‌شه.
    void StartGame()
    {
        SetContinueEnabled(SaveSystem.HasSave());
        startPanel.transform.SetAsLastSibling();
        startPanel.SetActive(true);
    }

    void StartNewGame()
    {
        SaveSystem.DeleteSave();
        GameStats.Instance.ResetState();
        CardDatabase.Instance.ClearFlags();
        HideStartPanel();
        panel.SetActive(false);
        // اولِ بازیِ جدید، بخشِ آموزشی (دیالوگِ مشاور) نشون داده می‌شه؛ بعدش بازی شروع می‌شه.
        TutorialUI.Show(targetCanvas, persianFont, () => cardSwipe.BeginGame());
    }

    void ContinueGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) { StartNewGame(); return; }

        GameStats.Instance.LoadFromSaveData(data);
        CardDatabase.Instance.SetActiveFlags(data.activeFlags ?? new List<string>());
        CardDatabase.Instance.SetResumeStoryIndex(data.storyIndex); // از همون کارتی که بود ادامه بده
        HideStartPanel();
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
