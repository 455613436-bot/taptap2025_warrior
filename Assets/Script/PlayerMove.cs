using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject platform;
    private Rigidbody2D platform_rb;
    public bool HaveLeftPlatformFlag;
    private Vector2 BasicVelocity;
    private Rigidbody2D rb;
    private Animator anim;
    public float moveSpeed;
    public float moveController;
    private bool isRun;
    public bool leftPlatform = false;
    public float leftSpeed = 0f;
    private PlayerJump pj;
    void Start()
    {
        platform_rb = platform.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        pj = GetComponent<PlayerJump>();
    }

    // Update is called once per frame
    void Update()
    {
        GetKeyboard();
        PlayerFace();
        if (pj.isOnPlatform && platform != null)
        {
            rb.velocity += platform_rb.velocity;
        }
        if (leftPlatform) { AddExtraPlatformVelocity(); }
        
    }

    private void AddExtraPlatformVelocity()
    {
        Vector2 tVelocity = new Vector2(leftSpeed, 0);
        rb.velocity+= tVelocity;
        if (pj.isGround || pj.isOnButton || pj.isOnStone)
        {
            leftPlatform = false;
        }
        //leftPlatform = false;
    }


    private void GetKeyboard()
    {
        moveController = Input.GetAxis("Horizontal");

        rb.velocity = new Vector2(moveSpeed * moveController, rb.velocity.y);
    }

    private void PlayerFace()
    {
        if (moveController < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        if (moveController > 0)
        {
            transform.localScale = new Vector2(1, 1);
        }
    }

    public void UpdateLayerCollision(LayerMask targetLayers, bool ignoreCollision)
    {
        int myLayer = gameObject.layer;

        for (int i = 0; i < 32; i++)
        {
            // 检查第 i 个图层是否在 targetLayers 蒙版中
            // (1 << i) 是一个位操作，用于获取第 i 层的"位"
            // (targetLayers | (1 << i)) == targetLayers 是一个高效的检查方法
            if (targetLayers == (targetLayers | (1 << i)))
            {
                // 如果在蒙版中，就应用碰撞设置
                Physics2D.IgnoreLayerCollision(myLayer, i, ignoreCollision);
                //Debug.Log($"设置 Layer {myLayer} 与 Layer {i} 碰撞: {!ignoreCollision}");
            }
        }
    }



}
