using UnityEngine;

// پس‌زمینه‌ی ثابت گیم‌پلی. چون کارت یه SpriteRenderer توی دنیاست (نه UI)، و Canvas اصلی
// از نوع Screen Space - Overlay هست (که همیشه روی همه‌چیزِ دنیا رندر می‌شه)، اگه پس‌زمینه رو
// روی Canvas بذاریم کارت رو می‌پوشونه. برای همین پس‌زمینه هم به‌صورت یه SpriteRenderer توی
// دنیا ساخته می‌شه با Sorting Order خیلی پایین، تا پشت کارت و بقیه‌چیز بمونه.
public class BackgroundUI : MonoBehaviour
{
    [Header("تصویر پس‌زمینه (مثلاً BGGame)")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("Sorting Order — باید از کارت (۰) کمتر باشه تا پشتش بمونه")]
    [SerializeField] private int sortingOrder = -100;

    void Start()
    {
        if (backgroundSprite == null) return;

        GameObject bg = new GameObject("GameplayBackground");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingOrder = sortingOrder;

        Camera cam = Camera.main;
        // پس‌زمینه رو دقیقاً هم‌اندازه‌ی دید دوربین بکش
        bg.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;
        Vector2 spriteSize = sr.sprite.bounds.size;
        bg.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
    }
}
