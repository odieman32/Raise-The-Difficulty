using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHit : MonoBehaviour
{
    Animator animator;
    public int hitCount = 0;
    public int maxHitsAllowed;
    public Text hitCountText;
    public PerformanceWaves waves;
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
        UpdateHitsUI();
    }

    public void RegisterHit()
    {
        hitCount++;
        Debug.Log("Player Hit!" + hitCount);
        UpdateHitsUI();

        animator.SetTrigger("isHit");
    }

    public void ResetHits()
    {
        hitCount = 0;
        UpdateHitsUI();
    }

    public void SetMaxHits(int maxHits)
    {
        maxHitsAllowed = maxHits;
        UpdateHitsUI();
    }

    private void UpdateHitsUI()
    {
        if (hitCountText != null)
        {
            hitCountText.text = $"Hits: {hitCount} / {maxHitsAllowed}";
            hitCountText.color = (hitCount > maxHitsAllowed) ? dangerColor : normalColor;
        }
    }
}
