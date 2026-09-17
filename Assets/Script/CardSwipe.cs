using UnityEngine;
using System.Collections;
using RTLTMPro;

// این اسکریپت رو روی آبجکت Card که Collider2D داره می‌ذاریم
public class CardSwipe : MonoBehaviour
{
    [Header("نمایش متن کارت (دو تا RTL Text Mesh Pro زیر یه Canvas)")]
    [SerializeField] private RTLTextMeshPro advisorText;
    [SerializeField] private RTLTextMeshPro bodyText;

    [Header("تنظیمات سوایپ")]
    [SerializeField] private float swipeThreshold = 1.5f;
    [SerializeField] private float rotationFactor = 15f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float flyOffDistance = 15f;

    [Header("جهت تصمیم (بله / خیر)")]
    [Tooltip("روشن: سوایپ راست = تایید (بله)، چپ = رد (خیر). خاموش: برعکس.")]
    [SerializeField] private bool swipeRightMeansApprove = true;

    [Header("چیدمان متن کارت")]
    [Tooltip("روشن: کد موقع Start چیدمان متن رو از روی مقادیر پایین اعمال می‌کنه. اگه می‌خوای خودت دستی " +
        "متن رو تو صحنه تنظیم کنی، این رو خاموش کن تا Play دیگه روش ننویسه (اول یه‌بار از راست‌کلیکِ " +
        "کامپوننت «اعمال چیدمان متن کارت» رو بزن تا مقدار اولیه‌ی درست رو بگیری).")]
    [SerializeField] private bool autoConfigureCardText = true;
    [SerializeField] private Color cardTextColor = new Color(0.96f, 0.90f, 0.78f); // کرمِ روشن
    [Tooltip("پهنای ناحیه‌ی متن نسبت به کارت — کمترش کنی، متن از لبه‌ها بیشتر فاصله می‌گیره")]
    [SerializeField, Range(0.3f, 1f)] private float textWidthFraction = 0.7f;
    [Tooltip("موقعیت عمودی اسم مشاور (کسری از ارتفاع کارت؛ مثبت = بالاتر)")]
    [SerializeField] private float advisorYFraction = 0.28f;
    [Tooltip("اندازه‌ی فونت اسم مشاور (کسری از ارتفاع کارت)")]
    [SerializeField] private float advisorFontFraction = 0.05f;
    [Tooltip("موقعیت عمودی متن تصمیم")]
    [SerializeField] private float bodyYFraction = -0.02f;
    [Tooltip("اندازه‌ی فونت متن تصمیم")]
    [SerializeField] private float bodyFontFraction = 0.04f;
    [Tooltip("ارتفاع ناحیه‌ی متن تصمیم (کسری از ارتفاع کارت)")]
    [SerializeField] private float bodyHeightFraction = 0.45f;

    [Header("پیش‌نمایش اثر تصمیم (اختیاری)")]
    [SerializeField] private StatBarUI[] statBars; // هر ۴ نوار وضعیت رو اینجا بریز
    [SerializeField] private float hintThreshold = 0.3f; // از این فاصله به بعد پیش‌نمایش ظاهر می‌شه

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private bool decided = false;
    private Camera mainCamera;
    private CardData currentCard;

    void Start()
    {
        startPosition = transform.position;
        mainCamera = Camera.main;
        FitColliderToCard();
        if (autoConfigureCardText) ConfigureCardText();
        EnsureTextRendersAboveCard();
    }

    // دکمه‌ی راست‌کلیک روی کامپوننت (تو حالت Edit) — چیدمان متن رو یه‌بار روی صحنه اعمال می‌کنه
    // تا بعدش بتونی autoConfigureCardText رو خاموش کنی و خودت دستی تنظیمش کنی بدون اینکه Play بازنویسیش کنه.
    [ContextMenu("اعمال چیدمان متن کارت روی صحنه")]
    void ApplyCardTextLayoutInEditor()
    {
        ConfigureCardText();
    }

