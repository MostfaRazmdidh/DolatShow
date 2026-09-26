using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

// صفحه‌ی فروشگاه — کد‌محور (مثلِ بقیه‌ی UI). آیتم‌ها از StoreSystem.Catalog میان و با «ریال» خریده می‌شن.
// پس‌زمینه: Resources/UI/Store background. UIِ نهایی/آیکون‌ها بعداً کامل می‌شه.
public class StoreUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private GameObject panel;
    private RTLTextMeshPro rialText;
    private RTLTextMeshPro msgText;
    private Transform rowsParent;

    static readonly Color GOLD = new Color(0.98f, 0.88f, 0.55f);
    static readonly Color CREAM = new Color(0.92f, 0.86f, 0.72f);

    public static StoreUI Open(Canvas canvas, TMP_FontAsset font)
    {
        if (canvas == null) return null;
        GameObject go = new GameObject("StoreUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        StoreUI ui = go.AddComponent<StoreUI>();
        ui.canvas = canvas; ui.font = font;
        ui.Build();
        return ui;
    }

    void Build()
    {
        // پس‌زمینه‌ی فروشگاه (تمام‌صفحه). اگه نبود، پنلِ تیره‌ی ساده.
        Sprite bg = Resources.Load<Sprite>("UI/Store background");
        if (bg != null)
            panel = RuntimeUIHelper.CreateImage(canvas.transform, "StorePanel", Vector2.zero, Vector2.one, bg, stretch: true);
        else
            panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "StorePanel", new Color(0.08f, 0.06f, 0.04f, 1f));
        panel.GetComponent<Image>().raycastTarget = true;
        RectTransform prt = (RectTransform)panel.transform;

        // عنوان (رو تابلوی بالای پس‌زمینه)
        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(prt, "Title", new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.95f), 46, font);
        title.text = "فروشگاه"; title.color = GOLD; title.fontStyle = FontStyles.Bold; title.raycastTarget = false;

        // موجودیِ ریال
        rialText = RuntimeUIHelper.CreateRTLText(prt, "Rial", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.875f), 30, font);
        rialText.color = CREAM; rialText.raycastTarget = false;

        // پیامِ کوتاه (خرید موفق / ریال کم)
        msgText = RuntimeUIHelper.CreateRTLText(prt, "Msg", new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.82f), 24, font);
        msgText.raycastTarget = false; msgText.text = "";

        // ظرفِ ردیفِ آیتم‌ها (وسطِ صفحه) — یه پس‌زمینه‌ی نیمه‌تیره پشتش برای خوانایی
        GameObject listBg = RuntimeUIHelper.CreateImage(prt, "ListBg", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.76f), null);
        listBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
        rowsParent = listBg.transform;

        BuildRows();

        // دکمه‌ی بستن
        RuntimeUIHelper.CreateButton(prt, "CloseButton", new Vector2(0.32f, 0.06f), new Vector2(0.68f, 0.13f), "بستن", font, new Color(0.4f, 0.25f, 0.2f, 1f), Close);

        panel.transform.SetAsLastSibling();
        RefreshRial();
        RuntimeUIHelper.PlayFadeIn(this, panel);
    }

    void BuildRows()
    {
        // ردیف‌های قبلی رو پاک کن (برای رفرش بعد از خرید)
        for (int i = rowsParent.childCount - 1; i >= 0; i--)
            Destroy(rowsParent.GetChild(i).gameObject);

        var items = StoreSystem.Catalog;
        int n = items.Count;
        float pad = 0.012f;
        for (int i = 0; i < n; i++)
        {
            var item = items[i];
            float rowTop = 1f - (float)i / n - pad;
            float rowBot = 1f - (float)(i + 1) / n + pad;

            GameObject row = RuntimeUIHelper.CreateImage(rowsParent, "Row_" + item.id,
                new Vector2(0.02f, rowBot), new Vector2(0.98f, rowTop), null);
            row.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.04f, 0.55f);
            RectTransform rrt = (RectTransform)row.transform;

            // اسم + تعداد/مالکیت (خطِ بالا، راست‌چین)
            string suffix = "";
            if (item.kind == StoreSystem.Kind.Consumable && StoreSystem.GetCount(item.id) > 0)
                suffix = "  (×" + ProfileSystem.ToPersianDigits(StoreSystem.GetCount(item.id).ToString()) + ")";
            RTLTextMeshPro name = RuntimeUIHelper.CreateRTLText(rrt, "Name", new Vector2(0.34f, 0.5f), new Vector2(0.98f, 0.98f), 28, font);
            name.text = item.title + suffix; name.color = GOLD; name.fontStyle = FontStyles.Bold;
            name.alignment = TextAlignmentOptions.MidlineRight; name.raycastTarget = false;

            // توضیح (خطِ پایین، راست‌چین، کوچیک‌تر)
            RTLTextMeshPro desc = RuntimeUIHelper.CreateRTLText(rrt, "Desc", new Vector2(0.34f, 0.04f), new Vector2(0.98f, 0.5f), 20, font);
            desc.text = item.description; desc.color = CREAM;
            desc.alignment = TextAlignmentOptions.MidlineRight; desc.raycastTarget = false;
            desc.enableAutoSizing = true; desc.fontSizeMin = 14; desc.fontSizeMax = 20;

            // دکمه‌ی خرید (سمتِ چپ)
            bool ownedPermanent = item.kind == StoreSystem.Kind.Permanent && StoreSystem.IsOwned(item.id);
            string label = ownedPermanent ? "خریداری‌شده" : (RialSystem.ToPersian(item.price) + " ریال");
            string capturedId = item.id;
            GameObject buy = RuntimeUIHelper.CreateButton(rrt, "Buy", new Vector2(0.02f, 0.15f), new Vector2(0.32f, 0.85f),
                label, font, new Color(0.3f, 0.4f, 0.2f, 1f), () => Buy(capturedId));
            if (ownedPermanent)
            {
                // دکمه‌ی غیرفعال (فقط نشون‌دهنده)
                Button b = buy.GetComponent<Button>();
                if (b != null) b.interactable = false;
            }
        }
    }

    void Buy(string id)
    {
        var result = StoreSystem.Buy(id);
        var item = StoreSystem.Get(id);
        switch (result)
        {
            case StoreSystem.BuyResult.Success:
                SetMsg("خریدی! «" + item.title + "» اضافه شد.", new Color(0.6f, 0.9f, 0.6f));
                break;
            case StoreSystem.BuyResult.NotEnoughRial:
                SetMsg("ریالِ کافی نداری! (لازم: " + RialSystem.ToPersian(item.price) + ")", new Color(0.95f, 0.6f, 0.5f));
                break;
            case StoreSystem.BuyResult.AlreadyOwned:
                SetMsg("این رو قبلاً خریدی.", new Color(0.9f, 0.8f, 0.5f));
                break;
        }
        RefreshRial();
        BuildRows();
    }

    void RefreshRial()
    {
        if (rialText != null) rialText.text = "موجودی: " + RialSystem.TotalPersian() + " ریال";
    }

    void SetMsg(string s, Color c) { if (msgText != null) { msgText.text = s; msgText.color = c; } }

    void Close()
    {
        Destroy(gameObject);
        Destroy(panel);
    }
}
