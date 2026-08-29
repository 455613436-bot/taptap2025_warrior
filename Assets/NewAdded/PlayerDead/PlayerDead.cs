


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerDead : MonoBehaviour
{
    public SoundEffect sound;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // 【新增】SpriteRenderer引用
    public int myHealth = 10;
    public TextMeshProUGUI healthtext;
    public bool isDead=false;
    private KeyHoldWithUI keyhold;

    [Header("伤害设置")]
    public float invulnerabilityDuration = 1.0f;
    private bool canTakeDamage = true;
    public Sprite hurtSprite;

    // 【新增】保存原始贴图
    private Sprite defaultSprite;
    private IEnumerator DamageCooldownRoutine()
    {
        canTakeDamage = false;
        // 获取层的索引 (推荐使用字符串名获取，确保正确)
        int hurtLayerIndex = animator.GetLayerIndex("HurtLayer");

        if (animator != null && hurtLayerIndex != -1)
        {
            // 1. 提升层权重：贴图立刻被 HurtOverride 动画覆盖
            animator.SetLayerWeight(hurtLayerIndex, 1f);

            // 2. 强制播放：确保 HurtOverride 动画立即生效
            // 参数: 状态名, 层索引, 播放起始时间 (0f)
            animator.Play("HurtOverride", hurtLayerIndex, 0f);
        }

        yield return new WaitForSeconds(invulnerabilityDuration);

        if (animator != null && hurtLayerIndex != -1)
        {
            // 3. 恢复层权重：基础层重新获得贴图控制权
            animator.SetLayerWeight(hurtLayerIndex, 0f);
        }

        canTakeDamage = true;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // 【新增】获取组件

        keyhold = GetComponent<KeyHoldWithUI>();

        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
        if (RespawnManager.Instance != null)
        {
            transform.position = RespawnManager.Instance.respawnPosition;
        }

        healthtext.text = myHealth.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Trap" && keyhold.isFrozen==false)
        {
            animator.SetTrigger("isDead");
            isDead = true;
            sound.PlayDeathSound();
            rb.bodyType = RigidbodyType2D.Static;
        }

        if ((collision.tag == "Enemy" || collision.tag == "Princess") && canTakeDamage)
        {
            if (keyhold.isFrozen) { return; }
            myHealth -= 1;
            if (myHealth == 0)
            {
                animator.SetTrigger("isDead");
                isDead = true;
                sound.PlayDeathSound();
                rb.bodyType = RigidbodyType2D.Static;
                myHealth = 10;
                healthtext.text = myHealth.ToString();
                return;
            }
            healthtext.text = myHealth.ToString();
            sound.PlayDeathSound();
            // 启动免疫协程
            StartCoroutine(DamageCooldownRoutine());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Trap" && keyhold.isFrozen == false)
        {
            animator.SetTrigger("isDead");
            isDead = true;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    public void Revive()
    {
        if (RespawnManager.Instance != null)
        {
            transform.position = RespawnManager.Instance.respawnPosition;
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        //sound.PlayReviveSound();
        canTakeDamage = true;
        isDead = false;
        myHealth = 10;
        healthtext.text = myHealth.ToString();

        // 确保复活时贴图颜色也是正常的
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // 假设默认颜色是白色
        }
        StartCoroutine(ReviveWithSoundDelay());
        RestartRoom.Instance.ResetScene();
    }

    private IEnumerator ReviveWithSoundDelay()
    {
        // 1. 播放音效
        sound.PlayReviveSound();

        // 2. 计算音效长度 (假设您能获取到 reviveClip)
        float clipLength = sound.reviveClip.length;

        // 3. 等待音效播放完毕
        yield return new WaitForSeconds(clipLength);

        // 4. 音效播放完毕后，执行重置
        RestartRoom.Instance.ResetScene();
    }
}