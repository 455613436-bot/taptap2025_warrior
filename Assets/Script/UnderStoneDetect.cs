using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderStoneDetect : MonoBehaviour
{
    public GameObject Player;
    private PlayerBeneathStone pbs;
    public bool isAttached = false;
    public Rigidbody2D rb;
    private KeyHoldWithUI kh;
    private bool leftFromFrozen=false;
    private Vector2 originalSpeed;
    public bool isInLongzi=false;
    // Start is called before the first frame update
    private RigidbodyConstraints2D originalConstraints;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        pbs = Player.GetComponent<PlayerBeneathStone>();
        kh = Player.GetComponent<KeyHoldWithUI>();
        rb = GetComponent<Rigidbody2D>();
        originalConstraints = rb.constraints;
    }

    // Update is called once per frame
    void Update()
    {
        if (kh.isFrozen)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            originalSpeed = rb.velocity;
            rb.velocity = new Vector2(0, 0);
            leftFromFrozen = true;
        }
        else
        {
            if (leftFromFrozen)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.velocity = originalSpeed;
                leftFromFrozen = false;
            }
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.WakeUp();
        }
        if (isInLongzi)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        if(collision.tag == "Head")
        {
            pbs.PressedStateEnter();
            // 如果已经吸附了，或者撞到的不是玩家，就什么也不做
            if (isAttached || pbs.hasStone)
            {
                return;
            }
            FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();

            // 拴住玩家的 Rigidbody2D (collision.transform.parent 就是玩家的Transform)
            joint.connectedBody = collision.transform.parent.GetComponent<Rigidbody2D>();

            // (可选) 禁用关节的自动碰撞
            joint.enableCollision = false;
            //rb.isKinematic = true;
            //rb.velocity = Vector2.zero;
            //rb.angularVelocity = 0f;
            //Vector3 playerPos = collision.transform.parent.position;
            //transform.position = new Vector3(playerPos.x, playerPos.y + 0.9f, 0);
            transform.SetParent(collision.transform.parent);
            transform.localPosition = new Vector3(0, 0.8f, 0);
            Collider2D myCollider = transform.GetComponent<Collider2D>();
            myCollider.offset = new Vector2(0, 0);
            // 4. 更新状态：自己附着，且玩家头上有了石头
            isAttached = true;
            pbs.hasStone = true; // 【新增】: 锁定玩家状态

            Debug.Log("石头吸附到了玩家身上！");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Head")
        {
            pbs.PressedStateStay();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Head")
        {
            if (isAttached)
            {
                return;
            }
            pbs.PressedStateExit();
        }
    }
    public void DetachAndFreeze()
    {
        if (!isAttached)
        {
            return;
        }
        pbs.hasStone = false;
        transform.SetParent(null);
        FixedJoint2D joint = GetComponent<FixedJoint2D>();
        if (joint != null)
        {
            Destroy(joint);
        }

        // 【新增】: 现在我们才把它设为 Kinematic 来冻结在空中
        rb.isKinematic = true;
        isAttached = false;
        Debug.Log("石头被挣脱并冻结了！");
    }

    public void RecoverStoneRB() { 
        // 2. 重新启用物理引擎，让它变回一个普通的石头
        rb.isKinematic = false;

    }
}
