using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListHandler : MonoBehaviour
{
    public List<ScriptableObject> groceryList = new List<ScriptableObject>();
    public GroceryItem groceryItem;
    public GameObject completeMessage;
    public int listLength;
    public List<ScriptableObject> bag = new List<ScriptableObject>();
    public bool shoppingComplete = false;
    public bool onLastItem = false;
    // Start is called before the first frame update
    void Start()
    {
        listLength = groceryList.Count;
        completeMessage = GameObject.Find("CompleteMessage");
    }

    // Update is called once per frame
    void Update()
    {
        if (bag.Count == listLength)
        {
            completeMessage.SetActive(true);
            shoppingComplete = true;
        }
        else
        {
            completeMessage.SetActive(false);
        }
        
       if (groceryList.Count == 1) 
        {
            onLastItem = true;
        }
        else 
        {
            onLastItem = false;
        }

    }

}
