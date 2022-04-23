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

    static float DESTROY_TIME_ENEMY = 1.5f;
    static float DESTORY_TIME_WALL = 0.7f;
    static float DESTORY_TIME_GROUND = 0.5f;

    float existedTime;
    float? destroyTime;
    bool destroyed = false;
    Collision2D collision;

    void DestroyProjectile(bool doDestroy = true) {
        if (destroyed) return;
        destroyed = true;
        if (doDestroy) Destroy(this.gameObject);
        slot.extra[0] = (int)slot.extra[0] - 1;
        EnItemInst.DisplaySingleSlot(slotId);
    }

    void OnDestroy() {
        if (destroyed) return;
        DestroyProjectile(false);
    }

    void OnCollisionEnter2D(Collision2D collisionObj) {
        // collision code

        // ignore if already collided
        if (collision != null) return;

        // set object
        collision = collisionObj;
        Destroy(GetComponent<PolygonCollider2D>());
        Destroy(GetComponent<Rigidbody2D>());
        transform.parent = collision.transform;

        var entity = collision.collider.gameObject;

        if (entity.layer == LayerMask.NameToLayer("Enemy") || entity.layer == LayerMask.NameToLayer("Player")) {
            // set destroy time
            destroyTime = existedTime + DESTROY_TIME_ENEMY;
            
            // only attack if vulnerable
            if (entity.GetComponent<EnHealth>() == null || entity.GetComponent<EnHealth>().invulnTime != 0.0f) return;

            // perform damage
            entity.GetComponent<EnHealth>().changeHealth(-attackStats.damage);
            entity.GetComponent<EnHealth>().hitInvulnTime = attackStats.invuln;

            // perform knockback
            var direction = entity.transform.position - transform.position;
            entity.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction.x > 0.0f ? 1.0f : -1.0f, 2.0f) * attackStats.knockback, ForceMode2D.Impulse);
            entity.GetComponent<EnAttack>().attackCooldown = 0.0f;
            if (entity.GetComponent<EnAttack>().STUN_CHANCE + attackStats.stunChance > Random.Range(0.0f, 1.0f))
                entity.GetComponent<EnMove>().inputLagTime = attackStats.hitstun;

            // grant suscoins
            if (isPlayer && !entity.GetComponent<EnHealth>().alive) {
                EnMainInst.susCoins += (int)(entity.GetComponent<EnHealth>().maxHealth * 1.25f);
                EnMainInst.SusCoinsText.text = EnMainInst.susCoins.ToString();
            }
        } else {
            var normal = collision.contacts[0].normal;
            if (Mathf.Abs(normal.x) < 0.5f && Mathf.Abs(normal.y) >= 0.5f) {
                // ground
                destroyTime = existedTime + DESTORY_TIME_GROUND;
            } else {
                // wall
                destroyTime = existedTime + DESTORY_TIME_WALL;
            }
        }
    }

    IEnumerator Start() {
        var velocity = GetComponent<Rigidbody2D>().velocity;
        yield return null;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    void Update() {
        existedTime += Time.deltaTime;

        // apply projectile gravity
        if (GetComponent<Rigidbody2D>() != null) {
            var rigidBody = GetComponent<Rigidbody2D>();
            rigidBody.AddForce(new Vector2(0.0f, -Mathf.Min(Mathf.Pow(existedTime * 18.0f, 4.0f), 100.0f)), ForceMode2D.Force);
            if (rigidBody.velocity.x != 0.0f && rigidBody.velocity.y != 0.0f)
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(rigidBody.velocity.y, rigidBody.velocity.x) * 180.0f / Mathf.PI - 45.0f);
        }

        // check for destroy condition
        if (destroyTime != null && existedTime > destroyTime ||
            collision != null && collision.collider != null && collision.collider.gameObject != null && collision.collider.gameObject.GetComponent<EnHealth>() != null && !collision.collider.gameObject.GetComponent<EnHealth>().alive) {
            DestroyProjectile();
        }
    }
}
