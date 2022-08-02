using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class titleanimation : MonoBehaviour
{
    private TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
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
    DOTweenTMPAnimator animator = new DOTweenTMPAnimator(text);
    Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < animator.textInfo.characterCount; ++i) {
            sequence.Append(animator.DOOffsetChar(i, new Vector3(0, 10, 0), .15f));
            sequence.Append(animator.DOOffsetChar(i, new Vector3(0, 0, 0), .15f));
    }

    }

}


