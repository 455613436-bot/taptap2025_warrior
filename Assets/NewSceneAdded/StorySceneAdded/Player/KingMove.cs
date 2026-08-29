using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    public float moveSpeed;
    public float moveController;

    public bool isrun;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetKeyboard();
        PlayerFace();
    }

    private void GetKeyboard()
    {
        moveController = Input.GetAxis("Horizontal");

        isrun = (moveController != 0f);

        rb.velocity = new Vector2(moveSpeed * moveController, rb.velocity.y);
    }

    private void PlayerFace()
    {
        if (moveController < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        if (moveController > 0)
        {
            transform.localScale = new Vector2(1, 1);
        }
    }
}
