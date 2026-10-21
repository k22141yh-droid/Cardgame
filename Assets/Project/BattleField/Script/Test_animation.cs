using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Test_animation : MonoBehaviour
{
    public GameObject Image;
    private Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = Image.GetComponent<Animator>();
        m_Animator.SetTrigger("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
