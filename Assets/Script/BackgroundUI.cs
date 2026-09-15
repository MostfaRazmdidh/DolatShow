using UnityEngine;

// پس‌زمینه‌ی ثابت صحنه (پشت کارت، نوارها، و منو) — همیشه دیده می‌شه و هیچ‌وقت مخفی نمی‌شه
public class BackgroundUI : MonoBehaviour
{
    [Header("Canvas اصلی صحنه")]
    [SerializeField] private Canvas targetCanvas;

    [Header("تصویر پس‌زمینه (مثلاً BGGame)")]
    [SerializeField] private Sprite backgroundSprite;

    void Start()
    {
        GameObject bg = RuntimeUIHelper.CreateImage(targetCanvas.transform, "GameplayBackground", Vector2.zero, Vector2.one, backgroundSprite, stretch: true);
        bg.transform.SetSiblingIndex(0); // همیشه پشت همه‌چیز دیگه بمونه
    }
}
