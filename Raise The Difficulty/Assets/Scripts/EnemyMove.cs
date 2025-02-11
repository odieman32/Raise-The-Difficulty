using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{

    Transform player;
    public float moveSpeed;
    public Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>().transform;
    }

   private void EnemyMoving()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime); //Constantly move towards player
    }
}

//Terresquall https://www.youtube.com/watch?v=RCOxhTsbAWo&list=PLgXA5L5ma2Bveih0btJV58REE2mzfQLOQ&index=4
