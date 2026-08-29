using UnityEngine;
using UnityEngine.UI;
using static KeyHoldWithUI;

public class ArrowIndicator : MonoBehaviour
{
    [Header("引用")]
    public KeyHoldWithUI playerKeyHold;
    public GameObject projectilePrefab;
    private RectTransform arrowRect;
    private Camera mainCamera;
    private Image arrowImage;

    [Header("路径设置")]
    public float padding = 50f;
    [Tooltip("箭头在路径上每秒移动的百分比 (0.25 = 4秒绕一圈)")]
    public float pathSpeedNormalized = 0.25f;

    [Header("发射设置")]
    public float launchSpeed = 20f;

    // --- (!!) 新增：默认视觉效果 (空闲时显示) ---
    [Header("默认视觉效果 (空闲时)")]
    [Tooltip("未按键且未冻结时，显示的默认贴图")]
    public Sprite defaultSprite;
    [Tooltip("未按键且未冻结时，显示的默认颜色")]
    public Color defaultColor = Color.white;
    // --- 新增结束 ---

    // --- 状态变量 ---
    private Vector2 lastInputDirection = Vector2.right;
    private bool wasFrozenLastFrame = false;
    private float pathProgress = 0f;

    void Start()
    {
        arrowRect = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        arrowImage = GetComponent<Image>();

        if (arrowImage == null)
        {
            Debug.LogError("ArrowIndicator 脚本需要一个 Image 组件才能工作！", this);
        }
        else
        {
            // --- (!!) 修改：始终可见，并设置为默认状态 ---
            arrowImage.enabled = true; // 始终可见
            arrowImage.sprite = defaultSprite;
            arrowImage.color = defaultColor;
            // --- 修改结束 ---
        }

        if (playerKeyHold == null)
        {
            playerKeyHold = FindObjectOfType<KeyHoldWithUI>();
        }
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab 未设置!");
        }

        lastInputDirection = Vector2.right;
        pathProgress = 0f;
        arrowRect.position = GetPositionOnPath(pathProgress);
        UpdateArrowRotation();

        wasFrozenLastFrame = playerKeyHold.isFrozen;
    }

    void Update()
    {
        HandleInput();
        UpdateArrowRotation();

        // --- (!!) 核心修改：调用新的视觉更新方法 ---
        UpdateHoldVisuals();
        // --- 修改结束 ---

        // 3. 检查冻结状态 (用于移动和发射)
        if (playerKeyHold.isFrozen)
        {
            // 冻结状态：箭头停止在路径上移动。
        }
        else
        {
            // 非冻结状态：
            pathProgress += pathSpeedNormalized * Time.deltaTime;
            pathProgress = Mathf.Repeat(pathProgress, 1.0f);
            arrowRect.position = GetPositionOnPath(pathProgress);

            if (wasFrozenLastFrame)
            {
                LaunchProjectile();
            }
        }

        wasFrozenLastFrame = playerKeyHold.isFrozen;
    }

    // --- (!!) 核心重写：UpdateHoldVisuals ---
    void UpdateHoldVisuals()
    {
        if (arrowImage == null) return;

        // 检查 KeyHoldWithUI 的状态
        KeyHoldConfig activeConfig = playerKeyHold.currentActiveKeyConfig;
        KeyHoldConfig successfulConfig = playerKeyHold.lastSuccessfulKeyConfig;

        if (playerKeyHold.isFrozen && successfulConfig != null)
        {
            // 状态 1: 冻结 (长按成功)
            // 使用 *成功配置* 中的 "OnSuccess" 贴图
            arrowImage.sprite = successfulConfig.spriteOnSuccess;
            arrowImage.color = Color.white; // 100% 成功的颜色
        }
        else if (activeConfig != null && activeConfig.isKeyHeld)
        {
            // 状态 2: 正在长按
            // 使用 *活动配置* 中的 "OnHold" 贴图
            arrowImage.sprite = activeConfig.spriteOnHold;

            // 使用 *KeyHoldWithUI* 的全局 "StartColor" 进行渐变
            float progress = Mathf.Clamp01(activeConfig.currentHoldTime / activeConfig.requiredHoldTime);
            arrowImage.color = Color.Lerp(playerKeyHold.holdProgressStartColor, Color.white, progress);
        }
        else
        {
            // 状态 3: 空闲 (未按键, 未冻结)
            // 恢复为你在 ArrowIndicator 上设置的默认贴图和颜色
            arrowImage.sprite = defaultSprite;
            arrowImage.color = defaultColor;
        }
    }
    // --- 重写结束 ---


    void HandleInput()
    {
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.D)) input.x = 1;
        if (Input.GetKey(KeyCode.A)) input.x = -1;
        if (Input.GetKey(KeyCode.Space)) input.y = 1;

        if (input.magnitude > 0.1f)
        {
            lastInputDirection = input.normalized;
        }
    }

    void UpdateArrowRotation()
    {
        float angle = Mathf.Atan2(lastInputDirection.y, lastInputDirection.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0, 0, angle);
    }

    void LaunchProjectile()
    {
        Debug.Log($"发射! 方向: {lastInputDirection}");

        Vector3 screenPos = arrowRect.position;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        if (projectilePrefab != null)
        {
            Quaternion launchRotation = Quaternion.Euler(0, 0, Mathf.Atan2(lastInputDirection.y, lastInputDirection.x) * Mathf.Rad2Deg);
            GameObject projectile = Instantiate(projectilePrefab, worldPos, launchRotation);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = lastInputDirection * launchSpeed;
            }
        }
    }

    Vector2 GetPositionOnPath(float progress)
    {
        float minX = padding;
        float maxX = Screen.width - padding;
        float minY = padding;
        float maxY = Screen.height - padding;

        progress = Mathf.Repeat(progress, 1.0f);

        if (progress < 0.25f)
        {
            float t = progress / 0.25f;
            return new Vector2(Mathf.Lerp(minX, maxX, t), minY);
        }
        else if (progress < 0.5f)
        {
            float t = (progress - 0.25f) / 0.25f;
            return new Vector2(maxX, Mathf.Lerp(minY, maxY, t));
        }
        else if (progress < 0.75f)
        {
            float t = (progress - 0.5f) / 0.25f;
            return new Vector2(Mathf.Lerp(maxX, minX, t), maxY);
        }
        else
        {
            float t = (progress - 0.75f) / 0.25f;
            return new Vector2(minX, Mathf.Lerp(maxY, minY, t));
        }
    }
}