using UnityEngine;

// سیستمِ پروفایلِ بازیکن (داده‌محور، بدونِ UIِ انتخاب — گرافیک/انتخاب بعداً اضافه می‌شه).
// نگه‌داریِ: آواتارِ انتخاب‌شده، و اینکه بازیکن ماهِ فروردین رو تموم کرده یا نه (تا پروفایل قبلش نشون داده نشه).
public static class ProfileSystem
{
    private const string AvatarKey = "DolatShow_Avatar";
    private const string DoneKey = "DolatShow_FarvardinDone";
    public const int AvatarCount = 4; // Resources/UI/Avatar0..3

    // آواتارِ انتخاب‌شده (فعلاً پیش‌فرض ۰؛ صفحه‌ی انتخاب بعداً)
    public static int SelectedAvatar
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(AvatarKey, 0), 0, AvatarCount - 1);
        set { PlayerPrefs.SetInt(AvatarKey, Mathf.Clamp(value, 0, AvatarCount - 1)); PlayerPrefs.Save(); }
    }

    public static Sprite GetAvatarSprite() => Resources.Load<Sprite>("UI/Avatar" + SelectedAvatar);

    // آیا بازیکن حداقل یه‌بار ماهِ فروردین رو کامل کرده؟ (پروفایل فقط بعد از اون نشون داده می‌شه)
    public static bool FarvardinDone => PlayerPrefs.GetInt(DoneKey, 0) == 1;
    public static void MarkFarvardinDone() { PlayerPrefs.SetInt(DoneKey, 1); PlayerPrefs.Save(); }

    // اسمِ کاربر (همون کلیدی که ResultGreetingUI ذخیره می‌کنه)
    public static string PlayerName => PlayerPrefs.GetString("DolatShow_PlayerName", "");
}
