using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    public GameObject stonePrefab;
    public PlayerBeneathStone beneathstone;
    public GameObject player;
    public float spawnInterval = 3f;
    public KeyHoldWithUI kh;

    // 1. 【新增】列表，用于存储所有生成的石头
    private List<GameObject> spawnedStones = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        beneathstone = player.GetComponent<PlayerBeneathStone>();
        kh = player.GetComponent<KeyHoldWithUI>();
        StartCoroutine(SpawnStoneRoutine());
    }

    IEnumerator SpawnStoneRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 2. 检查生成条件：没有被压扁 AND 没有被冻结
            // 注意：我们不再检查 beneathstone.hasStone，因为 Update() 负责销毁
            if (stonePrefab != null && !beneathstone.IsBeneathStone && !kh.isFrozen)
            {
                Vector3 spawnPosition = transform.position;

                // 3. 实例化石头并将其添加到列表中
                GameObject newStone = Instantiate(stonePrefab, spawnPosition, Quaternion.identity);
                spawnedStones.Add(newStone);
            }
            // 否则，不生成，继续等待下一轮
        }
    }

    // Update 是持续运行的，用于检测销毁条件
    void Update()
    {
        // 4. 【新增】检测玩家头上是否有石头
        if (beneathstone.hasStone)
        {
            DestroyAllOtherStones();
        }
    }

    // 5. 【新增】核心销毁方法
    // 5. 核心销毁方法（带安全检查的修改版）
    public void DestroyAllOtherStones()
    {
        // 假设玩家头上的石头就是唯一 isAttached == true 的石头
        // 我们需要找到它，并排除它。

        // 从列表末尾向前遍历，以安全地移除元素
        for (int i = spawnedStones.Count - 1; i >= 0; i--)
        {
            GameObject stone = spawnedStones[i];

            if (stone != null)
            {
                // 尝试获取 UnderStoneDetect 组件
                UnderStoneDetect usd = stone.GetComponent<UnderStoneDetect>();

                // 【关键安全检查】：如果这块石头是附着的（isAttached == true），就跳过销毁
                // 只有当石头是未附着 (isAttached == false) 且不是被冻结的石头时，才销毁。
                if (usd != null && usd.isAttached)
                {
                    // 跳过这块石头，它当前正被玩家使用
                    continue;
                }

                // 立即销毁该石头对象
                Destroy(stone);
            }

            // 无论石头是否被销毁，都将其从跟踪列表中移除，确保列表干净
            spawnedStones.RemoveAt(i);
        }

        if (spawnedStones.Count > 0)
        {
            Debug.Log("已销毁所有额外的石头。");
        }
    }
}