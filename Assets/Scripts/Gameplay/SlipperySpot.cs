using System.Collections;
using UnityEngine;

public class SlipperySpot : MonoBehaviour
{
private Rigidbody2D rb;
public float slipperySpeed;
public AudioSource slipperySound;

void Start()
{
	rb = GameObject.Find("Player").GetComponent<Rigidbody2D>();
}

void OnTriggerEnter2D(Collider2D other)
{
	if (other.gameObject.tag == "Player")
	{
		slipperySound.Play();
		rb.velocity = new Vector2(0, slipperySpeed);
	}
}
}