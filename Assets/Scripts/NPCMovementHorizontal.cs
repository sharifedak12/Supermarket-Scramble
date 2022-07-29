using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovementHorizontal : MonoBehaviour {

    public float moveSpeed;
    private Rigidbody2D rb;
    public bool isWalking;

    public Vector2 facing;

    public float walkTime;
    private float walkCounter;
    public float waitTime;
    private float waitCounter;

    private int walkDirection;

    public Animator anim;
 
 void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();
         
 }
 
 
 void Update ()
    {
        if(isWalking == false)
        {
            facing.x = 1;
            facing.y = 0;
        }

  if(isWalking == true)
        {
            walkCounter -= Time.deltaTime;
           

            switch (walkDirection)
            {


                case 0:
                    rb.velocity = new Vector2(moveSpeed, 0);
                    facing.y = 0;
                    facing.x = 1;
                    break;

                case 1:
                    rb.velocity = new Vector2(-moveSpeed, 0);
                    facing.y = 0;
                    facing.x = -1;
                    break;



                    

            }

            if (walkCounter < 0)
            {
                isWalking = false;
                waitCounter = waitTime;
            }
        }

        else
        {
           rb.velocity = Vector2.zero;

            waitCounter -= Time.deltaTime;

            if(waitCounter < 0)
            {
                ChooseDirection();

            }
        }

        anim.SetFloat("moveX", facing.x);
        anim.SetFloat("moveY", facing.y);
        anim.SetBool("moving", isWalking);
    }

    private void ChooseDirection()
    {
        walkDirection = Random.Range(0, 1);
        isWalking = true;
        walkCounter = walkTime;
    }
}