using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 确保 Boss 拥有所有必需的组件
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    // --- 状态机核心 ---
    private enum State
    {
        Idle,
        Moving,
        SpikeAttack,
        DownAttack
    }
    private State currentState;
    private float idleTimer;
    private int actionCycleIndex = 0;
    public SoundEffect sound;
    [Header("1. 玩家目标设置")]
    [Tooltip("玩家的游戏对象")]
    public GameObject player;

    // --- (!!) 新增：对玩家 KeyHold 脚本的引用 ---
    [Tooltip("拖拽玩家对象上的 KeyHoldWithUI 脚本到这里")]
    public KeyHoldWithUI playerKeyHold;
    // --- 新增结束 ---

    [Tooltip("在玩家头顶生成目标的偏移量")]
    public Vector2 targetOffset = new Vector2(0f, 4f);
    [Tooltip("在目标点周围随机选择的区域大小")]
    public Vector2 targetAreaSize = new Vector2(6f, 3f);

    [Header("2. 移动与速度")]
    public float startSpeed = 1.0f;
    public float maxSpeed = 8.0f;
    public float acceleration = 5.0f;

    [Header("3. 停顿设置")]
    public float minIdleTime = 1.0f;
    public float maxIdleTime = 3.0f;

    [Header("4. 视觉效果")]
    public float maxTiltAngle = 15.0f;
    public float tiltSpeed = 5.0f;

    [Header("5. 尖刺攻击 (来自 BossAttack)")]
    public GameObject thornPrefab;
    public float spawnRadius = 3.0f;
    public float spawnDelay = 0.2f;
    public bool spriteFacesUp = true;
    [SerializeField]
    private float _angleStep = 30.0f;

    [Header("6. 下劈攻击 (新)")]
    public float downAttackDuration = 1.5f;
    public Vector2 attackColliderSize = new Vector2(2.0f, 4.0f);
    public Vector2 attackColliderOffset = new Vector2(0f, -2.0f);

    [Header("7. 移动范围限制 (新)")]
    public float minX = -20f;
    public float maxX = 20f;
    public float minY = -10f;
    public float maxY = 10f;

    private Rigidbody2D rb2d;
    private Animator animator;
    private Collider2D bossCollider;

    [Header("8. 生命值与 UI (新)")]
    public float maxHealth = 100f;
    public Slider healthBarSlider;

    private float currentHealth;
    private Vector2 targetPosition;
    private float currentSpeed;
    private float originalScaleX;
    private float currentTargetZRotation = 0f;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    // --- (!!) 新增：用于在暂停时恢复动画速度 ---
    private float originalAnimatorSpeed = 1f;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bossCollider = GetComponent<Collider2D>();
        sound.PlayScareSound();
        if (bossCollider is BoxCollider2D)
        {
            originalColliderSize = ((BoxCollider2D)bossCollider).size;
            originalColliderOffset = bossCollider.offset;
        }
        else if (bossCollider is CapsuleCollider2D)
        {
            originalColliderSize = ((CapsuleCollider2D)bossCollider).size;
            originalColliderOffset = bossCollider.offset;
        }
        else
        {
            Debug.LogWarning("未知的碰撞框类型！下劈攻击可能无法正确改变碰撞框。");
            originalColliderSize = Vector2.one;
            originalColliderOffset = Vector2.zero;
        }

        if (rb2d.bodyType != RigidbodyType2D.Kinematic)
        {
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0;
        }
        playerKeyHold = player.GetComponent<KeyHoldWithUI>();
        // --- (!!) 新增：存储原始动画速度 ---
        originalAnimatorSpeed = animator.speed;
    }

    void Start()
    {
        // --- (!!) 新增：自动查找 playerKeyHold (如果未在 Inspector 中设置) ---
        if (player != null && playerKeyHold == null)
        {
            playerKeyHold = player.GetComponent<KeyHoldWithUI>();
        }
        if (playerKeyHold == null)
        {
            Debug.LogError("BossController 错误: 'playerKeyHold' 未设置! 无法实现冻结暂停。");
        }
        // --- 新增结束 ---

        originalScaleX = transform.localScale.x;
        currentSpeed = 0f;
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
            healthBarSlider.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("BossController 错误: 'healthBarSlider' 未在 Inspector 中设置!");
        }

        currentState = State.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
    }

    // Update 是我们的“状态机大脑”
    void Update()
    {
        // --- (!!) 新增：检查玩家是否冻结 ---
        if (playerKeyHold != null && playerKeyHold.isFrozen)
        {
            animator.speed = 0f; // 暂停动画
            return; // 跳过所有逻辑
        }
        animator.speed = originalAnimatorSpeed; // 恢复动画
        // --- 新增结束 ---


        // 状态机：检查当前状态并执行相应逻辑
        switch (currentState)
        {
            case State.Idle:
                // --- 停顿状态 ---
                // 1. 计时
                idleTimer -= Time.deltaTime;

                // 2. 计时结束，决定下一个动作
                if (idleTimer <= 0)
                {
                    PerformNextActionInCycle();
                }
                break;

            case State.Moving:
                break;

            case State.SpikeAttack:
                break;

            case State.DownAttack:
                break;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, currentTargetZRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    // FixedUpdate 专门处理物理
    void FixedUpdate()
    {
        // --- (!!) 新增：检查玩家是否冻结 ---
        if (playerKeyHold != null && playerKeyHold.isFrozen)
        {
            rb2d.velocity = Vector2.zero; // 确保 Boss 物理上停止
            return; // 跳过所有移动逻辑
        }
        // --- 新增结束 ---

        if (currentState == State.Moving)
        {
            Vector2 moveDirection = (targetPosition - rb2d.position).normalized;

            if (moveDirection.x < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }
            else if (moveDirection.x > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }

            if (moveDirection.y > 0.2f) currentTargetZRotation = maxTiltAngle;
            else if (moveDirection.y < -0.2f) currentTargetZRotation = -maxTiltAngle;
            else currentTargetZRotation = 0f;

            if (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }

            Vector2 newPosition = Vector2.MoveTowards(
                rb2d.position,
                targetPosition,
                currentSpeed * Time.fixedDeltaTime
            );
            rb2d.MovePosition(newPosition);

            if (Vector2.Distance(rb2d.position, targetPosition) < 0.1f)
            {
                currentState = State.Idle;
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                currentSpeed = 0f;
                currentTargetZRotation = 0f;
            }
        }
        else
        {
            currentTargetZRotation = 0f;
        }
    }

    void PerformNextActionInCycle()
    {
        switch (actionCycleIndex)
        {
            case 0:
                currentState = State.SpikeAttack;
                StartCoroutine(SpawnThornWave());
                break;

            case 1:
                currentState = State.DownAttack;
                StartCoroutine(DownAttackRoutine());
                break;

            case 2:
                currentState = State.Moving;
                currentSpeed = startSpeed;
                PickNewTarget();
                break;
        }

        actionCycleIndex = (actionCycleIndex + 1) % 3;
    }

    void PickNewTarget()
    {
        if (player == null)
        {
            Debug.LogError("BossController 错误: 'Player' 未在 Inspector 中设置!");
            targetPosition = transform.position;
            return;
        }

        Vector2 playerPos = player.transform.position;
        Vector2 targetCenter = playerPos + targetOffset;
        float targetX = Random.Range(targetCenter.x - targetAreaSize.x / 2, targetCenter.x + targetAreaSize.x / 2);
        float targetY = Random.Range(targetCenter.y - targetAreaSize.y / 2, targetCenter.y + targetAreaSize.y / 2);
        float clampedX = Mathf.Clamp(targetX, minX, maxX);
        float clampedY = Mathf.Clamp(targetY, minY, maxY);
        targetPosition = new Vector2(clampedX, clampedY);
    }


    // --- 攻击协程 1: 尖刺攻击 ---
    IEnumerator SpawnThornWave()
    {
        Vector2 bossPosition = (Vector2)transform.position;
        Vector2 playerPosition = player.transform.position;
        if ((playerPosition.x - bossPosition.x) * transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        Vector2 directionToPlayer = (playerPosition - bossPosition).normalized;
        float centerAngleDeg = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        sound.PlayThornSound();
        for (float angleOffset = -90; angleOffset <= 90; angleOffset += _angleStep)
        {
            float currentAngleDeg = centerAngleDeg + angleOffset;
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;

            float x = Mathf.Cos(currentAngleRad) * spawnRadius;
            float y = Mathf.Sin(currentAngleRad) * spawnRadius;
            Vector2 spawnOffset = new Vector2(x, y);
            Vector2 spawnPosition = bossPosition + spawnOffset;

            Vector2 directionFromBoss = spawnOffset.normalized;
            float zRotationRad = Mathf.Atan2(directionFromBoss.y, directionFromBoss.x);
            float zRotationDeg = zRotationRad * Mathf.Rad2Deg;
            if (spriteFacesUp)
            {
                zRotationDeg -= 90.0f;
            }
            Quaternion targetSpawnRotation = Quaternion.Euler(0, 0, zRotationDeg);

            GameObject newThorn = Instantiate(
                thornPrefab,
                spawnPosition,
                thornPrefab.transform.rotation
            );

            ThornMove thornScript = newThorn.GetComponent<ThornMove>();
            if (thornScript != null)
            {
                // --- (!!) 修改：传递 KeyHold 引用 ---
                thornScript.Initialize(this.spriteFacesUp, targetSpawnRotation, playerKeyHold);
            }
            else
            {
                Debug.LogWarning("尖刺 Prefab 上没有 ThornMove 脚本！");
            }

            // --- (!!) 修改：自定义的可暂停延迟 ---
            // yield return new WaitForSeconds(spawnDelay); (旧代码)
            yield return StartCoroutine(WaitWhileFrozen(spawnDelay)); // (新代码)
            // --- 修改结束 ---
        }

        currentState = State.Moving;
        currentSpeed = startSpeed;
        PickNewTarget();
    }

    // --- 攻击协程 2: 下劈攻击 ---
    IEnumerator DownAttackRoutine()
    {
        Vector2 bossPosition = (Vector2)transform.position;
        Vector2 playerPosition = player.transform.position;
        if ((playerPosition.x - bossPosition.x) * transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        animator.SetBool("downAttack", true);
        sound.PlayDownSound();
        if (bossCollider is BoxCollider2D)
        {
            ((BoxCollider2D)bossCollider).size = attackColliderSize;
            bossCollider.offset = attackColliderOffset;
        }
        else if (bossCollider is CapsuleCollider2D)
        {
            ((CapsuleCollider2D)bossCollider).size = attackColliderSize;
            bossCollider.offset = attackColliderOffset;
        }

        // --- (!!) 修改：自定义的可暂停延迟 ---
        // yield return new WaitForSeconds(downAttackDuration); (旧代码)
        yield return StartCoroutine(WaitWhileFrozen(downAttackDuration)); // (新代码)
        // --- 修改结束 ---

        animator.SetBool("downAttack", false);

        if (bossCollider is BoxCollider2D)
        {
            ((BoxCollider2D)bossCollider).size = originalColliderSize;
            bossCollider.offset = originalColliderOffset;
        }
        else if (bossCollider is CapsuleCollider2D)
        {
            ((CapsuleCollider2D)bossCollider).size = originalColliderSize;
            bossCollider.offset = originalColliderOffset;
        }

        currentState = State.Moving;
        currentSpeed = startSpeed;
        PickNewTarget();
    }

    // --- (!!) 新增：可暂停的等待协程 ---
    IEnumerator WaitWhileFrozen(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            // 只有在未冻结时才增加计时器
            if (playerKeyHold == null || !playerKeyHold.isFrozen)
            {
                timer += Time.deltaTime;
            }
            yield return null; // 等待下一帧
        }
    }
    // --- 新增结束 ---

    private IEnumerator RecoverAnim()
    {
        // 等待 2 秒
        yield return new WaitForSeconds(2f);

        // 恢复动画（Boss 还在，所以不需要 null 检查）
        ChangeAnim(false);
    }
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        sound.PlayGlitchHitSound();
        // 如果已经有恢复协程在运行，先停止它，防止冲突
        StopCoroutine("RecoverAnim");

        // 立即启动动画（受伤）
        ChangeAnim(true);

        // 启动恢复协程
        StartCoroutine("RecoverAnim");
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ChangeAnim(bool glitchstate)
    {
        animator.SetBool("Isglitch", glitchstate);
    }
    void Die()
    {
        Debug.Log("Boss 已被击败!");
        StopAllCoroutines();
        this.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;
        if (rb2d != null) rb2d.velocity = Vector2.zero;
        if (healthBarSlider != null)
        {
            healthBarSlider.gameObject.SetActive(false);
        }
        SceneManager.LoadScene("EndScene");
        // animator.SetTrigger("Death");
        // Destroy(gameObject, 3.0f); 
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Vector2 center = (Vector2)player.transform.position + targetOffset;
            Vector2 size = targetAreaSize;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}