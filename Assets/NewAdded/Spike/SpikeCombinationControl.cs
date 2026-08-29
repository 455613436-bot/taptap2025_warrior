using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeCombinationControl : MonoBehaviour
{
    public ButtonControl button;

    public GameObject[] Spikes = new GameObject[3];
    //public float fadeDuration = 2.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!button.isUp)
        {
            Spikes[0].SetActive(true);
            Spikes[1].SetActive(false);
            Spikes[2].SetActive(false);
        }
        else
        {
            Spikes[0].SetActive(false);
            Spikes[1].SetActive(true);
            Spikes[2].SetActive(true);
        }
    }

}
