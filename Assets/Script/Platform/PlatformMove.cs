using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{
    public Transform rightpivot;
    public Transform leftpivot;
    private Transform targetpivot;
    private float MoveSpeed=0f;
    public bool IsTowardLeft;
    public float moveAcc;
    private Rigidbody2D rb;

    public GameObject Player;
    private KeyHoldWithUI keyhold;

    // Start is called before the first frame update
    void Start()
    {
        targetpivot = rightpivot;
        rb = GetComponent<Rigidbody2D>();

        keyhold = Player.GetComponent<KeyHoldWithUI>();
    }

    // Update is called once per frame
    void Update()
    {
        bool justArrived = false;
        // 检测逻辑仍在Update中，因为这不涉及物理计算
        if (Vector2.Distance(transform.position, rightpivot.position) < 0.1f&&targetpivot == rightpivot)
        {
            targetpivot = leftpivot;
            justArrived = true;
        }
        if (Vector2.Distance(transform.position, leftpivot.position) < 0.1f && targetpivot == leftpivot)
        {
            targetpivot = rightpivot;
            justArrived = true;
        }
        if (justArrived)
        {
            MoveSpeed = 0f;
            rb.velocity = Vector2.zero; // 立即停止
        }
        IsTowardLeft = (targetpivot == leftpivot);
    }

    void FixedUpdate()
    {
        if (!keyhold.isFrozen)
        {
            MoveSpeed += moveAcc * Time.fixedDeltaTime;
        }
        // 移动逻辑移到FixedUpdate中，使用刚体速度
        Vector2 direction = (targetpivot.position - transform.position).normalized;
        //rb.velocity = direction * MoveSpeed;
        rb.velocity = (keyhold.isFrozen ? new Vector2(0f, 0f) : direction * MoveSpeed);
        
    }
}