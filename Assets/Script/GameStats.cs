using System;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    public enum StatType { Budget, Popularity, Security, Diplomacy }

    [Header("مقدار اولیه‌ی شاخص‌ها")]
    [SerializeField] private int startValue = 50;

    [Header("طول بازی (ماه)")]
    [SerializeField] private int totalMonths = 48;

    public int Budget { get; private set; }
    public int Popularity { get; private set; }
    public int Security { get; private set; }
    public int Diplomacy { get; private set; }

    public int CurrentMonth { get; private set; } = 1;
    public int TotalMonths => totalMonths;
    public bool IsGameOver { get; private set; } = false;
    public bool AdUsedThisRun { get; private set; } = false; // فعلاً فقط تو حافظه‌ست، تو سیو ذخیره نمی‌شه

    // هر بار شاخصی تغییر کنه صدا زده می‌شه — UI بهش گوش می‌ده تا خودکار آپدیت بشه
    public event Action<StatType, int> OnStatChanged;

    // وقتی شاخصی دقیقاً به ۰ یا ۱۰۰ برسه (مرز باخت)
    public event Action<StatType, bool> OnStatCritical; // bool: true = رسیده به ۱۰۰, false = رسیده به ۰

    public event Action<int> OnMonthChanged;
    public event Action OnGameWon;
    public event Action<StatType, bool> OnGameLost; // کدوم شاخص باعث باخت شد، و به کدوم مرز رسید

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // نکته‌ی مهم (رفعِ باگِ «دکمه‌ی خانه به منو نمی‌ره»):
        // کلِ بازی تو یه صحنه‌ست (SampleScene) و GameStats/CardDatabase/MainMenuUI/GameOverUI
        // همه رو یه آبجکت (GameManager) هستن. اگه اینجا DontDestroyOnLoad صدا بزنیم، موقعِ
        // ری‌لودِ صحنه (بازگشت به منو) این آبجکت باقی می‌مونه و نسخه‌ی تازه‌ی GameManager (با منوی نو)
        // به‌عنوان «تکراری» پاک می‌شه — برای همین بعد از ری‌لود منو دیده نمی‌شد.
        // چون بازی تک‌صحنه‌ایه، DontDestroyOnLoad اصلاً لازم نیست؛ حذفش می‌کنیم تا ری‌لود درست کار کنه.

        Budget = startValue;
        Popularity = startValue;
        Security = startValue;
        Diplomacy = startValue;
    }

    public int GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.Budget: return Budget;
            case StatType.Popularity: return Popularity;
            case StatType.Security: return Security;
            case StatType.Diplomacy: return Diplomacy;
            default: return 0;
        }
    }

    // متد اصلی — برای اعمال اثر یه تصمیم صداش می‌زنیم، مثلاً:
    // GameStats.Instance.ApplyEffect(GameStats.StatType.Budget, -15);
    public void ApplyEffect(StatType type, int amount)
    {
        if (IsGameOver) return;

        int newValue = Mathf.Clamp(GetStat(type) + amount, 0, 100);

        switch (type)
        {
            case StatType.Budget: Budget = newValue; break;
            case StatType.Popularity: Popularity = newValue; break;
            case StatType.Security: Security = newValue; break;
            case StatType.Diplomacy: Diplomacy = newValue; break;
        }

        OnStatChanged?.Invoke(type, newValue);

        if (newValue <= 0 || newValue >= 100)
        {
            bool hitMax = newValue >= 100;
            OnStatCritical?.Invoke(type, hitMax);

            IsGameOver = true;
            Debug.Log($"💀 باخت! شاخص {type} به {(hitMax ? "۱۰۰" : "۰")} رسید.");
            OnGameLost?.Invoke(type, hitMax);
        }
    }

    // ماه رو مستقیم ست می‌کنه (مثلاً شروعِ ماهِ داستانی → فروردین). event رو هم صدا می‌زنه تا UI آپدیت بشه.
    public void SetMonth(int month)
    {
        CurrentMonth = Mathf.Max(1, month);
        OnMonthChanged?.Invoke(CurrentMonth);
    }

    // بعد از هر تصمیم (سوایپ) صدا زده می‌شه، ماه رو جلو می‌بره
    public void AdvanceMonth()
    {
        if (IsGameOver) return;

        CurrentMonth++;
        OnMonthChanged?.Invoke(CurrentMonth);

        if (CurrentMonth > totalMonths)
        {
            IsGameOver = true;
            Debug.Log($"🏆 پیروزی! {totalMonths} ماه رو با موفقیت مدیریت کردی.");
            OnGameWon?.Invoke();
        }
    }

    // برای «بازی جدید» — همه‌چیز رو به مقدار اولیه برمی‌گردونه
    public void ResetState()
    {
        Budget = startValue;
        Popularity = startValue;
        Security = startValue;
        Diplomacy = startValue;
        CurrentMonth = 1;
        IsGameOver = false;
        AdUsedThisRun = false;

        NotifyAllStatsChanged();
        OnMonthChanged?.Invoke(CurrentMonth);
    }

    // شبیه‌سازی اثر تبلیغ جایزه‌دار (فعلاً بدون SDK واقعی) — طبق سند طراحی،
    // شاخص بحرانی رو به ۳۰ برمی‌گردونه و بازی رو ادامه می‌ده. حداکثر ۱ بار در هر بازی.
    public void RecoverStatViaAd(StatType type, bool hitMax)
    {
        if (AdUsedThisRun) return;

        const int recoveredValue = 30;
        switch (type)
        {
            case StatType.Budget: Budget = recoveredValue; break;
            case StatType.Popularity: Popularity = recoveredValue; break;
            case StatType.Security: Security = recoveredValue; break;
            case StatType.Diplomacy: Diplomacy = recoveredValue; break;
        }

        IsGameOver = false;
        AdUsedThisRun = true;
        OnStatChanged?.Invoke(type, recoveredValue);
    }

    // برای دکمه‌ی «ادامه‌ی بازی» — وضعیت ذخیره‌شده رو برمی‌گردونه
    public void LoadFromSaveData(SaveData data)
    {
        Budget = data.budget;
        Popularity = data.popularity;
        Security = data.security;
        Diplomacy = data.diplomacy;
        CurrentMonth = data.currentMonth;
        IsGameOver = false;

        NotifyAllStatsChanged();
        OnMonthChanged?.Invoke(CurrentMonth);
    }

    // یه عکس‌لحظه‌ای از وضعیت فعلی برای ذخیره‌سازی می‌سازه
    public SaveData CreateSaveData()
    {
        return new SaveData
        {
            budget = Budget,
            popularity = Popularity,
            security = Security,
            diplomacy = Diplomacy,
            currentMonth = CurrentMonth,
            activeFlags = CardDatabase.Instance.GetActiveFlags(),
            storyIndex = CardDatabase.Instance.GetStoryIndex()
        };
    }

    void NotifyAllStatsChanged()
    {
        OnStatChanged?.Invoke(StatType.Budget, Budget);
        OnStatChanged?.Invoke(StatType.Popularity, Popularity);
        OnStatChanged?.Invoke(StatType.Security, Security);
        OnStatChanged?.Invoke(StatType.Diplomacy, Diplomacy);
    }
}
