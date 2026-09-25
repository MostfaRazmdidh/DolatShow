using UnityEngine;

// سیستمِ تنظیماتِ بازی — همه‌ی گزینه‌ها تو PlayerPrefs ذخیره می‌شن و بینِ اجراها می‌مونن.
// این کلاس فقط «حالت» رو نگه می‌داره و اعمال می‌کنه؛ خودِ صفحه‌ی تنظیمات تو SettingsUI.cs ساخته می‌شه.
//
// چطور روی صدا اثر می‌ذاره؟ همه‌ی صداهای بازی (کارت، دیالوگ، تشویق، شمارش) از AudioSource پخش می‌شن،
// و AudioListener.volume یه ضریبِ سراسری روی *همه‌ی* اون‌هاست — پس با یه خط، صدای کلِ بازی کنترل می‌شه
// بدونِ اینکه لازم باشه تک‌تکِ فایل‌ها دستکاری بشن.
public static class SettingsSystem
{
    private const string SoundOnKey = "DolatShow_SoundOn";
    private const string SoundVolKey = "DolatShow_SoundVol";
    private const string MusicOnKey = "DolatShow_MusicOn";
    private const string MusicVolKey = "DolatShow_MusicVol";
    private const string VibrationKey = "DolatShow_Vibration";

    // --- صدا (افکت‌ها و صداها) ---
    public static bool SoundOn
    {
        get => PlayerPrefs.GetInt(SoundOnKey, 1) == 1;
        set { PlayerPrefs.SetInt(SoundOnKey, value ? 1 : 0); PlayerPrefs.Save(); Apply(); }
    }
    // بلندیِ صدا ۰..۱
    public static float SoundVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(SoundVolKey, 1f));
        set { PlayerPrefs.SetFloat(SoundVolKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); Apply(); }
    }

    // --- موسیقیِ پس‌زمینه (اگه فایلِ موسیقی تو Resources/Sound/Music باشه) ---
    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt(MusicOnKey, 1) == 1;
        set { PlayerPrefs.SetInt(MusicOnKey, value ? 1 : 0); PlayerPrefs.Save(); MusicManager.ApplySettings(); }
    }
    public static float MusicVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolKey, 0.6f));
        set { PlayerPrefs.SetFloat(MusicVolKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); MusicManager.ApplySettings(); }
    }

    // --- لرزش (هپتیک؛ فقط رو موبایل معنی داره) ---
    public static bool VibrationOn
    {
        get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        set { PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // اعمالِ تنظیماتِ صدا روی موتورِ بازی. باید موقعِ شروع یه بار صدا زده بشه.
    public static void Apply()
    {
        AudioListener.volume = SoundOn ? SoundVolume : 0f;
        MusicManager.ApplySettings();
    }

    // لرزشِ کوتاه (اگه روشن باشه و دستگاه پشتیبانی کنه)
    public static void Vibrate()
    {
        if (!VibrationOn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
