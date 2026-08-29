using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("重生点数据")]
    public Vector3 respawnPosition;
    //public string currentCheckpointName;
    public bool checkpointActivated = false;

    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁

            // 初始化默认重生点
            InitializeDefaultRespawn();
        }
        else
        {
            // 如果已存在实例，销毁新创建的
            Destroy(gameObject);
        }
    }

    void InitializeDefaultRespawn()
    {
        // 你可以在这里设置初始重生点
        // 或者在第一个检查点被激活时设置
        respawnPosition = transform.position;
        //currentCheckpointName = "初始点";
        checkpointActivated = false;
    }

    // 设置新的重生点
    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
        //currentCheckpointName = checkpointName;
        checkpointActivated = true;

        Debug.Log($"重生点已更新 位置: {position}");
    }

    // 获取当前重生点位置
    public Vector3 GetRespawnPosition()
    {
        return respawnPosition;
    }

    // 重置为默认重生点
    public void ResetToDefault()
    {
        checkpointActivated = false;
        //currentCheckpointName = "初始点";
        // 注意：这里不重置respawnPosition，因为可能需要保留位置
    }
}