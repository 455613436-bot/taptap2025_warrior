using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStoneDetect : MonoBehaviour
{
    public GameObject Player;
    public PlayerJump pj;


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        pj = Player.GetComponent<PlayerJump>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Feet")
        {
            pj.isOnStone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Feet")
        {
            pj.isOnStone = false;
        }
    }
}
