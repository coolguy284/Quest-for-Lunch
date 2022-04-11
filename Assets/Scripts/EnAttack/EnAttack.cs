using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnAttack : MonoBehaviour {
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    EnMain EnMainInst;
    public TextMeshProUGUI DebugText3;
    bool isPlayer = false;

    float ATTACK_STARTUP = 2.0f;
    float ATTACK_COOLDOWN = 2.0f;
    float BASIC_HITBOT_SIZE = 1.0f;
    float FASTDROP_HITBOT_SIZE = 0.6f;
    float FASTDROP_LAG = 0.5f;
    //float TELEPORT_ATTACK_MAX = 10.0f;
    float MOMENTUM_HALT_TIME = 1.0f;
    float KNOCKBACK_SCALE = 0.9f; // what amount of the damage is the knockback force
    float HITSTUN_SCALE = 0.02f;
    
    float attackCooldown = 0.0f;
    //float teleportAttack = 0.0f;
    //int attackCombo = 0;
    bool givenMomentumHalt = false; // for the one guaranteed momentum halt
    float extraMomentumHalt = 0.0f; // extra momentum halt time

    void attackRaycast(Vector2 origin, Vector2 direction, float distance) {
        // check for intersection with wall
        var wallRaycast = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Default", "Platform"));
        float attackDistance;
        if (wallRaycast.collider == null) attackDistance = distance;
        else attackDistance = wallRaycast.fraction;

        // calculate attack ray
        Debug.DrawRay(new Vector3(origin.x, origin.y, 0.0f), new Vector3(direction.x, direction.y, 0.0f) * attackDistance, isPlayer ? Color.red : Color.yellow, 0.1f, false);
        var attackRaycast = Physics2D.RaycastAll(origin, direction, attackDistance, isPlayer ? LayerMask.GetMask("Enemy") : LayerMask.GetMask("Player"));
        
        float damage = isPlayer ? 50.0f : 10.0f;

        // for each thing ray hit
        foreach (var raycast in attackRaycast) {
            var entity = raycast.collider;
            // only attack if vulnerable
            if (entity.GetComponent<EnHealth>().invulnTime == 0.0f) {
                // perform damage
                entity.GetComponent<EnHealth>().changeHealth(-damage);

                // perform knockback
                entity.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction.x > 0.0f ? 1.0f : -1.0f, 2.0f) * Mathf.Pow(damage, 0.25f) * KNOCKBACK_SCALE, ForceMode2D.Impulse);
                entity.GetComponent<EnAttack>().attackCooldown = 0.0f;
                entity.GetComponent<EnMove>().inputLagTime = damage * HITSTUN_SCALE;

                // grant suscoins
                if (isPlayer && !entity.GetComponent<EnHealth>().alive) {
                    EnMainInst.susCoins += (int)(entity.GetComponent<EnHealth>().maxHealth * 1.25f);
                    EnMainInst.SusCoinsText.text = EnMainInst.susCoins.ToString();
                }
            }
        }
    }

    void FastDropAttack() {
        // fastdrop attack
        attackRaycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX - EnMainInst.bounds.extraGap, transform.position.y - EnMainInst.bounds.extentY * 0.5f), Vector2.left, FASTDROP_HITBOT_SIZE);
        attackRaycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX + EnMainInst.bounds.extraGap, transform.position.y - EnMainInst.bounds.extentY * 0.5f), Vector2.right, FASTDROP_HITBOT_SIZE);
        GetComponent<EnMove>().inputLagTime = FASTDROP_LAG;
    }

    IEnumerator NormalAttack() {
        // startup lag
        attackCooldown = ATTACK_STARTUP;
        yield return new WaitForSeconds(ATTACK_STARTUP);

        // normal attack
        if (GetComponent<EnMove>().facingRight) {
            attackRaycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX + EnMainInst.bounds.extraGap, transform.position.y), Vector2.right, BASIC_HITBOT_SIZE);
        } else {
            attackRaycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX - EnMainInst.bounds.extraGap, transform.position.y), Vector2.left, BASIC_HITBOT_SIZE);
        }
        attackCooldown = ATTACK_COOLDOWN;

        // halt momentum
        if (!givenMomentumHalt) {
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
            EnMainInst.haltMotion = true;
            givenMomentumHalt = true;
            if (GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME) {
                extraMomentumHalt = GetComponent<EnMove>().inAirTime;
            }
        } else if (GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME + extraMomentumHalt) {
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
            EnMainInst.haltMotion = true;
        } else {
            EnMainInst.haltMotion = false;
        }
    }

    void Start() {
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        EnMainInst = GetComponent<EnMain>();
        isPlayer = EnMainInst.isPlayer;
        if (isPlayer) {
            ATTACK_STARTUP = 0.1f;
            ATTACK_COOLDOWN = 0.3f;
        } else {
            ATTACK_STARTUP = 2.0f;
            ATTACK_COOLDOWN = 2.0f;
        }
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        isPlayer = EnMainInst.isPlayer;

        if (GetComponent<EnMove>().fastDropStoppedFrame) {
            FastDropAttack();
        } else if (GetComponent<EnMove>().inputs.attackMelee && attackCooldown == 0.0f && GetComponent<EnMove>().isNormalState) {
            StartCoroutine(NormalAttack());
        }

        if (attackCooldown > 0.0f && (!givenMomentumHalt || GetComponent<EnMove>().inAirTime < MOMENTUM_HALT_TIME + extraMomentumHalt)) {
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
            EnMainInst.haltMotion = true;
        } else {
            EnMainInst.haltMotion = false;
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
