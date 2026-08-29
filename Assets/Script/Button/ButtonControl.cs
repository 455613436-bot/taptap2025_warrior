using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
    private Animator ButtonAnimator;
    public Sprite newSprite;
    public Sprite originalSprite;
    public GameObject Player;
    private PlayerJump pj;
    private KeyHoldWithUI keyhold;
    private SpriteRenderer spriteRenderer;
    public BoxCollider2D buttonCollider;
    public Vector2 NormalColliderSize;
    // Start is called before the first frame update
    private Coroutine recoverCoroutine;
    public bool isUp = true;
    public bool feetIn = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
        buttonCollider = GetComponent<BoxCollider2D>();
        NormalColliderSize = buttonCollider.size;
        ButtonAnimator = GetComponent<Animator>();
        Player = GameObject.FindGameObjectWithTag("Player");

        pj = Player.GetComponent<PlayerJump>();
        keyhold = Player.GetComponent<KeyHoldWithUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (keyhold.isFrozen)
        {
            //ButtonAnimator.SetBool("IsOnButton", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag=="Stone"||(collision.tag == "Feet" && !keyhold.isFrozen))
        {
            Debug.Log("Enter button"+collision.tag);
            if (collision.tag == "Feet")
            {
                feetIn = true;
            }
            if (recoverCoroutine != null)
            {
                StopCoroutine(recoverCoroutine);
                recoverCoroutine = null; // 清空引用
                Debug.Log("Canceled");
            }
            //ButtonAnimator.SetBool("IsOnButton", true);
            ChangeSprite();
            //pj.isOnButton = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Stone" || (collision.tag == "Feet" && !keyhold.isFrozen))
        {
            // 只要玩家在上面, 就必须确保任何“恢复”协程都被取消
            if (recoverCoroutine != null)
            {
                StopCoroutine(recoverCoroutine);
                recoverCoroutine = null;
            }
            //pj.isOnButton = true;          
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Feet" || collision.tag == "Stone")
        {
            if (keyhold.isFrozen&&collision.tag=="Stone")
            {
                Debug.Log("Leave blocked due to Frozen state.");
                return; // 立即返回，不启动恢复协程
            }
            if(collision.tag == "Feet" && !feetIn)
            {
                return;
            }
            if (collision.tag == "Feet")
            {
                feetIn = false;
            }
            Debug.Log(collision.tag+"Leave button");
            if (recoverCoroutine == null)
            {
                recoverCoroutine = StartCoroutine(RecoverAfterDelay(0.5f));
            }
        }
    }

    IEnumerator RecoverAfterDelay(float recoveryDelay)
    {
        // 延迟指定的秒数
        yield return new WaitForSeconds(recoveryDelay);

        // 只有在延迟结束后，才真正执行恢复操作
        RecoverSprite();
        //pj.isOnButton = false;

        // 恢复完成后，清空引用
        recoverCoroutine = null;
    }
    public void ChangeSprite()
    {
        buttonCollider.size = new Vector2(NormalColliderSize.x, NormalColliderSize.y * 0.5f);
        spriteRenderer.sprite = newSprite;
        isUp = false;
    }

    public void RecoverSprite()
    {
        buttonCollider.size = NormalColliderSize;
        spriteRenderer.sprite = originalSprite;
        isUp = true;
    }
}
