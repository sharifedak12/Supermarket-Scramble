using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class itemsObtained : MonoBehaviour
{
    public GameObject completeItemPrefab;

      public void DrawList()
    {
        foreach (GroceryItem groceryItem in LevelCompleteItems.items) //
        {
                    GameObject listItem = Instantiate(completeItemPrefab) as GameObject; 
                    listItem.transform.SetParent(this.transform);
                    Transform image = listItem.transform.Find("Image");
                    image.GetComponent<Image>().sprite = groceryItem.icon;
                    Transform text = listItem.transform.Find("Text");
                    text.GetComponent<TextMeshProUGUI>().text = groceryItem.objectName;
                    listItem.name = groceryItem.objectName;
        }
    }

    void Start ()
    {
        DrawList();


    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
