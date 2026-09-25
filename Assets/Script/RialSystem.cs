using UnityEngine;

// ارزِ بازی: «ریال». ارزشش بالاست و به‌دست‌آوردنش سخته (هر دور فقط چند ریال).
// جایگزینِ «امتیاز» شد؛ برخلافِ امتیاز، ریال بینِ بازی‌ها جمع می‌شه (تو PlayerPrefs) و
// تو منوی اصلی (داخلِ باکسِ ریال) نشون داده می‌شه. بعداً برای فروشگاه/ارتقا استفاده می‌شه.
public static class RialSystem
{
    private const string Key = "DolatShow_Rial";

    // موجودیِ کلِ ریال
    public static int Total => PlayerPrefs.GetInt(Key, 0);

    // اضافه‌کردنِ ریال به موجودی
    public static void Add(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(Key, Total + amount);
        PlayerPrefs.Save();
    }

    // محاسبه‌ی ریالِ به‌دست‌آمده از یه دور بازی (کم و سخت):
    // فقط بابتِ «بالاتر از میانگین نگه‌داشتنِ کشور» ریال می‌دیم. یه دورِ خیلی خوب ~۶ ریال،
    // دورِ متوسط ۱-۲ ریال، دورِ بد ۰. (ضریب‌ها قابلِ تنظیمن.)
    public static int EarnFromStats(int budget, int popularity, int security, int diplomacy)
    {
        int sum = budget + popularity + security + diplomacy;   // ۰..۴۰۰
        int aboveAverage = Mathf.Max(0, sum - 200);             // فقط بالای میانگین (۵۰ هر کدوم)
        return Mathf.RoundToInt(aboveAverage / 25f);            // ~۰..۸ ریال
    }

    // ریالِ دور رو حساب و به موجودی اضافه می‌کنه؛ مقدارِ اضافه‌شده رو برمی‌گردونه
    public static int RewardRun(int budget, int popularity, int security, int diplomacy)
    {
        int earned = EarnFromStats(budget, popularity, security, diplomacy);
        Add(earned);
        return earned;
    }

    // رقمِ فارسیِ موجودی (برای نمایش)
    public static string TotalPersian() => ToPersian(Total);

    public static string ToPersian(int n)
    {
        string r = "";
        foreach (char c in n.ToString())
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
