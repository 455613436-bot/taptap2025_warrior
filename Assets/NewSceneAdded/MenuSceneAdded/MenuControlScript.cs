using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControlScript : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel; // 主菜单按钮的父物体
    [SerializeField] private GameObject developerList; // 开发者名单ScrollView
    [SerializeField] private GameObject settingList;   // 设置列表ScrollView

    void Start()
    {
        // 确保游戏开始时只显示主菜单
        ShowMainMenu();
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    public void OnStartGameClicked()
    {
        // 加载故事场景
        SceneManager.LoadScene("StoryScene");
    }

    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
    public void OnExitGameClicked()
    {
        // 在编辑模式下停止播放，在构建版本中退出游戏
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// 开发者名单按钮点击事件
    /// </summary>
    public void OnDeveloperListClicked()
    {
        // 隐藏主菜单按钮，显示开发者名单
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (developerList != null)
            developerList.SetActive(true);

        // 确保设置列表被隐藏
        if (settingList != null)
            settingList.SetActive(false);
    }

    /// <summary>
    /// 设置按钮点击事件
    /// </summary>
    public void OnSettingClicked()
    {
        // 隐藏主菜单按钮，显示设置列表
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingList != null)
            settingList.SetActive(true);

        // 确保开发者名单被隐藏
        if (developerList != null)
            developerList.SetActive(false);
    }

    /// <summary>
    /// 返回主菜单（可用于开发者名单和设置中的返回按钮）
    /// </summary>
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// 显示主菜单界面
    /// </summary>
    public void ShowMainMenu()
    {
        // 显示主菜单按钮
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // 隐藏其他界面
        if (developerList != null)
            developerList.SetActive(false);

        if (settingList != null)
            settingList.SetActive(false);
    }
}