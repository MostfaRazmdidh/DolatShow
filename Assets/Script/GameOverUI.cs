using UnityEngine;
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

    // چون بعد از این دکمه به منوی اصلی برمی‌گردیم (نه مستقیم بازی جدید)، سیو رو نگه می‌داریم
    // مگر اینکه از قبل به‌خاطر باخت پاک شده باشه (تو CardSwipe انجام می‌شه)
    void BackToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // کل ساختار صفحه‌ی پایان بازی (پس‌زمینه‌ی تیره + متن + دکمه‌ی بازگشت) رو با کد می‌سازه
    void BuildUI()
    {
        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.85f));

        messageText = RuntimeUIHelper.CreateRTLText(panel.transform, "Message", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.8f), 36, persianFont);

        RuntimeUIHelper.CreateButton(panel.transform, "BackToMenuButton", new Vector2(0.3f, 0.15f), new Vector2(0.7f, 0.25f), "بازگشت به منو", persianFont, new Color(0.2f, 0.55f, 0.25f, 1f), BackToMenu);

        panel.SetActive(false);
    }
}
