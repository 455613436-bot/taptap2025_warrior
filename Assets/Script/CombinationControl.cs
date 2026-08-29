using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationControl : MonoBehaviour
{
    public ButtonControl button;
    public UnderStoneDetect USD;
    public Rigidbody2D stoneRigidbody;
    public GameObject longzi;
    //public float fadeDuration = 2.0f;
    private bool runOnce = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!button.isUp && runOnce)
        {
            if (stoneRigidbody == null)
            {
                Debug.LogError("你还没有在Inspector中指定 'Stone Rigidbody'！");
                return;
            }
            Destroy(longzi);
            //stoneRigidbody.isKinematic = false;
            USD.isInLongzi = false;
            stoneRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            stoneRigidbody.WakeUp();
            runOnce = false;
        }
    }

}
