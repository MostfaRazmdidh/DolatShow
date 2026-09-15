using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RTLTMPro;
using TMPro;

// این اسکریپت کل صفحه‌ی پایان بازی (باخت/پیروزی) رو موقع اجرا با کد می‌سازه،
// چون امکان ساختن دستی آبجکت‌های UI تو خودِ Unity Editor نبود.
// کافیه این اسکریپت رو به یه آبجکت تو صحنه (مثلاً GameManager) اضافه کنی و
// دو فیلد Persian Font و Target Canvas رو تو Inspector پر کنی (پایین توضیح داده شده).
public class GameOverUI : MonoBehaviour
{
    [Header("فونت فارسی (همون NotoNaskhArabic-Regular SDF که برای کارت‌ها استفاده می‌شه)")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن، نه Canvas داخل کارت)")]
    [SerializeField] private Canvas targetCanvas;

    private GameObject panel;
    private RTLTextMeshPro messageText;

    void Start()
    {
        BuildUI();
        GameStats.Instance.OnGameLost += HandleGameLost;
        GameStats.Instance.OnGameWon += HandleGameWon;
    }

    void OnDestroy()
    {
        if (GameStats.Instance != null)
        {
            GameStats.Instance.OnGameLost -= HandleGameLost;
            GameStats.Instance.OnGameWon -= HandleGameWon;
        }
    }

    void HandleGameLost(GameStats.StatType type, bool hitMax)
    {
        messageText.text = GetLossMessage(type, hitMax);
        panel.SetActive(true);
    }

    void HandleGameWon()
    {
        messageText.text = "چهار سال ریاست‌جمهوری‌ت با موفقیت به پایان رسید!";
        panel.SetActive(true);
    }

    // پیام‌های ساده‌ی هر ۸ نوع باخت — بعداً موقع نوشتن محتوای اصلی می‌شه با متن بهتر جایگزین‌شون کرد
    string GetLossMessage(GameStats.StatType type, bool hitMax)
    {
        switch (type)
        {
            case GameStats.StatType.Budget:
                return hitMax
                    ? "ابرتورم! چاپ بی‌رویه‌ی پول اقتصاد کشور رو نابود کرد."
                    : "ورشکستگی! خزانه‌ی دولت کاملاً خالی شد.";
            case GameStats.StatType.Popularity:
                return hitMax
                    ? "هرج‌ومرج پوپولیستی! وعده‌های بیش‌ازحد، اوضاع رو از کنترل خارج کرد."
                    : "استعفای اجباری! مردم دیگه بهت اعتماد ندارن.";
            case GameStats.StatType.Security:
                return hitMax
                    ? "دولت پلیسی! سرکوب بیش‌ازحد، نظم اجتماعی رو از بین برد."
                    : "هرج‌ومرج! امنیت داخلی کشور کاملاً فروپاشید.";
            default: // Diplomacy
                return hitMax
                    ? "وابستگی کامل به خارج! کشور استقلالش رو از دست داد."
                    : "انزوای بین‌المللی! کشور از همه‌ی متحدانش بریده شد.";
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // کل ساختار صفحه‌ی پایان بازی (پس‌زمینه‌ی تیره + متن + دکمه‌ی شروع دوباره) رو با کد می‌سازه
    void BuildUI()
    {
        GameObject panelGO = new GameObject("GameOverPanel", typeof(RectTransform));
        panelGO.transform.SetParent(targetCanvas.transform, false);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        messageText = CreateRTLText(panelGO.transform, "Message", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.8f), 36);

        GameObject buttonGO = new GameObject("RestartButton", typeof(RectTransform));
        buttonGO.transform.SetParent(panelGO.transform, false);
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.15f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.25f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        buttonGO.AddComponent<Image>().color = new Color(0.2f, 0.55f, 0.25f, 1f);
        buttonGO.AddComponent<Button>().onClick.AddListener(RestartGame);

        RTLTextMeshPro buttonText = CreateRTLText(buttonGO.transform, "Text", Vector2.zero, Vector2.one, 28);
        buttonText.text = "دوباره بازی کن";

        panel = panelGO;
        panel.SetActive(false);
    }

    RTLTextMeshPro CreateRTLText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform));
        textGO.transform.SetParent(parent, false);
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RTLTextMeshPro text = textGO.AddComponent<RTLTextMeshPro>();
        text.font = persianFont;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        return text;
    }
}
