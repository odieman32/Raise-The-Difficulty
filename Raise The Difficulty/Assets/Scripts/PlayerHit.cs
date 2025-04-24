using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerHit : MonoBehaviour
{
    #region References
    Animator animator;
    public int hitCount = 0;
    public int maxHitsAllowed;
    public Text hitCountText;
    public PerformanceWaves waves;
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;
    #endregion

    #region Audio/Cinemachine
    [SerializeField] AudioClip hitAudio;
    private AudioSource audioSource;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    #endregion

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
        audioSource = GetComponentInParent<AudioSource>();
        UpdateHitsUI();
    }

    public void RegisterHit()
    {
        hitCount++;
        Debug.Log("Player Hit!" + hitCount);
        UpdateHitsUI();

        animator.SetTrigger("isHit");
        GetComponentInParent<AudioSource>().PlayOneShot(hitAudio);


        if (impulseSource != null )
        {
            impulseSource.GenerateImpulse();
        }
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
