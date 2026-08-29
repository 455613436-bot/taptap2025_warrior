using UnityEngine;
using System.Collections;

public class MenuBGMControl : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip bgm1; // 第一个BGM（不循环）
    [SerializeField] private AudioClip bgm2; // 第二个BGM（循环播放）
    [SerializeField] private float fadeDuration = 1.0f; // 淡入淡出时间

    private AudioSource audioSource;
    private bool isTransitioning = false;

    void Start()
    {
        // 获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 设置AudioSource基础属性
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // 开始播放BGM序列
        StartCoroutine(PlayBGMSequence());
    }

    /// <summary>
    /// 播放BGM序列：先播放bgm1，然后循环播放bgm2
    /// </summary>
    private IEnumerator PlayBGMSequence()
    {
        if (bgm1 == null || bgm2 == null)
        {
            Debug.LogWarning("BGM clips are not assigned!");
            yield break;
        }

        // 播放第一个BGM
        audioSource.clip = bgm1;
        audioSource.loop = false;
        audioSource.Play();

        // 等待第一个BGM播放完毕
        yield return new WaitForSeconds(bgm1.length);

        // 淡出第一个BGM（可选）
        yield return StartCoroutine(FadeOut(fadeDuration));

        // 播放第二个BGM并设置为循环
        audioSource.clip = bgm2;
        audioSource.loop = true;
        audioSource.Play();

        // 淡入第二个BGM（可选）
        yield return StartCoroutine(FadeIn(fadeDuration));
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        isTransitioning = true;
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        isTransitioning = false;
    }

    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn(float duration)
    {
        isTransitioning = true;
        float targetVolume = audioSource.volume;
        audioSource.volume = 0;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isTransitioning = false;
    }

    /// <summary>
    /// 停止所有BGM播放
    /// </summary>
    public void StopBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOut(fadeDuration));
        }
    }

    /// <summary>
    /// 重新开始BGM序列（从bgm1开始）
    /// </summary>
    public void RestartBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        StopAllCoroutines();
        StartCoroutine(PlayBGMSequence());
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// 当脚本被禁用时停止所有协程
    /// </summary>
    void OnDisable()
    {
        StopAllCoroutines();
    }
}