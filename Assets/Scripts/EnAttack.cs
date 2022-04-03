using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnAttack : MonoBehaviour {
    BoxCollider2D Self_BoxCollider;
    bool isPlayer = false;

    float ATTACK_COOLDOWN = 1.0f;

    float attackCooldown = 0.0f;

    void Start() {
        Self_BoxCollider = GetComponent<BoxCollider2D>();
    }

    void Update() {
        isPlayer = GetComponent<EnMain>().isPlayer;
        if ((GetComponent<EnMove>().inputs.attack1) && attackCooldown == 0.0f) {
            var attackRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y), Vector2.left, 5.0f, LayerMask.GetMask("Entity"));
            var attackRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y), Vector2.right, 5.0f, LayerMask.GetMask("Entity"));
            if (attackRaycastL.collider != null) attackRaycastL.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            if (attackRaycastR.collider != null) attackRaycastR.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            attackCooldown = ATTACK_COOLDOWN;
        }

        if (attackCooldown > 0.0f) {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown < 0.0f) attackCooldown = 0.0f;
        }
    }
}
