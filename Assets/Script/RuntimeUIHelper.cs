using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;
using System.Collections;

// چون بعضی صفحه‌های UI این پروژه (منوی اصلی، صفحه‌ی پایان بازی) موقع اجرا با کد ساخته می‌شن
// (نه از پیش تو صحنه)، این کلاس متدهای مشترک ساخت متن/دکمه/پنل رو بین‌شون به اشتراک می‌ذاره
public static class RuntimeUIHelper
{
    // انیمیشنِ سبکِ باز شدنِ یه پنل: محو (fade) + پرشِ ریزِ اندازه (۰.۹۶→۱). سبک و روان.
    // چون static نمی‌تونه Coroutine اجرا کنه، یه MonoBehaviour (host) می‌گیره.
    public static void PlayFadeIn(MonoBehaviour host, GameObject go, float duration = 0.2f)
    {
        if (host == null || go == null) return;
        host.StartCoroutine(FadeInRoutine(go, duration));
    }

    static IEnumerator FadeInRoutine(GameObject go, float duration)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        float t = 0f;
        cg.alpha = 0f;
        go.transform.localScale = Vector3.one * 0.96f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float e = 1f - (1f - p) * (1f - p); // ease-out
            cg.alpha = e;
            go.transform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, e);
            yield return null;
        }
        cg.alpha = 1f;
        go.transform.localScale = Vector3.one;
    }

    public static GameObject CreateFullScreenPanel(Transform parent, string name, Color backgroundColor)
    {
        GameObject panelGO = new GameObject(name, typeof(RectTransform));
        panelGO.transform.SetParent(parent, false);
        RectTransform rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panelGO.AddComponent<Image>().color = backgroundColor;
        return panelGO;
    }

    public static RTLTextMeshPro CreateRTLText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float fontSize, TMP_FontAsset font)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform));
        textGO.transform.SetParent(parent, false);
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RTLTextMeshPro text = textGO.AddComponent<RTLTextMeshPro>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        return text;
    }

    // قابِ آماده‌ی دکمه (Resources/UI/Button) که یه‌بار لود و کش می‌شه. اگه بود، همه‌ی دکمه‌های
    // متنی به‌جای رنگِ ساده این قابِ تزئینی رو می‌گیرن؛ اگه نبود به رنگِ ساده برمی‌گرده.
    static Sprite _btnFrame;
    static bool _btnFrameTried;
    static Sprite ButtonFrame()
    {
        if (!_btnFrameTried) { _btnFrame = Resources.Load<Sprite>("UI/Button"); _btnFrameTried = true; }
        return _btnFrame;
    }

    public static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label, TMP_FontAsset font, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform));
        buttonGO.transform.SetParent(parent, false);
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = buttonGO.AddComponent<Image>();
        Sprite frame = ButtonFrame();
        Color textColor = Color.white;
        if (frame != null)
        {
            img.sprite = frame;         // قابِ طلاییِ آماده
            img.type = Image.Type.Simple;
            img.color = Color.white;     // رنگِ واقعیِ تصویر
            textColor = new Color(0.98f, 0.9f, 0.62f); // متنِ کرمِ روشن روی قابِ تیره
        }
        else
        {
            img.color = backgroundColor; // حالتِ قدیمی: رنگِ ساده
        }
        buttonGO.AddComponent<Button>().onClick.AddListener(onClick);

        // متن یه‌کم از لبه‌های قاب فاصله بگیره تا رو تزئیناتِ گوشه نیفته
        RTLTextMeshPro t = CreateRTLText(buttonGO.transform, "Text", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f), 28, font);
        t.text = label;
        t.color = textColor;
        t.enableAutoSizing = true; t.fontSizeMin = 14; t.fontSizeMax = 30;

        return buttonGO;
    }

    // یه Image ساده (بدون دکمه) — برای پس‌زمینه، لوگو، آیکون شاخص‌ها و... که خودشون متن/برچسب دارن
    // stretch=true یعنی دقیقاً پر کن (برای پس‌زمینه‌ها)، false یعنی نسبت تصویر رو حفظ کن (برای لوگو/آیکون)
    public static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite, bool stretch = false)
    {
        GameObject imageGO = new GameObject(name, typeof(RectTransform));
        imageGO.transform.SetParent(parent, false);
        RectTransform rect = imageGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = imageGO.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = !stretch;
        return imageGO;
    }

    // دکمه‌ای که خودِ عکسش برچسب/متن رو داره (مثل «شروع بازی» یا «بازگشت» که از قبل رو تصویر نوشته شده)
    public static GameObject CreateImageButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonGO = CreateImage(parent, name, anchorMin, anchorMax, sprite);
        buttonGO.AddComponent<Button>().onClick.AddListener(onClick);
        return buttonGO;
    }
}
