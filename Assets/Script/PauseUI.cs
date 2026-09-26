using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RTLTMPro;
using TMPro;

// سیستمِ توقف (Pause) موقعِ گیم‌پلی:
// - یه دکمه‌ی گِردِ توقف گوشه‌ی بالا نشون داده می‌شه (فقط وقتی بازی شروع شده، مثلِ نوارهای وضعیت).
// - با کلیک روش، یه باکس باز می‌شه با سه دکمه: «ادامه بازی»، «بازی دوباره»، «خانه».
// تصویرها از اطلسِ برش‌خورده‌ی توقف (Resources/UI) با Resources.LoadAll لود می‌شن؛ اگه پیدا نشد،
// دکمه‌ی متنیِ موقت ساخته می‌شه تا چیزی خراب نشه.
public class PauseUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private CardSwipe cardSwipe;

    private GameObject pauseButton;   // دکمه‌ی گِردِ توقف (گوشه‌ی بالا)
    private GameObject panel;          // پنلِ باز‌شونده با سه دکمه

    // اسپرایت‌های اطلس
    private Sprite sprPause, sprResume, sprHome, sprReplay;

    public static PauseUI Create(Canvas canvas, TMP_FontAsset font, CardSwipe cardSwipe)
    {
        if (canvas == null) return null;
        GameObject go = new GameObject("PauseUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        PauseUI ui = go.AddComponent<PauseUI>();
        ui.canvas = canvas; ui.font = font; ui.cardSwipe = cardSwipe;
        ui.Build();
        return ui;
    }

    void Build()
    {
        LoadSprites();

        // دکمه‌ی توقف — پایینِ صفحه، دقیقاً سمتِ چپِ باکسِ تاریخ (طبقِ طرحِ توسعه‌دهنده).
        // اولش مخفیه، با شروعِ بازی روشن می‌شه.
        if (sprPause != null)
            pauseButton = RuntimeUIHelper.CreateImageButton(canvas.transform, "PauseButton",
                Vector2.zero, Vector2.one, sprPause, OpenPanel);
        else
            pauseButton = RuntimeUIHelper.CreateButton(canvas.transform, "PauseButton",
                Vector2.zero, Vector2.one, "II", font, new Color(0.3f, 0.22f, 0.12f, 0.95f), OpenPanel);

        // مختصاتِ پیکسلیِ هم‌سبک با باکسِ تاریخ (وسط-پایین) تا همیشه کنارِ باکس بمونه.
        // باکسِ تاریخ: anchor (0.5,0)، size 760×200، y=120. لبه‌ی چپش x=-380؛ دکمه رو چپِ اون می‌ذاریم.
        RectTransform rt = (RectTransform)pauseButton.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(190f, 190f);
        rt.anchoredPosition = new Vector2(-490f, 125f);

        // تضمینِ اینکه دکمه‌ی توقف همیشه روی همه‌چیز (نوارها/کارت) رندر شه
        Canvas cv = pauseButton.AddComponent<Canvas>();
        cv.overrideSorting = true; cv.sortingOrder = 500;

        pauseButton.SetActive(false); // تا شروعِ بازی مخفی

        CardSwipe.GameStarted += OnGameStarted;
    }

    void OnDestroy()
    {
        CardSwipe.GameStarted -= OnGameStarted;
    }

    void OnGameStarted()
    {
        if (pauseButton != null)
        {
            pauseButton.transform.SetAsLastSibling();
            pauseButton.SetActive(true);
        }
    }

    // --- باز/بسته کردنِ پنل ---
    void OpenPanel()
    {
        if (panel == null) BuildPanel();
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false); // موقعِ باز بودنِ پنل، خودِ دکمه‌ی توقف مخفی
        if (cardSwipe != null) cardSwipe.enabled = false; // ورودیِ سوایپِ کارت متوقف بشه
        RuntimeUIHelper.PlayFadeIn(this, panel);
    }

    void Resume()
    {
        if (panel != null) panel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true); // دکمه‌ی توقف دوباره پیدا شه
        if (cardSwipe != null) cardSwipe.enabled = true;
    }

    void BuildPanel()
    {
        // پس‌زمینه‌ی تیره؛ کلیک روی فضای خالی = ادامه‌ی بازی
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "PausePanel", new Color(0f, 0f, 0f, 0.8f));
        Button overlay = panel.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        overlay.onClick.AddListener(Resume);

        // باکسِ وسط (از Box؛ اگه نبود رنگِ ساده)
        Sprite boxSpr = Resources.Load<Sprite>("UI/Box");
        GameObject box;
        if (boxSpr != null)
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.14f, 0.24f), new Vector2(0.86f, 0.76f), boxSpr, stretch: true);
        else
        {
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.14f, 0.24f), new Vector2(0.86f, 0.76f), null);
            box.GetComponent<Image>().color = new Color(0.16f, 0.10f, 0.05f, 0.99f);
        }
        box.GetComponent<Image>().raycastTarget = true; // کلیک رو باکس نباید ببنده
        RectTransform b = (RectTransform)box.transform;

        // عنوان
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(b, "Title", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), 40, font);
        title.text = "توقف"; title.color = new Color(0.98f, 0.88f, 0.55f); title.fontStyle = FontStyles.Bold; title.raycastTarget = false;

        // سه دکمه: ادامه (بالا) / بازیِ دوباره (وسط) / خانه (پایین)
        AddPanelButton(b, "Resume", sprResume, "ادامه بازی", new Vector2(0.12f, 0.60f), new Vector2(0.88f, 0.78f), Resume);
        AddPanelButton(b, "Replay", sprReplay, "بازی دوباره", new Vector2(0.12f, 0.38f), new Vector2(0.88f, 0.56f), ReplayGame);
        AddPanelButton(b, "Home", sprHome, "خانه", new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.34f), GoHome);
    }

    // یه دکمه‌ی پنل: اگه تصویرش تو اطلس بود، تصویری (بدونِ متن، چون خودِ عکس نوشته داره)؛ وگرنه دکمه‌ی متنی.
    void AddPanelButton(RectTransform parent, string name, Sprite spr, string label, Vector2 aMin, Vector2 aMax, UnityEngine.Events.UnityAction onClick)
    {
        if (spr != null)
            RuntimeUIHelper.CreateImageButton(parent, name, aMin, aMax, spr, onClick);
        else
            RuntimeUIHelper.CreateButton(parent, name, aMin, aMax, label, font, new Color(0.3f, 0.22f, 0.12f, 1f), onClick);
    }

    void ReplayGame()
    {
        if (panel != null) panel.SetActive(false);
        if (cardSwipe != null) { cardSwipe.enabled = true; cardSwipe.RestartNewGame(); }
    }

    void GoHome()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- لودِ اسپرایت‌ها از اطلسِ برش‌خورده‌ی توقف ---
    void LoadSprites()
    {
        // اسم‌های احتمالیِ فایلِ اطلس رو امتحان می‌کنیم (چون هنوز اسمش رو مطمئن نیستم)
        string[] candidates = { "Step", "Steppe", "Stepp", "Stop", "Pause", "Estep", "StepButtons", "PauseButtons", "Step Button", "Pause Button" };
        Sprite[] atlas = null;
        foreach (string c in candidates)
        {
            atlas = Resources.LoadAll<Sprite>("UI/" + c);
            if (atlas != null && atlas.Length > 0) break;
        }
        if (atlas == null || atlas.Length == 0) return;

        // نکته: اطلسِ Steppe برش‌های خالیِ اضافه (Steppe_2..18) هم داره که شاملِ «step»ان،
        // پس برای دکمه‌ی توقف فقط «stop/pause» رو می‌گیریم (نه «step») تا برشِ خالی انتخاب نشه.
        sprResume = Find(atlas, "continue", "continu", "resume", "play", "ادامه");
        sprHome   = Find(atlas, "home", "خانه");
        sprReplay = Find(atlas, "replay", "retry", "restart", "دوباره");
        sprPause  = Find(atlas, "stop", "pause");
    }

    static Sprite Find(Sprite[] atlas, params string[] keys)
    {
        foreach (var sp in atlas)
        {
            string low = sp.name.ToLower();
            foreach (var k in keys) if (low.Contains(k)) return sp;
        }
        return null;
    }
}
