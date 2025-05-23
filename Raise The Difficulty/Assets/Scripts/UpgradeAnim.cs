using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class UpgradeAnim : MonoBehaviour
{
    [SerializeField] RectTransform upgradePanel, staminaBar, hitTracker, timerRect;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDur;

    public void UpgradeIntro()
    {
        canvasGroup.DOFade(1, tweenDur).SetUpdate(true);
        upgradePanel.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
        staminaBar.DOAnchorPosX(-530, tweenDur).SetUpdate(true);
        hitTracker.DOAnchorPosY(93, tweenDur).SetUpdate(true);
        timerRect.DOAnchorPosX(297, tweenDur).SetUpdate(true);
    }  
    
    public void UpgradeOutro()
    {
        canvasGroup.DOFade(0, tweenDur).SetUpdate(true);
        upgradePanel.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true);
        staminaBar.DOAnchorPosX(-53, tweenDur).SetUpdate(true);
        hitTracker.DOAnchorPosY(-115, tweenDur).SetUpdate(true);
        timerRect.DOAnchorPosX(-290, tweenDur).SetUpdate(true);
    }
}
