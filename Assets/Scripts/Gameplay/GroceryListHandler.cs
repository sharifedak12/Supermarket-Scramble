using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GroceryListHandler : MonoBehaviour
{ 
    public GameObject listItemPrefab;
    // public GroceryListItems listItemPrefabScript;
    private ListHandler listHandler;
    private Image image;

// Draw the list of items in the grocery list.
    public void DrawList()
    {
        foreach (GroceryItem groceryItem in listHandler.groceryList)
        {
                    GameObject listItem = Instantiate(listItemPrefab) as GameObject;
                    listItem.transform.SetParent(this.transform, false);
                    listItem.GetComponent<Image>().sprite = groceryItem.icon; 
                    listItem.name = groceryItem.objectName;
        }
    }

    void Start ()
    {
        listHandler = GameObject.Find("ListHandler").GetComponent<ListHandler>();
        DrawList();


    }
    void Update ()
    {

    }


}
