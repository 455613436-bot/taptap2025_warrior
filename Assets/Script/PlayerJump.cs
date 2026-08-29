using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    public SoundEffect sound;
    public PlayerBeneathStone pbs;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float isGroundCheckLine; 
    [SerializeField] private LayerMask obstacleLayerMask;
    public bool isOnPlatform=false;
    public bool isOnButton = false;
    public bool isGround;
    public bool isOnStone;
    public bool isEnableJump;
    private bool isRunScript;
    private bool isJump;
    public Collider2D feetCollider;
    public float groundCheckRadius;
    public bool isJumping=false;
    // Start is called before the first frame update
    private void EnableJumpCheck()
    {
        isEnableJump = isGround || isOnStone || isOnPlatform || isOnButton;
    }
    void Start()
    {
        pbs = GetComponent<PlayerBeneathStone>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isGroundCheck();
        EnableJumpCheck();
        KeyboardJump();
    }



    private void KeyboardJump()
    {
        if (Input.GetButtonDown("Jump") && isEnableJump)
        {
            //rb.AddForce(new Vector2(0, jumpSpeed),ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            isJumping = true;
            if (pbs.IsBeneathStone)
            {
                sound.PlayJumpShortSound();
            }
            else
            {
                sound.PlayJumpSound();
            }
        }
        if (Input.GetButtonUp("Jump") )
        {
            //rb.AddForce(new Vector2(0, jumpSpeed),ForceMode2D.Impulse);
            isJumping = false;
        }
        if (!isJumping)
        {
            rb.velocity -= new Vector2(0, 9.81f * Time.deltaTime);
        }
    }

    private void isGroundCheck()
    {
        Vector2 worldPosition = feetCollider.transform.position;

        // 2. 将 Collider 的本地偏移量转换到世界坐标系
        // 实际上，由于没有旋转，直接相加即可得到 Collider 的中心的世界坐标。
        Vector2 checkPoint = worldPosition + feetCollider.offset;
        isGround = Physics2D.OverlapCircle(checkPoint, groundCheckRadius, obstacleLayerMask);
        //isGround = Physics2D.Raycast(transform.position,Vector2.down,isGroundCheckLine, obstacleLayerMask);
        //bool isWall=Physics2D.Raycast(transform.position, Vector2.down, isGroundCheckLine, GroundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        // 确保 Transform 引用不为空，否则尝试访问其 position 会导致空引用错误
        if (feetCollider == null)
        {
            return;
        }

        // 1. 设置 Gizmo 的颜色
        // 可以根据 isGround 的状态来设置颜色，提供更好的视觉反馈
        if (Application.isPlaying)
        {
            // 在运行时，根据检测结果改变颜色
            Gizmos.color = isGround ? Color.green : Color.red;
        }
        else
        {
            // 在编辑模式下，使用中性的颜色
            Gizmos.color = Color.yellow;
        }

        // 2. 绘制圆形的线框
        // Gizmos.DrawWireSphere 是 3D 函数，但在 2D 视图中可以用来绘制圆形线框
        // 注意：Physics2D 使用 Vector2，但 Gizmos.DrawWireSphere 需要 Vector3。Unity 会自动处理 Z 轴。
        Vector2 worldPosition = feetCollider.transform.position;

        // 2. 将 Collider 的本地偏移量转换到世界坐标系
        // 实际上，由于没有旋转，直接相加即可得到 Collider 的中心的世界坐标。
        Vector2 checkPoint = worldPosition + feetCollider.offset;
        //Vector3 position = feetCollider.position;
        Gizmos.DrawWireSphere(checkPoint, groundCheckRadius);
    }
}
