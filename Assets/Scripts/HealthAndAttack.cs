using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthAndAttack : MonoBehaviour {
    public Image HealthBarImage;
    public TextMeshProUGUI DeadText;
    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public bool alive = true;

    void Awake() {
        DeadText.enabled = false;
    }

    void changeHealth(float amount) {
        if (!alive) return;
        health = Mathf.Min(Mathf.Max(health + amount, 0.0f), maxHealth);
        HealthBarImage.fillAmount = health / maxHealth;
        if (health == 0.0f) {
            alive = false;
            DeadText.enabled = true;
        }
    }

    void Update() {
        //changeHealth(-0.2f);
    }
}
