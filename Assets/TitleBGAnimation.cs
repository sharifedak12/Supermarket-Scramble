using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TitleBGAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    { 
        titleAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void titleAnimation()
{
    transform.DOLocalMoveY(50, 1);
    transform.DOLocalMoveX(0, 1);
    transform.DOLocalMoveZ(0, 1);
    }

    }

