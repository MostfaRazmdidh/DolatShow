using System;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    public enum StatType { Budget, Popularity, Security, Diplomacy }

    [Header("مقدار اولیه‌ی شاخص‌ها")]
    [SerializeField] private int startValue = 50;

    public int Budget { get; private set; }
    public int Popularity { get; private set; }
    public int Security { get; private set; }
    public int Diplomacy { get; private set; }

    // هر بار شاخصی تغییر کنه صدا زده می‌شه — UI بهش گوش می‌ده تا خودکار آپدیت بشه
    public event Action<StatType, int> OnStatChanged;

    // وقتی شاخصی دقیقاً به ۰ یا ۱۰۰ برسه (مرز باخت) — فعلاً فقط event رو می‌فرستیم،
    // خود منطق باخت رو تو فاز بعد (حلقه‌ی اصلی بازی) بهش گوش می‌دیم
    public event Action<StatType, bool> OnStatCritical; // bool: true = رسیده به ۱۰۰, false = رسیده به ۰

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
            OnStatCritical?.Invoke(type, newValue >= 100);
        }
    }
}
