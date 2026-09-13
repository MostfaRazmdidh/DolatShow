using UnityEngine;
using UnityEngine.UI;
using TMPro;

// این اسکریپت رو روی هر کدوم از ۴ آبجکت نوار وضعیت می‌ذاریم
// و تو Inspector مشخص می‌کنیم این نوار مال کدوم شاخصه
public class StatBarUI : MonoBehaviour
{
    [SerializeField] private GameStats.StatType statType;
    [SerializeField] private Slider slider; // یه UI Slider معمولی، فقط برای نمایش (تعاملی نیست)

    [Header("پیش‌نمایش اثر تصمیم (اختیاری — موقع کشیدن کارت نشون داده می‌شه)")]
    [SerializeField] private TMP_Text hintText; // یه متن کوچیک کنار نوار، مثلاً +۱۵ یا -۲۰ (نیازی به RTL نداره چون فقط عدد و علامته)

    public GameStats.StatType StatType => statType;

    void Start()
    {
        // مقدار اولیه رو بگیر و نمایش بده
        slider.value = GameStats.Instance.GetStat(statType);

        // از این به بعد هر تغییری رخ بده خودکار آپدیت می‌شه
        GameStats.Instance.OnStatChanged += HandleStatChanged;
    }

    void OnDestroy()
    {
        // جلوگیری از خطای احتمالی وقتی صحنه عوض می‌شه
        if (GameStats.Instance != null)
            GameStats.Instance.OnStatChanged -= HandleStatChanged;
    }

    void HandleStatChanged(GameStats.StatType changedType, int newValue)
    {
        if (changedType != statType) return; // این نوار فقط به شاخص خودش واکنش نشون می‌ده
        slider.value = newValue;
    }

    // موقع کشیدن کارت صدا زده می‌شه تا نشون بده اگه همین الان رها کنی، این شاخص چقدر تغییر می‌کنه
    public void ShowHint(int amount)
    {
        if (hintText == null) return;

        if (amount == 0)
        {
            HideHint();
            return;
        }

        hintText.gameObject.SetActive(true);
        hintText.text = (amount > 0 ? "+" : "") + amount;
        hintText.color = amount > 0 ? Color.green : Color.red;
    }

    public void HideHint()
    {
        if (hintText != null) hintText.gameObject.SetActive(false);
    }
}
