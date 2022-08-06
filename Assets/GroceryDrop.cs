using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NameOfClass : MonoBehaviour
{   private GameObject hungryBubble;
    
    // Start is called before the first frame update
    void Start()
    {
        hungryBubble = transform.GetChild(0).gameObject;
        // OR
        // hungryBubble = transform.Find("HungryBubble").gameObject;
         }

    public void showHungry()
    {
        hungryBubble.SetActive(true);

    }
}
