using System;
using UnityEngine;
#if ADIVERY_ADS
using AdiveryUnity;   // فقط وقتی SDKِ ادیوری نصب و سوییچِ ADIVERY_ADS روشن باشه
#endif

// مدیرِ تبلیغِ بازی (ادیوری) — بر اساسِ APIِ رسمیِ پلاگینِ یونیتیِ ادیوری:
//   Adivery.Configure(appId) / Adivery.PrepareRewardedAd(zone) / Adivery.Show(zone) /
//   AdiveryListener (event-محور) + Adivery.AddListener(listener)
//
// طراحی‌شده که **بدونِ نصبِ SDK هم پروژه کامپایل بشه**: همه‌ی کدِ ادیوری پشتِ سوییچِ ADIVERY_ADS هست.
// روشِ فعال‌سازی (بعد از نصبِ Adivery.unitypackage):
//   ۱) Player Settings → تیکِ «Custom Launcher Gradle Template»؛ بعد تو
//      Assets/Plugins/Android/…gradle خطِ  implementation 'com.adivery:sdk:4.9.0'  رو اضافه کن.
//   ۲) Player Settings → Other Settings → Scripting Define Symbols → اضافه کن:  ADIVERY_ADS
public static class AdManager
{
    private const string AppId = "29eb0ccc-c206-4e3a-89b7-57d47d4ab829";   // «کلید اپلیکیشن» از پنلِ ادیوری
    // ⚠️ بعد از ساختِ «جایگاهِ جایزه‌دار» تو ادیوری، Zone ID رو اینجا بذار:
    private const string RewardedZone = "PUT_REWARDED_ZONE_ID_HERE";

    private static bool initialized;
    private static bool rewardedReady;      // با رویدادِ Loaded true می‌شه
    private static Action pendingReward;     // callbackِ جایزه

#if ADIVERY_ADS
    private static AdiveryListener listener;
#endif

    // موقعِ شروعِ بازی یه‌بار صدا زده می‌شه (تو MainMenuUI.Start).
    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
#if ADIVERY_ADS
        Adivery.Configure(AppId);

        listener = new AdiveryListener();
        // ⚠️ اگه امضای این رویدادها با AdiveryListener.cs نصب‌شده فرق داشت، همین‌جا اصلاحش کن.
        listener.OnRewardedAdLoaded += (sender, placementId) => { rewardedReady = true; };
        listener.OnRewardedAdClosed += (sender, isRewarded) =>
        {
            rewardedReady = false;
            if (isRewarded) { Action cb = pendingReward; pendingReward = null; cb?.Invoke(); }
            Adivery.PrepareRewardedAd(RewardedZone);   // برای دفعه‌ی بعد آماده کن
        };
        listener.OnError += (sender, placementId, reason) =>
        {
            rewardedReady = false;
            Adivery.PrepareRewardedAd(RewardedZone);
        };
        Adivery.AddListener(listener);

        Adivery.PrepareRewardedAd(RewardedZone);   // برای اولین نمایش آماده کن
#endif
    }

    // آیا تبلیغِ جایزه‌دار آماده‌ی نمایشه؟ (بدونِ SDK همیشه true = شبیه‌سازی)
    public static bool IsRewardedReady
    {
        get
        {
#if ADIVERY_ADS
            return rewardedReady;
#else
            return true;
#endif
        }
    }

    // نمایشِ تبلیغِ جایزه‌دار. اگه بازیکن جایزه رو گرفت، onReward صدا زده می‌شه.
    public static void ShowRewarded(Action onReward)
    {
#if ADIVERY_ADS
        if (rewardedReady)
        {
            pendingReward = onReward;
            Adivery.Show(RewardedZone);
        }
        else
        {
            // هنوز لود نشده؛ برای دفعه‌ی بعد آماده کن و فعلاً جایزه رو بده (تجربه‌ی بهتر)
            Adivery.PrepareRewardedAd(RewardedZone);
            onReward?.Invoke();
        }
#else
        // بدونِ SDK: شبیه‌سازی — مستقیم جایزه بده (همون رفتارِ فعلیِ بازی)
        onReward?.Invoke();
#endif
    }
}
