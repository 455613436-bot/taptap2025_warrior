using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 确保引入UI命名空间

public class KeyHoldWithUI : MonoBehaviour
{
    [System.Serializable]
    public class KeyHoldConfig
    {
        public KeyCode targetKey;
        public float requiredHoldTime = 2f;
        public UnityEngine.UI.Image holdProgressBar;

        // --- 新增 (Request 2b) ---
        public Sprite spriteOnHold; // 正在按住时的 Sprite
        public Sprite spriteOnSuccess; // 成功后的 Sprite
        // --- 结束 ---

        [HideInInspector] public float currentHoldTime = 0f;
        [HideInInspector] public bool isKeyHeld = false;
    }

    public List<KeyHoldConfig> keyConfigs = new List<KeyHoldConfig>
    {
        new KeyHoldConfig { targetKey = KeyCode.D, requiredHoldTime = 2f },
        new KeyHoldConfig { targetKey = KeyCode.A, requiredHoldTime = 2f },
        new KeyHoldConfig { targetKey = KeyCode.Space, requiredHoldTime = 2f }
    };
    public PlayerDead pd;
    public bool isFrozen = false;
    public int wallLayer;
    public int stoneLayer;
    public LayerMask layersToIgnoreOnFreeze;
    [Header("副本设置")]
    public Material copyMaterial;
    public float copyLifetime = 2f;
    public Color originalColor;

    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 originalVelocity;
    private float originalAnimatorSpeed;
    public SoundEffect sound;
    private Coroutine unfreezeCoroutine;
    private Coroutine copyCreationCoroutine;
    private List<GameObject> createdCopies = new List<GameObject>();
    public PlayerMove playerMove;
    public UnderStoneDetect USD = null;
    public PlayerBeneathStone playerbeneathstone;
    public KeyHoldConfig lastSuccessfulKeyConfig = null;
    [Header("专属按键设置")]
    public float fadeOutDelay = 1.0f;
    public float fadeOutDuration = 0.5f;
    // --- 新增 (Request 2a) ---
    public Color holdProgressStartColor = Color.green; // 0% 进度的颜色
    // --- 结束 ---

    public KeyHoldConfig currentActiveKeyConfig = null;
    private KeyHoldConfig lastShownKeyConfig = null;
    private float timeSinceLastAction = 0f;

    void Start()
    {
        PlayerDead pd = GetComponent<PlayerDead>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<PlayerMove>();
        originalColor = GetComponent<SpriteRenderer>().color;
        playerbeneathstone = GetComponent<PlayerBeneathStone>();
        if (anim != null)
        {
            originalAnimatorSpeed = anim.speed;
        }

        foreach (var config in keyConfigs)
        {
            if (config.holdProgressBar != null)
            {
                // 初始化时，设置回 "holding" sprite
                if (config.spriteOnHold != null)
                {
                    config.holdProgressBar.sprite = config.spriteOnHold;
                }

                // 设置为起始颜色并完全透明
                Color color = holdProgressStartColor;
                color.a = 0f;
                config.holdProgressBar.color = color;
                config.holdProgressBar.fillAmount = 0f;
            }
        }
        timeSinceLastAction = fadeOutDelay + fadeOutDuration;
    }

    void Update()
    {
        HandleAllKeyHolds();
        UpdateAllUI();
    }

