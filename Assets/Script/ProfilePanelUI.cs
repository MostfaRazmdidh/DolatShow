using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System;

// پنلِ پروفایل — با کلیک روی باکسِ پروفایل باز می‌شه. کاربر شناسه‌ش رو می‌بینه،
// می‌تونه آواتارش رو عوض کنه (رایگان) و اسمش رو عوض کنه (اولین بار ۲ ریال، هر بار دوبرابر).
// کد‌محور و با اسپرایت‌های موجود ساخته می‌شه؛ گرافیکِ اختصاصی بعداً.
public class ProfilePanelUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private Action onChanged;   // برای رفرشِ باکسِ پروفایلِ منو

    private GameObject panel;
    private Image avatarImg;
    private RTLTextMeshPro nameText;
    private RTLTextMeshPro changeNameLabel;
    private RTLTextMeshPro msgText;      // پیامِ «ریال کافی نیست» و...
    private GameObject inputOverlay;
    private TMP_InputField nameInput;
    private RTLTextMeshPro inputMsg;   // پیامِ خطا داخلِ پنلِ تعویضِ نام (تا پشتِ overlay قایم نشه)

    public static ProfilePanelUI Open(Canvas canvas, TMP_FontAsset font, Action onChanged)
    {
        if (canvas == null) return null;
        GameObject go = new GameObject("ProfilePanelUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        ProfilePanelUI ui = go.AddComponent<ProfilePanelUI>();
        ui.canvas = canvas; ui.font = font; ui.onChanged = onChanged;
        ui.Build();
        return ui;
    }

    void Build()
    {
        // پس‌زمینه‌ی تیره — جلوی کلیکِ چیزهای پشتش رو می‌گیره ولی خودش با کلیک بسته نمی‌شه
        // (فقط دکمه‌ی «بستن» می‌بنده).
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "ProfilePanel", new Color(0f, 0f, 0f, 0.8f));

        // باکسِ وسط (از Box؛ عمودیه پس رِکتِ عمودی تا کِش نیاد. اگه نبود رنگِ ساده)
        Sprite boxSpr = Resources.Load<Sprite>("UI/Box");
        GameObject box;
        if (boxSpr != null)
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.88f), boxSpr, stretch: true);
        else
        {
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.88f), null);
            box.GetComponent<Image>().color = new Color(0.16f, 0.10f, 0.05f, 0.98f);
        }
        // کلیک روی خودِ باکس نباید ببنده
        box.GetComponent<Image>().raycastTarget = true;

        RectTransform brt = (RectTransform)box.transform;

        // عنوان
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(brt, "Title", new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.98f), 40, font);
        title.text = "پروفایل"; title.color = new Color(0.98f, 0.88f, 0.55f); title.fontStyle = FontStyles.Bold; title.raycastTarget = false;

        // آواتار (بالا-وسط) — با دو فلشِ چپ/راست بینِ آواتارها جابه‌جا می‌شه
        GameObject av = RuntimeUIHelper.CreateImage(brt, "Avatar", new Vector2(0.37f, 0.62f), new Vector2(0.63f, 0.84f), ProfileSystem.GetAvatarSprite());
        avatarImg = av.GetComponent<Image>(); avatarImg.raycastTarget = false;

        // فلشِ قبلی (چپ) و بعدی (راست) برای عوض‌کردنِ آواتار
        AddArrowButton(brt, "PrevAvatar", new Vector2(0.13f, 0.66f), new Vector2(0.27f, 0.80f), "<", PrevAvatar);
        AddArrowButton(brt, "NextAvatar", new Vector2(0.73f, 0.66f), new Vector2(0.87f, 0.80f), ">", NextAvatar);

        // شناسه
        RTLTextMeshPro idText = RuntimeUIHelper.CreateRTLText(brt, "IdText", new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.57f), 28, font);
        idText.text = "شناسه: " + ProfileSystem.ToPersianDigits(ProfileSystem.UserId);
        idText.color = new Color(0.85f, 0.82f, 0.7f); idText.raycastTarget = false;

        // اسمِ فعلی
        nameText = RuntimeUIHelper.CreateRTLText(brt, "NameText", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.49f), 32, font);
        nameText.color = new Color(0.98f, 0.9f, 0.62f); nameText.fontStyle = FontStyles.Bold; nameText.raycastTarget = false;

        // دکمه‌ی تعویضِ نام (با هزینه)
        GameObject cn = RuntimeUIHelper.CreateButton(brt, "ChangeNameButton", new Vector2(0.18f, 0.30f), new Vector2(0.82f, 0.40f), "", font, new Color(0.3f, 0.45f, 0.2f, 1f), ShowNameInput);
        changeNameLabel = cn.GetComponentInChildren<RTLTextMeshPro>();

        // پیامِ کوچیک (خطا/موفقیت)
        msgText = RuntimeUIHelper.CreateRTLText(brt, "Msg", new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.29f), 24, font);
        msgText.color = new Color(0.95f, 0.6f, 0.5f); msgText.raycastTarget = false; msgText.text = "";

        // دکمه‌ی بستن (داخلِ کادر، بالای لبه‌ی پایین)
        RuntimeUIHelper.CreateButton(brt, "CloseButton", new Vector2(0.30f, 0.11f), new Vector2(0.70f, 0.20f), "بستن", font, new Color(0.4f, 0.25f, 0.2f, 1f), Close);

        RefreshTexts();
        panel.transform.SetAsLastSibling();
    }

    void RefreshTexts()
    {
        string pn = ProfileSystem.PlayerName;
        if (nameText != null) nameText.text = "نام: " + (string.IsNullOrEmpty(pn) ? "—" : pn);
        if (changeNameLabel != null)
            changeNameLabel.text = "تغییر نام — " + RialSystem.ToPersian(ProfileSystem.NextNameChangeCost) + " ریال";
        if (avatarImg != null) avatarImg.sprite = ProfileSystem.GetAvatarSprite();
    }

    void PrevAvatar()
    {
        ProfileSystem.SelectedAvatar = (ProfileSystem.SelectedAvatar - 1 + ProfileSystem.AvatarCount) % ProfileSystem.AvatarCount;
        RefreshTexts();
        onChanged?.Invoke();
    }

    void NextAvatar()
    {
        ProfileSystem.SelectedAvatar = (ProfileSystem.SelectedAvatar + 1) % ProfileSystem.AvatarCount;
        RefreshTexts();
        onChanged?.Invoke();
    }

    // فلشِ ساده (بدونِ قابِ دکمه) با گلیفِ متنیِ ASCII (< یا >) — از TextMeshProUGUIِ معمولی
    // (نه RTL) تا گلیف برعکس/بازچینش نشه.
    void AddArrowButton(RectTransform parent, string name, Vector2 aMin, Vector2 aMax, string glyph, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0.28f, 0.19f, 0.10f, 0.92f);
        go.GetComponent<Button>().onClick.AddListener(onClick);

        GameObject tGO = new GameObject("T", typeof(RectTransform));
        tGO.transform.SetParent(rt, false);
        RectTransform trt = (RectTransform)tGO.transform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = tGO.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = glyph; t.fontSize = 44; t.color = new Color(0.98f, 0.88f, 0.55f);
        t.alignment = TextAlignmentOptions.Center; t.fontStyle = FontStyles.Bold; t.raycastTarget = false;
    }

    // --- تعویضِ نام (با گرافیکِ اطلسِ «Name Change Box»: Main box / Text / OK / NO) ---
    void ShowNameInput()
    {
        if (inputOverlay != null) return;
        // پس‌زمینه‌ی تیره — با کلیک بسته نمی‌شه (فقط دکمه‌ی انصراف)
        inputOverlay = RuntimeUIHelper.CreateFullScreenPanel(panel.transform, "NameInputOverlay", new Color(0f, 0f, 0f, 0.75f));

        // اطلسِ گرافیک‌ها (Multiple) — با نام برش‌ها پیدا می‌شن
        Sprite[] atlas = Resources.LoadAll<Sprite>("UI/Name Change Box");
        Sprite sMain = FindSprite(atlas, "main box");
        Sprite sText = FindSprite(atlas, "text");
        Sprite sOK = FindSprite(atlas, "ok");
        Sprite sNO = FindSprite(atlas, "no");

        // باکسِ پس‌زمینه (از Main box؛ اگه نبود رنگِ ساده)
        GameObject b;
        if (sMain != null)
            b = RuntimeUIHelper.CreateImage(inputOverlay.transform, "Box", new Vector2(0.07f, 0.34f), new Vector2(0.93f, 0.66f), sMain, stretch: true);
        else
        {
            b = RuntimeUIHelper.CreateImage(inputOverlay.transform, "Box", new Vector2(0.07f, 0.34f), new Vector2(0.93f, 0.66f), null);
            b.GetComponent<Image>().color = new Color(0.14f, 0.09f, 0.04f, 0.99f);
        }
        b.GetComponent<Image>().raycastTarget = true;
        RectTransform brt = (RectTransform)b.transform;

        // عنوان: «نام جدید رو وارد کن» — اگه تصویرش بود تصویری، وگرنه متن
        if (sText != null)
        {
            GameObject tg = RuntimeUIHelper.CreateImage(brt, "Title", new Vector2(0.15f, 0.76f), new Vector2(0.85f, 0.94f), sText);
            tg.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(brt, "T", new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.95f), 26, font);
            t.text = "نامِ جدید رو وارد کن"; t.color = new Color(0.98f, 0.86f, 0.55f); t.raycastTarget = false;
        }

        // فیلدِ ورودی
        BuildInputField(brt, new Vector2(0.12f, 0.50f), new Vector2(0.88f, 0.72f));

        // پیامِ خطا داخلِ همین پنل (تا پشتِ overlay قایم نشه)
        inputMsg = RuntimeUIHelper.CreateRTLText(brt, "Msg", new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.48f), 22, font);
        inputMsg.color = new Color(0.95f, 0.6f, 0.5f); inputMsg.raycastTarget = false; inputMsg.text = "";

        // دکمه‌های تایید/انصراف (تصویری اگه بود، وگرنه متنی)
        AddImgOrTextButton(brt, "OK", sOK, "تأیید", new Vector2(0.52f, 0.08f), new Vector2(0.9f, 0.32f), ConfirmNameChange);
        AddImgOrTextButton(brt, "Cancel", sNO, "انصراف", new Vector2(0.1f, 0.08f), new Vector2(0.48f, 0.32f), CancelNameInput);
    }

    // دکمه: اگه اسپرایتش تو اطلس بود تصویری (متنِ روش رو خودِ عکس داره)، وگرنه دکمه‌ی متنی
    void AddImgOrTextButton(RectTransform parent, string name, Sprite spr, string label, Vector2 aMin, Vector2 aMax, UnityEngine.Events.UnityAction onClick)
    {
        if (spr != null)
            RuntimeUIHelper.CreateImageButton(parent, name, aMin, aMax, spr, onClick);
        else
            RuntimeUIHelper.CreateButton(parent, name, aMin, aMax, label, font, new Color(0.3f, 0.4f, 0.2f, 1f), onClick);
    }

    // پیدا کردنِ برشِ اطلس با نام (اول تطبیقِ دقیق، بعد شاملِ نام)
    static Sprite FindSprite(Sprite[] atlas, string lowerName)
    {
        if (atlas == null) return null;
        foreach (var s in atlas) if (s != null && s.name.ToLower() == lowerName) return s;
        foreach (var s in atlas) if (s != null && s.name.ToLower().Contains(lowerName)) return s;
        return null;
    }

    void CancelNameInput()
    {
        if (inputOverlay != null) { Destroy(inputOverlay); inputOverlay = null; nameInput = null; }
    }

    void ConfirmNameChange()
    {
        string entered = nameInput != null ? nameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(entered)) { SetInputMsg("اسم خالیه!"); return; }
        int cost = ProfileSystem.NextNameChangeCost;
        if (ProfileSystem.TryChangeName(entered))
        {
            CancelNameInput();
            RefreshTexts();
            onChanged?.Invoke();
            SetMsg("نام عوض شد. (" + RialSystem.ToPersian(cost) + " ریال کم شد)");
            msgText.color = new Color(0.6f, 0.9f, 0.6f);
        }
        else
        {
            // پنل باز می‌مونه؛ پیام رو روی خودِ پنل نشون بده (نه پشتش)
            SetInputMsg("ریالِ کافی نداری! (لازم: " + RialSystem.ToPersian(cost) + " ریال)");
        }
    }

    void SetInputMsg(string s) { if (inputMsg != null) inputMsg.text = s; }

    void SetMsg(string s) { if (msgText != null) msgText.text = s; }

    // فیلدِ ورودِ اسم با RTLTextMeshPro (فارسیِ درست)
    void BuildInputField(Transform parent, Vector2 aMin, Vector2 aMax)
    {
        GameObject inGO = new GameObject("NameInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        inGO.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)inGO.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
        inGO.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.02f, 1f);

        nameInput = inGO.AddComponent<TMP_InputField>();

        GameObject area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(inGO.transform, false);
        RectTransform art = (RectTransform)area.transform;
        art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(20f, 6f); art.offsetMax = new Vector2(-20f, -6f);

        GameObject phGO = new GameObject("Placeholder", typeof(RectTransform));
        phGO.transform.SetParent(area.transform, false);
        RTLTextMeshPro ph = phGO.AddComponent<RTLTextMeshPro>();
        StretchFull((RectTransform)phGO.transform);
        if (font != null) ph.font = font;
        ph.text = "نامِ جدید..."; ph.fontSize = 32; ph.color = new Color(0.75f, 0.68f, 0.5f, 0.75f);
        ph.alignment = TextAlignmentOptions.MidlineRight;

        GameObject txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(area.transform, false);
        RTLTextMeshPro txt = txtGO.AddComponent<RTLTextMeshPro>();
        StretchFull((RectTransform)txtGO.transform);
        if (font != null) txt.font = font;
        txt.fontSize = 32; txt.color = new Color(0.98f, 0.92f, 0.78f);
        txt.alignment = TextAlignmentOptions.MidlineRight;

        nameInput.textViewport = art;
        nameInput.textComponent = txt;
        nameInput.placeholder = ph;
        if (font != null) nameInput.fontAsset = font;
        nameInput.pointSize = 32;
        nameInput.characterLimit = 16;
        nameInput.lineType = TMP_InputField.LineType.SingleLine;
        nameInput.text = ProfileSystem.PlayerName;
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
