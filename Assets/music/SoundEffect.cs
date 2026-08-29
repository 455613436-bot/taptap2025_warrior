using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;

    // 2. 公共变量：用于在 Inspector 中分配不同的音效文件
    [Header("角色音效文件")]
    public AudioClip jumpClip1;
    public AudioClip jumpClipshort;
    public AudioClip hurtClip;
    public AudioClip stoneClip;
    public AudioClip deathClip;
    public AudioClip reviveClip;
    public AudioClip scareClip;
    public AudioClip glitchClip;
    public AudioClip thornClip;
    public AudioClip glitchhitClip;
    public AudioClip downClip;
    // Start is called before the first frame update
    void Awake() // 推荐在 Awake() 中获取组件
    {
        // 尝试获取角色身上的 AudioSource 组件
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("角色缺少 AudioSource 组件，无法播放音效！");
        }
    }
    void Start()
    {
        
    }
    public void PlayJumpSound()
    {
        PlaySound(jumpClip1);
    }

    public void PlayJumpShortSound()
    {
        PlaySound(jumpClipshort);
    }
    /// <summary>
    /// 在角色受伤时调用，播放受伤音效。
    /// </summary>
    public void PlayHurtSound()
    {
        PlaySound(hurtClip);
    }

    // ... 其他音效函数（如 PlayAttackSound, PlayDeathSound） ...
    public void PlayStoneSound()
    {
        PlaySound(stoneClip);
    }
    public void PlayDeathSound()
    {
        PlaySound(deathClip);
    }
    public void PlayReviveSound()
    {
        PlaySound(reviveClip);
    }

    public void PlayScareSound()
    {
        PlaySound(scareClip);
    }
    public void PlayGlitchSound()
    {
        PlaySound(glitchClip);
    }
    public void PlayGlitchHitSound()
    {
        PlaySound(glitchhitClip);
    }
    public void PlayThornSound()
    {
        PlaySound(thornClip);
    }
    public void PlayDownSound()
    {
        PlaySound(downClip);
    }
    /// <summary>
    /// 核心播放方法：安全地播放一个 AudioClip
    /// </summary>
    /// <param name="clip">要播放的音效文件</param>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // 使用 PlayOneShot() 可以在不中断当前正在播放的音效的情况下，播放新的音效。
            audioSource.PlayOneShot(clip);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
