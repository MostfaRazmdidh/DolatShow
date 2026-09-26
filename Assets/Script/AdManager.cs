using System;
using UnityEngine;
#if ADIVERY_ADS
using AdiveryUnity;   // فقط وقتی SDKِ ادیوری نصب و سوییچِ ADIVERY_ADS روشن باشه
#endif

// مدیرِ تبلیغِ بازی (ادیوری). طراحی‌شده که **بدونِ نصبِ SDK هم پروژه کامپایل بشه**:
// همه‌ی کدِ ادیوری پشتِ سوییچِ کامپایلِ ADIVERY_ADS هست.
//
// روشِ فعال‌سازی (بعد از نصبِ SDK ادیوری تو Unity):
//   Edit → Project Settings → Player → Other Settings → Scripting Define Symbols
//   → اضافه کن:  ADIVERY_ADS
// تا وقتی این سوییچ خاموشه، ShowRewarded مثلِ قبل «شبیه‌سازی» می‌کنه (مستقیم جایزه می‌ده).
public static class AdManager
{
    // از پنلِ ادیوری:
    private const string AppKey = "29eb0ccc-c206-4e3a-89b7-57d47d4ab829";
    // ⚠️ بعد از ساختِ «جایگاهِ جایزه‌دار» تو ادیوری، Zone ID رو اینجا بذار:
    private const string RewardedZone = "PUT_REWARDED_ZONE_ID_HERE";

    private static bool initialized;
    private static Action pendingReward;   // callbackِ جایزه که موقعِ بسته‌شدنِ تبلیغ صدا زده می‌شه

    // موقعِ شروعِ بازی یه‌بار صدا زده می‌شه (تو MainMenuUI.Start).
    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
#if ADIVERY_ADS
        Adivery.Configure(AppKey);
        Adivery.addListener(new DolatAdListener());
        Adivery.prepareRewardedAd(RewardedZone);   // برای اولین نمایش آماده کن
#endif
    }

    // آیا تبلیغِ جایزه‌دار آماده‌ی نمایشه؟ (بدونِ SDK همیشه true = شبیه‌سازی)
    public static bool IsRewardedReady
    {
        get
        {
#if ADIVERY_ADS
            return Adivery.isLoaded(RewardedZone);
#else
            return true;
#endif
        }
    }

    // نمایشِ تبلیغِ جایزه‌دار. اگه بازیکن جایزه رو گرفت (تبلیغ کامل دیده شد)، onReward صدا زده می‌شه.
    public static void ShowRewarded(Action onReward)
    {
#if ADIVERY_ADS
        pendingReward = onReward;
        if (Adivery.isLoaded(RewardedZone))
        {
            Adivery.showAd(RewardedZone);
        }
        else
        {
            // اگه هنوز لود نشده، برای دفعه‌ی بعد آماده کن و فعلاً جایزه رو بده (تجربه‌ی بهتر برای بازیکن)
            Adivery.prepareRewardedAd(RewardedZone);
            pendingReward = null;
            onReward?.Invoke();
        }
#else
        // بدونِ SDK: شبیه‌سازی — مستقیم جایزه بده (همون رفتارِ فعلیِ بازی)
        onReward?.Invoke();
#endif
    }

#if ADIVERY_ADS
    // شنونده‌ی رویدادهای ادیوری. اگه امضای متدها با نسخه‌ی SDKِ تو فرق داشت، همین‌جا اصلاحش کن.
    private class DolatAdListener : AdListener
    {
        public override void onRewardedAdClosed(string placementId, bool isRewarded)
        {
            if (isRewarded)
            {
                Action cb = pendingReward;
                pendingReward = null;
                cb?.Invoke();
            }
            Adivery.prepareRewardedAd(RewardedZone);   // برای دفعه‌ی بعد دوباره آماده کن
        }

        public override void onError(string placementId, string reason)
        {
            Adivery.prepareRewardedAd(RewardedZone);
        }
    }
#endif
}