    // --- 重写：HandleAllKeyHolds (Request 1 核心修改) ---
    void HandleAllKeyHolds()
    {
        bool anyKeyAction = false;

        // 1. 检查是否有一个键 *已经* 被锁定并正在计时
        if (currentActiveKeyConfig != null && currentActiveKeyConfig.isKeyHeld)
        {
            // 如果是，我们就只处理这一个键，*忽略*所有其他键的 GetKeyDown

            if (Input.GetKey(currentActiveKeyConfig.targetKey))
            {
                // 键仍然被按住
                anyKeyAction = true;
                timeSinceLastAction = 0f;
                currentActiveKeyConfig.currentHoldTime += Time.deltaTime;

                if (currentActiveKeyConfig.currentHoldTime >= currentActiveKeyConfig.requiredHoldTime && !isFrozen)
                {
                    Debug.Log($"{currentActiveKeyConfig.targetKey} 键长按成功！");
                    OnLongPressSuccess(currentActiveKeyConfig);

                    // 成功后，重置状态，释放锁定
                    currentActiveKeyConfig.currentHoldTime = 0f;
                    currentActiveKeyConfig.isKeyHeld = false;
                    currentActiveKeyConfig = null; // 允许下一帧检测新按键
                }
            }
            else
            {
                // 键被中途松开了
                if (currentActiveKeyConfig.isKeyHeld)
                {
                    // Debug.Log($"{currentActiveKeyConfig.targetKey} 键按压了 {currentActiveKeyConfig.currentHoldTime:F2} 秒");
                    currentActiveKeyConfig.isKeyHeld = false;
                }
                currentActiveKeyConfig.currentHoldTime = 0f;
                currentActiveKeyConfig = null; // 释放锁定，允许下一帧检测新按键
            }
        }
        // 2. 如果没有键被锁定 (isKeyHeld == false)，并且角色未被冻结，才检查 *新* 的按键
        else if (!isFrozen)
        {
            foreach (var config in keyConfigs)
            {
                if (Input.GetKeyDown(config.targetKey))
                {
                    // 找到了一个新按键，锁定它
                    currentActiveKeyConfig = config;
                    currentActiveKeyConfig.currentHoldTime = 0f;
                    currentActiveKeyConfig.isKeyHeld = true; // 设为 true，实现锁定

                    // --- Request 2b: 重置为 "holding" sprite ---
                    if (currentActiveKeyConfig.holdProgressBar != null && currentActiveKeyConfig.spriteOnHold != null)
                    {
                        currentActiveKeyConfig.holdProgressBar.sprite = currentActiveKeyConfig.spriteOnHold;
                    }
                    // --- 结束 ---

                    lastShownKeyConfig = currentActiveKeyConfig;
                    timeSinceLastAction = 0f;
                    anyKeyAction = true;

                    break; // 立即跳出循环，实现“同一时间只激活一个”
                }
            }
        }

        // 3. 更新空闲计时器
        if (!anyKeyAction)
        {
            timeSinceLastAction += Time.deltaTime;
        }
    }


    void UpdateAllUI()
    {
        float targetAlpha = 1f;
        if (timeSinceLastAction > fadeOutDelay)
        {
            float fadeProgress = (timeSinceLastAction - fadeOutDelay) / fadeOutDuration;
            targetAlpha = 1f - Mathf.Clamp01(fadeProgress);
        }

        foreach (var config in keyConfigs)
        {
            if (config.holdProgressBar == null) continue;

            if (config == lastShownKeyConfig)
            {
                // 更新填充和基础颜色
                UpdateUI(config);

                // 应用计算出的透明度
                Color newColor = config.holdProgressBar.color;
                newColor.a = targetAlpha;
                config.holdProgressBar.color = newColor;
            }
            else
            {
                // 强制隐藏其他UI
                Color newColor = config.holdProgressBar.color;
                newColor.a = 0f;
                config.holdProgressBar.color = newColor;
                config.holdProgressBar.fillAmount = 0f;
            }
        }
    }

    // --- 修改：UpdateUI (Request 2a 核心修改) ---
    void UpdateUI(KeyHoldConfig config)
    {
        if (config.holdProgressBar != null)
        {
            float progress = Mathf.Clamp01(config.currentHoldTime / config.requiredHoldTime);
            config.holdProgressBar.fillAmount = progress;

            Color baseColor;
            if (isFrozen)
            {
                baseColor = Color.red;
            }
            else
            {
                // --- Request 2a: 颜色从 'StartColor' 渐变到 '白色' ---
                baseColor = Color.Lerp(holdProgressStartColor, Color.white, progress);
                // --- 结束 ---
            }

            baseColor.a = config.holdProgressBar.color.a;
            config.holdProgressBar.color = baseColor;
        }
    }

