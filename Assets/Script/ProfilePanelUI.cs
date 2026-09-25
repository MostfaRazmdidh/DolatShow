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
        // پس‌زمینه‌ی تیره؛ کلیک روی فضای خالی → بستن
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "ProfilePanel", new Color(0f, 0f, 0f, 0.8f));
        Button overlay = panel.AddComponent<Button>();
        overlay.transition = Selectable.Transition.None;
        overlay.onClick.AddListener(Close);

        // باکسِ وسط (از StartBox استفاده می‌کنیم؛ اگه نبود رنگِ ساده)
        Sprite boxSpr = Resources.Load<Sprite>("UI/StartBox");
        GameObject box;
        if (boxSpr != null)
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.74f), boxSpr, stretch: true);
        else
        {
            box = RuntimeUIHelper.CreateImage(panel.transform, "Box", new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.74f), null);
            box.GetComponent<Image>().color = new Color(0.16f, 0.10f, 0.05f, 0.98f);
        }
        // کلیک روی خودِ باکس نباید ببنده
        box.GetComponent<Image>().raycastTarget = true;

        RectTransform brt = (RectTransform)box.transform;

        // عنوان
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(brt, "Title", new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.98f), 40, font);
        title.text = "پروفایل"; title.color = new Color(0.98f, 0.88f, 0.55f); title.fontStyle = FontStyles.Bold; title.raycastTarget = false;

        // آواتار (بالا-وسط)
        GameObject av = RuntimeUIHelper.CreateImage(brt, "Avatar", new Vector2(0.36f, 0.58f), new Vector2(0.64f, 0.86f), ProfileSystem.GetAvatarSprite());
        avatarImg = av.GetComponent<Image>(); avatarImg.raycastTarget = false;

        // دکمه‌ی تعویضِ آواتار (رایگان، چرخشی)
        RuntimeUIHelper.CreateButton(brt, "AvatarButton", new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.57f), "تغییر آواتار", font, new Color(0.2f, 0.4f, 0.5f, 1f), CycleAvatar);

        // شناسه
        RTLTextMeshPro idText = RuntimeUIHelper.CreateRTLText(brt, "IdText", new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.48f), 28, font);
        idText.text = "شناسه: " + ProfileSystem.ToPersianDigits(ProfileSystem.UserId);
        idText.color = new Color(0.85f, 0.82f, 0.7f); idText.raycastTarget = false;

        // اسمِ فعلی
        nameText = RuntimeUIHelper.CreateRTLText(brt, "NameText", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.4f), 32, font);
        nameText.color = new Color(0.98f, 0.9f, 0.62f); nameText.fontStyle = FontStyles.Bold; nameText.raycastTarget = false;

        // دکمه‌ی تعویضِ نام (با هزینه)
        GameObject cn = RuntimeUIHelper.CreateButton(brt, "ChangeNameButton", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.28f), "", font, new Color(0.3f, 0.45f, 0.2f, 1f), ShowNameInput);
        changeNameLabel = cn.GetComponentInChildren<RTLTextMeshPro>();

        // پیامِ کوچیک (خطا/موفقیت)
        msgText = RuntimeUIHelper.CreateRTLText(brt, "Msg", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.19f), 24, font);
        msgText.color = new Color(0.95f, 0.6f, 0.5f); msgText.raycastTarget = false; msgText.text = "";

        // دکمه‌ی بستن
        RuntimeUIHelper.CreateButton(brt, "CloseButton", new Vector2(0.35f, 0.03f), new Vector2(0.65f, 0.11f), "بستن", font, new Color(0.4f, 0.25f, 0.2f, 1f), Close);

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

    void CycleAvatar()
    {
        ProfileSystem.SelectedAvatar = (ProfileSystem.SelectedAvatar + 1) % ProfileSystem.AvatarCount;
        RefreshTexts();
        onChanged?.Invoke();
    }

    // --- تعویضِ نام ---
    void ShowNameInput()
    {
        if (inputOverlay != null) return;
        inputOverlay = RuntimeUIHelper.CreateFullScreenPanel(panel.transform, "NameInputOverlay", new Color(0f, 0f, 0f, 0.6f));
        Button ob = inputOverlay.AddComponent<Button>();
        ob.transition = Selectable.Transition.None;
        ob.onClick.AddListener(CancelNameInput);

        // یه باکسِ ساده برای ورودِ نام
        GameObject b = RuntimeUIHelper.CreateImage(inputOverlay.transform, "Box", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.58f), null);
        b.GetComponent<Image>().color = new Color(0.14f, 0.09f, 0.04f, 0.99f);

        RectTransform brt = (RectTransform)b.transform;
        RTLTextMeshPro t = RuntimeUIHelper.CreateRTLText(brt, "T", new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.98f), 26, font);
        t.text = "نامِ جدید رو وارد کن"; t.color = new Color(0.98f, 0.86f, 0.55f); t.raycastTarget = false;

        BuildInputField(brt, new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.68f));

        RuntimeUIHelper.CreateButton(brt, "OK", new Vector2(0.55f, 0.04f), new Vector2(0.94f, 0.3f), "تأیید", font, new Color(0.2f, 0.5f, 0.25f, 1f), ConfirmNameChange);
        RuntimeUIHelper.CreateButton(brt, "Cancel", new Vector2(0.06f, 0.04f), new Vector2(0.45f, 0.3f), "انصراف", font, new Color(0.45f, 0.25f, 0.2f, 1f), CancelNameInput);
    }

    void CancelNameInput()
    {
        if (inputOverlay != null) { Destroy(inputOverlay); inputOverlay = null; nameInput = null; }
    }

    void ConfirmNameChange()
    {
        string entered = nameInput != null ? nameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(entered)) { SetMsg("اسم خالیه!"); return; }
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
            SetMsg("ریالِ کافی نداری! (لازم: " + RialSystem.ToPersian(cost) + ")");
            msgText.color = new Color(0.95f, 0.6f, 0.5f);
        }
    }

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
