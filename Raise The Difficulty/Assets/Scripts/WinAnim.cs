using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinAnim : MonoBehaviour
{
    [SerializeField] RectTransform winPanel, staminaBar, hitTracker, timerRect;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDur;

    public void WinIntro()
    {
        canvasGroup.DOFade(1, tweenDur).SetUpdate(true);
        winPanel.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
        staminaBar.DOAnchorPosX(-530, tweenDur).SetUpdate(true);
        hitTracker.DOAnchorPosY(93, tweenDur).SetUpdate(true);
        timerRect.DOAnchorPosX(2194, tweenDur).SetUpdate(true);
    }
}