    // --- 修改：OnLongPressSuccess (Request 2b 核心修改) ---
    void OnLongPressSuccess(KeyHoldConfig config)
    {
        lastSuccessfulKeyConfig = config;
        if (pd.isDead)
        {
            return;
        }
        sound.PlayGlitchSound();
        // --- Request 2b: 长按成功，切换到 "success" sprite ---
        if (config.holdProgressBar != null && config.spriteOnSuccess != null)
        {
            config.holdProgressBar.sprite = config.spriteOnSuccess;
        }
        // --- 结束 ---

        // 冻结角色
        FreezeCharacter();
        USD = GetComponentInChildren<UnderStoneDetect>();
        if (USD != null)
        {
            Debug.Log("Detachandfreeze");
            USD.DetachAndFreeze();
        }
        else
        {
            Debug.Log("身上没有附着石头。");
        }

        if (copyCreationCoroutine != null)
        {
            StopCoroutine(copyCreationCoroutine);
        }
        copyCreationCoroutine = StartCoroutine(CreateCopiesRoutine());

        if (unfreezeCoroutine != null)
        {
            StopCoroutine(unfreezeCoroutine);
        }
        unfreezeCoroutine = StartCoroutine(UnfreezeAfterDelay(2f));

        Debug.Log($"执行 {config.targetKey} 键长按成功动作 - 角色已冻结");
    }

