using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System;

// بخشِ آموزشیِ اولِ بازی — «خانم رستمی» (مشاورِ ارشد) با دیالوگ و صدا مکانیک‌ها رو توضیح می‌ده.
// موقعِ اجرا با کد ساخته می‌شه؛ همه‌چیز از Resources لود می‌شه (بدونِ وایرینگِ صحنه):
//   - کاراکتر: Resources/Characters/Advisor
//   - باکسِ دیالوگ: Resources/UI/DialogBox
//   - دکمه‌ی رد کردن: Resources/UI/Reject
//   - صدای هر خط: Resources/Voice/Dialogue/Dialog_1 .. Dialog_8
// متنِ روی صفحه باید دقیقاً با صداها یکی باشه.
public class TutorialUI : MonoBehaviour
{
    private TMP_FontAsset font;
    private Canvas canvas;
    private Action onComplete;

    private GameObject panel;
    private RTLTextMeshPro dialogText;
    private RTLTextMeshPro hintText;
    private AudioSource audioSrc;
    private AudioClip[] clips;
    private int index;

    // اسمِ مشاور
    private const string advisorName = "خانم رستمی";

    // بزرگ‌ترکردنِ باکس (چون متن‌ها جا نمی‌شدن) — ضریبِ ارتفاعِ باکس نسبت به نسبتِ اصلیِ تصویر
    private const float boxWidthFrac = 0.97f;
    private const float boxHeightMultiplier = 1.55f;

    // دیالوگ‌های آموزش (هماهنگ با فایل‌های صوتی Dialog_1..Dialog_8). هر خط یه «صفحه»ست.
    private static readonly string[] lines =
    {
        "سلام، جنابِ رئیس‌جمهور. من رستمی‌ام، مشاورِ ارشدِ شما. از همین امروز کنارتونم.",
        "الان فروردینه و ما وسطِ کمپینِ انتخاباتیم. تا رسیدن به اون کرسی، راهِ سختی در پیش داریم.",
        "کارِ ما اینه: هر بار یه موضوع می‌آد جلوتون و باید موضع بگیرید. کارت رو بکشید به راست یعنی بله، به چپ یعنی خیر.",
        "بالای صفحه چهار شاخص می‌بینید: بودجه، محبوبیت، امنیت، و دیپلماسی. این‌ها وضعیت شما رو نشون می‌دن.",
        "هر تصمیمی که بگیرید، این چهار شاخص کم یا زیاد می‌شن. بعد از هر انتخاب، تغییرش رو کنارِ همون شاخص می‌بینید.",
        "حواستون باشه: اگه هر کدوم از این شاخص‌ها به صفر یا به صد برسه، کار تمومه. باید هر چهارتا رو متعادل نگه دارید.",
        "سعی کنید بینِ خواسته‌ی مردم و توانِ واقعیِ کشور تعادل برقرار کنید؛ نه همه رو ناامید کنید، نه بیش از حد قول بدید.",
        "خب، اولین موضوع آماده‌ست. تصمیم با شماست، قربان. من همین‌جا کنارتونم.",
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

        // AudioSource برای پخشِ صدای دیالوگ‌ها + بارگذاریِ کلیپ‌ها از Resources
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        clips = new AudioClip[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            clips[i] = Resources.Load<AudioClip>($"Voice/Dialogue/Dialog_{i + 1}");

        // پس‌زمینه‌ی تیره — با ضربه روی هرجاش، دیالوگ می‌ره صفحه‌ی بعد
        panel = RuntimeUIHelper.CreateFullScreenPanel(canvas.transform, "TutorialPanel", new Color(0f, 0f, 0f, 0.88f));
        Button advanceBtn = panel.AddComponent<Button>();
        advanceBtn.transition = Selectable.Transition.None;
        advanceBtn.onClick.AddListener(Next);

        // کاراکترِ مشاور — پایین-چپ
        Sprite advisor = Resources.Load<Sprite>("Characters/Advisor");
        if (advisor != null)
        {
            float hFrac = 0.60f;
            float aspect = advisor.rect.width / advisor.rect.height;
            float wFrac = (hFrac * canvasSize.y * aspect) / canvasSize.x;
            GameObject a = RuntimeUIHelper.CreateImage(panel.transform, "Advisor",
                new Vector2(0.0f, 0.0f), new Vector2(wFrac, hFrac), advisor);
            a.GetComponent<Image>().raycastTarget = false; // تا ضربه به پنل برسه
        }

        // باکسِ دیالوگ — بالای صفحه (بزرگ‌تر شده تا متن‌ها جا شن)
        Sprite box = Resources.Load<Sprite>("UI/DialogBox");
        RectTransform boxRT;
        if (box != null)
        {
            float w = boxWidthFrac;
            float aspect = box.rect.width / box.rect.height;
            float hFrac = (w * canvasSize.x / aspect) / canvasSize.y * boxHeightMultiplier;
            float yMax = 0.93f;
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox",
                new Vector2(0.5f - w / 2f, yMax - hFrac), new Vector2(0.5f + w / 2f, yMax), box);
            Image bimg = b.GetComponent<Image>();
            bimg.preserveAspect = false; // اجازه‌ی کشیدگیِ عمودی تا باکس بزرگ‌تر شه
            bimg.raycastTarget = false;
            boxRT = b.GetComponent<RectTransform>();
        }
        else
        {
            GameObject b = RuntimeUIHelper.CreateImage(panel.transform, "DialogBox", Vector2.zero, Vector2.one, null);
            Image bi = b.GetComponent<Image>();
            bi.sprite = null; bi.color = new Color(0.18f, 0.12f, 0.06f, 0.96f); bi.raycastTarget = false;
            RectTransform r = b.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.04f, 0.63f); r.anchorMax = new Vector2(0.96f, 0.93f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            boxRT = r;
        }

        // متنِ دیالوگ داخلِ باکس — ناحیه‌ی بزرگ‌تر + اندازه‌ی خودکار تا هیچ‌وقت سرریز نشه
        dialogText = RuntimeUIHelper.CreateRTLText(boxRT, "Line", new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.86f), 34, font);
        dialogText.color = new Color(0.96f, 0.90f, 0.78f); // کرمِ روشن رو زمینه‌ی تیره
        dialogText.enableAutoSizing = true;
        dialogText.fontSizeMin = 20;
        dialogText.fontSizeMax = 46;
        dialogText.raycastTarget = false;

