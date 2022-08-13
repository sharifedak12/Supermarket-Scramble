using UnityEngine;
using System.Collections;

public class Escalator : MonoBehaviour
{
    public float distance;
    private Rigidbody2D rb;
    void Start()
    {
        rb = GameObject.Find("Player").GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
           rb.velocity = new Vector2 (0, distance);
        }
    }
}