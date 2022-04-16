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

    //float TELEPORT_ATTACK_MAX = 10.0f;
    float MOMENTUM_HALT_TIME = 1.0f;
    
    float attackCooldown = 0.0f;
    //float teleportAttack = 0.0f;
    //int attackCombo = 0;
    bool givenMomentumHalt = false; // for the one guaranteed momentum halt
    float extraMomentumHalt = 0.0f; // extra momentum halt time

    void AttackHitbox(Level.TWeaponStats attackStats) {
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
            if (entity.GetComponent<EnHealth>() == null || entity.GetComponent<EnHealth>().invulnTime != 0.0f) continue;

            // perform damage
            entity.GetComponent<EnHealth>().changeHealth(-attackStats.damage);
            entity.GetComponent<EnHealth>().hitInvulnTime = attackStats.invuln;

            // perform knockback
            entity.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction.x > 0.0f ? 1.0f : -1.0f, 2.0f) * attackStats.knockback, ForceMode2D.Impulse);
            entity.GetComponent<EnAttack>().attackCooldown = 0.0f;
            entity.GetComponent<EnMove>().inputLagTime = attackStats.hitstun;

            // grant suscoins
            if (isPlayer && !entity.GetComponent<EnHealth>().alive) {
                EnMainInst.susCoins += (int)(entity.GetComponent<EnHealth>().maxHealth * 1.25f);
                EnMainInst.SusCoinsText.text = EnMainInst.susCoins.ToString();
            }
        }
    }

    void AttackRanged(Level.TWeaponStats attackStats) {
        Instantiate(EnMainInst.ProjectileDict[attackStats.fires], transform.position, Quaternion.identity);
    }

    IEnumerator PerformAttack(int attackSlot) {
        // get stats
        Level.TWeaponStats attackStats;
        if (attackSlot == -1) {
            // fastdrop attack
            attackStats = EnMainInst.WeaponStats["Fastdrop"];
        } else {
            // other attacks
            if (isPlayer) {
                var weaponSlot = GetComponent<EnItem>().Slots[attackSlot];
                attackStats = EnMainInst.WeaponStats[weaponSlot == "" ? "Player_Basic" : weaponSlot];
            } else {
                attackStats = EnMainInst.WeaponStats["Enemy_Basic1"];
            }
        }

        // startup lag
        GetComponent<EnMove>().StartAttack(attackStats.name);
        float startupTime = 0.0f;
        while (startupTime < attackStats.startup) {
            if (GetComponent<EnMove>().inputLagTime > 0.0f) {
                GetComponent<EnMove>().StopAttack();
                yield break;
            }
            startupTime += Time.deltaTime;
            yield return null;
        }

        // perform attack
        attackCooldown = attackStats.active;
        switch (attackStats.type) {
            case "normal":
                float activeTime = 0.0f;
                while (activeTime < attackStats.active) {
                    AttackHitbox(attackStats);
                    activeTime += Time.deltaTime;
                    yield return null;
                }
                break;
            
            case "ranged":
                AttackRanged(attackStats);
                break;
        }
        GetComponent<EnMove>().StopAttack();
        attackCooldown = attackStats.cooldown;

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
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        isPlayer = EnMainInst.isPlayer;

        if (GetComponent<EnMove>().fastDropStoppedFrame) {
            StartCoroutine(PerformAttack(-1));
        } else if (EnMainInst.inputs.attack1 && attackCooldown == 0.0f && GetComponent<EnMove>().isNormalState) {
            StartCoroutine(PerformAttack(0));
        } else if (EnMainInst.inputs.attack2 && attackCooldown == 0.0f && GetComponent<EnMove>().isNormalState) {
            StartCoroutine(PerformAttack(1));
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
