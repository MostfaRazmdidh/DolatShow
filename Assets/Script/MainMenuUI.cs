using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;
using System.Collections;
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

    // منوی کشویی سمتِ راست (فروشگاه/درباره/بستن) که با دکمه‌ی همبرگری باز/بسته می‌شه
    private RectTransform slidePanelRT;
    private CanvasGroup slideGroup;
    private bool slideOpen;
    private Coroutine slideCo;

    // صفحه‌ی «درباره‌ی ما» (یه‌بار ساخته و بعد نشون/مخفی می‌شه)
    private GameObject aboutPanel;

    // متنِ اسمِ روی باکسِ پروفایل (تا بعد از واردکردنِ اسم رفرش بشه)
    private RTLTextMeshPro profileNameText;
    private GameObject profileBoxGO;      // خودِ باکسِ پروفایل (برای چشمک/کلیک)
    private Coroutine profileBlinkCo;     // انیمیشنِ چشمک

    void Start()
    {
        // تنظیماتِ صدا/موسیقی از همون ابتدا اعمال می‌شن (بلندیِ صدا، روشن/خاموشِ موسیقی)
        MusicManager.EnsureExists();
        MusicManager.SetMenu(true);  // تو منوییم → آهنگِ اصلی پخش شه (بعد از برگشت از بازی هم دوباره روشن می‌شه)
        UISound.EnsureExists();   // صدای کلیکِ سراسریِ دکمه‌ها
        SettingsSystem.Apply();

        BuildUI();

        // دکمه‌های منو تازه ساخته شدن — همین الان صدای کلیک بهشون وصل شه (بی‌تأخیر)
        UISound.HookAll();

        // اگه بازیکن تازه ماهِ فروردین رو تموم کرده باشه، خانم رستمی تو منوی اصلی تبریک می‌گه
        // و اسمِ کاربری رو می‌پرسه (پرچمش تو CardSwipe.EndStoryMonth ست می‌شه).
        if (PlayerPrefs.GetInt("DolatShow_PendingResult", 0) == 1)
        {
            PlayerPrefs.SetInt("DolatShow_PendingResult", 0);
            PlayerPrefs.Save();
            ResultGreetingUI.Show(targetCanvas, persianFont, RefreshProfileName);
        }
    }

    void BuildUI()
    {
        // اول سعی می‌کنیم منو رو از Prefabِ آماده (که تو ادیتور قابلِ ویرایشه) بسازیم؛
        // اگه Prefab نبود یا ایراد داشت، به روشِ کدیِ قبلی برمی‌گردیم (تا منو هیچ‌وقت خراب نشه).
        if (BuildFromPrefab()) { /* Prefab لود شد */ }
        else
        {
            panel = backgroundSprite != null
                ? RuntimeUIHelper.CreateImage(targetCanvas.transform, "MainMenuPanel", Vector2.zero, Vector2.one, backgroundSprite, stretch: true)
                : RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MainMenuPanel", new Color(0.05f, 0.05f, 0.08f, 1f));

            // سه دکمه (اگه اسپرایتشون خالی بمونه، نسخه‌ی رنگ‌ساده‌ی متنی نشون داده می‌شه)
            AddMenuButton("StartButton", startButtonSprite, startButtonCenter, "شروع بازی", StartGame);
            AddMenuButton("SettingsButton", settingsButtonSprite, settingsButtonCenter, "تنظیمات", OpenSettings);
            AddMenuButton("ExitButton", exitButtonSprite, exitButtonCenter, "خروج", QuitGame);

            // باکسِ پروفایل (بالا-چپ) — اسمِ کاربر + موجودیِ ریال.
            BuildProfileBox();
        }

        // پنلِ انتخابِ شروع/ادامه رو (یه‌بار) بساز و مخفی نگه‌دار (فعلاً همچنان با کد)
        BuildStartPanel();

        // منوی کشویی سمتِ راست (روبه‌روی باکسِ ریال) — با انیمیشن باز/بسته می‌شه
        BuildSlideMenu();

        // منو باید روی همه‌چیز (از جمله نوارهای وضعیت که موقع اجرا روی Canvas ساخته می‌شن) باشه
        panel.transform.SetAsLastSibling();
    }

    // منو رو از Prefabِ Resources/Prefabs/MainMenu می‌سازه (چیدمان/تصاویر تو خودِ Prefab تعریف شدن و
    // تو ادیتور قابلِ ویرایشن). فقط رفتارِ دکمه‌ها و متنِ ریال رو با کد وصل می‌کنیم.
    // اگه Prefab پیدا نشد false برمی‌گردونه تا BuildUI به روشِ کدیِ قبلی برگرده.
    bool BuildFromPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/MainMenu");
        if (prefab == null) return false;

        panel = Instantiate(prefab, targetCanvas.transform);
        panel.name = "MainMenuPanel";

        // RectTransformِ ریشه رو تمام‌صفحه می‌کنیم (محضِ اطمینان که دقیق پرِ صفحه بشه)
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // وصل‌کردنِ کلیکِ دکمه‌ها (چون onClick تو Prefab به متد وصل نمی‌شه، اینجا با کد وصلش می‌کنیم)
        WirePrefabButton("StartButton", StartGame);
        WirePrefabButton("SettingsButton", OpenSettings);
        WirePrefabButton("ExitButton", QuitGame);

        // باکسِ ریالِ قدیمیِ داخلِ Prefab رو حذف می‌کنیم و به‌جاش Profile Box می‌سازیم
        Transform oldRial = FindDeep(panel.transform, "RialBox");
        if (oldRial != null) Destroy(oldRial.gameObject);
        BuildProfileBox();

        return true;
    }

    // یه دکمه‌ی داخلِ Prefab رو پیدا می‌کنه و کلیکش رو به متدِ داده‌شده وصل می‌کنه
    void WirePrefabButton(string childName, UnityEngine.Events.UnityAction action)
    {
        Transform child = FindDeep(panel.transform, childName);
        if (child == null) return;
        // اگه به هر دلیلی کامپوننتِ Button از Prefab لود نشده باشه، خودمون اضافه می‌کنیم (تورِ ایمنی)
        Button btn = child.GetComponent<Button>();
        if (btn == null) btn = child.gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    // جست‌وجوی بازگشتیِ یه فرزند با اسمِ مشخص (تو هر عمقی)
    static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform c in parent)
        {
            Transform r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    // باکسِ پروفایل بالا-چپِ منو (جایگزینِ باکسِ ریال): دایره‌ی پروفایل (چپ، فعلاً خالی) +
    // اسمِ کاربر تو بنرِ بالا + عددِ ریال روبه‌روی سکه تو بنرِ پایین. تصویر از Resources/UI/ProfileBox.
    void BuildProfileBox()
    {
        // پروفایل فقط بعد از تموم‌شدنِ ماهِ فروردین نشون داده می‌شه (قبلش اصلاً ساخته نمی‌شه)
        if (!ProfileSystem.FarvardinDone) return;

        Sprite boxSpr = Resources.Load<Sprite>("UI/ProfileBox");
        if (boxSpr == null) return;

        Vector2 canvasSize = ((RectTransform)targetCanvas.transform).rect.size;
        float h = 0.12f; // ارتفاعِ باکس (کسری از صفحه)
        float aspect = boxSpr.rect.width / boxSpr.rect.height;
        float w = (h * canvasSize.y * aspect) / canvasSize.x; // عرض دقیقاً هم‌نسبتِ تصویر تا کِش/لِتربکس نشه
        float left = 0.02f, top = 0.985f;

        GameObject boxGO = RuntimeUIHelper.CreateImage(panel.transform, "ProfileBox",
            new Vector2(left, top - h), new Vector2(left + w, top), boxSpr);
        profileBoxGO = boxGO;
        // باکس کلیک‌پذیره → پنلِ پروفایل باز می‌شه
        boxGO.GetComponent<Image>().raycastTarget = true;
        Button pbtn = boxGO.AddComponent<Button>();
        pbtn.transition = Selectable.Transition.None;
        pbtn.onClick.AddListener(OpenProfilePanel);

        // آواتارِ پروفایل — داخلِ دایره‌ی سمتِ چپِ باکس (کسرها نسبت به خودِ باکس؛ قابلِ تنظیم)
        Sprite avatar = ProfileSystem.GetAvatarSprite();
        if (avatar != null)
        {
            GameObject av = RuntimeUIHelper.CreateImage(boxGO.transform, "Avatar",
                new Vector2(0.03f, 0.15f), new Vector2(0.30f, 0.86f), avatar); // preserveAspect: گرد وسطِ دایره
            av.GetComponent<Image>().raycastTarget = false;
        }

        // اسمِ کاربر — وسطِ بنرِ بالا (کسرها نسبت به خودِ تصویرِ باکس)
        string playerName = ProfileSystem.PlayerName;
        RTLTextMeshPro nameT = RuntimeUIHelper.CreateRTLText(boxGO.transform, "ProfileName",
            new Vector2(0.40f, 0.42f), new Vector2(0.92f, 0.72f), 32, persianFont);
        nameT.text = string.IsNullOrEmpty(playerName) ? "رئیس‌جمهور" : playerName;
        nameT.color = new Color(0.98f, 0.90f, 0.62f);
        nameT.fontStyle = FontStyles.Bold;
        nameT.alignment = TextAlignmentOptions.Center;
        nameT.raycastTarget = false;
        profileNameText = nameT;

        // عددِ ریال — تو بنرِ پایین، روبه‌روی نشانِ «ریال» (نشان سمتِ چپه، عدد سمتِ راست/وسطش)
        RTLTextMeshPro rialT = RuntimeUIHelper.CreateRTLText(boxGO.transform, "RialAmount",
            new Vector2(0.60f, 0.12f), new Vector2(0.85f, 0.30f), 30, persianFont);
        rialT.text = RialSystem.TotalPersian();
        rialT.color = new Color(0.98f, 0.88f, 0.55f);
        rialT.fontStyle = FontStyles.Bold;
        rialT.alignment = TextAlignmentOptions.Center;
        rialT.raycastTarget = false;

        // عددِ سطح — داخلِ نشانِ «سطح» (بالا-چپ)، فقط رقم (چون کلمه‌ی «سطح» رو خودِ نشان داره)
        RTLTextMeshPro lvlT = RuntimeUIHelper.CreateRTLText(boxGO.transform, "LevelText",
            new Vector2(0.31f, 0.79f), new Vector2(0.47f, 0.92f), 24, persianFont);
        lvlT.text = ProfileSystem.ToPersianDigits(LevelSystem.Level.ToString());
        lvlT.color = new Color(0.98f, 0.90f, 0.62f);
        lvlT.fontStyle = FontStyles.Bold;
        lvlT.alignment = TextAlignmentOptions.Center;
        lvlT.raycastTarget = false;

        // اگه تازه اسمش رو وارد کرده، باکس چشمک بزنه تا بفهمه روش کلیک کنه
        if (ProfileSystem.ProfileHintPending)
            profileBlinkCo = StartCoroutine(BlinkProfile());
    }

    // باز کردنِ پنلِ پروفایل + قطعِ چشمک
    void OpenProfilePanel()
    {
        ProfileSystem.SetProfileHint(false);
        if (profileBlinkCo != null) { StopCoroutine(profileBlinkCo); profileBlinkCo = null; }
        if (profileBoxGO != null) profileBoxGO.transform.localScale = Vector3.one;
        ProfilePanelUI.Open(targetCanvas, persianFont, RefreshProfileBox);
    }

    // چشمکِ ملایمِ باکسِ پروفایل (پرش/کوچیک‌شدنِ نرمِ اندازه) تا اولین کلیک
    IEnumerator BlinkProfile()
    {
        float t = 0f;
        while (profileBoxGO != null)
        {
            t += Time.unscaledDeltaTime * 3f;
            float s = 1f + Mathf.Sin(t) * 0.05f; // ۰.۹۵ تا ۱.۰۵
            profileBoxGO.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    // بعد از تغییرِ نام/آواتار تو پنل، باکسِ پروفایلِ منو رو تازه می‌کنه (ساده: دوباره می‌سازیم)
    void RefreshProfileBox()
    {
        if (profileBoxGO != null) Destroy(profileBoxGO);
        if (profileBlinkCo != null) { StopCoroutine(profileBlinkCo); profileBlinkCo = null; }
        profileBoxGO = null; profileNameText = null;
        BuildProfileBox();
    }

    // بعد از واردکردنِ اسم (صحنه‌ی تبریک)، اسمِ روی پروفایل رو تازه می‌کنه
    void RefreshProfileName()
    {
        if (profileNameText == null) return;
        string pn = ProfileSystem.PlayerName;
        profileNameText.text = string.IsNullOrEmpty(pn) ? "رئیس‌جمهور" : pn;
    }

    // منوی کشویی سمتِ راست: دکمه‌ی همبرگری (⋮) همیشه دیده می‌شه؛ با زدنش ستونِ
    // «فروشگاه / درباره‌ی ما / بستن» با انیمیشن باز/بسته می‌شه. تصاویر از اسپرایت‌شیتِ
    // Resources/UI/SlideMenu لود می‌شن (برش‌های نام‌دار: Store, About Us, Hamburger-style, Cross).
    void BuildSlideMenu()
    {
        Sprite[] sheet = Resources.LoadAll<Sprite>("UI/SlideMenu");
        if (sheet == null || sheet.Length == 0) return;
        Sprite ham = FindSprite(sheet, "Hamburger-style");
        Sprite store = FindSprite(sheet, "Store");
        Sprite about = FindSprite(sheet, "About Us");
        Sprite cross = FindSprite(sheet, "Cross");

        // دکمه‌ی همبرگری (باز/بست‌کن) — بالا-راست، روبه‌روی باکسِ ریال (یه‌ذره کوچیک‌تر شد)
        if (ham != null)
            RuntimeUIHelper.CreateImageButton(panel.transform, "SlideToggle",
                new Vector2(0.825f, 0.907f), new Vector2(0.955f, 0.985f), ham, ToggleSlideMenu);

        // پنلِ کشویی (زیرِ دکمه‌ی همبرگری) — شاملِ سه دکمه‌ی ستونی
        GameObject sp = new GameObject("SlidePanel", typeof(RectTransform));
        sp.transform.SetParent(panel.transform, false);
        slidePanelRT = sp.GetComponent<RectTransform>();
        slidePanelRT.anchorMin = new Vector2(0.825f, 0.63f);
        slidePanelRT.anchorMax = new Vector2(0.955f, 0.895f);
        slidePanelRT.offsetMin = slidePanelRT.offsetMax = Vector2.zero;
        slidePanelRT.pivot = new Vector2(0.5f, 1f); // از بالا (از زیرِ دکمه) باز می‌شه

        slideGroup = sp.AddComponent<CanvasGroup>();

        // سه دکمه‌ی ستونی (بالا→پایین): فروشگاه، درباره‌ی ما، بستن
        if (store != null)
            RuntimeUIHelper.CreateImageButton(sp.transform, "StoreButton",
                new Vector2(0f, 0.68f), new Vector2(1f, 1f), store, OpenStore);
        if (about != null)
            RuntimeUIHelper.CreateImageButton(sp.transform, "AboutButton",
                new Vector2(0f, 0.34f), new Vector2(1f, 0.66f), about, OpenAbout);
        if (cross != null)
            RuntimeUIHelper.CreateImageButton(sp.transform, "CloseSlideButton",
                new Vector2(0f, 0f), new Vector2(1f, 0.32f), cross, CloseSlideMenu);

        // حالتِ اولیه: بسته (نامرئی و غیرقابلِ کلیک)
        slideOpen = false;
        slidePanelRT.localScale = new Vector3(1f, 0f, 1f);
        slideGroup.alpha = 0f;
        slideGroup.blocksRaycasts = false;
        slideGroup.interactable = false;
    }

    static Sprite FindSprite(Sprite[] arr, string n)
    {
        foreach (var s in arr) if (s != null && s.name == n) return s;
        return null;
    }

    void ToggleSlideMenu()
    {
        slideOpen = !slideOpen;
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(AnimateSlide(slideOpen));
    }

    void CloseSlideMenu()
    {
        if (!slideOpen) return;
        slideOpen = false;
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(AnimateSlide(false));
    }

    // انیمیشنِ باز/بسته‌شدنِ ستون (unroll از بالا + محوشدن)
    IEnumerator AnimateSlide(bool opening)
    {
        if (slidePanelRT == null || slideGroup == null) yield break;
        const float dur = 0.28f;
        float t = 0f;
        float start = slidePanelRT.localScale.y;
        float end = opening ? 1f : 0f;
        if (opening) { slideGroup.blocksRaycasts = true; slideGroup.interactable = true; }
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            float eased = opening ? (1f - (1f - p) * (1f - p)) : (p * p); // باز: ease-out، بست: ease-in
            ApplySlideState(Mathf.Lerp(start, end, eased));
            yield return null;
        }
        ApplySlideState(end);
        if (!opening) { slideGroup.blocksRaycasts = false; slideGroup.interactable = false; }
        slideCo = null;
    }

    void ApplySlideState(float v)
    {
        if (slidePanelRT != null)
        {
            Vector3 s = slidePanelRT.localScale;
            s.y = v;
            slidePanelRT.localScale = s;
        }
        if (slideGroup != null) slideGroup.alpha = v;
    }

    // دکمه‌های منوی کشویی — فعلاً placeholder (مثلِ «تنظیمات»)؛ بعداً صفحه/محتواشون ساخته می‌شه
    void OpenStore()
    {
        CloseSlideMenu();
        StoreUI.Open(targetCanvas, persianFont);
    }

    // صفحه‌ی «درباره‌ی ما» — عکسِ Resources/UI/AboutUs رو تمام‌قد وسطِ صفحه نشون می‌ده،
    // با کلیک روی هرجاش بسته می‌شه. با انیمیشنِ سبکِ محو+پرش باز می‌شه.
    void OpenAbout()
    {
        CloseSlideMenu();
        if (aboutPanel == null) BuildAboutPanel();
        if (aboutPanel == null) return;
        aboutPanel.transform.SetAsLastSibling();
        aboutPanel.SetActive(true);
        StartCoroutine(FadeInPanel(aboutPanel));
    }

    void BuildAboutPanel()
    {
        aboutPanel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "AboutPanel", new Color(0f, 0f, 0f, 0.88f));
        Button overlay = aboutPanel.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        overlay.onClick.AddListener(HideAboutPanel);

        Sprite img = Resources.Load<Sprite>("UI/About Us");
        if (img != null)
        {
            GameObject go = RuntimeUIHelper.CreateImage(aboutPanel.transform, "AboutImage",
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), img); // preserveAspect: تمام‌قد و وسط
            go.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            // اگه عکس هنوز اضافه نشده باشه، یه متنِ ساده به‌جاش (تا صفحه خالی نباشه)
            RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(aboutPanel.transform, "AboutText",
                new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f), 34, persianFont);
            t.text = "درباره‌ی ما\n(عکس رو تو Resources/UI/About Us بذار)";
            t.color = new Color(0.96f, 0.9f, 0.78f);
            t.raycastTarget = false;
        }

        aboutPanel.SetActive(false);
    }

    void HideAboutPanel()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    // انیمیشنِ سبکِ باز شدنِ پنل: محو (fade) + یه پرشِ ریزِ اندازه (۰.۹۶→۱). سبک و روان.
    IEnumerator FadeInPanel(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        const float d = 0.2f;
        float t = 0f;
        cg.alpha = 0f;
        go.transform.localScale = Vector3.one * 0.96f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / d);
            float e = 1f - (1f - p) * (1f - p); // ease-out
            cg.alpha = e;
            go.transform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, e);
            yield return null;
        }
        cg.alpha = 1f;
        go.transform.localScale = Vector3.one;
    }

    // پنلِ «شروع بازی جدید / ادامه بازی قبلی» رو با کد می‌سازه (تصاویرش از Resources/UI لود می‌شن،
    // پس نیازی به وایرینگِ دستی تو صحنه نیست). با زدنِ دکمه‌ی «شروع بازی» نشون داده می‌شه.
    void BuildStartPanel()
    {
        // اول از Prefab (قابلِ ویرایش تو ادیتور) امتحان می‌کنیم؛ اگه نبود به روشِ کدیِ قبلی برمی‌گردیم
        if (BuildStartPanelFromPrefab()) return;

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

    // پنلِ شروع/ادامه رو از Resources/Prefabs/StartPanel می‌سازه (چیدمان تو خودِ Prefab).
    // فقط رفتارِ دکمه‌ها (شروع جدید/ادامه/بستن) رو با کد وصل می‌کنیم. اگه Prefab نبود false.
    bool BuildStartPanelFromPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/StartPanel");
        if (prefab == null) return false;

        startPanel = Instantiate(prefab, targetCanvas.transform);
        startPanel.name = "StartPanel";

        // کلیک روی پس‌زمینه‌ی تیره → بستنِ پنل
        Button overlay = startPanel.GetComponent<Button>();
        if (overlay == null) overlay = startPanel.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        overlay.onClick.RemoveAllListeners();
        overlay.onClick.AddListener(HideStartPanel);

        // دکمه‌ها
        WireStartPanelButton("NewGameButton", StartNewGame);
        continueButtonGO = null;
        Transform cont = FindDeep(startPanel.transform, "ContinueButton");
        if (cont != null)
        {
            continueButtonGO = cont.gameObject;
            Button b = cont.GetComponent<Button>();
            if (b == null) b = cont.gameObject.AddComponent<Button>();
            b.onClick.RemoveAllListeners(); b.onClick.AddListener(ContinueGame);
        }

        startPanel.SetActive(false);
        return true;
    }

    void WireStartPanelButton(string childName, UnityEngine.Events.UnityAction action)
    {
        Transform child = FindDeep(startPanel.transform, childName);
        if (child == null) return;
        Button btn = child.GetComponent<Button>();
        if (btn == null) btn = child.gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
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
        StartCoroutine(FadeInPanel(startPanel)); // باز شدنِ نرم (محو + پرشِ ریز)
    }

    // بونوسِ شروع از فروشگاه (اگه خریده باشه): +۱۰ یا +۲۰ روی هر چهار شاخص، یه‌بار مصرف می‌شه.
    void ApplyStoreStartBonus()
    {
        int bonus = StoreSystem.ConsumeStartBonus();
        if (bonus <= 0) return;
        GameStats.Instance.ApplyEffect(GameStats.StatType.Budget, bonus);
        GameStats.Instance.ApplyEffect(GameStats.StatType.Popularity, bonus);
        GameStats.Instance.ApplyEffect(GameStats.StatType.Security, bonus);
        GameStats.Instance.ApplyEffect(GameStats.StatType.Diplomacy, bonus);
    }

    void StartNewGame()
    {
        SaveSystem.DeleteSave();
        GameStats.Instance.ResetState();
        CardDatabase.Instance.ClearFlags();
        HideStartPanel();
        panel.SetActive(false);
        // اولِ بازیِ جدید، بخشِ آموزشی (دیالوگِ مشاور) نشون داده می‌شه؛ بعدش بازی شروع می‌شه.
        // اگه آیتمِ «شروعِ قدرتمند/طلایی» از فروشگاه خریده باشه، همون‌جا مصرف و روی شاخص‌ها اعمال می‌شه.
        TutorialUI.Show(targetCanvas, persianFont, () =>
        {
            ApplyStoreStartBonus();
            cardSwipe.BeginGame();
        });
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

    // صفحه‌ی تنظیمات رو باز می‌کنه (صدا، موسیقی، لرزش، پاک‌کردنِ اطلاعات، نسخه).
    void OpenSettings()
    {
        SettingsUI.Open(targetCanvas, persianFont);
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
