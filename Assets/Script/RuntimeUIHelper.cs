using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

// چون بعضی صفحه‌های UI این پروژه (منوی اصلی، صفحه‌ی پایان بازی) موقع اجرا با کد ساخته می‌شن
// (نه از پیش تو صحنه)، این کلاس متدهای مشترک ساخت متن/دکمه/پنل رو بین‌شون به اشتراک می‌ذاره
public static class RuntimeUIHelper
{
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

    public static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label, TMP_FontAsset font, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform));
        buttonGO.transform.SetParent(parent, false);
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        buttonGO.AddComponent<Image>().color = backgroundColor;
        buttonGO.AddComponent<Button>().onClick.AddListener(onClick);

        CreateRTLText(buttonGO.transform, "Text", Vector2.zero, Vector2.one, 28, font).text = label;

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
