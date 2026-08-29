using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartRoom : MonoBehaviour
{
    public static RestartRoom Instance;
    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetScene()
    {
        // 获取当前场景的索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // 重新加载当前场景
        SceneManager.LoadScene(currentSceneIndex);

    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
