using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GroceryDrop : MonoBehaviour
{   private GameObject item1;
    private GameObject item2;
    private GameObject item3;
    private GameObject item4;
    private GameObject item5;
    private GameObject item6;
    private GameObject item7;
    private GameObject item8;
    private GameObject item9;
    private GameObject item10;
    private GameObject item11;
    private GameObject item12;
    private GameObject item13;
    private GameObject item14;
    private GameObject item15;
    private GameObject item16;
    private GameObject item17;
    private GameObject item18;
    private GameObject item19;
    private GameObject item20;

    // Start is called before the first frame update
    void Start()
    {
        item1 = transform.GetChild(0).gameObject;
        item2 = transform.GetChild(1).gameObject;
        item3 = transform.GetChild(2).gameObject;
        item4 = transform.GetChild(3).gameObject;
        item5 = transform.GetChild(4).gameObject;
        item6 = transform.GetChild(5).gameObject;
        item7 = transform.GetChild(6).gameObject;
        item8 = transform.GetChild(7).gameObject;
        item9 = transform.GetChild(8).gameObject;
        item10 = transform.GetChild(9).gameObject;
        item11 = transform.GetChild(10).gameObject;
        item12 = transform.GetChild(11).gameObject;
        item13 = transform.GetChild(12).gameObject;
        item14 = transform.GetChild(13).gameObject;
        item15 = transform.GetChild(14).gameObject;
        item16 = transform.GetChild(15).gameObject;
        item17 = transform.GetChild(16).gameObject;
        item18 = transform.GetChild(17).gameObject;
        item19 = transform.GetChild(18).gameObject;
        item20 = transform.GetChild(19).gameObject;

        dropGroceries();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void dropGroceries()
    {
        item1.transform.DOLocalMoveY(-410, 5);
        item2.transform.DOLocalMoveY(-375, 1);
    }
}
