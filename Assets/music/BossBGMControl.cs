using UnityEngine;

public class BossBGMControl : MonoBehaviour
{
    [Header("Boss BGM Settings")]
    [SerializeField] private AudioClip bossBGM; // Boss战斗BGM
    [SerializeField] private AudioClip bossroomBGM;
    private AudioSource audioSource;
    private bool isPlayingBossBGM = false;

    void Awake()
    {
        // 获取AudioSource组件
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("BossBGMControl: 未找到AudioSource组件！");
            return;
        }

    }

    /// <summary>
    /// 播放Boss战斗BGM
    /// </summary>
    public void PlayBossBGM()
    {
        if (audioSource == null || bossBGM == null)
        {
            Debug.LogWarning("BossBGMControl: AudioSource或BossBGM未设置！");
            return;
        }

        // 如果已经在播放Boss BGM，则不需要重复播放
        if (isPlayingBossBGM && audioSource.clip == bossBGM && audioSource.isPlaying)
        {
            return;
        }


        // 停止当前播放
        audioSource.Stop();

        // 设置Boss BGM并播放
        audioSource.clip = bossBGM;
        audioSource.loop = true; // 设置循环
        audioSource.Play();

        isPlayingBossBGM = true;

        Debug.Log("开始播放Boss战斗BGM");
    }

    public void PlayBossroomBGM()
    {
        if (audioSource == null || bossBGM == null)
        {
            Debug.LogWarning("BossBGMControl: AudioSource或BossBGM未设置！");
            return;
        }

        // 如果已经在播放Boss BGM，则不需要重复播放
        if (isPlayingBossBGM && audioSource.clip == bossBGM && audioSource.isPlaying)
        {
            return;
        }


        // 停止当前播放
        audioSource.Stop();

        // 设置Boss BGM并播放
        audioSource.clip = bossroomBGM;
        audioSource.loop = true; // 设置循环
        audioSource.Play();

        //isPlayingBossBGM = true;

        Debug.Log("开始播放Boss战斗BGM");
    }
    /// <summary>
    /// 停止播放Boss BGM
    /// </summary>
    /// <param name="shiftedBGM">切换的背景音乐，如果不传入则不播放任何音乐</param>
    public void StopBossBGM(AudioClip shiftedBGM = null)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("BossBGMControl: AudioSource未设置！");
            return;
        }

        // 如果当前没有在播放Boss BGM，则不需要处理
        if (!isPlayingBossBGM)
        {
            Debug.Log("当前没有在播放Boss BGM");
            return;
        }

        // 停止当前播放
        audioSource.Stop();

        // 根据参数决定播放什么音乐
        if (shiftedBGM != null)
        {
            // 播放传入的新背景音乐
            audioSource.clip = shiftedBGM;
            audioSource.Play();
            Debug.Log($"切换到新的背景音乐: {shiftedBGM.name}");
        }
        else
        {
            // 不传入新音乐，则保持停止状态
            audioSource.clip = null;
            Debug.Log("停止播放背景音乐");
        }

        isPlayingBossBGM = false;
    }

    /// <summary>
    /// 检查当前是否正在播放Boss BGM
    /// </summary>
    public bool IsPlayingBossBGM()
    {
        return isPlayingBossBGM;
    }

    /// <summary>
    /// 设置Boss BGM（也可以在Inspector中设置）
    /// </summary>
    public void SetBossBGM(AudioClip newBossBGM)
    {
        bossBGM = newBossBGM;
    }
}