using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMovement : MonoBehaviour
{
    [Header("目标设置 (新)")]
    [Tooltip("玩家的游戏对象")]
    public GameObject player;

    [Tooltip("在玩家头顶生成目标的偏移量")]
    public Vector2 targetOffset = new Vector2(0f, 4f);

    [Tooltip("在目标点周围随机选择的区域大小 (X为宽, Y为高)")]
    public Vector2 targetAreaSize = new Vector2(6f, 3f);

    [Header("速度设置")]
    [Tooltip("开始移动时的初始速度")]
    public float startSpeed = 1.0f;
    [Tooltip("移动的最大速度")]
    public float maxSpeed = 8.0f;
    [Tooltip("加速度 (每秒增加的速度)")]
    public float acceleration = 5.0f;

    [Header("视觉效果 (新)")]
    [Tooltip("Boss上下移动时的最大倾斜角度")]
    public float maxTiltAngle = 15.0f;
    [Tooltip("Boss恢复水平或倾斜时的旋转速度")]
    public float tiltSpeed = 5.0f;

    [Header("停顿设置")]
    [Tooltip("到达目标点后停顿的最短时间")]
    public float minIdleTime = 1.0f;
    [Tooltip("到达目标点后停顿的最长时间")]
    public float maxIdleTime = 3.0f;

    // --- 私有变量 ---
    private Rigidbody2D rb2d;
    private Vector2 targetPosition; // 当前移动的目标点
    private float currentSpeed;

    private enum State { Idle, Moving }
    private State currentState = State.Idle;
    private float idleTimer;

    // (新) 用于翻转和旋转
    private float originalScaleX;
    private float currentTargetZRotation = 0f;


    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d.bodyType != RigidbodyType2D.Kinematic)
        {
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0;
        }
    }

    void Start()
    {
        // (新) 存储原始的X缩放值，用于翻转
        originalScaleX = transform.localScale.x;

        // 移除 startPosition 逻辑
        currentSpeed = 0f;
        currentState = State.Idle;
        // 游戏开始时先停顿
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
    }

    void Update()
    {
        // --- 1. 状态机逻辑 ---
        if (currentState == State.Idle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                currentState = State.Moving;
                currentSpeed = startSpeed; // 重置为初始速度
                PickNewTarget(); // (重要) 在 *开始移动时* 才选择新目标
            }
        }

        // --- 2. 旋转逻辑 (新) ---
        // 无论何时，都平滑地转向 'currentTargetZRotation'
        Quaternion targetRotation = Quaternion.Euler(0, 0, currentTargetZRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (currentState == State.Moving)
        {
            // --- 1. 计算方向与视觉 (新) ---
            Vector2 moveDirection = (targetPosition - rb2d.position).normalized;

            // 需求 1: 左右翻转
            if (moveDirection.x < -0.1f) // 向左移动
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }
            else if (moveDirection.x > 0.1f) // 向右移动
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }

            // 需求 2: 上下倾斜 (设置目标角度，由 Update 平滑执行)
            if (moveDirection.y > 0.2f) // 向上移动
            {
                currentTargetZRotation = maxTiltAngle;
            }
            else if (moveDirection.y < -0.2f) // 向下移动
            {
                currentTargetZRotation = -maxTiltAngle;
            }
            else // 水平移动
            {
                currentTargetZRotation = 0f;
            }

            // --- 2. 加速逻辑 (不变) ---
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }

            // --- 3. 移动逻辑 (不变) ---
            Vector2 newPosition = Vector2.MoveTowards(
                rb2d.position,
                targetPosition,
                currentSpeed * Time.fixedDeltaTime
            );
            rb2d.MovePosition(newPosition);

            // --- 4. 到达检测 (逻辑优化) ---
            // (我们把 'PickNewTarget' 移到了 Update 中)
            if (Vector2.Distance(rb2d.position, targetPosition) < 0.1f) // 阈值可以稍大一点
            {
                currentState = State.Idle; // 到达，切换到停顿
                idleTimer = Random.Range(minIdleTime, maxIdleTime); // 设置随机停顿时间
                currentSpeed = 0f;
                currentTargetZRotation = 0f; // 停下时恢复水平
            }
        }
        else // 如果是 Idle 状态
        {
            // 确保停顿时，Boss会慢慢恢复水平
            currentTargetZRotation = 0f;
        }
    }

    // (已完全重写) 需求 3: 在玩家头顶选择目标
    void PickNewTarget()
    {
        if (player == null)
        {
            Debug.LogError("BossMovement 错误: 'Player' 未在 Inspector 中设置!");
            targetPosition = transform.position; // 原地不动
            return;
        }

        // 1. 获取玩家位置
        Vector2 playerPos = player.transform.position;
        // 2. 计算目标区域的中心点 (玩家位置 + 偏移)
        Vector2 targetCenter = playerPos + targetOffset;

        // 3. 在该区域内随机选择一个点
        float targetX = Random.Range(targetCenter.x - targetAreaSize.x / 2, targetCenter.x + targetAreaSize.x / 2);
        float targetY = Random.Range(targetCenter.y - targetAreaSize.y / 2, targetCenter.y + targetAreaSize.y / 2);

        targetPosition = new Vector2(targetX, targetY);
    }

    // (已修改) 绘制Gizmos以显示新的目标区域
    void OnDrawGizmosSelected()
    {
        // 仅当 player 被设置时才绘制
        if (player != null)
        {
            // 计算目标区域的中心
            Vector2 center = (Vector2)player.transform.position + targetOffset;
            // 计算区域大小
            Vector2 size = targetAreaSize;

            // 绘制一个红色的线框来显示随机区域
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}