    // متن‌های کارت (اسم مشاور + متن تصمیم) رنگ مشکی و اندازه‌ی میکروسکوپی داشتن (fontSize ۰.۳-۰.۵
    // روی یه Canvasِ ۳۰۰×۴۰۰ که با اسکیل‌های تودرتو کوچیک شده بود) — عملاً نامرئی. اینجا Canvasِ متن و
    // خودِ متن‌ها رو بر اساس اندازه‌ی واقعیِ اسپرایت کارت از نو و تمیز می‌چینیم و رنگ رو روشن می‌کنیم.
    void ConfigureCardText()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null || advisorText == null || bodyText == null) return;

        Vector2 cardSize = sr.sprite.bounds.size; // واحدهای محلیِ کارت، مثلاً 10.86 × 14.48

        // Canvasِ متن رو دقیقاً هم‌اندازه‌ی صفحه‌ی کارت کن (نسبت ۱:۱ با فضای محلیِ کارت)
        Canvas textCanvas = advisorText.GetComponentInParent<Canvas>();
        if (textCanvas != null)
        {
            RectTransform canvasRT = textCanvas.GetComponent<RectTransform>();
            canvasRT.localScale = Vector3.one;
            canvasRT.sizeDelta = cardSize;
            canvasRT.anchoredPosition = Vector2.zero;
        }

        // اسم مشاور — بالای کارت
        StyleCardText(advisorText, new Vector2(0f, cardSize.y * advisorYFraction),
            new Vector2(cardSize.x * textWidthFraction, cardSize.y * 0.14f), cardSize.y * advisorFontFraction, cardTextColor);

        // متن تصمیم — وسط کارت، بزرگ‌تر و چندخطی
        StyleCardText(bodyText, new Vector2(0f, cardSize.y * bodyYFraction),
            new Vector2(cardSize.x * textWidthFraction, cardSize.y * bodyHeightFraction), cardSize.y * bodyFontFraction, cardTextColor);
    }

    void StyleCardText(RTLTextMeshPro t, Vector2 pos, Vector2 size, float maxFontSize, Color color)
    {
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        t.alignment = TMPro.TextAlignmentOptions.Center;
        t.enableAutoSizing = true;
        t.fontSizeMin = 0.05f;
        t.fontSizeMax = maxFontSize;
        t.color = color;
    }

    // Collider کارت تو صحنه فقط ۱×۱ واحد بود، ولی خودِ کارت خیلی بزرگ‌تره — برای همین بیشترِ
    // سطح کارت کلیک/سوایپ نمی‌گرفت. اینجا Collider رو دقیقاً هم‌اندازه‌ی اسپرایت کارت می‌کنیم.
    void FitColliderToCard()
    {
        var sr = GetComponent<SpriteRenderer>();
        var box = GetComponent<BoxCollider2D>();
        if (sr != null && sr.sprite != null && box != null)
        {
            box.size = sr.sprite.bounds.size;   // اندازه‌ی محلیِ اسپرایت (قبل از اسکیلِ ترنسفورم)
            box.offset = sr.sprite.bounds.center;
        }
    }

    // کارت (Card cover) یه SpriteRenderer ماته با sortingOrder=۰. Canvasِ متنِ کارت (World Space)
    // هم پیش‌فرض sortingOrder=۰ داره، برای همین کارت روی متن می‌افته و متن دیده نمی‌شه.
    // اینجا Canvasِ متن رو مجبور می‌کنیم با sortingOrder بالاتر رندر بشه تا روی کارت بیاد.
    void EnsureTextRendersAboveCard()
    {
        int cardOrder = 0;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) cardOrder = sr.sortingOrder;

        if (advisorText != null)
        {
            Canvas textCanvas = advisorText.GetComponentInParent<Canvas>();
            if (textCanvas != null)
            {
                textCanvas.overrideSorting = true;
                textCanvas.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
                textCanvas.sortingOrder = cardOrder + 5;
            }
        }
    }

    // با «بازی جدید» یا «ادامه‌ی بازی» از منوی اصلی صدا زده می‌شه
    public void BeginGame()
    {
        LoadNextCard();
    }

    // کارت بعدی رو از دیتابیس می‌گیره و متنش رو نمایش می‌ده
    void LoadNextCard()
    {
        if (GameStats.Instance.IsGameOver)
        {
            Debug.Log("بازی تموم شد — کارت جدیدی نمایش داده نمی‌شه.");
            gameObject.SetActive(false);
            return;
        }

        currentCard = CardDatabase.Instance.GetNextCard();

        if (currentCard == null)
        {
            Debug.Log("کارتی برای نمایش نمونده!");
            gameObject.SetActive(false);
            return;
        }

        advisorText.text = currentCard.advisorName;
        bodyText.text = currentCard.cardText;

        decided = false;
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }

    void OnMouseDown()
    {
        if (decided) return;
        isDragging = true;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        transform.position = GetMouseWorldPosition() + dragOffset;

        float xOffset = transform.position.x - startPosition.x;
        transform.rotation = Quaternion.Euler(0, 0, -xOffset * rotationFactor);

        UpdateHints(xOffset);
    }

    // بر اساس جهت و فاصله‌ی کشیدن، نشون می‌ده اگه همین الان رها کنی چه اثری رخ می‌ده
    void UpdateHints(float xOffset)
    {
        if (statBars == null || currentCard == null) return;

        if (Mathf.Abs(xOffset) < hintThreshold)
        {
            HideAllHints();
            return;
        }

        var effects = IsApprove(xOffset) ? currentCard.approveEffects : currentCard.rejectEffects;

        foreach (var bar in statBars)
        {
            var effect = effects.Find(e => e.type == bar.StatType);
            if (effect != null)
                bar.ShowHint(effect.amount);
            else
                bar.HideHint();
        }
    }

    void HideAllHints()
    {
        if (statBars == null) return;
        foreach (var bar in statBars) bar.HideHint();
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        HideAllHints();

        float xOffset = transform.position.x - startPosition.x;

        if (Mathf.Abs(xOffset) > swipeThreshold)
        {
            bool draggedRight = xOffset > 0;      // کارت به کدوم سمت کشیده شد
            bool approved = IsApprove(xOffset);   // اون سمت یعنی تایید یا رد
            decided = true;
            ApplyCardEffects(approved);
            StartCoroutine(FlyOffScreen(draggedRight)); // کارت به سمتی که کشیده شد پرت می‌شه
        }
        else
        {
            StartCoroutine(ReturnToCenter());
        }
    }

    // بر اساس گزینه‌ی swipeRightMeansApprove مشخص می‌کنه سوایپ به این جهت یعنی تایید یا رد
    bool IsApprove(float xOffset)
    {
        bool draggedRight = xOffset > 0;
        return swipeRightMeansApprove ? draggedRight : !draggedRight;
    }

    // اثرات تصمیم رو روی شاخص‌ها اعمال می‌کنه و پرچم مربوطه رو (اگه بود) ست می‌کنه
    void ApplyCardEffects(bool approved)
    {
        var effects = approved ? currentCard.approveEffects : currentCard.rejectEffects;
        foreach (var effect in effects)
        {
            GameStats.Instance.ApplyEffect(effect.type, effect.amount);
        }

        string flag = approved ? currentCard.setFlagOnApprove : currentCard.setFlagOnReject;
        CardDatabase.Instance.SetFlag(flag);

        GameStats.Instance.AdvanceMonth();

        // بعد از هر تصمیم، وضعیت رو خودکار ذخیره می‌کنیم تا «ادامه‌ی بازی» درست کار کنه
        if (GameStats.Instance.IsGameOver)
            SaveSystem.DeleteSave();
        else
            SaveSystem.Save(GameStats.Instance.CreateSaveData());
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    IEnumerator ReturnToCenter()
    {
        while (Vector3.Distance(transform.position, startPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * returnSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * returnSpeed);
            yield return null;
        }
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }

    // toRight = کارت به کدوم سمت (فیزیکی) پرت بشه — همون سمتی که کاربر کشیدش
    IEnumerator FlyOffScreen(bool toRight)
    {
        Vector3 direction = toRight ? Vector3.right : Vector3.left;
        Vector3 fromPos = transform.position;
        Vector3 toPos = fromPos + direction * flyOffDistance;

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(fromPos, toPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        LoadNextCard(); // کارت بعدی رو بیار
    }
}
