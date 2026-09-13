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
        DontDestroyOnLoad(gameObject);

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
}
