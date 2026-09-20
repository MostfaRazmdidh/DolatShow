using UnityEngine;

// سیستمِ امتیاز و رکورد — برای انگیزه‌ی «یه بار دیگه» بازی کردن.
// امتیاز بر اساسِ وضعیتِ نهاییِ شاخص‌ها حساب می‌شه: هم مجموعِ شاخص‌ها مهمه،
// هم «تعادل» بینشون (هرچی شاخص‌ها به هم نزدیک‌تر باشن، امتیازِ بیشتری می‌گیری).
// رکوردِ شخصی تو PlayerPrefs ذخیره می‌شه (رو خودِ دستگاه می‌مونه).
public static class ScoreSystem
{
    private const string BestKey = "DolatShow_BestScore";

    // محاسبه‌ی امتیاز از روی ۴ شاخصِ نهایی (۰ تا ~۴۵۰)
    public static int Compute(int budget, int popularity, int security, int diplomacy)
    {
        int sum = budget + popularity + security + diplomacy; // پایه: مجموع (۰ تا ۴۰۰)

        int max = Mathf.Max(Mathf.Max(budget, popularity), Mathf.Max(security, diplomacy));
        int min = Mathf.Min(Mathf.Min(budget, popularity), Mathf.Min(security, diplomacy));

        // جایزه‌ی تعادل: اختلافِ کمترِ شاخص‌ها = جایزه‌ی بیشتر (حداکثر ۵۰)
        int balanceBonus = Mathf.RoundToInt((100 - (max - min)) * 0.5f);
        if (balanceBonus < 0) balanceBonus = 0;

        return sum + balanceBonus;
    }

    // بهترین امتیازِ ثبت‌شده تا حالا
    public static int Best => PlayerPrefs.GetInt(BestKey, 0);

    // امتیازِ جدید رو ثبت می‌کنه؛ اگه رکورد شکسته بشه true برمی‌گردونه
    public static bool Submit(int score)
    {
        if (score > Best)
        {
            PlayerPrefs.SetInt(BestKey, score);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}
