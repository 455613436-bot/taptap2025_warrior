using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndStoryController : MonoBehaviour
{
    [Header("动画设置")]
    public Image blackScreen;
    public Image[] effectFrames = new Image[8]; // 8帧动画
    public Image backImage; // 背景图片
    public Image dialog1; // 第一张对话图片
    public Image dialog2; // 第二张对话图片
    public Image endImage; // 结束图片
    public GameObject princess;
    [Header("按钮设置")]
    public Button skipButton; // 跳过按钮

    [Header("音效设置")]
    public AudioClip sfx0; // 8帧动画音效
    public AudioClip sfx1; // 对话音效（新增）
    public AudioSource audioSource; // 音频源

    [Header("时间设置")]
    public float blackScreenDuration = 1f; // 黑屏持续时间
    public float effectInterval = 0.2f; // 8帧动画间隔
    public float dialogInterval = 2.5f; // 对话间隔
    public float endImageDuration = 3f; // 结束图片持续时间
    public float effectToDialogDelay = 1f; // 8帧动画到对话的延迟

    private bool isPlaying = false;
    private Coroutine animationCoroutine;

    // 静态实例，方便其他脚本访问
    public static EndStoryController Instance { get; private set; }

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始时隐藏所有元素
        HideAllEffectFrames();
        HideAllDialogs();
        HideEndImage();
        HideBackImage();
        blackScreen.gameObject.SetActive(true);
        // 初始时隐藏跳过按钮
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // 确保有AudioSource组件
        if (audioSource == null)
            audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 检查设置
        Debug.Log("EndStoryController: 初始化完成");
        Debug.Log($"Effect Frames 数量: {effectFrames.Length}");
        for (int i = 0; i < effectFrames.Length; i++)
        {
            if (effectFrames[i] == null)
                Debug.LogWarning($"Effect Frame {i} 未分配!");
            else
                Debug.Log($"Effect Frame {i}: {effectFrames[i].name}");
        }
    }

    void Start()
    {
        // 设置按钮初始状态
        SetupButtons();

        // 自动开始播放动画
        AutoPlayStoryAnimation();
    }

    void Update()
    {
        // 可以在这里添加键盘快捷键，比如按空格跳过动画
        if (isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            SkipAnimation();
        }
    }

    /// <summary>
    /// 设置按钮初始状态和点击事件
    /// </summary>
    private void SetupButtons()
    {
        // 设置跳过按钮
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipAnimation);
        }
    }

    /// <summary>
    /// 自动播放故事动画
    /// </summary>
    public void AutoPlayStoryAnimation()
    {
        if (isPlaying)
        {
            Debug.Log("动画已在播放中");
            return;
        }

        Debug.Log("开始自动播放动画");
        animationCoroutine = StartCoroutine(PlayAnimationCoroutine());
    }

    /// <summary>
    /// 停止播放故事动画
    /// </summary>
    public void StopStoryAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            Debug.Log("停止动画播放");
        }

        // 停止音频源
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        EndAnimation();
    }

    /// <summary>
    /// 跳过动画，直接跳到结束图片
    /// </summary>
    public void SkipAnimation()
    {
        Debug.Log("跳过动画");

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // 隐藏所有元素
        HideAllEffectFrames();
        HideAllDialogs();
        HideBackImage();

        // 隐藏跳过按钮
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // 直接播放结束图片
        StartCoroutine(PlayEndImageOnly());
    }

    /// <summary>
    /// 动画播放协程
    /// </summary>
    private IEnumerator PlayAnimationCoroutine()
    {
        isPlaying = true;
        Debug.Log("动画协程开始");

        // 第一阶段：黑屏1秒
        Debug.Log("开始黑屏阶段");
        yield return StartCoroutine(PlayBlackScreen());

        // 第二阶段：显示跳过按钮并播放8帧动画
        if (skipButton != null)
        {
            //skipButton.gameObject.SetActive(true);
            Debug.Log("显示跳过按钮");
        }

        Debug.Log("开始8帧动画");
        princess.SetActive(true);
        yield return StartCoroutine(PlayEffectAnimation());

        // 第三阶段：显示BackImage，等待1秒后播放两张对话图片
        Debug.Log("8帧动画结束，显示BackImage");
        ShowBackImage();
        yield return new WaitForSecondsRealtime(effectToDialogDelay);
        Debug.Log("开始对话序列");
        yield return StartCoroutine(PlayDialogSequence());

        // 第四阶段：播放结束图片
        Debug.Log("开始结束图片");
        yield return StartCoroutine(PlayEndImage());

        // 动画自然结束
        EndAnimation();
    }

    /// <summary>
    /// 播放黑屏效果
    /// </summary>
    private IEnumerator PlayBlackScreen()
    {
        // 确保所有元素都是隐藏状态
        HideAllEffectFrames();
        HideAllDialogs();
        HideEndImage();
        //HideBackImage();
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        Debug.Log("黑屏中...");
        // 等待黑屏持续时间
        yield return new WaitForSecondsRealtime(blackScreenDuration);
        Debug.Log("黑屏结束");
    }

    /// <summary>
    /// 播放8帧特效动画
    /// </summary>
    private IEnumerator PlayEffectAnimation()
    {
        if (effectFrames.Length == 0)
        {
            Debug.LogError("Effect Frames 数组为空!");
            yield break;
        }

        // 播放sfx0音效
        if (sfx0 != null && audioSource != null)
        {
            audioSource.PlayOneShot(sfx0);
            Debug.Log("播放音效: " + sfx0.name);
        }
        else
        {
            Debug.LogWarning("音效或AudioSource未设置");
        }

        // 隐藏所有特效帧
        HideAllEffectFrames();

        // 逐帧播放特效
        for (int i = 0; i < effectFrames.Length; i++)
        {
            // 隐藏所有特效帧
            HideAllEffectFrames();

            // 显示当前帧
            if (effectFrames[i] != null)
            {
                Debug.Log($"显示第 {i + 1} 帧: {effectFrames[i].name}");
                effectFrames[i].gameObject.SetActive(true);

                // 添加淡入效果 - 使用更短的淡入时间
                yield return StartCoroutine(FadeInImage(effectFrames[i], effectInterval * 0.5f));

                // 等待剩余时间
                yield return new WaitForSecondsRealtime(effectInterval * 0.5f);
            }
            else
            {
                Debug.LogWarning($"第 {i + 1} 帧为空，跳过");
                // 如果这一帧为空，仍然等待完整间隔
                yield return new WaitForSecondsRealtime(effectInterval);
            }
        }

        // 隐藏所有特效帧
        HideAllEffectFrames();
        Debug.Log("8帧动画播放完毕");
    }

    /// <summary>
    /// 播放对话序列
    /// </summary>
    private IEnumerator PlayDialogSequence()
    {
        // 显示第一张对话图片
        if (dialog1 != null)
        {
            dialog1.gameObject.SetActive(true);
            Debug.Log("显示 Dialog1");

            // 播放对话音效
            PlayDialogSound();

            yield return StartCoroutine(FadeInImage(dialog1));

            // 等待指定间隔
            yield return new WaitForSecondsRealtime(dialogInterval);

            // 淡出第一张对话图片
            yield return StartCoroutine(FadeOutImage(dialog1));
            dialog1.gameObject.SetActive(false);
            Debug.Log("隐藏 Dialog1");
        }
        else
        {
            Debug.LogWarning("Dialog1 未设置");
            yield return new WaitForSecondsRealtime(dialogInterval);
        }

        // 显示第二张对话图片
        if (dialog2 != null)
        {
            dialog2.gameObject.SetActive(true);
            Debug.Log("显示 Dialog2");

            // 播放对话音效
            PlayDialogSound();

            yield return StartCoroutine(FadeInImage(dialog2));

            // 等待指定间隔
            yield return new WaitForSecondsRealtime(dialogInterval);

            // 同时淡出第二张对话图片和BackImage
            Debug.Log("同时淡出 Dialog2 和 BackImage");
            yield return StartCoroutine(FadeOutDialog2AndBackImage());
        }
        else
        {
            Debug.LogWarning("Dialog2 未设置");
            yield return new WaitForSecondsRealtime(dialogInterval);
            HideBackImage();
        }
    }

    /// <summary>
    /// 播放对话音效
    /// </summary>
    private void PlayDialogSound()
    {
        if (sfx1 != null && audioSource != null)
        {
            audioSource.PlayOneShot(sfx1);
            Debug.Log("播放对话音效: " + sfx1.name);
        }
        else
        {
            if (sfx1 == null)
                Debug.LogWarning("对话音效 sfx1 未设置");
            if (audioSource == null)
                Debug.LogWarning("AudioSource 未设置");
        }
    }

    /// <summary>
    /// 同时淡出Dialog2和BackImage
    /// </summary>
    private IEnumerator FadeOutDialog2AndBackImage()
    {
        if (dialog2 == null && backImage == null)
        {
            Debug.LogWarning("Dialog2 和 BackImage 都为空，无法淡出");
            yield break;
        }

        float fadeTime = 0.5f;
        float timer = 0f;

        Debug.Log("开始同步淡出 Dialog2 和 BackImage");

        // 获取初始颜色
        Color dialog2OriginalColor = dialog2 != null ? dialog2.color : Color.white;
        Color backImageOriginalColor = backImage != null ? backImage.color : Color.white;

        Color dialog2Transparent = dialog2OriginalColor;
        dialog2Transparent.a = 0f;

        Color backImageTransparent = backImageOriginalColor;
        backImageTransparent.a = 0f;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeTime;

            // 同时淡出Dialog2和BackImage
            if (dialog2 != null)
            {
                dialog2.color = Color.Lerp(dialog2OriginalColor, dialog2Transparent, progress);
            }

            if (backImage != null)
            {
                backImage.color = Color.Lerp(backImageOriginalColor, backImageTransparent, progress);
            }

            yield return null;
        }

        // 确保完全透明并隐藏
        if (dialog2 != null)
        {
            dialog2.color = dialog2Transparent;
            dialog2.gameObject.SetActive(false);
        }

        if (backImage != null)
        {
            backImage.color = backImageTransparent;
            backImage.gameObject.SetActive(false);
        }

        Debug.Log("同步淡出完成");
    }

    /// <summary>
    /// 播放结束图片
    /// </summary>
    private IEnumerator PlayEndImage()
    {
        // 隐藏跳过按钮
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
            Debug.Log("隐藏跳过按钮");
        }

        if (endImage != null)
        {
            endImage.gameObject.SetActive(true);
            Debug.Log("显示结束图片");
            princess.SetActive(false);
            // 淡入结束图片
            yield return StartCoroutine(FadeInImage(endImage));

            // 等待指定持续时间
            Debug.Log($"结束图片显示 {endImageDuration} 秒");
            yield return new WaitForSecondsRealtime(endImageDuration);

            // 淡出结束图片
            yield return StartCoroutine(FadeOutImage(endImage));
            endImage.gameObject.SetActive(false);
            Debug.Log("隐藏结束图片");
        }
        else
        {
            Debug.LogWarning("结束图片未设置");
            yield return new WaitForSecondsRealtime(endImageDuration);
        }

        // 加载菜单场景
        Debug.Log("加载菜单场景: MenuScene");
        SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// 仅播放结束图片（用于跳过功能）
    /// </summary>
    private IEnumerator PlayEndImageOnly()
    {
        isPlaying = true;
        Debug.Log("直接播放结束图片");

        if (endImage != null)
        {
            endImage.gameObject.SetActive(true);

            // 淡入结束图片
            yield return StartCoroutine(FadeInImage(endImage));

            // 等待指定持续时间
            yield return new WaitForSecondsRealtime(endImageDuration);

            // 淡出结束图片
            yield return StartCoroutine(FadeOutImage(endImage));
            endImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("结束图片未设置");
            yield return new WaitForSecondsRealtime(endImageDuration);
        }

        // 加载菜单场景
        SceneManager.LoadScene("MenuScene");

        isPlaying = false;
    }

    /// <summary>
    /// 动画结束处理
    /// </summary>
    private void EndAnimation()
    {
        // 隐藏所有元素
        HideAllEffectFrames();
        HideAllDialogs();
        HideEndImage();
        HideBackImage();

        // 隐藏跳过按钮
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        isPlaying = false;
        Debug.Log("动画完全结束");
    }

    /// <summary>
    /// 隐藏所有特效帧
    /// </summary>
    private void HideAllEffectFrames()
    {
        foreach (Image frame in effectFrames)
        {
            if (frame != null)
                frame.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 隐藏所有对话图片
    /// </summary>
    private void HideAllDialogs()
    {
        if (dialog1 != null)
            dialog1.gameObject.SetActive(false);
        if (dialog2 != null)
            dialog2.gameObject.SetActive(false);
    }

    /// <summary>
    /// 隐藏结束图片
    /// </summary>
    private void HideEndImage()
    {
        if (endImage != null)
            endImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 隐藏背景图片
    /// </summary>
    private void HideBackImage()
    {
        if (backImage != null)
            backImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示背景图片
    /// </summary>
    private void ShowBackImage()
    {
        if (backImage != null)
        {
            backImage.gameObject.SetActive(true);
            // 确保背景图片完全不透明
            Color color = backImage.color;
            color.a = 1f;
            backImage.color = color;
            Debug.Log("显示 BackImage");
        }
        else
        {
            Debug.LogWarning("BackImage 未设置");
        }
    }

    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeInImage(Image image, float fadeTime = 0.5f)
    {
        if (image == null)
        {
            Debug.LogWarning("尝试淡入的Image为空");
            yield break;
        }

        Color originalColor = image.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;

        image.color = transparentColor;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeTime;
            image.color = Color.Lerp(transparentColor, originalColor, progress);
            yield return null;
        }

        image.color = originalColor;
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOutImage(Image image, float fadeTime = 0.5f)
    {
        if (image == null)
        {
            Debug.LogWarning("尝试淡出的Image为空");
            yield break;
        }

        Color originalColor = image.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeTime;
            image.color = Color.Lerp(originalColor, transparentColor, progress);
            yield return null;
        }

        image.color = transparentColor;
        image.gameObject.SetActive(false);
    }

    /// <summary>
    /// 检查是否正在播放动画
    /// </summary>
    public bool IsAnimationPlaying()
    {
        return isPlaying;
    }

    void OnDestroy()
    {
        // 清理
        if (isPlaying)
        {
            Time.timeScale = 1f;
        }
    }
}