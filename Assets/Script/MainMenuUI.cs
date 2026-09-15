using UnityEngine;
using RTLTMPro;
using TMPro;
using System.Collections.Generic;

// منوی اصلی بازی رو موقع اجرا با کد می‌سازه (مثل GameOverUI، چون امکان ساختنش تو Editor نبود)
// و بر اساس اینکه فایل سیو وجود داره یا نه، دکمه‌ی «ادامه‌ی بازی» رو نشون می‌ده یا نه
public class MainMenuUI : MonoBehaviour
{
    [Header("فونت فارسی")]
    [SerializeField] private TMP_FontAsset persianFont;

    [Header("Canvas اصلی صحنه (همونی که نوارهای وضعیت زیرشن)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("آبجکت کارت (برای شروع بازی بعد از بستن منو)")]
    [SerializeField] private CardSwipe cardSwipe;

    private GameObject panel;

    void Start()
    {
        BuildUI();
    }

    void StartNewGame()
    {
        SaveSystem.DeleteSave();
        GameStats.Instance.ResetState();
        CardDatabase.Instance.ClearFlags();
        panel.SetActive(false);
        cardSwipe.BeginGame();
    }

    void ContinueGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        GameStats.Instance.LoadFromSaveData(data);
        CardDatabase.Instance.SetActiveFlags(data.activeFlags ?? new List<string>());
        panel.SetActive(false);
        cardSwipe.BeginGame();
    }

    void BuildUI()
    {
        panel = RuntimeUIHelper.CreateFullScreenPanel(targetCanvas.transform, "MainMenuPanel", new Color(0.05f, 0.05f, 0.08f, 1f));

        RTLTextMeshPro title = RuntimeUIHelper.CreateRTLText(panel.transform, "Title", new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.85f), 48, persianFont);
        title.text = "دولت شو";

        RuntimeUIHelper.CreateButton(panel.transform, "NewGameButton", new Vector2(0.25f, 0.42f), new Vector2(0.75f, 0.52f), "بازی جدید", persianFont, new Color(0.2f, 0.45f, 0.6f, 1f), StartNewGame);

        GameObject continueButton = RuntimeUIHelper.CreateButton(panel.transform, "ContinueButton", new Vector2(0.25f, 0.28f), new Vector2(0.75f, 0.38f), "ادامه‌ی بازی", persianFont, new Color(0.2f, 0.45f, 0.6f, 1f), ContinueGame);
        continueButton.SetActive(SaveSystem.HasSave());
    }
}
