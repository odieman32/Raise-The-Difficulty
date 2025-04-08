using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthbar : MonoBehaviour
{
    public Image healthFill;

    private BossEnemy boss;
    private float maxHealth;


    public void SetBoss(BossEnemy boss)
    {
        this.boss = boss;
        maxHealth = boss.health;
        UpdateHealthBar();
    }

    private void Update()
    {
        if (boss != null)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthFill != null && boss != null)
        {
            healthFill.fillAmount = boss.health / maxHealth;
        }
    }
}
