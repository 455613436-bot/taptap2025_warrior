using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneBreaks : MonoBehaviour
{
    public SoundEffect sound;
    // Start is called before the first frame update
    public LayerMask groundLayer;
    public float minBreakSpeed = 11f;
    // 当两个“实体”碰撞体接触时，此函数会被调用
    // (确保你的石块和地面都没有勾选 Is Trigger)
    void Awake()
    {
        GameObject managerObject = GameObject.Find("Music");
        sound = managerObject.GetComponent<SoundEffect>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool layerMatches = (groundLayer == (groundLayer | (1 << collision.gameObject.layer)));

        if (layerMatches)
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 normal = contact.normal;

            // 3. 检查碰撞是否主要是垂直的
            // 我们检查法线的Y分量绝对值是否"占主导" (例如，大于0.7)
            // (1 表示纯垂直, 0 表示纯水平)
            if (Mathf.Abs(normal.y) > 0.8f)
            {
                // 4. 【修改】: 只获取 Y 轴 (竖直) 上的相对速度
                float verticalImpactSpeed = Mathf.Abs(collision.relativeVelocity.y);

                // 5. 同时检查图层、竖直速度 和 碰撞方向
                if (verticalImpactSpeed > minBreakSpeed)
                {
                    // 如果是地面/墙壁 并且 竖直速度足够快 并且 是竖直碰撞，才销毁
                    Debug.Log(collision.gameObject.layer + " 竖直碰撞销毁！速度："+ verticalImpactSpeed);
                    Destroy(gameObject);
                    sound.PlayStoneSound();
                }
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