        // اسمِ مشاور — بالای باکس، سمتِ راست
        RTLTextMeshPro nameText = RuntimeUIHelper.CreateRTLText(panel.transform, "Name",
            new Vector2(0.5f, 0.935f), new Vector2(0.96f, 0.985f), 32, font);
        nameText.text = advisorName;
        nameText.color = new Color(0.98f, 0.82f, 0.42f); // طلایی
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Right;
        nameText.raycastTarget = false;

        // راهنمای «ادامه» — پایین-چپِ باکس
        hintText = RuntimeUIHelper.CreateRTLText(boxRT, "Hint", new Vector2(0.08f, 0.05f), new Vector2(0.55f, 0.24f), 22, font);
        hintText.color = new Color(0.85f, 0.78f, 0.6f, 0.9f);
        hintText.alignment = TextAlignmentOptions.Left;
        hintText.raycastTarget = false;

        // دکمه‌ی «رد کردن» (تصویرِ Reject) — گوشه‌ی بالا-چپ
        Sprite skip = Resources.Load<Sprite>("UI/Reject");
        if (skip != null)
        {
            float hB = 0.06f;
            float aspB = skip.rect.width / skip.rect.height;
            float wB = (hB * canvasSize.y * aspB) / canvasSize.x;
            float top = 0.985f, left = 0.03f;
            RuntimeUIHelper.CreateImageButton(panel.transform, "SkipButton",
                new Vector2(left, top - hB), new Vector2(left + wB, top), skip, Finish);
        }
        else
        {
            RuntimeUIHelper.CreateButton(panel.transform, "SkipButton",
                new Vector2(0.03f, 0.94f), new Vector2(0.26f, 0.99f),
                "رد کردن", font, new Color(0.3f, 0.3f, 0.3f, 0.7f), Finish);
        }

        index = 0;
        panel.transform.SetAsLastSibling();
        ShowLine();
    }

    void ShowLine()
    {
        if (dialogText != null) dialogText.text = lines[index];
        if (hintText != null)
            hintText.text = (index >= lines.Length - 1) ? "شروع بازی ›" : "برای ادامه بزن ›";

        // پخشِ صدای این خط
        if (audioSrc != null && clips != null && index < clips.Length && clips[index] != null)
        {
            audioSrc.Stop();
            audioSrc.clip = clips[index];
            audioSrc.Play();
        }
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
        if (audioSrc != null) audioSrc.Stop();
        if (panel != null) Destroy(panel);
        Destroy(gameObject);
        cb?.Invoke();
    }
}
