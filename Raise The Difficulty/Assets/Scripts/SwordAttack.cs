using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Collider2D swordCollider;
    public float damage;
    Vector2 rightAttackOffset;

    private void Start() 
    {
        rightAttackOffset = transform.position;
    }

    public void AttackRight() 
    {
        swordCollider.enabled = true;
        transform.localPosition = rightAttackOffset;
    }

    public void AttackLeft() 
    {
        swordCollider.enabled = true;
        transform.localPosition = new Vector3(rightAttackOffset.x * -1, rightAttackOffset.y);
    }

    public void StopAttack() 
    {
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.tag == "Enemy") 
        {
            // Deal damage to the enemy
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null) 
            {
                enemy.Health -= damage;
            }
        }
    }
}


//Chris' Tutorials https://www.youtube.com/watch?v=7iYWpzL9GkM
