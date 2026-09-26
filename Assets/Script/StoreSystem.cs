using System.Collections.Generic;
using UnityEngine;

// سیستمِ فروشگاه: کاتالوگِ آیتم‌ها، خرید با «ریال»، و نگه‌داریِ مالکیت/تعداد تو PlayerPrefs.
// داده‌محوره — برای اضافه‌کردنِ آیتمِ جدید فقط یه سطر به Catalog اضافه کن.
// UI جداست (StoreUI.cs)؛ این کلاس فقط منطق و داده‌ست.
public static class StoreSystem
{
    // نوعِ آیتم: Consumable = خرج‌شدنی (شمارنده داره، هر بار خرید یکی اضافه می‌شه، موقع استفاده کم می‌شه)
    //           Permanent  = دائمی (یه‌بار خرید، مالِ همیشه — مثلِ تمِ ظاهری)
    public enum Kind { Consumable, Permanent }

    public class StoreItem
    {
        public string id;
        public string title;       // اسمِ فارسیِ آیتم
        public string description;  // توضیحِ کوتاه
        public int price;           // قیمت به ریال
        public Kind kind;
        public string iconName;     // اسمِ آیکون تو Resources/UI (اختیاری، برای UIِ بعدی)

        public StoreItem(string id, string title, string description, int price, Kind kind, string iconName = "")
        {
            this.id = id; this.title = title; this.description = description;
            this.price = price; this.kind = kind; this.iconName = iconName;
        }
    }

    // --- کاتالوگِ آیتم‌ها (اینجا آیتمِ جدید اضافه کن) ---
    public static readonly List<StoreItem> Catalog = new List<StoreItem>
    {
        // خرج‌شدنی‌ها — حلقه‌ی خرجِ ریال
        new StoreItem("headstart", "شروعِ قدرتمند",
            "بازیِ بعدی با +۱۰ روی هر چهار شاخص شروع می‌شه.", 6, Kind.Consumable, "Item_HeadStart"),
        new StoreItem("superstart", "شروعِ طلایی",
            "بازیِ بعدی با +۲۰ روی هر چهار شاخص شروع می‌شه.", 12, Kind.Consumable, "Item_SuperStart"),
        new StoreItem("veto", "وتوی ریاست‌جمهوری",
            "موقعِ باخت، بدونِ دیدنِ تبلیغ یک بار کشور را نجات می‌دهد.", 10, Kind.Consumable, "Item_Veto"),

        // دائمی — ظاهری (اثرِ بصری‌شون تو UIِ بعدی کامل می‌شه)
        new StoreItem("frame_royal", "قابِ سلطنتی",
            "قابِ ویژه‌ی طلایی برای پروفایل.", 25, Kind.Permanent, "Item_Frame"),
        new StoreItem("theme_dark", "تمِ شبانه‌ی کارت",
            "ظاهرِ تیره و ویژه برای کارت‌ها.", 30, Kind.Permanent, "Item_Theme"),
    };

    public static StoreItem Get(string id) => Catalog.Find(i => i.id == id);

    // --- مالکیت / تعداد ---
    static string Key(string id) => "DolatShow_Store_" + id;

    // تعدادِ آیتمِ خرج‌شدنی (یا برای دائمی: ۰ یا ۱)
    public static int GetCount(string id) => PlayerPrefs.GetInt(Key(id), 0);

    // آیتمِ دائمی خریداری/فعال شده؟
    public static bool IsOwned(string id) => GetCount(id) > 0;

    // نتیجه‌ی تلاش برای خرید
    public enum BuyResult { Success, NotEnoughRial, AlreadyOwned, Invalid }

    public static BuyResult Buy(string id)
    {
        StoreItem item = Get(id);
        if (item == null) return BuyResult.Invalid;

        // آیتمِ دائمی که قبلاً خریده شده، دوباره خریده نمی‌شه
        if (item.kind == Kind.Permanent && IsOwned(id)) return BuyResult.AlreadyOwned;

        if (!RialSystem.Spend(item.price)) return BuyResult.NotEnoughRial;

        PlayerPrefs.SetInt(Key(id), GetCount(id) + 1);
        PlayerPrefs.Save();
        return BuyResult.Success;
    }

    // یکی از آیتمِ خرج‌شدنی رو مصرف می‌کنه (اگه موجود باشه). true = مصرف شد.
    public static bool Consume(string id)
    {
        int c = GetCount(id);
        if (c <= 0) return false;
        PlayerPrefs.SetInt(Key(id), c - 1);
        PlayerPrefs.Save();
        return true;
    }

    // --- اثرها ---

    // بونوسِ شروعِ بازی: قوی‌ترین آیتمِ موجود رو مصرف و مقدارش رو برمی‌گردونه (۰ اگه چیزی نبود).
    // MainMenuUI.StartNewGame قبل از شروعِ بازی صداش می‌زنه.
    public static int ConsumeStartBonus()
    {
        if (Consume("superstart")) return 20;
        if (Consume("headstart")) return 10;
        return 0;
    }
}
