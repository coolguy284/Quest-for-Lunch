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

    public float ATTACK_DAMAGE = 10.0f;
    float ATTACK_STARTUP = 2.0f;
    float ATTACK_ACTIVE = 0.5f;
    float ATTACK_COOLDOWN = 2.0f;
    float FASTDROP_LAG = 0.5f;
    //float TELEPORT_ATTACK_MAX = 10.0f;
    float MOMENTUM_HALT_TIME = 1.0f;
    float KNOCKBACK_SCALE = 0.9f; // what amount of the damage is the knockback force
    float HITSTUN_SCALE = 0.02f;
    float HITINVULN_SCALE = 0.04f;
    
    float attackCooldown = 0.0f;
    //float teleportAttack = 0.0f;
    //int attackCombo = 0;
    bool givenMomentumHalt = false; // for the one guaranteed momentum halt
    float extraMomentumHalt = 0.0f; // extra momentum halt time

    void attack() {
        var colliders = transform.Find("HitBox").GetComponent<ColliderTracker>().colliders;

        // for each thing hitbox hit
        foreach (var entity in new List<Collider2D>(colliders)) {
            var direction = entity.transform.position - transform.position;

            // check for intersection with wall
            var wallRaycast = Physics2D.Raycast(
                new Vector2(transform.position.x, transform.position.y),
                direction,
                direction.magnitude,
                LayerMask.GetMask("Default", "Platform")
            );
            if (wallRaycast.collider != null) continue;
            
            // only attack if vulnerable
            if (entity.GetComponent<EnHealth>() != null && entity.GetComponent<EnHealth>().invulnTime == 0.0f) {
                // perform damage
                entity.GetComponent<EnHealth>().changeHealth(-ATTACK_DAMAGE);

                // perform knockback
                entity.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction.x > 0.0f ? 1.0f : -1.0f, 2.0f) * Mathf.Pow(ATTACK_DAMAGE, 0.25f) * KNOCKBACK_SCALE, ForceMode2D.Impulse);
                entity.GetComponent<EnAttack>().attackCooldown = 0.0f;
                entity.GetComponent<EnMove>().inputLagTime = ATTACK_DAMAGE * HITSTUN_SCALE;
                entity.GetComponent<EnHealth>().hitInvulnTime = ATTACK_DAMAGE * HITINVULN_SCALE;

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
        attack();
        GetComponent<EnMove>().inputLagTime = FASTDROP_LAG;
    }

    IEnumerator NormalAttack() {
        // startup lag
        GetComponent<EnMove>().inAttackSword = true;
        float startupTime = 0.0f;
        while (startupTime < ATTACK_STARTUP) {
            if (GetComponent<EnMove>().inputLagTime > 0.0f) {
                GetComponent<EnMove>().inAttackSword = false;
                yield break;
            }
            startupTime += Time.deltaTime;
            yield return null;
        }

        // normal attack
        attackCooldown = ATTACK_ACTIVE;
        float activeTime = 0.0f;
        while (activeTime < ATTACK_ACTIVE) {
            attack();
            activeTime += Time.deltaTime;
            yield return null;
        }
        attackCooldown = ATTACK_COOLDOWN;
        GetComponent<EnMove>().inAttackSword = false;

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
            ATTACK_ACTIVE = 0.3f;
            ATTACK_COOLDOWN = 0.3f;
        }
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        isPlayer = EnMainInst.isPlayer;

        if (GetComponent<EnMove>().fastDropStoppedFrame) {
            FastDropAttack();
        } else if (EnMainInst.inputs.attackMelee && attackCooldown == 0.0f && GetComponent<EnMove>().isNormalState) {
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
