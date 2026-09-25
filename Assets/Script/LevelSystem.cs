using UnityEngine;

// سیستمِ سطح (لِوِل) بازیکن — بر اساسِ عملکردش (شاخص‌ها). با هر ماهِ کامل‌شده XP می‌گیره و سطحش بالا می‌ره.
// نمایش به‌فارسی: «سطح یک»، «سطح دو»، ... . بازکردنِ آپشن‌های جدید در هر سطح، بعداً تنظیم می‌شه.
public static class LevelSystem
{
    private const string XpKey = "DolatShow_XP";
    private const int XpPerLevel = 100;   // XPِ لازم برای هر سطح (قابلِ تنظیم)

    public static int TotalXP => PlayerPrefs.GetInt(XpKey, 0);

    public static void AddXP(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(XpKey, TotalXP + amount);
        PlayerPrefs.Save();
    }

    // XPِ یه دورِ کامل‌شده بر اساسِ عملکرد: پایه + جایزه‌ی «بالای میانگین نگه‌داشتنِ کشور»
    public static int XpFromRun(int budget, int popularity, int security, int diplomacy)
    {
        int sum = budget + popularity + security + diplomacy;   // ۰..۴۰۰
        int aboveAverage = Mathf.Max(0, sum - 200);             // فقط بالای میانگین
        return 30 + aboveAverage / 5;                           // ~۳۰..۷۰
    }

    public static int RewardRun(int budget, int popularity, int security, int diplomacy)
    {
        int xp = XpFromRun(budget, popularity, security, diplomacy);
        AddXP(xp);
        return xp;
    }

    public static int Level => 1 + TotalXP / XpPerLevel;

    // XPِ داخلِ سطحِ فعلی و XPِ لازم برای سطحِ بعد (برای نوارِ پیشرفت در آینده)
    public static int XpIntoLevel => TotalXP % XpPerLevel;
    public static int XpForNextLevel => XpPerLevel;

    public static string LevelPersian() => "سطح " + Word(Level);

    // --- تبدیلِ عدد به کلمه‌ی فارسی (۰..۹۹؛ بالاتر با رقمِ فارسی) ---
    private static readonly string[] ones = { "", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
    private static readonly string[] teens = { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
    private static readonly string[] tens = { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };

    public static string Word(int n)
    {
        if (n <= 0) return "صفر";
        if (n < 10) return ones[n];
        if (n < 20) return teens[n - 10];
        if (n < 100)
        {
            int t = n / 10, o = n % 10;
            return o == 0 ? tens[t] : tens[t] + " و " + ones[o];
        }
        return ToPersianDigits(n); // ۱۰۰ به بالا با رقمِ فارسی
    }

    private static string ToPersianDigits(int n)
    {
        string r = "";
        foreach (char c in n.ToString())
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
