using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingSfx : MonoBehaviour
{

    public AudioClip JumpSfx;
    public AudioSource audioSource;

    private KingMove kingMove;
    // Start is called before the first frame update
    void Start()
    {
        kingMove = GetComponent<KingMove>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
