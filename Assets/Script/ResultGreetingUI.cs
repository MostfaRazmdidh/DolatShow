using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System;

// صحنه‌ی «تبریکِ پایانِ فروردین» — بعد از تمومِ ماهِ داستانی، وقتی بازیکن به منوی اصلی برمی‌گرده،
// خانم رستمی (مشاورِ ارشد) تبریک می‌گه (صدای Result.wav + تشویقِ آروم‌تر) و بعد اسمِ کاربری رو می‌پرسه.
// موقعِ اجرا با کد ساخته می‌شه؛ همه‌چیز از Resources لود می‌شه.
public class ResultGreetingUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private Action onComplete;

    private GameObject panel;
    private AudioSource voiceSrc;   // صدای مشاور (بلند)
    private AudioSource sfxSrc;     // صدای تشویق (آروم‌تر)
    private GameObject namePanel;
    private TMP_InputField nameInput;
    private bool nameShown;

    public const string PlayerNameKey = "DolatShow_PlayerName";

    // همون دیالوگِ ثابتِ خانم رستمی (هماهنگ با فایلِ صوتی Result.wav)
    private const string greetLine =
        "تبریک می‌گم، جنابِ رئیس‌جمهور. مردم رأی‌شون رو دادن و از امروز، سکانِ این کشور دستِ شماست. راهِ سختی در پیشه، اما من تا آخرش کنارتونم. حالا وقتِ کار کردنه، قربان.";

    public static ResultGreetingUI Show(Canvas canvas, TMP_FontAsset font, Action onComplete)
    {
        if (canvas == null) { onComplete?.Invoke(); return null; }
        GameObject go = new GameObject("ResultGreetingUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        ResultGreetingUI ui = go.AddComponent<ResultGreetingUI>();
        ui.canvas = canvas;
        ui.font = font;
        ui.onComplete = onComplete;
        ui.Build();
        return ui;
    }

    void Build()
    {
        Vector2 canvasSize = ((RectTransform)canvas.transform).rect.size;

        // دو AudioSource: یکی صدای مشاور، یکی تشویق
        voiceSrc = gameObject.AddComponent<AudioSource>();
        voiceSrc.playOnAwake = false;
        sfxSrc = gameObject.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;

        AudioClip voice = Resources.Load<AudioClip>("Voice/Result/Result");
        AudioClip applause = Resources.Load<AudioClip>("Voice/Result/Applause sound effect");

        // پس‌زمینه‌ی تیره؛ ضربه روی هرجاش → رفتن به مرحله‌ی پرسیدنِ اسم
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "ResultPanel", new Color(0f, 0f, 0f, 0.9f));
        Button advanceBtn = panel.AddComponent<Button>();
        advanceBtn.transition = Selectable.Transition.None;
        advanceBtn.onClick.AddListener(ShowNamePanel);

        // کاراکترِ مشاور — پایین-چپ
        Sprite advisor = Resources.Load<Sprite>("Characters/Advisor");
        if (advisor != null)
        {
            float hFrac = 0.60f;
            float aspect = advisor.rect.width / advisor.rect.height;
            float wFrac = (hFrac * canvasSize.y * aspect) / canvasSize.x;
            GameObject a = RuntimeUIHelper.CreateImage(panel.transform, "Advisor",
                new Vector2(0f, 0f), new Vector2(wFrac, hFrac), advisor);
            a.GetComponent<Image>().raycastTarget = false;
        }

        // باکسِ دیالوگ — بالای صفحه
        Sprite box = Resources.Load<Sprite>("UI/DialogBox");
        RectTransform boxRT;
        if (box != null)
        {
            float w = 0.97f;
            float aspect = box.rect.width / box.rect.height;
            float hFrac = (w * canvasSize.x / aspect) / canvasSize.y * 1.9f;
            float yMax = 0.93f;
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox",
                new Vector2(0.5f - w / 2f, yMax - hFrac), new Vector2(0.5f + w / 2f, yMax), box);
            Image bimg = b.GetComponent<Image>();
            bimg.preserveAspect = false;
            bimg.raycastTarget = false;
            boxRT = b.GetComponent<RectTransform>();
        }
        else
        {
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox", Vector2.zero, Vector2.one, null);
            Image bi = b.GetComponent<Image>();
            bi.color = new Color(0.18f, 0.12f, 0.06f, 0.96f); bi.raycastTarget = false;
            RectTransform r = b.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.04f, 0.63f); r.anchorMax = new Vector2(0.96f, 0.93f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            boxRT = r;
        }

        RTLTextMeshPro line = RuntimeUIHelper.CreateRTLText(boxRT, "Line",
            new Vector2(0.07f, 0.14f), new Vector2(0.93f, 0.90f), 34, font);
        line.color = new Color(0.96f, 0.90f, 0.78f);
        line.enableAutoSizing = true; line.fontSizeMin = 12; line.fontSizeMax = 40;
        line.raycastTarget = false;
        line.text = greetLine;

        RTLTextMeshPro nameText = RuntimeUIHelper.CreateRTLText(panel.transform, "Name",
            new Vector2(0.5f, 0.935f), new Vector2(0.96f, 0.985f), 32, font);
        nameText.text = "خانم رستمی";
        nameText.color = new Color(0.98f, 0.82f, 0.42f);
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Right;
        nameText.raycastTarget = false;

        RTLTextMeshPro hint = RuntimeUIHelper.CreateRTLText(boxRT, "Hint",
            new Vector2(0.08f, 0.04f), new Vector2(0.6f, 0.22f), 22, font);
        hint.text = "برای ادامه بزن ›";
        hint.color = new Color(0.85f, 0.78f, 0.6f, 0.9f);
        hint.alignment = TextAlignmentOptions.Left;
        hint.raycastTarget = false;

        panel.transform.SetAsLastSibling();

        // پخشِ صدا: مشاور با ولومِ کامل، تشویق آروم‌تر (طبقِ خواسته)
        if (voice != null) { voiceSrc.clip = voice; voiceSrc.volume = 1f; voiceSrc.Play(); }
        if (applause != null) { sfxSrc.clip = applause; sfxSrc.volume = 0.35f; sfxSrc.Play(); }
    }

    // مرحله‌ی دوم: باکسِ ورودِ نامِ کاربری
    void ShowNamePanel()
    {
        if (nameShown) return; // فقط یه‌بار
        nameShown = true;

        namePanel = new GameObject("NamePanel", typeof(RectTransform), typeof(Image));
        namePanel.transform.SetParent(panel.transform, false);
        RectTransform nprt = (RectTransform)namePanel.transform;
        nprt.anchorMin = Vector2.zero; nprt.anchorMax = Vector2.one;
        nprt.offsetMin = nprt.offsetMax = Vector2.zero;
        // یه لایه‌ی نامرئی که کلیک‌ها رو می‌گیره تا ضربه به پس‌زمینه دوباره چیزی رو تریگر نکنه
        namePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        Sprite[] sheet = Resources.LoadAll<Sprite>("UI/NameBox");
        Sprite boxSpr = FindSprite(sheet, "Box");
        Sprite titleSpr = FindSprite(sheet, "Title");
        Sprite inputSpr = FindSprite(sheet, "Input");
        Sprite tikSpr = FindSprite(sheet, "Tik");

        // باکسِ اصلی (وسط)
        if (boxSpr != null)
        {
            GameObject bg = RuntimeUIHelper.CreateImage(namePanel.transform, "Box",
                new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.67f), boxSpr);
            bg.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            GameObject bg = RuntimeUIHelper.CreateImage(namePanel.transform, "Box", Vector2.zero, Vector2.one, null);
            Image bi = bg.GetComponent<Image>(); bi.color = new Color(0.16f, 0.10f, 0.05f, 0.98f); bi.raycastTarget = false;
            RectTransform r = (RectTransform)bg.transform;
            r.anchorMin = new Vector2(0.08f, 0.33f); r.anchorMax = new Vector2(0.92f, 0.67f);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        // بنرِ عنوان + متنِ «نام کاربری خود را وارد کنید»
        if (titleSpr != null)
        {
            GameObject tb = RuntimeUIHelper.CreateImage(namePanel.transform, "TitleBanner",
                new Vector2(0.2f, 0.61f), new Vector2(0.8f, 0.69f), titleSpr);
            tb.GetComponent<Image>().raycastTarget = false;
        }
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(namePanel.transform, "Title",
            new Vector2(0.2f, 0.61f), new Vector2(0.8f, 0.69f), 30, font);
        title.text = "نام کاربری خود را وارد کنید";
        title.color = new Color(0.98f, 0.86f, 0.55f);
        title.fontStyle = FontStyles.Bold;
        title.raycastTarget = false;

        // فیلدِ ورودِ اسم
        BuildInputField(inputSpr, new Vector2(0.15f, 0.45f), new Vector2(0.85f, 0.55f));

        // دکمه‌ی تأیید (Tik) — پایینِ باکس
        if (tikSpr != null)
        {
            RuntimeUIHelper.CreateImageButton(namePanel.transform, "ConfirmButton",
                new Vector2(0.44f, 0.35f), new Vector2(0.56f, 0.43f), tikSpr, ConfirmName);
        }
        else
        {
            RuntimeUIHelper.CreateButton(namePanel.transform, "ConfirmButton",
                new Vector2(0.38f, 0.35f), new Vector2(0.62f, 0.42f), "تأیید", font,
                new Color(0.2f, 0.5f, 0.25f, 1f), ConfirmName);
        }
    }

    // ساختِ یه TMP_InputField ساده (با فونتِ فارسی). Persian کاملاً reshape نمی‌شه ولی متن ذخیره می‌شه.
    void BuildInputField(Sprite bgSprite, Vector2 aMin, Vector2 aMax)
    {
        GameObject inGO = new GameObject("NameInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        inGO.transform.SetParent(namePanel.transform, false);
        RectTransform rt = (RectTransform)inGO.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = inGO.GetComponent<Image>();
        if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.08f, 0.05f, 0.02f, 1f);

        nameInput = inGO.AddComponent<TMP_InputField>();

        // ناحیه‌ی متن (viewport)
        GameObject area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(inGO.transform, false);
        RectTransform art = (RectTransform)area.transform;
        art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(20f, 8f); art.offsetMax = new Vector2(-20f, -8f);

        // متنِ placeholder
        GameObject phGO = new GameObject("Placeholder", typeof(RectTransform));
        phGO.transform.SetParent(area.transform, false);
        TextMeshProUGUI ph = phGO.AddComponent<TextMeshProUGUI>();
        StretchFull((RectTransform)phGO.transform);
        if (font != null) ph.font = font;
        ph.text = "اسم شما...";
        ph.fontSize = 34; ph.color = new Color(0.7f, 0.65f, 0.5f, 0.7f);
        ph.alignment = TextAlignmentOptions.MidlineRight;

        // متنِ ورودی
        GameObject txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(area.transform, false);
        TextMeshProUGUI txt = txtGO.AddComponent<TextMeshProUGUI>();
        StretchFull((RectTransform)txtGO.transform);
        if (font != null) txt.font = font;
        txt.fontSize = 34; txt.color = new Color(0.98f, 0.92f, 0.78f);
        txt.alignment = TextAlignmentOptions.MidlineRight;
        txt.richText = false;

        nameInput.textViewport = art;
        nameInput.textComponent = txt;
        nameInput.placeholder = ph;
        if (font != null) nameInput.fontAsset = font;
        nameInput.pointSize = 34;
        nameInput.characterLimit = 16;
        nameInput.lineType = TMP_InputField.LineType.SingleLine;
        nameInput.text = PlayerPrefs.GetString(PlayerNameKey, "");
    }

    static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    void ConfirmName()
    {
        string entered = nameInput != null ? nameInput.text.Trim() : "";
        if (!string.IsNullOrEmpty(entered))
        {
            PlayerPrefs.SetString(PlayerNameKey, entered);
            PlayerPrefs.Save();
        }
        Finish();
    }

    static Sprite FindSprite(Sprite[] arr, string n)
    {
        if (arr == null) return null;
        foreach (var s in arr) if (s != null && s.name == n) return s;
        return null;
    }

    void Finish()
    {
        Action cb = onComplete;
        onComplete = null;
        if (voiceSrc != null) voiceSrc.Stop();
        if (sfxSrc != null) sfxSrc.Stop();
        if (panel != null) Destroy(panel);
        Destroy(gameObject);
        cb?.Invoke();
    }
}
