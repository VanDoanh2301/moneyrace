using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void Jump()
    {
        _anim.SetTrigger("Jump");
    }
}
