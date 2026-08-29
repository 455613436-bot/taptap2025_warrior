using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("攻击设置")]
    [Tooltip("要实例化的2D尖刺Prefab")]
    public GameObject thornPrefab;
    public KeyHoldWithUI playerKeyHold;
    [Tooltip("尖刺生成的半径（与Boss的距离）")]
    public float spawnRadius = 3.0f;

    [Tooltip("尖刺发射的速度")]
    public float thornSpeed = 8.0f;
    public float spawnDelay = 0.2f; // 特性 3
    [Tooltip("每次攻击的间隔时间")]
    public float attackCooldown = 3.0f;

    [Tooltip("生成尖刺的角度间隔")]
    [SerializeField] // [SerializeField] 使得私有变量也能在Inspector中显示和修改
    private float _angleStep = 30.0f;
    public bool spriteFacesUp = true;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        // 无限循环，以便Boss可以周期性攻击
        while (true)
        {
            // 等待冷却时间
            yield return new WaitForSeconds(attackCooldown);

            // 执行攻击波次协程
            // 'yield return' 会让这个协程暂停，直到 SpawnThornWave 协程执行完毕
            // 这样可以防止在上一波未发射完时就开始下一次冷却计时
            yield return StartCoroutine(SpawnThornWave());
        }
    }

    IEnumerator SpawnThornWave()
    {
        Vector2 bossPosition = (Vector2)transform.position;

        // 循环从 -90度 (左) 到 +90度 (右)
        for (float angleOffset = -90; angleOffset <= 90; angleOffset += _angleStep)
        {
            // --- 1. 计算位置 (与之前相同) ---
            float currentAngleDeg = 270 + angleOffset;
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
            float x = Mathf.Cos(currentAngleRad) * spawnRadius;
            float y = Mathf.Sin(currentAngleRad) * spawnRadius;
            Vector2 spawnOffset = new Vector2(x, y);
            Vector2 spawnPosition = bossPosition + spawnOffset;

            // --- 2. 计算旋转 (与之前相同) ---
            Vector2 directionFromBoss = spawnOffset.normalized;
            float zRotationRad = Mathf.Atan2(directionFromBoss.y, directionFromBoss.x);
            float zRotationDeg = zRotationRad * Mathf.Rad2Deg;
            if (spriteFacesUp)
            {
                zRotationDeg -= 90.0f;
            }
            Quaternion targetSpawnRotation = Quaternion.Euler(0, 0, zRotationDeg);

            // --- 3. 实例化并配置 (已修改) ---

            // (修改) 实例化时，使用 Prefab 的 *原始* 旋转
            GameObject newThorn = Instantiate(
                thornPrefab,
                spawnPosition,
                thornPrefab.transform.rotation // 使用Prefab的默认旋转
            );

            // 获取新尖刺上的 ThornMovement 脚本
            ThornMove thornScript = newThorn.GetComponent<ThornMove>();
            if (thornScript != null)
            {
                // *** 关键：调用新脚本的初始化方法，并传入 Sprite 朝向 ***
                thornScript.Initialize(this.spriteFacesUp, targetSpawnRotation, playerKeyHold);
            }
            else
            {
                // 确保 Prefab 上添加了新脚本
                Debug.LogWarning("尖刺 Prefab 上没有 ThornMovement 脚本！请添加该脚本。");
            }

            // --- 4. 间隔等待 (特性 3) ---
            // 等待指定的时间后再生成下一个尖刺
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
