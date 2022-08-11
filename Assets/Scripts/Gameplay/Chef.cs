using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chef : MonoBehaviour
{
    public GroceryItem item;
    public ListHandler listHandler;
    public int pickupTime;
    public GameObject itemIndicator;
    public AudioSource sFX;
    private bool canInteract = false;
    private bool isWaiting = false;
    private bool canPickup = false;
    public TMP_Text waitMessage;
    public GameObject waitMessageBox;
    public GameObject successMessage;
    public TMP_Text readyMessage;

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
        if (canPickup)
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
        else if (!isWaiting) {
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
        waitMessage.text = item.objectName + " will be ready in " + pickupTime + " seconds!";
        Debug.Log(waitMessage.text);
        waitMessageBox.SetActive(true);
        yield return new WaitForSeconds(2);
        waitMessage.text = "";
        waitMessageBox.SetActive(false);
        isWaiting = true;
        yield return new WaitForSeconds(pickupTime);
        canPickup = true;
        isWaiting = false;
        waitMessageBox.SetActive(true);
        readyMessage.text = item.objectName + " is ready for pickup!";
        yield return new WaitForSeconds(2);
        waitMessageBox.SetActive(false);
        readyMessage.text = "";
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