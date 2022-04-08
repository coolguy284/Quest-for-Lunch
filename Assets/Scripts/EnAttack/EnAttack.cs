using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnAttack : MonoBehaviour {
    BoxCollider2D Self_BoxCollider;
    bool isPlayer = false;

    float ATTACK_COOLDOWN = 1.0f;
    float BASIC_HITBOT_SIZE = 1.0f;

    float attackCooldown = 0.0f;

    RaycastHit2D[] attackRaycast(Vector2 origin, Vector2 direction, float distance) {
        Debug.DrawRay(new Vector3(origin.x, origin.y, 0.0f), new Vector3(direction.x, direction.y, 0.0f), isPlayer ? Color.red : Color.yellow, 0.1f, false);
        return Physics2D.RaycastAll(origin, direction, distance, LayerMask.GetMask("Entity"));
    }

    void Start() {
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        isPlayer = GetComponent<EnMain>().isPlayer;
        if (isPlayer) {
            ATTACK_COOLDOWN = 0.1f;
        } else {
            ATTACK_COOLDOWN = 2.0f;
        }
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        isPlayer = GetComponent<EnMain>().isPlayer;
        if ((GetComponent<EnMove>().inputs.attack1) && attackCooldown == 0.0f) {
            var attackRaycastL = attackRaycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y), Vector2.left, BASIC_HITBOT_SIZE);
            var attackRaycastR = attackRaycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y), Vector2.right, BASIC_HITBOT_SIZE);
            foreach (var raycast in attackRaycastL) {
                raycast.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            }
            foreach (var raycast in attackRaycastR) {
                raycast.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            }
            attackCooldown = ATTACK_COOLDOWN;
        }

        if (GetComponent<EnMove>().fastDropStoppedFrame) {
            var attackRaycastL = attackRaycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y + Self_BoxCollider.bounds.extents.y - 0.1f), Vector2.left, BASIC_HITBOT_SIZE);
            var attackRaycastR = attackRaycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y + Self_BoxCollider.bounds.extents.y - 0.1f), Vector2.right, BASIC_HITBOT_SIZE);
            foreach (var raycast in attackRaycastL) {
                raycast.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            }
            foreach (var raycast in attackRaycastR) {
                raycast.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            }
        }

        if (attackCooldown > 0.0f) {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown < 0.0f) attackCooldown = 0.0f;
        }
    }
}
