using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovementVertical : MonoBehaviour {

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
            facing.x = 0;
            facing.y = 1;
        }

  if(isWalking == true)
        {
            walkCounter -= Time.deltaTime;
            switch (walkDirection)
            {


                case 0:
                //up
                    rb.velocity = new Vector2(0, moveSpeed);
                    facing.y = 1;
                    facing.x = 0;
                    break;

                case 1:
                //down
                    rb.velocity = new Vector2(0, -moveSpeed);
                    facing.y = -1;
                    facing.x = 0;
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
        if (walkDirection == 0)
        {
            walkDirection = 1;
        }
        else 
        {
            walkDirection = 0;
        }
        isWalking = true;
        walkCounter = walkTime;
    }
}