using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public SoundEffect sound;
    public GameObject healthbar;
    public GameObject healthtext;
    public GameObject dragonPrin;
    public GameObject arrowattack;
    public GameObject word;
    public BossBGMControl bgm;
    public PlayerMove pm;
    // Start is called before the first frame update
    private bool isInDialogueZone = false;

    // Start is called before the first frame update
    void Start()
    {
        // 推荐：在游戏开始时确保这些物体是隐藏的
        healthbar.SetActive(false);
        healthtext.SetActive(false);
        dragonPrin.SetActive(false);
        arrowattack.SetActive(false);
        word.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            isInDialogueZone = true;
            word.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isInDialogueZone && Input.GetMouseButtonDown(0))
        {
            // 鼠标左键被点击时执行的内容
            TriggerAction();
        }
    }
    void TriggerAction()
    {
        // 触发相应内容
        healthbar.SetActive(true);
        healthtext.SetActive(true);
        dragonPrin.SetActive(true);
        arrowattack.SetActive(true);
        bgm.PlayBossBGM();
        // 隐藏/销毁触发器
        word.SetActive(false);
        gameObject.SetActive(false);
        sound.PlayScareSound();
        pm.moveSpeed = 4f;
        // 可选：重置状态（尽管隐藏了 gameObject，但保持习惯）
        isInDialogueZone = false;
    }
}
