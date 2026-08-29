using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private enum Anim { idle, run, jump, fall };
    private Anim state;
    private Animator anim;
    private PlayerMove playerMove;
    private PlayerJump playerJump;
    public RuntimeAnimatorController normalAnimatorController;  // 你的 "Player_Base_Animator"
    public RuntimeAnimatorController armoredAnimatorOverride; // 你的 "Player_Armored_Override"
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerJump = GetComponent<PlayerJump>();
        playerMove = GetComponent<PlayerMove>();
        if (normalAnimatorController != null)
        {
            anim.runtimeAnimatorController = normalAnimatorController;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(playerMove.moveController != 0 &&playerJump.isEnableJump)
        {
            state = Anim.run;
        }
        else
        {
            state = Anim.idle;
        }
        if (playerJump.rb.velocity.y > 0.3f)
        {
            state = Anim.jump;
        }
        if (playerJump.rb.velocity.y < -0.3f &&!playerJump.isEnableJump)
        {
            state = Anim.fall;
        }
        anim.SetInteger("state", (int)state);
    }

    public void SwitchToArmoredSet()
    {
        // 只需要这一行代码！
        // 动画状态机会保持（例如，如果之前在 "Walk"，现在会播放 "Armored_Walk"）
        anim.runtimeAnimatorController = armoredAnimatorOverride;
        Debug.Log("switchanim");
    }

    // 切换回“普通”状态
    public void SwitchToNormalSet()
    {
        anim.runtimeAnimatorController = normalAnimatorController;
        Debug.Log("switchanim_back");
    }
}
