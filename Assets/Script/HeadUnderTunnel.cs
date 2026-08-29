using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadUnderTunnel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // "other" 参数代表那个刚刚进入这个触发器的碰撞体
        // 在这里编写当某物进入时需要执行的代码
        Debug.Log(other.gameObject.name + " 进入了触发区域！");
    }
}
