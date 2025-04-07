using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float life = 3f;

    private void Start()
    {
        Destroy(gameObject, life);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHit hit = other.GetComponent<PlayerHit>() ?? other.GetComponentInParent<PlayerHit>();
        

        if (hit != null)
        {
            hit.RegisterHit();
            Destroy(gameObject);
        }
    }
}
