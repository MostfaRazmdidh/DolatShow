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

    [Header("حالتِ بقا (بعد از فروردین) — کارت‌های عمومیِ تصادفی")]
    [Tooltip("مسیرِ حوضچه‌ی کارت‌های عمومیِ حالتِ بقا داخلِ Assets/Resources")]
    public string generalResourcesPath = "Story/General";
    private List<CardData> generalPool;

    // آیا آخرین کارتی که داده شد از مرحله‌ی مقدماتی (۱۲ کارتِ فروردین) بود؟
    // (تو مقدماتی ماه جلو نمی‌ره و سختی ثابته؛ تو بقا برعکس.)
    public bool LastCardWasIntro { get; private set; }

    // آیا هنوز داخلِ مرحله‌ی مقدماتی (فروردین) هستیم؟
    public bool IntroActive => storyMode && storyCards != null && storyIndex < storyCards.Count;

    // آیا باید ماهِ داستانی (از نو یا با ادامه) شروع بشه؟ — برای اینکه «ادامه بازی» و «تبلیغ و ادامه»
    // به‌اشتباه بازی رو از اولِ فروردین شروع نکنن. true فقط وقتی هنوز چیزی لود نشده یا ادامه‌ای در انتظاره.
    public bool NeedsStoryStart => storyCards == null || resumeStoryIndex >= 0;

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

    // متد اصلی — یه کارت مناسب برای نمایش بعدی برمی‌گردونه.
    // مرحله‌ی مقدماتی (فروردین): ۱۲ کارت به‌ترتیب. بعدش «حالتِ بقا»: کارت‌های تصادفیِ عمومی
    // که ماه‌به‌ماه ادامه پیدا می‌کنن تا وقتی یه شاخص به ۰/۱۰۰ برسه یا ۴۸ ماه کامل شه.
    public CardData GetNextCard()
    {
        if (storyMode && storyCards == null) StartStoryMonth();

        // مرحله‌ی مقدماتی: کارت‌های فروردین به‌ترتیب
        if (storyMode && storyCards != null && storyIndex < storyCards.Count)
        {
            LastCardWasIntro = true;
            return storyCards[storyIndex++];
        }

        // حالتِ بقا: کارتِ تصادفیِ عمومی
        LastCardWasIntro = false;
        return GetNextRandomCard();
    }

    // حوضچه‌ی کارت‌های عمومیِ حالتِ بقا رو (یه‌بار) از Resources لود می‌کنه
    void EnsureGeneralPool()
    {
        if (generalPool != null) return;
        generalPool = Resources.LoadAll<CardData>(generalResourcesPath).ToList();
        if (generalPool.Count == 0)
            Debug.LogWarning($"CardDatabase: حوضچه‌ی عمومیِ بقا تو Resources/{generalResourcesPath} خالیه.");
    }

    // انتخابِ تصادفیِ وزن‌دار برای حالتِ بقا (از حوضچه‌ی عمومی؛ اگه خالی بود از allCards)
    CardData GetNextRandomCard()
    {
        EnsureGeneralPool();
        List<CardData> source = (generalPool != null && generalPool.Count > 0) ? generalPool : allCards;

        List<CardData> eligible = source.Where(c =>
            (string.IsNullOrEmpty(c.requiredFlag) || HasFlag(c.requiredFlag)) &&
            !recentCardIds.Contains(c.cardId)
        ).ToList();

        // اگه با محدودیت تکرار چیزی نموند، محدودیت رو موقتاً بردار
        if (eligible.Count == 0)
        {
            eligible = source.Where(c =>
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
