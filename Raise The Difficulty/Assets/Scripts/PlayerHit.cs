using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHit : MonoBehaviour
{
    Animator animator;
    private int hitCount = 0;
    public Text hitCountText;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public int HitCount
    {

    get { return hitCount; }

    }

    public void RegisterHit()
    {
        hitCount++;
        Debug.Log("Player Hit!" + hitCount);

        if (hitCountText != null)
        {
            hitCountText.text = "Hits: " + hitCount;
        }
    }
}
