using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingBagManager : MonoBehaviour
{

    public GameObject listItemPrefab;
    public ListHandler listHandler;
    public Image image;

    public void DrawItem(GroceryItem item)
    {
        GameObject listItem = Instantiate(listItemPrefab) as GameObject;
        listItem.transform.SetParent(this.transform);
        listItem.GetComponent<Image>().sprite = item.icon;
        listItem.name = item.objectName;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
