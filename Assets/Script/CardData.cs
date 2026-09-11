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
}
