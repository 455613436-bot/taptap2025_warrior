using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingAnim : MonoBehaviour
{
    private Animator anim;
    private KingMove kingMove;

    public bool HasMoved;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        kingMove = GetComponent<KingMove>();

        HasMoved = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!HasMoved)
        {
            if(kingMove.isrun)
            {
                HasMoved = true;
                anim.SetTrigger("HasMoved");
            }
        }

        anim.SetBool("IsRun", kingMove.isrun);


    }
}
