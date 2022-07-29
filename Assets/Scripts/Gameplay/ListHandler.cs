using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListHandler : MonoBehaviour
{
    public List<ScriptableObject> groceryList = new List<ScriptableObject>();
    public GameObject completeMessage;
    public int listLength;
    public List<ScriptableObject> bag = new List<ScriptableObject>();
    public bool shoppingComplete = false;
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
    }

}
