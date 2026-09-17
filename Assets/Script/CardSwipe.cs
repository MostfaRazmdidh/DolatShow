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
        EnsureTextRendersAboveCard();
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
                textCanvas.sortingOrder = cardOrder + 1;
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

        var effects = xOffset > 0 ? currentCard.approveEffects : currentCard.rejectEffects;

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
            bool approved = xOffset > 0;
            decided = true;
            ApplyCardEffects(approved);
            StartCoroutine(FlyOffScreen(approved));
        }
        else
        {
            StartCoroutine(ReturnToCenter());
        }
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

    IEnumerator FlyOffScreen(bool approved)
    {
        Vector3 direction = approved ? Vector3.right : Vector3.left;
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

        Debug.Log(approved ? "APPROVED (تایید شد)" : "REJECTED (رد شد)");
        LoadNextCard(); // کارت بعدی رو بیار
    }
}
