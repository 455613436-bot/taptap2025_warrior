using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerBeneathStone : MonoBehaviour
{
    public SoundEffect sound;
    public bool IsBeneathStone;
    public bool IsBeneathTunnel = false;
    private BoxCollider2D headCollider;
    private BoxCollider2D feetCollider;
    private bool IsStartExitTimeCount;
    private float timer;
    private Vector2 headOffset;
    private Vector2 feetOffset;
    public Vector2 NormalColliderSize;
    // Start is called before the first frame update
    private BoxCollider2D PlayerCollider;
    public Transform[] TriggerTransforms;
    public bool isAttemptingToRestore = false;
    [SerializeField] private float RestoreTime;
    [Header("站立检测设置")]
    [Tooltip("在玩家站起时，需要检测的障碍物图层 (例如 'Wall', 'Ceiling')")]
    public LayerMask obstacleLayerMask;
    [Tooltip("在头顶检测的半径")]
    public float headCheckRadius = 0.2f;
    private PlayerAnim pa;
    public bool hasStone=false;
    void Start()
    {
        TriggerTransforms = GetComponentsInChildren<Transform>();
        PlayerCollider = GetComponent<BoxCollider2D>();
        headCollider = TriggerTransforms[1].GetComponent<BoxCollider2D>();
        headOffset = headCollider.offset;
        feetCollider = TriggerTransforms[2].GetComponent<BoxCollider2D>();
        feetOffset = feetCollider.offset;
        NormalColliderSize = PlayerCollider.size;
        pa = GetComponent<PlayerAnim>();
    }

    // Update is called once per frame
    void Update()
    {
        /*timer += Time.deltaTime;

        if (!IsStartExitTimeCount)
        {
            timer = 0;
        }

        if(timer >= RestoreTime)
        {
            IsStartExitTimeCount = false;
            //PlayerTransform.localScale = Vector3.one;
            PlayerCollider.size = NormalColliderSize;
            headCollider.offset = headOffset;
            feetCollider.offset = feetOffset;
            //TriggerTransforms[1].localPosition = Vector3.zero;
            //TriggerTransforms[2].localPosition = Vector3.zero;
            timer = 0;
        }*/
        // 如果我们收到了 "Exit" 信号 (isAttemptingToRestore == true)
        // 并且我们当前仍然是“被压扁”状态
        if (isAttemptingToRestore && IsBeneathStone)
        {
            if (hasStone)
            {
                isAttemptingToRestore = false;
                return;
            }
            // 每帧都检查头顶是否安全
            if (IsHeadClear())
            {
                // 安全了！现在恢复体型
                RestoreNormalSize();
            }
            else
            {
                Debug.Log("obstacles ahead");
            }
            // else: 不安全，保持蹲伏，下一帧继续检查...
        }
    }

    private void RestoreNormalSize()
    {
        Debug.Log("Restore");
        IsBeneathStone = false;       // 标记为“已站立”
        isAttemptingToRestore = false; // 停止尝试
        //PlayerTransform.localScale = Vector3.one;
        PlayerCollider.size = NormalColliderSize;
        headCollider.offset = headOffset;
        feetCollider.offset = feetOffset;
        pa.SwitchToNormalSet();
    }
    private bool IsHeadClear()
    {
        // 从 headCollider 的当前位置发射一个圆形检测
        // !Physics2D.OverlapCircle(...) 的意思是 "如果没有碰到任何东西"
        return !Physics2D.OverlapCircle(headCollider.transform.position, headCheckRadius, obstacleLayerMask);
    }

    public void PressedStateEnter()
    {
        if (IsBeneathStone)
        {
            return;
        }
        else
        {
            sound.PlayStoneSound();
            Debug.Log("enter");
            IsStartExitTimeCount = false;
            timer = 0;
            IsBeneathStone = true;
            //PlayerTransform.localScale = new Vector3(1.0f,  0.5f, 1.0f);
            PlayerCollider.size = new Vector2(NormalColliderSize.x, NormalColliderSize.y * 0.45f);
            Vector2 tempHeadOffset = new Vector2(headOffset.x, 0f);
            headCollider.offset = tempHeadOffset;
            Vector2 tempFeetOffset = new Vector2(feetOffset.x, 0.8f);
            feetCollider.offset = tempFeetOffset;
            pa.SwitchToArmoredSet();
            //TriggerTransforms[1].localPosition = new Vector3(0, NormalColliderSize.y / 4, 0);
            //TriggerTransforms[2].localPosition = new Vector3(0, -NormalColliderSize.y / 4, 0);
        }

    }

    public void PressedStateStay()
    {
        IsBeneathStone = true;
        IsStartExitTimeCount = false;
    }

    public void PressedStateExit()
    {
        Debug.Log("exit");
        //IsBeneathStone = false;
        isAttemptingToRestore = true;
        IsStartExitTimeCount = true;
    }
}
