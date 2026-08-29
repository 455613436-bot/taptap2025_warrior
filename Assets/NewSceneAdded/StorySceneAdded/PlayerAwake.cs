using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerAwake : MonoBehaviour
{
    [Header("UI设置")]
    public Canvas targetCanvas; // 需要隐藏的Canvas
    public Canvas menuCanvas;   // 不需要隐藏的MenuCanvas

    [Header("场景物体设置")]
    public GameObject palaceRoom; // 需要激活的PalaceRoom物体

    [Header("黑屏设置")]
    public Image blackScreen; // 用于黑屏的Image组件
    public float blackScreenDuration = 1.0f; // 黑屏持续时间

    [Header("动画监听")]
    public StoryAnim storyAnim; // 故事动画脚本引用

    void Start()
    {
        // 如果未手动指定storyAnim，尝试自动获取
        if (storyAnim == null)
        {
            storyAnim = FindObjectOfType<StoryAnim>();
        }

        // 订阅动画结束事件
        if (storyAnim != null)
        {
            // 使用轮询方式等待动画结束
            StartCoroutine(WaitForStoryAnimEnd());
        }
        else
        {
            Debug.LogWarning("PlayerAwake: 未找到StoryAnim组件，将无法监听动画结束事件");
        }

        // 初始状态设置
        InitializeSceneState();
    }

    /// <summary>
    /// 初始化场景状态
    /// </summary>
    private void InitializeSceneState()
    {
        // 确保PalaceRoom初始时是隐藏的
        if (palaceRoom != null)
        {
            palaceRoom.SetActive(false);
        }

        // 确保目标Canvas初始时是显示的
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
        }

        // 确保MenuCanvas初始时是显示的
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
        }

        // 确保黑屏初始时是隐藏的
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 等待故事动画结束的协程
    /// </summary>
    private IEnumerator WaitForStoryAnimEnd()
    {
        // 等待直到动画开始播放
        while (!storyAnim.IsAnimationPlaying())
        {
            yield return null;
        }

        // 等待直到动画结束
        while (storyAnim.IsAnimationPlaying())
        {
            yield return null;
        }

        // 动画结束，执行场景切换（带黑屏效果）
        yield return StartCoroutine(SceneTransitionWithBlackScreen());
    }

    /// <summary>
    /// 带黑屏效果的场景切换
    /// </summary>
    private IEnumerator SceneTransitionWithBlackScreen()
    {
        // 隐藏目标Canvas
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
            Debug.Log("隐藏Canvas: " + targetCanvas.name);
        }
        else
        {
            Debug.LogWarning("PlayerAwake: 未指定要隐藏的Canvas");
        }

        // 显示黑屏
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);

            // 淡入黑屏
            yield return StartCoroutine(FadeBlackScreen(1f, 1f, 0.5f));

            // 保持黑屏状态
            yield return new WaitForSecondsRealtime(blackScreenDuration);

            // 激活PalaceRoom
            if (palaceRoom != null)
            {
                palaceRoom.SetActive(true);
                Debug.Log("激活PalaceRoom: " + palaceRoom.name);
            }
            else
            {
                Debug.LogWarning("PlayerAwake: 未指定要激活的PalaceRoom");
            }

            // 淡出黑屏
            yield return StartCoroutine(FadeBlackScreen(1f, 1f, 0.5f));

            // 隐藏黑屏
            blackScreen.gameObject.SetActive(false);
        }
        else
        {
            // 如果没有黑屏，直接激活PalaceRoom
            if (palaceRoom != null)
            {
                palaceRoom.SetActive(true);
                Debug.Log("激活PalaceRoom: " + palaceRoom.name);
            }
            Debug.LogWarning("PlayerAwake: 未指定黑屏Image，将跳过黑屏效果");
        }

        // MenuCanvas保持不变，不需要特别处理

        // 可选：执行其他初始化操作
        InitializePlayerState();
    }

    /// <summary>
    /// 黑屏淡入淡出效果
    /// </summary>
    private IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        Color color = blackScreen.color;
        color.a = fromAlpha;
        blackScreen.color = color;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, progress);
            blackScreen.color = color;
            yield return null;
        }

        color.a = toAlpha;
        blackScreen.color = color;
    }

    /// <summary>
    /// 初始化玩家状态（可选）
    /// </summary>
    private void InitializePlayerState()
    {
        // 这里可以添加玩家状态初始化的代码
        // 例如：恢复玩家控制、重置玩家位置等

        // 示例：恢复玩家控制
        // if (playerController != null)
        // {
        //     playerController.EnableControl();
        // }

        Debug.Log("故事动画结束，场景状态已更新");
    }

    /// <summary>
    /// 手动触发场景切换（用于测试）
    /// </summary>
    [ContextMenu("手动触发场景切换")]
    public void ManualTriggerSceneChange()
    {
        StartCoroutine(SceneTransitionWithBlackScreen());
    }

    void OnDestroy()
    {
        // 清理工作
        StopAllCoroutines();
    }
}