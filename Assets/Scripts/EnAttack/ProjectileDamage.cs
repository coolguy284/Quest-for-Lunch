using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour {
    public EnMain EnMainInst;
    public EnItem EnItemInst;
    public EnAttack EnAttackInst;
    public bool isPlayer;
    public Level.TWeaponStats attackStats;
    public int slotId;
    public EnItem.Slot slot;
    float existedTime;
    bool destroyed = false;

    void DestroyProjectile() {
        if (destroyed) return;
        Destroy(this.gameObject);
        slot.extra[0] = (int)slot.extra[0] - 1;
        EnItemInst.DisplaySingleSlot(slotId);
        destroyed = true;
    }

    void AttackHitbox() {
        var colliders = GetComponent<ColliderTracker>().colliders;

        // for each thing hitbox hit
        foreach (var entity in new List<Collider2D>(colliders)) {
            var relVel = entity.GetComponent<Rigidbody2D>().velocity - GetComponent<Rigidbody2D>().velocity;

            // ignore collision if too slow
            if (relVel.magnitude < 10.0f) continue;

            var direction = entity.transform.position - transform.position;
            
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

            // destroy arrow
            DestroyProjectile();
        }
    }

    void Update() {
        AttackHitbox();

        existedTime += Time.deltaTime;
        if (existedTime > 5.0f) {
            DestroyProjectile();
        }
    }
}
