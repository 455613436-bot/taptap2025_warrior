using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{

    public GameObject Player;
    private PlayerJump pj;
    private PlayerMove pm;
    // Start is called before the first frame update
    void Start()
    {
        pj = Player.GetComponent<PlayerJump>();
        pm = Player.GetComponent<PlayerMove>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Feet")
        {
            //collision.transform.parent.parent = transform;
            pj.isOnPlatform = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Feet")
        {
            pj.isOnPlatform = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Feet")
        {
            //Transform PlayerTransform = collision.transform.parent;
            //PlayerTransform.parent = null;
            pj.isOnPlatform = false;
            
            pm.leftPlatform = true;
            //Rigidbody2D=gameObject.GetComponent<Rigidbody2D>()
            pm.leftSpeed = transform.GetComponent<Rigidbody2D>().velocity.x;
            Debug.Log("exit platform" + pm.leftSpeed);
        }
    }
}
