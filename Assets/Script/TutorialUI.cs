using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System;

// بخشِ آموزشیِ اولِ بازی — «خانمِ مشاور» با دیالوگ مکانیک‌ها رو توضیح می‌ده.
// موقعِ اجرا با کد ساخته می‌شه (مثلِ بقیه‌ی UI)، تصاویرش از Resources لود می‌شن:
//   - کاراکتر: Resources/Characters/Advisor
//   - باکسِ دیالوگ: Resources/UI/DialogBox
// اگه هر کدوم نبود، نسخه‌ی ساده‌ی جایگزین نشون داده می‌شه تا بازی نخوابه.
public class TutorialUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private Action onComplete;

    private GameObject panel;
    private RTLTextMeshPro dialogText;
    private RTLTextMeshPro hintText;
    private int index;

    // اسمِ مشاور (اگه خواستی عوضش کن)
    private const string advisorName = "مشاورِ ارشد";

    // دیالوگ‌های آموزش — هر خط یه «صفحه»ی دیالوگه (با ضربه/کلیک می‌ره بعدی)
    private static readonly string[] lines =
    {
        "سلام، جنابِ رئیس‌جمهور. من مشاورِ ارشدِ شمام؛ از همین امروز کنارتونم. نگران نباشید، با هم پیش می‌ریم.",
        "کارتون ساده به‌نظر می‌رسه ولی نیست: قراره این مملکت رو اداره کنید. هر بار یه پرونده روی میزتون میاد.",
        "برای هر پرونده فقط دو راه دارید: بکشیدش به راست یعنی «بله»، بکشیدش به چپ یعنی «خیر». تصمیم با شماست.",
        "ولی هر تصمیم عاقبت داره. بالای صفحه چهار شاخص دارید: بودجه، محبوبیت، امنیت، و دیپلماسی.",
        "هر «بله» یا «خیر» این چهارتا رو کم و زیاد می‌کنه. بعد از هر انتخاب می‌بینید کدوم بالا رفت و کدوم پایین اومد.",
        "حواستون باشه: اگه هر کدوم از این شاخص‌ها به صفر یا به صد برسه، کارِ دولتتون تمومه. تعادل حرفِ اول رو می‌زنه.",
        "نه اون‌قدر به مردم قول بدید که خزانه خالی شه، نه اون‌قدر سخت بگیرید که کسی طرفتون نمونه. وسطش رو پیدا کنید.",
        "خب... وقتشه. اولین پرونده آماده‌ست. مملکت منتظرِ تصمیمِ شماست، جنابِ رئیس‌جمهور.",
    };

    public static TutorialUI Show(Canvas canvas, TMP_FontAsset font, Action onComplete)
    {
        if (canvas == null) { onComplete?.Invoke(); return null; }
        GameObject go = new GameObject("TutorialUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        TutorialUI ui = go.AddComponent<TutorialUI>();
        ui.canvas = canvas;
        ui.font = font;
        ui.onComplete = onComplete;
        ui.Build();
        return ui;
    }

    void Build()
    {
        Vector2 canvasSize = ((RectTransform)canvas.transform).rect.size;

        // پس‌زمینه‌ی تیره — با ضربه روی هرجاش، دیالوگ می‌ره صفحه‌ی بعد
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "TutorialPanel", new Color(0f, 0f, 0f, 0.88f));
        Button advanceBtn = panel.AddComponent<Button>();
        advanceBtn.transition = Selectable.Transition.None;
        advanceBtn.onClick.AddListener(Next);

        // کاراکترِ مشاور — پایین-چپ
        Sprite advisor = Resources.Load<Sprite>("Characters/Advisor");
        if (advisor != null)
        {
            float hFrac = 0.62f;
            float aspect = advisor.rect.width / advisor.rect.height;
            float wFrac = (hFrac * canvasSize.y * aspect) / canvasSize.x;
            GameObject a = RuntimeUIHelper.CreateImage(panel.transform, "Advisor",
                new Vector2(0.0f, 0.0f), new Vector2(wFrac, hFrac), advisor);
            a.GetComponent<Image>().raycastTarget = false; // تا ضربه به پنل برسه
        }

        // باکسِ دیالوگ — بالای صفحه
        Sprite box = Resources.Load<Sprite>("UI/DialogBox");
        RectTransform boxRT;
        if (box != null)
        {
            float w = 0.95f;
            float aspect = box.rect.width / box.rect.height;
            float hFrac = (w * canvasSize.x / aspect) / canvasSize.y;
            float yMax = 0.90f;
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox",
                new Vector2(0.5f - w / 2f, yMax - hFrac), new Vector2(0.5f + w / 2f, yMax), box);
            b.GetComponent<Image>().raycastTarget = false;
            boxRT = b.GetComponent<RectTransform>();
        }
        else
        {
            // جایگزینِ ساده اگه تصویرِ باکس نبود
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox", Vector2.zero, Vector2.one,
                null);
            Image bi = b.GetComponent<Image>();
            bi.sprite = null; bi.color = new Color(0.18f, 0.12f, 0.06f, 0.96f); bi.raycastTarget = false;
            RectTransform r = b.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.05f, 0.68f); r.anchorMax = new Vector2(0.95f, 0.9f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            boxRT = r;
        }

        // متنِ دیالوگ داخلِ باکس
        dialogText = RuntimeUIHelper.CreateRTLText(boxRT, "Line", new Vector2(0.07f, 0.32f), new Vector2(0.93f, 0.86f), 32, font);
        dialogText.color = new Color(0.96f, 0.90f, 0.78f); // کرمِ روشن رو زمینه‌ی تیره
        dialogText.enableAutoSizing = true;
        dialogText.fontSizeMin = 18;
        dialogText.fontSizeMax = 38;
        dialogText.raycastTarget = false;

        // اسمِ مشاور — بالای باکس، سمتِ راست
        RTLTextMeshPro nameText = RuntimeUIHelper.CreateRTLText(panel.transform, "Name",
            new Vector2(0.5f, 0.90f), new Vector2(0.96f, 0.955f), 30, font);
        nameText.text = advisorName;
        nameText.color = new Color(0.98f, 0.82f, 0.42f); // طلایی
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Right;
        nameText.raycastTarget = false;

        // راهنمای «بعدی» — پایین-چپِ باکس
        hintText = RuntimeUIHelper.CreateRTLText(boxRT, "Hint", new Vector2(0.07f, 0.06f), new Vector2(0.5f, 0.26f), 22, font);
        hintText.text = "برای ادامه بزن ›";
        hintText.color = new Color(0.85f, 0.78f, 0.6f, 0.9f);
        hintText.alignment = TextAlignmentOptions.Left;
        hintText.raycastTarget = false;

        // دکمه‌ی «رد کردن» — گوشه‌ی بالا-چپ
        RuntimeUIHelper.CreateButton(panel.transform, "SkipButton",
            new Vector2(0.03f, 0.94f), new Vector2(0.28f, 0.99f),
            "رد کردن ▸", font, new Color(0.3f, 0.3f, 0.3f, 0.7f), Finish);

        index = 0;
        panel.transform.SetAsLastSibling();
        ShowLine();
    }

    void ShowLine()
    {
        if (dialogText != null) dialogText.text = lines[index];
        // خطِ آخر: راهنما رو عوض کن
        if (hintText != null)
            hintText.text = (index >= lines.Length - 1) ? "شروع بازی ›" : "برای ادامه بزن ›";
    }

    void Next()
    {
        index++;
        if (index >= lines.Length) { Finish(); return; }
        ShowLine();
    }

    void Finish()
    {
        Action cb = onComplete;
        onComplete = null; // جلوگیری از دوباره صدا خوردن (رد کردن + پایان هم‌زمان)
        if (panel != null) Destroy(panel);
        Destroy(gameObject);
        cb?.Invoke();
    }
}
