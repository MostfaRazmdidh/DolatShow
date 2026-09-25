using UnityEngine;

// سیستمِ پروفایلِ بازیکن: اسم، شناسه‌ی یکتا، آواتار، سطحِ اتمامِ فروردین، و منطقِ تعویضِ نام (پولی).
public static class ProfileSystem
{
    private const string AvatarKey = "DolatShow_Avatar";
    private const string DoneKey = "DolatShow_FarvardinDone";
    private const string NameKey = "DolatShow_PlayerName";
    private const string IdKey = "DolatShow_UserId";
    private const string NameChangesKey = "DolatShow_NameChanges";
    private const string HintKey = "DolatShow_ProfileHint";

    public const int AvatarCount = 4; // Resources/UI/Avatar0..3

    // --- آواتار ---
    public static int SelectedAvatar
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(AvatarKey, 0), 0, AvatarCount - 1);
        set { PlayerPrefs.SetInt(AvatarKey, Mathf.Clamp(value, 0, AvatarCount - 1)); PlayerPrefs.Save(); }
    }
    public static Sprite GetAvatarSprite() => Resources.Load<Sprite>("UI/Avatar" + SelectedAvatar);
    public static Sprite GetAvatarSprite(int i) => Resources.Load<Sprite>("UI/Avatar" + Mathf.Clamp(i, 0, AvatarCount - 1));

    // --- اتمامِ فروردین (پروفایل فقط بعد از این نشون داده می‌شه) ---
    public static bool FarvardinDone => PlayerPrefs.GetInt(DoneKey, 0) == 1;
    public static void MarkFarvardinDone() { PlayerPrefs.SetInt(DoneKey, 1); PlayerPrefs.Save(); }

    // --- اسم ---
    public static string PlayerName => PlayerPrefs.GetString(NameKey, "");
    public static void SetName(string name)
    {
        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.Save();
    }

    // --- شناسه‌ی یکتا (یه‌بار ساخته می‌شه و ثابت می‌مونه) ---
    public static string UserId
    {
        get
        {
            string id = PlayerPrefs.GetString(IdKey, "");
            if (string.IsNullOrEmpty(id))
            {
                id = GenerateId();
                PlayerPrefs.SetString(IdKey, id);
                PlayerPrefs.Save();
            }
            return id;
        }
    }

    static string GenerateId()
    {
        // شناسه‌ی ۸ رقمی (بینِ ۱۰۰۰۰۰۰۰ تا ۹۹۹۹۹۹۹۹)
        return Random.Range(10000000, 100000000).ToString();
    }

    // --- تعویضِ نام (پولی: اولین بار ۲ ریال، هر بار دوبرابر: ۲، ۴، ۸، ...) ---
    public static int NameChangeCount => PlayerPrefs.GetInt(NameChangesKey, 0);
    public static int NextNameChangeCost => 2 * (1 << NameChangeCount); // 2,4,8,16,...

    // تلاش برای تعویضِ نام: اگه ریال کافی باشه، کم می‌کنه، اسم رو عوض و شمارنده رو زیاد می‌کنه.
    public static bool TryChangeName(string newName)
    {
        newName = (newName ?? "").Trim();
        if (string.IsNullOrEmpty(newName)) return false;
        int cost = NextNameChangeCost;
        if (!RialSystem.Spend(cost)) return false;
        SetName(newName);
        PlayerPrefs.SetInt(NameChangesKey, NameChangeCount + 1);
        PlayerPrefs.Save();
        return true;
    }

    // --- پرچمِ «چشمک بزن تا کاربر بفهمه روی پروفایل کلیک کنه» (بعد از اولین واردکردنِ اسم) ---
    public static bool ProfileHintPending => PlayerPrefs.GetInt(HintKey, 0) == 1;
    public static void SetProfileHint(bool on) { PlayerPrefs.SetInt(HintKey, on ? 1 : 0); PlayerPrefs.Save(); }

    // رقمِ فارسی (برای نمایشِ شناسه)
    public static string ToPersianDigits(string s)
    {
        string r = "";
        foreach (char c in s)
            r += (c >= '0' && c <= '9') ? "۰۱۲۳۴۵۶۷۸۹"[c - '0'] : c;
        return r;
    }
}
