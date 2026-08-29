using UnityEngine;
using System.Collections;
public class ProjectileControl : MonoBehaviour
{
    [Header("碰撞设置")]
    [Tooltip("子弹应该在碰撞时销毁自己的图层名称")]
    public string targetLayerName = "Princess";

    // 存储目标图层的整数 ID
    private int targetLayer;

    void Start()
    {
        // 将图层名称 (string) 转换为整数 (int) 以提高效率
        // 这样我们就不需要在每次碰撞时都进行字符串比较
        targetLayer = LayerMask.NameToLayer(targetLayerName);

        if (targetLayer == -1)
        {
            Debug.LogError($"错误：名为 '{targetLayerName}' 的图层不存在。请检查拼写或在 Project Settings -> Tags and Layers 中创建它。", this);
        }
    }

    // 当这个碰撞体/刚体开始接触另一个刚体/碰撞体时调用
    /*void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查我们碰到的物体是否在 "targetLayer" 上
        if (collision.gameObject.layer == targetLayer)
        {
            // 碰到了！销毁这个子弹
            Destroy(gameObject);
        }
    }*/

    // --- 备用方案：如果你的 Princess 是一个 "Trigger" ---
    // 如果你的 Princess 碰撞体被标记为 "Is Trigger"，
    // 你需要使用下面的 OnTriggeEnter2D 方法，并注释掉上面的 OnCollisionEnter2D
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查我们碰到的物体是否在 "targetLayer" 上
        if (other.gameObject.layer == targetLayer && other.isTrigger)
        {
            BossController bc = other.gameObject.GetComponent<BossController>();
            bc.TakeDamage(10f);
            //bc.ChangeAnim(true);
            //StartCoroutine(StopBossAnimAfterDelay(bc, 2f));
            // 两个条件都满足！销毁这个子弹
            Debug.Log("Destroy!");
            Destroy(gameObject);
        }
    }

    private IEnumerator StopBossAnimAfterDelay(BossController boss, float delayTime)
    {
        // 1. 暂停协程执行，等待指定的延迟时间
        yield return new WaitForSeconds(delayTime);

        // 2. 延迟时间到达后，执行操作
        // ⚠️ 重要检查：在等待期间 Boss 可能被销毁，因此需要检查
        if (boss != null)
        {
            boss.ChangeAnim(false);
        }
    }

}