using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public bool canPickup = true;
    private ListHandler listHandler;
    public GroceryItem item;
    // Start is called before the first frame update
    void Start()
    {
        listHandler = GameObject.Find("ListHandler").GetComponent<ListHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("interact"))
        {
            if (canPickup)
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
        listHandler.groceryList.Remove(item);
        var toBeDestroyed = GameObject.Find(item.objectName);
        Destroy(toBeDestroyed);
        GameObject.Find("Shopping Bag").GetComponent<ShoppingBagManager>().DrawItem(item);
        listHandler.bag.Add(item);
        Destroy(this.gameObject);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            canPickup = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            canPickup = false;
        }
    }
}
