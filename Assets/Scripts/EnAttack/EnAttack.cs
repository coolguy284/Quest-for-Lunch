using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnAttack : MonoBehaviour {
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    public TextMeshProUGUI DebugText3;
    bool isPlayer = false;

    float ATTACK_COOLDOWN = 1.0f;
    float BASIC_HITBOT_SIZE = 1.0f;
    float FASTDROP_HITBOT_SIZE = 0.6f;
    float FASTDROP_LAG = 0.5f;
    //float TELEPORT_ATTACK_MAX = 10.0f;
    float MOMENTUM_HALT_TIME = 1.0f;
    
    float attackCooldown = 0.0f;
    //float teleportAttack = 0.0f;
    //int attackCombo = 0;
    bool givenMomentumHalt = false; // for the one guaranteed momentum halt
    float extraMomentumHalt = 0.0f; // extra momentum halt time

    void attackRaycast(Vector2 origin, Vector2 direction, float distance) {
        var wallRaycast = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Default", "Platform"));
        float attackDistance;
        if (wallRaycast.collider == null) attackDistance = distance;
        else attackDistance = wallRaycast.fraction;
        Debug.DrawRay(new Vector3(origin.x, origin.y, 0.0f), new Vector3(direction.x, direction.y, 0.0f) * attackDistance, isPlayer ? Color.red : Color.yellow, 0.1f, false);
        var attackRaycast = Physics2D.RaycastAll(origin, direction, attackDistance, LayerMask.GetMask("Entity"));
        foreach (var raycast in attackRaycast) {
            raycast.collider.GetComponent<EnHealth>().changeHealth(isPlayer ? -50.0f : -10.0f);
            if (isPlayer && !raycast.collider.GetComponent<EnHealth>().alive) {
                GetComponent<EnMain>().susCoins += (int)raycast.collider.GetComponent<EnHealth>().maxHealth;
                GetComponent<EnMain>().SusCoinsText.text = GetComponent<EnMain>().susCoins.ToString();
            }
        }
    }

    void Start() {
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        isPlayer = GetComponent<EnMain>().isPlayer;
        if (isPlayer) {
            ATTACK_COOLDOWN = 0.3f;
        } else {
            ATTACK_COOLDOWN = 2.0f;
        }
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        isPlayer = GetComponent<EnMain>().isPlayer;

        if (GetComponent<EnMove>().fastDropStoppedFrame) {
            // fastdrop attack
            attackRaycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y - Self_BoxCollider.bounds.extents.y * 0.5f), Vector2.left, FASTDROP_HITBOT_SIZE);
            attackRaycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y - Self_BoxCollider.bounds.extents.y * 0.5f), Vector2.right, FASTDROP_HITBOT_SIZE);
            GetComponent<EnMove>().inputLagTime = FASTDROP_LAG;
        } else if (GetComponent<EnMove>().inputs.attackMelee && attackCooldown == 0.0f && GetComponent<EnMove>().isNormalState) {
            // normal attack
            if (GetComponent<EnMove>().facingRight) {
                attackRaycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y), Vector2.right, BASIC_HITBOT_SIZE);
            } else {
                attackRaycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y), Vector2.left, BASIC_HITBOT_SIZE);
            }
            attackCooldown = ATTACK_COOLDOWN;

            // halt momentum
            if (!givenMomentumHalt) {
                Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
                GetComponent<EnMain>().haltMotion = true;
                givenMomentumHalt = true;
                if (GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME) {
                    extraMomentumHalt = GetComponent<EnMove>().inAirTime;
                }
            } else if (GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME + extraMomentumHalt) {
                Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
                GetComponent<EnMain>().haltMotion = true;
            } else {
                GetComponent<EnMain>().haltMotion = false;
            }
        }

        if (attackCooldown > 0.0f && (!givenMomentumHalt || GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME + extraMomentumHalt)) {
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
            GetComponent<EnMain>().haltMotion = true;
        } else {
            GetComponent<EnMain>().haltMotion = false;
        }

        // refresh momentumhalts if on ground
        if (GetComponent<EnMove>().isGrounded) {
            givenMomentumHalt = false;
            extraMomentumHalt = 0.0f;
        }

        // reduce attack cooldown by time passed
        if (attackCooldown > 0.0f) {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown < 0.0f) attackCooldown = 0.0f;
        }
        
        // debug text
        if (isPlayer) DebugText3.text = string.Format("GivenHalt: {0}\nExtraHalt: {1:0.000}", givenMomentumHalt, extraMomentumHalt);
    }
}
