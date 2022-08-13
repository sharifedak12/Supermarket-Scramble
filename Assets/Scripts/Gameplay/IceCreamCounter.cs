using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IceCreamCounter : MonoBehaviour
{
    public GroceryItem item;
    public ListHandler listHandler;
    public int pickupTime;
    public GameObject itemIndicator;
    public AudioSource sFX;
    private bool canInteract = false;
    public GameObject waitMessageBox;
    public GameObject successMessage;
    public TMP_Text notReadyMessage;

    void Start()
    {
        listHandler = GameObject.Find("ListHandler").GetComponent<ListHandler>();
        sFX = GameObject.Find("Pickup SFX").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetButtonDown("interact"))
        {
            if (canInteract)
            {          
                Interact();
            }
        }
      
    }

    public void Interact()
    {
        if (listHandler.onLastItem)
        {
        StartCoroutine(pickupMessage());
        listHandler.groceryList.Remove(item);
        var toBeDestroyed = GameObject.Find(item.objectName);
        Destroy(toBeDestroyed);
        GameObject.Find("Shopping Bag").GetComponent<ShoppingBagManager>().DrawItem(item);
        listHandler.bag.Add(item);
        Destroy(itemIndicator.gameObject);
        sFX.Play();
        }
        else 
        {
            StartCoroutine (waitForPickup());
        }
        }

    
    IEnumerator pickupMessage()
    {
        successMessage.SetActive(true);
        yield return new WaitForSeconds(1);
        successMessage.SetActive(false);
    }

    IEnumerator waitForPickup()
    {
        waitMessageBox.SetActive(true);
        notReadyMessage.text = "Sorry, you can't pick up " + item.objectName + " yet!";
        yield return new WaitForSeconds(2);
        waitMessageBox.SetActive(false);
        notReadyMessage.text = "";
        }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            canInteract = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            canInteract = false;
        }
    }

}