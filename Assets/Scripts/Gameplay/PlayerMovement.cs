using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    private float speed = 4f;
    private Rigidbody2D myRigidbody;
    private Vector3 playerMovement;
    private Animator animator;
    public bool isSlippery;
    public AudioSource slipSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody2D>();
        slipSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Slippery")
        {
            isSlippery = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Slippery")
        {
            isSlippery = false;
           
        }
       
    }

    private void FixedUpdate()
    {
        if (isSlippery)
        {
            speed = 10f;
            myRigidbody.velocity = 10 * playerMovement;
            StartCoroutine(playerSlipSound());
        }
        else
        {
            speed = 4f;
        }
        playerMovement = Vector3.zero;
        playerMovement.x = Input.GetAxisRaw("Horizontal");
        playerMovement.y = Input.GetAxisRaw("Vertical");

        UpdateAnimationAndMove();
   
    }

    private void UpdateAnimationAndMove()
    {
        if (playerMovement != Vector3.zero)
        {
            MoveCharacter();
            animator.SetFloat("moveX", playerMovement.x);
            animator.SetFloat("moveY", playerMovement.y);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    private void MoveCharacter()
    {
        myRigidbody.MovePosition(transform.position + playerMovement * speed * Time.deltaTime);
    }

    IEnumerator playerSlipSound()
    {
        myRigidbody.drag = 0.5f;
        slipSound.Play();
        yield return new WaitForSeconds(2);
        myRigidbody.drag = 8f;
    }

}
