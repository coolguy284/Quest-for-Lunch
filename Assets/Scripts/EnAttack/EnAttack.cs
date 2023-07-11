using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnAttack : MonoBehaviour {
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    EnMain EnMainInst;
    EnMove EnMoveInst;
    public TextMeshProUGUI DebugText3;
    bool isPlayer = false;
    
    [HideInInspector]
    public float STUN_CHANCE = 0.1f; // default is stun chance for enemies
    static float PLAYER_STUN_CHANCE = 0.5f;
    //float TELEPORT_ATTACK_MAX = 10.0f;
    float MOMENTUM_HALT_TIME = 1.0f;
    
    [HideInInspector]
    public float attackCooldown = 0.0f;
    //float teleportAttack = 0.0f;
    //int attackCombo = 0;
    bool givenMomentumHalt = false; // for the one guaranteed momentum halt
    float extraMomentumHalt = 0.0f; // extra momentum halt time
    bool notPerformingAttack = true;
    
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
            entity.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction.x > 0.0f ? 1.0f : -1.0f, 0.5f) * attackStats.knockback, ForceMode2D.Impulse);
            entity.GetComponent<EnAttack>().attackCooldown = 0.0f;
            if (entity.GetComponent<EnAttack>().STUN_CHANCE + attackStats.stunChance > Random.Range(0.0f, 1.0f))
                entity.GetComponent<EnMove>().inputLagTime = attackStats.hitstun;
            
            // grant suscoins
            if (isPlayer && !entity.GetComponent<EnHealth>().alive) {
                EnMainInst.susCoins += (int)(entity.GetComponent<EnHealth>().maxHealth * 1.25f);
                EnMainInst.SusCoinsText.text = EnMainInst.susCoins.ToString();
            }
        }
    }
    
    void AttackRanged(Level.TWeaponStats attackStats, EnItem.Slot weaponSlot, int weaponSlotId) {
        var projectile = Instantiate(EnMainInst.ProjectileDict[attackStats.fires], Vector3.zero, Quaternion.identity);
        projectile.transform.parent = EnMainInst.ProjectilesList.transform;
        projectile.transform.localPosition = transform.position;
        projectile.GetComponent<Rigidbody2D>().AddForce(new Vector2(EnMoveInst.facingRight ? attackStats.damage : -attackStats.damage, 0.0f), ForceMode2D.Impulse);
        projectile.transform.rotation = Quaternion.Euler(0.0f, 0.0f, EnMoveInst.facingRight ? -45.0f : 135.0f);
        projectile.GetComponent<SpriteRenderer>().enabled = false;
        var projectileDamage = projectile.GetComponent<ProjectileDamage>();
        projectileDamage.EnMainInst = EnMainInst;
        projectileDamage.EnItemInst = GetComponent<EnItem>();
        projectileDamage.EnAttackInst = this;
        projectileDamage.isPlayer = isPlayer;
        projectileDamage.attackStats = EnMainInst.WeaponStats[attackStats.fires];
        projectileDamage.slotId = weaponSlotId;
        projectileDamage.slot = weaponSlot;
    }
    
    IEnumerator PerformAttack(int attackSlot) {
        // get stats
        notPerformingAttack = false;
        Level.TWeaponStats attackStats;
        EnItem.Slot weaponSlot;
        if (attackSlot == -1) {
            // fastdrop attack
            weaponSlot = new EnItem.Slot();
            attackStats = EnMainInst.WeaponStats["Fastdrop"];
        } else {
            // other attacks
            if (isPlayer) {
                weaponSlot = GetComponent<EnItem>().Slots[attackSlot];
                attackStats = EnMainInst.WeaponStats[weaponSlot.name == "" ? "Player_Basic" : weaponSlot.name];
            } else {
                weaponSlot = new EnItem.Slot();
                attackStats = EnMainInst.WeaponStats["Enemy_Basic1"];
            }
        }
        
        // check whether arrows available
        if (attackStats.type == "ranged") {
            if ((int)weaponSlot.extra[0] >= attackStats.extraInt[0]) {
                notPerformingAttack = true;
                yield break;
            }
        }
        
        // set sprite texture (quick method, will be improved eventually)
        if (attackStats.type == "normal" && EnMainInst.SpriteDict.ContainsKey(attackStats.name)) {
            var imageObject = transform.Find("HitBox/Image");
            if (imageObject != null) imageObject.GetComponent<SpriteRenderer>().sprite = EnMainInst.SpriteDict[attackStats.name];
        }
        
        // startup lag
        EnMoveInst.StartAttack(attackStats.name);
        attackCooldown = attackStats.startup + 0.1f;
        EnMainInst.haltMotion = true;
        if (attackSlot == -1 && Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
            EnMoveInst.StartHaltState();
        float startupTime = 0.0f;
        while (startupTime < attackStats.startup) {
            if (EnMoveInst.inputLagTime > 0.0f || EnMoveInst.platformPullUp) {
                EnMoveInst.StopAttack();
                notPerformingAttack = true;
                yield break;
            }
            startupTime += Time.deltaTime;
            yield return null;
        }
        
        // perform attack
        attackCooldown = attackStats.active + 0.1f;
        switch (attackStats.type) {
            case "normal":
                float activeTime = 0.0f;
                while (activeTime < attackStats.active && !EnMoveInst.platformPullUp) {
                    AttackHitbox(attackStats);
                    activeTime += Time.deltaTime;
                    yield return null;
                }
                if (EnMoveInst.platformPullUp) {
                    EnMoveInst.StopAttack();
                    notPerformingAttack = true;
                    yield break;
                }
                break;
            
            case "ranged":
                AttackRanged(attackStats, weaponSlot, attackSlot);
                break;
        }
        EnMoveInst.StopAttack();
        attackCooldown = attackStats.cooldown;
        
        // decrease arrow count
        if (attackStats.type == "ranged") {
            if ((int)weaponSlot.extra[0] < attackStats.extraInt[0]) {
                weaponSlot.extra[0] = (int)weaponSlot.extra[0] + 1;
                GetComponent<EnItem>().DisplaySingleSlot(attackSlot);
            }
        }
        
        // halt momentum
        if (!givenMomentumHalt) {
            givenMomentumHalt = true;
            if (EnMoveInst.inAirTime < MOMENTUM_HALT_TIME) {
                extraMomentumHalt = EnMoveInst.inAirTime;
            }
        }
        
        // unset sprite texture
        if (attackStats.type == "normal") {
            var imageObject = transform.Find("HitBox/Image");
            if (imageObject != null) imageObject.GetComponent<SpriteRenderer>().sprite = null;
        }
        
        notPerformingAttack = true;
    }
    
    void Start() {
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        EnMainInst = GetComponent<EnMain>();
        EnMoveInst = GetComponent<EnMove>();
        isPlayer = EnMainInst.isPlayer;
        if (isPlayer) {
            STUN_CHANCE = PLAYER_STUN_CHANCE;
        }
    }
    
    void Update() {
        if (Time.timeScale == 0.0f) return;
        
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        if (EnMoveInst == null) EnMoveInst = GetComponent<EnMove>();
        isPlayer = EnMainInst.isPlayer;
        
        if (notPerformingAttack) {
            // begin performing attacks
            if (EnMoveInst.fastDropStoppedFrame) {
                StartCoroutine(PerformAttack(-1));
            } else if (EnMainInst.inputs.attack1 && attackCooldown == 0.0f && EnMoveInst.isNormalState) {
                StartCoroutine(PerformAttack(0));
            } else if (EnMainInst.inputs.attack2 && attackCooldown == 0.0f && EnMoveInst.isNormalState) {
                StartCoroutine(PerformAttack(1));
            }
        }
        
        // major drag when haltmotion is active
        if (EnMainInst.haltMotion) {
            Self_RigidBody.AddForce(new Vector2(Self_RigidBody.velocity.x * -10f, Self_RigidBody.velocity.y * -10f), ForceMode2D.Force);
        }
        
        // unhalt motion
        if ((attackCooldown == 0.0f || givenMomentumHalt && EnMoveInst.inAirTime > MOMENTUM_HALT_TIME + extraMomentumHalt || EnMoveInst.platformPullUp) && EnMainInst.haltMotion) {
            EnMainInst.haltMotion = false;
            if (Self_RigidBody.bodyType == RigidbodyType2D.Static)
                EnMoveInst.StopHaltState();
        }
        
        // refresh momentumhalts if on ground
        if (EnMoveInst.isGrounded) {
            givenMomentumHalt = false;
            extraMomentumHalt = 0.0f;
        }
        
        // reduce attack cooldown by time passed
        if (attackCooldown > 0.0f) {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown < 0.0f) attackCooldown = 0.0f;
        }
        
        // debug text
        if (isPlayer) DebugText3.text = string.Format("GivenHalt: {0}\nExtraHalt: {1:0.000}\nAttackCooldown: {2:0.000}\nHaltMotion: {3}", givenMomentumHalt, extraMomentumHalt, attackCooldown, EnMainInst.haltMotion);
    }
}
