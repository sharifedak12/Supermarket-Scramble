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
	private GameObject ohNo;

	private void Start()
	{
		animator = GetComponent<Animator>();
		myRigidbody = GetComponent<Rigidbody2D>();
		slipSound = GetComponent<AudioSource>();
		ohNo = GameObject.Find("Oh No");
		ohNo.SetActive(false);
	}


	private void FixedUpdate()
	{
		if (isSlippery)
		{
		
		 Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * speed;
			myRigidbody.AddForce(direction, ForceMode2D.Impulse);
			myRigidbody.velocity = myRigidbody.velocity;
			StartCoroutine(OhNo());
		}
		else
		{
		playerMovement = Vector3.zero;
		playerMovement.x = Input.GetAxisRaw("Horizontal");
		playerMovement.y = Input.GetAxisRaw("Vertical");

		UpdateAnimationAndMove();
		}
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
	
	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.tag == "Slippery")
		{
			isSlippery = true;
			speed = 2000f;
			slipSound.Play();
		}
	}
	void OnTriggerExit2D (Collider2D other)
	{
		if (other.tag == "Slippery")
		{
			isSlippery = false;
			speed = 4f;
		}
	}
	IEnumerator OhNo()
	{
		ohNo.SetActive(true);
		yield return new WaitForSeconds(1);
		ohNo.SetActive(false);
	}

}
