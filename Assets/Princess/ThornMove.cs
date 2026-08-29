using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornMove : MonoBehaviour
{
    [Header("运动设置")]
    [Tooltip("尖刺的初始速度")]
    public float initialSpeed = 2.0f;
    public float rotationSpeed = 360.0f;
    [Tooltip("每秒增加的速度（加速度）")]
    public float acceleration = 3.0f;

    [Tooltip("尖刺的最大速度")]
    public float maxSpeed = 15.0f;

    [Header("生命周期")]
    [Tooltip("尖刺存活的时间（秒）")]
    public float lifetime = 5.0f;

    // --- 私有变量 ---
    private Rigidbody2D rb2d;
    private Vector2 forwardDirection;
    private float currentSpeed;
    private bool isInitialized = false;
    private bool isRotating = false;
    private Quaternion targetRotation;

    // --- (!!) 新增：对 KeyHold 脚本的引用 ---
    private KeyHoldWithUI playerKeyHoldRef;

    // (Start() 在这个脚本中不是必需的)

    // --- (!!) 修改：Initialize 方法，添加新参数 ---
    public void Initialize(bool spriteFacesUp, Quaternion targetRotation, KeyHoldWithUI keyHoldRef)
    {
        rb2d = GetComponent<Rigidbody2D>();
        currentSpeed = initialSpeed;

        // --- (!!) 新增：存储引用 ---
        this.playerKeyHoldRef = keyHoldRef;

        if (spriteFacesUp)
        {
            forwardDirection = transform.up;
        }
        else
        {
            forwardDirection = transform.right;
        }
        this.targetRotation = targetRotation;
        this.isRotating = true;

        // --- (!!) 修改：使用可暂停的协程来处理生命周期 ---
        // Destroy(gameObject, lifetime); (旧代码)
        StartCoroutine(LifetimeRoutine()); // (新代码)
        // --- 修改结束 ---

        isInitialized = true;
    }

    void FixedUpdate()
    {
        // --- (!!) 新增：检查是否冻结 ---
        if (playerKeyHoldRef != null && playerKeyHoldRef.isFrozen)
        {
            if (rb2d != null) rb2d.velocity = Vector2.zero; // 确保物理停止
            return; // 跳过移动
        }
        // --- 新增结束 ---

        if (!isInitialized || isRotating)
        {
            if (rb2d != null)
            {
                rb2d.velocity = Vector2.zero;
            }
            return;
        }

        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        forwardDirection = transform.up;


        rb2d.velocity = forwardDirection * currentSpeed;
    }

    void Update()
    {
        // --- (!!) 新增：检查是否冻结 ---
        if (playerKeyHoldRef != null && playerKeyHoldRef.isFrozen)
        {
            return; // 跳过旋转
        }
        // --- 新增结束 ---

        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false; // 停止旋转
            }
        }
    }

    // --- (!!) 新增：可暂停的生命周期协程 ---
    IEnumerator LifetimeRoutine()
    {
        float timer = 0f;
        while (timer < lifetime)
        {
            // 只有在未冻结时才增加计时器
            if (playerKeyHoldRef == null || !playerKeyHoldRef.isFrozen)
            {
                timer += Time.deltaTime;
            }
            yield return null; // 等待下一帧
        }
        Destroy(gameObject); // 时间到了，销毁自己
    }
    // --- 新增结束 ---
}