    IEnumerator CreateCopiesRoutine()
    {
        ClearAllCopies();
        while (isFrozen)
        {
            CreateStaticCopy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void CreateStaticCopy()
    {
        GameObject copy = new GameObject($"{gameObject.name}_FrozenCopy_{Time.time}");
        copy.transform.position = transform.position;
        copy.transform.rotation = transform.rotation;
        copy.transform.localScale = transform.localScale;

        CopyRendererComponents(copy);

        CopyAutoDestroy autoDestroy = copy.AddComponent<CopyAutoDestroy>();
        autoDestroy.lifetime = copyLifetime;
        createdCopies.Add(copy);
        Debug.Log($"创建冻结副本: {copy.name}");
    }

    void CopyRendererComponents(GameObject copy)
    {
        SpriteRenderer originalSpriteRenderer = GetComponent<SpriteRenderer>();
        if (originalSpriteRenderer != null)
        {
            SpriteRenderer copySpriteRenderer = copy.AddComponent<SpriteRenderer>();
            copySpriteRenderer.sortingOrder = originalSpriteRenderer.sortingOrder - 1;
            copySpriteRenderer.flipX = originalSpriteRenderer.flipX;
            copySpriteRenderer.flipY = originalSpriteRenderer.flipY;
            if (copyMaterial != null)
            {
                copySpriteRenderer.material = copyMaterial;
            }
        }

        if (anim != null)
        {
            Animator copyAnimator = copy.AddComponent<Animator>();
            copyAnimator.runtimeAnimatorController = anim.runtimeAnimatorController;
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            copyAnimator.Play(stateInfo.fullPathHash, 0, stateInfo.normalizedTime);
            copyAnimator.speed = 0f;
        }
    }

    void ClearAllCopies()
    {
        foreach (GameObject copy in createdCopies)
        {
            if (copy != null)
            {
                Destroy(copy);
            }
        }
        createdCopies.Clear();
    }

    void FreezeCharacter()
    {
        if (isFrozen) return;
        isFrozen = true;

        if (rb != null)
        {
            originalVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
            playerMove.UpdateLayerCollision(layersToIgnoreOnFreeze, true);
        }

        if (anim != null)
        {
            anim.speed = 0f;
        }
        SpriteRenderer originalSpriteRenderer = GetComponent<SpriteRenderer>();
        originalSpriteRenderer.color = new Color(0.5f, 0.8f, 1f, 0f);
    }

    // --- 修改：UnfreezeCharacter (Request 2b 修改) ---
    void UnfreezeCharacter()
    {
        if (!isFrozen) return;
        isFrozen = false;

        if (copyCreationCoroutine != null)
        {
            StopCoroutine(copyCreationCoroutine);
            copyCreationCoroutine = null;
        }

        if (anim != null)
        {
            anim.speed = originalAnimatorSpeed;
        }
        SpriteRenderer nowRen = GetComponent<SpriteRenderer>();
        nowRen.color = originalColor;
        playerMove.UpdateLayerCollision(layersToIgnoreOnFreeze, false);

        // 重置所有按键的UI状态
        foreach (var config in keyConfigs)
        {
            config.currentHoldTime = 0f;
            config.isKeyHeld = false;
            if (config.holdProgressBar != null)
            {
                config.holdProgressBar.fillAmount = 0f;

                // --- Request 2b: 重置回 "holding" sprite ---
                if (config.spriteOnHold != null)
                {
                    config.holdProgressBar.sprite = config.spriteOnHold;
                }
                // --- 结束 ---

                // --- Request 2a: 重置回 "start" 颜色并隐藏 ---
                Color color = holdProgressStartColor; // 使用起始颜色
                color.a = 0f;
                config.holdProgressBar.color = color;
                // --- 结束 ---
            }
        }

        currentActiveKeyConfig = null;
        lastShownKeyConfig = null;
        lastSuccessfulKeyConfig = null;
        timeSinceLastAction = fadeOutDelay + fadeOutDuration;
        foreach (var config in keyConfigs)
        {
            if (Input.GetKey(config.targetKey))
            {
                // 找到了！立即将此键设为活动键
                currentActiveKeyConfig = config;
                currentActiveKeyConfig.currentHoldTime = 0f;
                currentActiveKeyConfig.isKeyHeld = true; // 设为 true, 实现锁定

                // 确保 sprite 是正确的
                if (currentActiveKeyConfig.holdProgressBar != null && currentActiveKeyConfig.spriteOnHold != null)
                {
                    currentActiveKeyConfig.holdProgressBar.sprite = currentActiveKeyConfig.spriteOnHold;
                }

                // 设置为"最后显示"
                lastShownKeyConfig = currentActiveKeyConfig;
                timeSinceLastAction = 0f;

                // 立即更新一次UI，使其显示为0%（而不是透明）
                UpdateUI(currentActiveKeyConfig);
                if (currentActiveKeyConfig.holdProgressBar != null)
                {
                    Color newColor = currentActiveKeyConfig.holdProgressBar.color;
                    newColor.a = 1f; // 设为不透明
                    currentActiveKeyConfig.holdProgressBar.color = newColor;
                }

                break; // 只检测第一个被按住的键，符合我们的锁定逻辑
            }
        }
        Debug.Log("角色已解冻，恢复正常运动");
    }

    IEnumerator UnfreezeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnfreezeCharacter();
        if (USD != null)
        {
            USD.RecoverStoneRB();
            USD = null;
            playerbeneathstone.PressedStateExit();
        }

        yield return new WaitForSeconds(0.5f);
        ClearAllCopies();
    }

    public void ForceUnfreeze()
    {
        if (unfreezeCoroutine != null)
        {
            StopCoroutine(unfreezeCoroutine);
        }
        UnfreezeCharacter();
        ClearAllCopies();
    }

    void OnDisable()
    {
        if (isFrozen)
        {
            UnfreezeCharacter();
        }
        ClearAllCopies();
    }
}

public class CopyAutoDestroy : MonoBehaviour
{
    public float lifetime = 2f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}