using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCIdleAnim : MonoBehaviour
{
   public float moveSpeed;
    private Rigidbody2D rb;
    public Vector2 facing;
    public Animator anim;
 
 void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
         
 }
 
 
 void Update ()
    {  
        anim.SetFloat("moveX", facing.x);
        anim.SetFloat("moveY", facing.y);
        anim.SetBool("moving", false);
    }
}

