using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System;

// صفحه‌ی تنظیمات — کد‌محور (مثلِ بقیه‌ی UIهای پروژه)، با اسپرایت‌های موجود؛ گرافیکِ اختصاصی بعداً.
// گزینه‌ها: صدا (روشن/خاموش + بلندی)، موسیقی (روشن/خاموش + بلندی)، لرزش، پاک‌کردنِ اطلاعات، نسخه.
public class SettingsUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private GameObject panel;

    // رنگ‌های تم
    static readonly Color GOLD = new Color(0.98f, 0.88f, 0.55f);
    static readonly Color CREAM = new Color(0.9f, 0.85f, 0.72f);
    static readonly Color ON = new Color(0.32f, 0.62f, 0.34f);   // سبزِ روشن
    static readonly Color OFF = new Color(0.4f, 0.28f, 0.22f);   // قهوه‌ایِ خاموش
    static readonly Color KNOB = new Color(0.96f, 0.93f, 0.82f);

    public static SettingsUI Open(Canvas canvas, TMP_FontAsset font)
    {
        if (canvas == null) return null;
        GameObject go = new GameObject("SettingsUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        SettingsUI ui = go.AddComponent<SettingsUI>();
        ui.canvas = canvas; ui.font = font;
        ui.Build();
        return ui;
    }

    void Build()
    {
        // پس‌زمینه‌ی تیره؛ کلیک روی فضای خالی → بستن
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "SettingsPanel", new Color(0f, 0f, 0f, 0.82f));
        Button overlay = panel.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        overlay.onClick.AddListener(Close);

        // باکسِ وسط (از Box؛ اگه نبود رنگِ ساده)
        Sprite boxSpr = Resources.Load<Sprite>("UI/Box");
        GameObject box;
        if (boxSpr != null)
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.07f, 0.12f), new Vector2(0.93f, 0.88f), boxSpr, stretch: true);
        else
        {
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.07f, 0.12f), new Vector2(0.93f, 0.88f), null);
            box.GetComponent<Image>().color = new Color(0.16f, 0.10f, 0.05f, 0.99f);
        }
        box.GetComponent<Image>().raycastTarget = true; // کلیک رو باکس نباید ببنده
        RectTransform b = (RectTransform)box.transform;

        // عنوان
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(b, "Title", new Vector2(0.1f, 0.90f), new Vector2(0.9f, 0.99f), 44, font);
        title.text = "تنظیمات"; title.color = GOLD; title.fontStyle = FontStyles.Bold; title.raycastTarget = false;

        // --- ردیفِ صدا ---
        MakeLabel(b, "صدا", new Vector2(0.30f, 0.80f), new Vector2(0.94f, 0.87f));
        MakeToggle(b, new Vector2(0.06f, 0.80f), new Vector2(0.27f, 0.87f),
            () => SettingsSystem.SoundOn, v => SettingsSystem.SoundOn = v);
        MakeSlider(b, new Vector2(0.08f, 0.735f), new Vector2(0.92f, 0.775f),
            () => SettingsSystem.SoundVolume, v => SettingsSystem.SoundVolume = v);

        // --- ردیفِ موسیقی ---
        MakeLabel(b, "موسیقی", new Vector2(0.30f, 0.635f), new Vector2(0.94f, 0.705f));
        MakeToggle(b, new Vector2(0.06f, 0.635f), new Vector2(0.27f, 0.705f),
            () => SettingsSystem.MusicOn, v => SettingsSystem.MusicOn = v);
        MakeSlider(b, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.61f),
            () => SettingsSystem.MusicVolume, v => SettingsSystem.MusicVolume = v);

        // --- ردیفِ لرزش ---
        MakeLabel(b, "لرزش", new Vector2(0.30f, 0.47f), new Vector2(0.94f, 0.54f));
        MakeToggle(b, new Vector2(0.06f, 0.47f), new Vector2(0.27f, 0.54f),
            () => SettingsSystem.VibrationOn, v => SettingsSystem.VibrationOn = v);

        // --- دکمه‌ی پاک‌کردنِ اطلاعات ---
        RuntimeUIHelper.CreateButton(b, "ResetButton", new Vector2(0.15f, 0.32f), new Vector2(0.85f, 0.41f),
            "پاک‌کردنِ اطلاعات", font, new Color(0.5f, 0.2f, 0.18f, 1f), ShowResetConfirm);

        // --- نسخه ---
        RTLTextMeshPro ver = RuntimeUIHelper.CreateRTLText(b, "Version", new Vector2(0.1f, 0.235f), new Vector2(0.9f, 0.29f), 22, font);
        ver.text = "نسخه‌ی " + ProfileSystem.ToPersianDigits(Application.version);
        ver.color = new Color(0.7f, 0.66f, 0.55f); ver.raycastTarget = false;

        // --- دکمه‌ی بستن ---
        RuntimeUIHelper.CreateButton(b, "CloseButton", new Vector2(0.32f, 0.05f), new Vector2(0.68f, 0.14f),
            "بستن", font, new Color(0.4f, 0.25f, 0.2f, 1f), Close);

        panel.transform.SetAsLastSibling();
        RuntimeUIHelper.PlayFadeIn(this, box);
    }

    void MakeLabel(RectTransform parent, string txt, Vector2 aMin, Vector2 aMax)
    {
        RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(parent, "Lbl_" + txt, aMin, aMax, 32, font);
        t.text = txt; t.color = CREAM; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.MidlineRight; t.raycastTarget = false;
    }

    // --- دکمه‌ی کلیدیِ روشن/خاموش (سوییچِ کشویی) ---
    void MakeToggle(RectTransform parent, Vector2 aMin, Vector2 aMax, Func<bool> get, Action<bool> set)
    {
        GameObject row = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Button));
        row.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)row.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image bg = row.GetComponent<Image>();
        Button rowBtn = row.GetComponent<Button>();
        rowBtn.transition = Selectable.Transition.None; // رنگِ روشن/خاموش رو خودمون مدیریت می‌کنیم

        // دستگیره (knob)
        GameObject knobGO = new GameObject("Knob", typeof(RectTransform), typeof(Image));
        knobGO.transform.SetParent(row.transform, false);
        RectTransform krt = (RectTransform)knobGO.transform;
        knobGO.GetComponent<Image>().color = KNOB;
        knobGO.GetComponent<Image>().raycastTarget = false;

        Action refresh = () =>
        {
            bool on = get();
            bg.color = on ? ON : OFF;
            // دستگیره سمتِ راست وقتی روشن، سمتِ چپ وقتی خاموش
            krt.anchorMin = on ? new Vector2(0.55f, 0.12f) : new Vector2(0.06f, 0.12f);
            krt.anchorMax = on ? new Vector2(0.94f, 0.88f) : new Vector2(0.45f, 0.88f);
            krt.offsetMin = krt.offsetMax = Vector2.zero;
        };
        refresh();

        rowBtn.onClick.AddListener(() => { set(!get()); refresh(); });
    }

    // --- نوارِ بلندیِ صدا (Slider) ---
    void MakeSlider(RectTransform parent, Vector2 aMin, Vector2 aMax, Func<float> get, Action<float> set)
    {
        GameObject sGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sGO.transform.SetParent(parent, false);
        RectTransform srt = (RectTransform)sGO.transform;
        srt.anchorMin = aMin; srt.anchorMax = aMax; srt.offsetMin = srt.offsetMax = Vector2.zero;

        // پس‌زمینه‌ی نوار (track تیره)
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(sGO.transform, false);
        StretchFull((RectTransform)bgGO.transform);
        bgGO.GetComponent<Image>().color = new Color(0.1f, 0.07f, 0.04f, 1f);

        // ناحیه‌ی پُرشدن
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sGO.transform, false);
        RectTransform fart = (RectTransform)fillArea.transform;
        fart.anchorMin = new Vector2(0f, 0.25f); fart.anchorMax = new Vector2(1f, 0.75f);
        fart.offsetMin = new Vector2(6f, 0f); fart.offsetMax = new Vector2(-6f, 0f);

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillArea.transform, false);
        RectTransform fillrt = (RectTransform)fillGO.transform;
        fillrt.anchorMin = new Vector2(0f, 0f); fillrt.anchorMax = new Vector2(1f, 1f);
        fillrt.offsetMin = Vector2.zero; fillrt.offsetMax = Vector2.zero;
        fillGO.GetComponent<Image>().color = GOLD;

        // دستگیره
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sGO.transform, false);
        RectTransform hart = (RectTransform)handleArea.transform;
        hart.anchorMin = new Vector2(0f, 0f); hart.anchorMax = new Vector2(1f, 1f);
        hart.offsetMin = new Vector2(10f, 0f); hart.offsetMax = new Vector2(-10f, 0f);

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(handleArea.transform, false);
        RectTransform hrt = (RectTransform)handleGO.transform;
        hrt.sizeDelta = new Vector2(26f, 0f);
        hrt.anchorMin = new Vector2(0f, 0f); hrt.anchorMax = new Vector2(0f, 1f);
        handleGO.GetComponent<Image>().color = KNOB;

        Slider slider = sGO.GetComponent<Slider>();
        slider.fillRect = fillrt;
        slider.handleRect = hrt;
        slider.targetGraphic = handleGO.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value = get();
        slider.onValueChanged.AddListener(v => set(v));
    }

    // --- تاییدِ پاک‌کردنِ اطلاعات ---
    void ShowResetConfirm()
    {
        GameObject ov = RuntimeUIHelper.CreateFullScreenPanel(panel.transform, "ResetConfirm", new Color(0f, 0f, 0f, 0.7f));
        Button ob = ov.AddComponent<Button>();
        ob.transition = Selectable.Transition.None;
        ob.onClick.AddListener(() => Destroy(ov));

        GameObject cbox = RuntimeUIHelper.CreateImage(ov.transform, "Box", new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.62f), null);
        cbox.GetComponent<Image>().color = new Color(0.14f, 0.09f, 0.04f, 0.99f);
        cbox.GetComponent<Image>().raycastTarget = true;
        RectTransform cb = (RectTransform)cbox.transform;

        RTLTextMeshPro q = RuntimeUIHelper.CreateRTLText(cb, "Q", new Vector2(0.06f, 0.5f), new Vector2(0.94f, 0.95f), 28, font);
        q.text = "همه‌ی اطلاعات (ذخیره، ریال، پروفایل) پاک بشه؟ این کار برگشت‌ناپذیره.";
        q.color = new Color(0.96f, 0.86f, 0.6f); q.raycastTarget = false;

        RuntimeUIHelper.CreateButton(cb, "Yes", new Vector2(0.55f, 0.1f), new Vector2(0.94f, 0.42f),
            "بله، پاک کن", font, new Color(0.55f, 0.2f, 0.18f, 1f), () => { DoReset(); Destroy(ov); });
        RuntimeUIHelper.CreateButton(cb, "No", new Vector2(0.06f, 0.1f), new Vector2(0.45f, 0.42f),
            "انصراف", font, new Color(0.3f, 0.3f, 0.3f, 1f), () => Destroy(ov));
    }

    void DoReset()
    {
        // ذخیره‌ی بازی + همه‌ی PlayerPrefs پاک می‌شه (ریال، پروفایل، تنظیمات، ...)
        SaveSystem.DeleteSave();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SettingsSystem.Apply(); // صدا به حالتِ پیش‌فرض برگرده
        // صحنه رو دوباره لود می‌کنیم تا منو با حالتِ تازه بالا بیاد
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    void Close()
    {
        Destroy(gameObject);
        Destroy(panel);
    }
}
