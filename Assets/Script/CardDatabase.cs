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

    [Header("حالت داستانی (کمپین فروردین)")]
    [Tooltip("روشن: کارت‌های داستانیِ ماه به‌ترتیب از پوشه‌ی Resources لود و پشت‌سرِهم پخش می‌شن (نه تصادفی)")]
    public bool storyMode = true;
    [Tooltip("مسیرِ کارت‌های داستانی داخلِ Assets/Resources — بدونِ پسوند")]
    public string storyResourcesPath = "Story/Farvardin";
    [Tooltip("شماره‌ی ماهِ این فصلِ داستانی (فروردین=۱) — برای نمایشِ درستِ تاریخ")]
    public int storyMonthNumber = 1;

    private HashSet<string> activeFlags = new HashSet<string>();
    private Queue<string> recentCardIds = new Queue<string>();
    private const int recentHistorySize = 5;

    // وضعیتِ ماهِ داستانی
    private List<CardData> storyCards;
    private int storyIndex;
    // اگه ≥۰ باشه، StartStoryMonth به‌جای شروع از کارتِ اول، از این کارت ادامه می‌ده (برای «ادامه بازی»)
    private int resumeStoryIndex = -1;

    // وقتی همه‌ی کارت‌های ماهِ داستانی تموم شدن true می‌شه (CardSwipe ازش برای نمایشِ کارنامه استفاده می‌کنه)
    public bool StoryMonthComplete => storyMode && storyCards != null && storyIndex >= storyCards.Count;

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
        storyCards = null;
        storyIndex = 0;
        resumeStoryIndex = -1;
    }

    // جای فعلیِ بازیکن داخلِ کارت‌های ماهِ داستانی (برای ذخیره‌سازی)
    public int GetStoryIndex() => storyIndex;

    // برای «ادامه بازی» — می‌گه StartStoryMonth از این کارت ادامه بده (نه از اول). یه‌بار مصرفه.
    public void SetResumeStoryIndex(int index) => resumeStoryIndex = index;

    // شروعِ (یا ادامه‌ی) ماهِ داستانی — کارت‌ها رو از پوشه‌ی Resources به‌ترتیب لود می‌کنه.
    // اگه resumeStoryIndex ست شده باشه (ادامه بازی)، از همون کارت شروع می‌کنه، وگرنه از اول.
    public void StartStoryMonth()
    {
        storyCards = Resources.LoadAll<CardData>(storyResourcesPath)
            .OrderBy(c => c.orderInMonth)
            .ToList();
        storyIndex = (resumeStoryIndex >= 0) ? Mathf.Clamp(resumeStoryIndex, 0, storyCards.Count) : 0;
        resumeStoryIndex = -1; // مصرف شد
        if (storyCards.Count == 0)
            Debug.LogWarning($"هیچ کارتِ داستانی‌ای تو مسیرِ Resources/{storyResourcesPath} پیدا نشد.");
    }

    // متد اصلی — یه کارت مناسب برای نمایش بعدی برمی‌گردونه، یا null اگه چیزی نمونده
    // (تو حالتِ داستانی، کارتِ بعدیِ ماه رو به‌ترتیب می‌ده؛ وقتی تموم شد null برمی‌گردونه)
    public CardData GetNextCard()
    {
        if (storyMode)
        {
            if (storyCards == null) StartStoryMonth();
            if (storyIndex < storyCards.Count) return storyCards[storyIndex++];
            return null; // ماهِ داستانی تموم شد → StoryMonthComplete = true
        }

        return GetNextRandomCard();
    }

    // انتخابِ تصادفیِ وزن‌دار (حالتِ غیرداستانی/ماه‌های آینده)
    CardData GetNextRandomCard()
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
