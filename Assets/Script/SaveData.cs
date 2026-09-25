using System.Collections.Generic;

// یه عکس‌لحظه‌ای از وضعیت بازی که ذخیره و بارگذاری می‌شه
[System.Serializable]
public class SaveData
{
    public int budget;
    public int popularity;
    public int security;
    public int diplomacy;
    public int currentMonth;
    public List<string> activeFlags;

    // جای فعلیِ بازیکن داخلِ کارت‌های ماهِ داستانی (کدوم کارت از ۱۲ کارت) — تا «ادامه بازی»
    // از همون کارت ادامه بده، نه از اولِ ماه.
    public int storyIndex;
}
