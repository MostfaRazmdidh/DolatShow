using UnityEngine;
using System.Collections.Generic;

public enum CardCategory { Normal, Crisis, Chain, Special }

// یه اثر روی یه شاخص — مثلاً «بودجه -۱۵»
[System.Serializable]
public class StatEffect
{
    public GameStats.StatType type;
    public int amount;
}

// این کلاس یه "کارت" رو به‌صورت داده تعریف می‌کنه، نه کد
// هر کارت واقعی تو بازی یه asset از همین نوعه که تو Project می‌سازیم
[CreateAssetMenu(fileName = "NewCard", menuName = "دولت شو/کارت جدید")]
public class CardData : ScriptableObject
{
    [Header("محتوا")]
    public string cardId;
    public string advisorName;      // مثلاً "وزیر اقتصاد"
    [TextArea(3, 6)]
    public string cardText;          // متن اصلی کارت

    [Header("دسته‌بندی")]
    public CardCategory category = CardCategory.Normal;

    [Header("تصمیم راست (تایید)")]
    public List<StatEffect> approveEffects = new List<StatEffect>();

    [Header("تصمیم چپ (رد)")]
    public List<StatEffect> rejectEffects = new List<StatEffect>();

    [Header("سیستم پرچم — برای خط‌های داستانی (اختیاری، خالی بذار اگه لازم نداری)")]
    public string requiredFlag;      // این کارت فقط وقتی این پرچم ست شده باشه ظاهر می‌شه
    public string setFlagOnApprove;  // با تایید، این پرچم ست می‌شه
    public string setFlagOnReject;   // با رد، این پرچم ست می‌شه

    [Header("حالت داستانی (کمپین فروردین و ماه‌های داستانی بعدی)")]
    [Tooltip("ترتیبِ کارت داخل ماه — کارت‌های داستانی به همین ترتیب پشت‌سرِ هم میان (کوچیک‌تر، زودتر)")]
    public int orderInMonth = 0;

    [Header("تصویر و صدا (اختیاری — برای «آب‌وتاب» بصری/صوتیِ کارت)")]
    [Tooltip("تصویرِ اختصاصیِ کارت (ریزکاتسین). اگه خالی باشه، همون طرحِ پیش‌فرضِ کارت می‌مونه.")]
    public Sprite illustration;
    [Tooltip("صدای مخصوصِ این کارت موقع تایید — اگه خالی باشه، صدای پیش‌فرضِ «بله» پخش می‌شه")]
    public AudioClip approveSfx;
    [Tooltip("صدای مخصوصِ این کارت موقع رد — اگه خالی باشه، صدای پیش‌فرضِ «خیر» پخش می‌شه")]
    public AudioClip rejectSfx;
}
