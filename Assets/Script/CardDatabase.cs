using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    [Header("همه‌ی کارت‌های بازی رو اینجا بریز")]
    public List<CardData> allCards = new List<CardData>();

    [Header("وزن هر دسته (جمعشون مهم نیست، فقط نسبتشونه)")]
    public int normalWeight = 60;
    public int crisisWeight = 20;
    public int chainWeight = 15;
    public int specialWeight = 5;

    private HashSet<string> activeFlags = new HashSet<string>();
    private Queue<string> recentCardIds = new Queue<string>();
    private const int recentHistorySize = 5;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && activeFlags.Contains(flag);

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag)) activeFlags.Add(flag);
    }

    // برای ذخیره‌سازی — لیستی از پرچم‌های فعال فعلی
    public List<string> GetActiveFlags() => activeFlags.ToList();

    // برای «ادامه‌ی بازی» — پرچم‌های ذخیره‌شده رو برمی‌گردونه
    public void SetActiveFlags(List<string> flags)
    {
        activeFlags = new HashSet<string>(flags);
    }

    // برای «بازی جدید» — همه‌چیز رو پاک می‌کنه
    public void ClearFlags()
    {
        activeFlags.Clear();
        recentCardIds.Clear();
    }

    // متد اصلی — یه کارت مناسب برای نمایش بعدی برمی‌گردونه، یا null اگه چیزی نمونده
    public CardData GetNextCard()
    {
        List<CardData> eligible = allCards.Where(c =>
            (string.IsNullOrEmpty(c.requiredFlag) || HasFlag(c.requiredFlag)) &&
            !recentCardIds.Contains(c.cardId)
        ).ToList();

        // اگه با محدودیت تکرار چیزی نموند، محدودیت رو موقتاً بردار
        if (eligible.Count == 0)
        {
            eligible = allCards.Where(c =>
                string.IsNullOrEmpty(c.requiredFlag) || HasFlag(c.requiredFlag)
            ).ToList();
        }

        if (eligible.Count == 0) return null;

        CardCategory chosenCategory = PickWeightedCategory();
        List<CardData> inCategory = eligible.Where(c => c.category == chosenCategory).ToList();
        List<CardData> pool = inCategory.Count > 0 ? inCategory : eligible;

        CardData picked = pool[Random.Range(0, pool.Count)];

        recentCardIds.Enqueue(picked.cardId);
        if (recentCardIds.Count > recentHistorySize) recentCardIds.Dequeue();

        return picked;
    }

    CardCategory PickWeightedCategory()
    {
        int total = normalWeight + crisisWeight + chainWeight + specialWeight;
        int roll = Random.Range(0, total);
        if (roll < normalWeight) return CardCategory.Normal;
        roll -= normalWeight;
        if (roll < crisisWeight) return CardCategory.Crisis;
        roll -= crisisWeight;
        if (roll < chainWeight) return CardCategory.Chain;
        return CardCategory.Special;
    }
}
