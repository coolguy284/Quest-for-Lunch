using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMovement : MonoBehaviour {
    public float MOVEMENT_FORCE = 0.5f;
    public float MOVEMENT_SPEED = 6.0f;

    public float JUMP_FORCE = 7.0f;
    public float AIRJUMP_FORCE = 8.0f;
    public float WALLJUMP_FORCE = 10.0f;
    public float LONGJUMP_GRAV_MULT = 0.5f;
    
    public float WALL_JUMP_LAG = 0.25f;

    public int JUMPS_FROM_GND = 1;
    public int JUMPS_FROM_WALL = 1;

    public float FASTDROP_FORCE = 2.0f;

    public float WALL_DROP_SPEED = 1.0f;

    public BoxCollider2D Player_BoxCollider;
    public Rigidbody2D Player_RigidBody;
    public TextMeshProUGUI debugText;

    int jumps = 0; // number of jumps available
    bool jumpButton = false; // whether jump button is being held down
    bool lockGround = false; // if true ground cannot refresh jump counter
    bool lockWall = false; // if true wall cannot refresh jump counter
    float lagTime = 0.0f; // amount of time that inputs will be ignored due to lag
    bool inLag = false;
    bool isGrounded = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool isWalled = false;
    bool isHoldingWall = false;

    bool isOnGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y - Player_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Player_BoxCollider.bounds.size.x);
        var isGrounded = groundRaycast.collider != null;
        return isGrounded;
    }

    bool isOnWallLeft() {
        var leftWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x - 0.02f, transform.position.y - Player_BoxCollider.bounds.extents.y), Vector2.up, Player_BoxCollider.bounds.size.y);
        return leftWallRaycast.collider != null;
    }

    bool isOnWallRight() {
        var rightWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x + Player_BoxCollider.bounds.extents.x + 0.02f, transform.position.y - Player_BoxCollider.bounds.extents.y), Vector2.up, Player_BoxCollider.bounds.size.y);
        return rightWallRaycast.collider != null;
    }

    bool isOnWall() {
        var isWalled = isOnWallLeft() || isOnWallRight();
        return isWalled;
    }

    void Jump() {
        Player_RigidBody.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
    }

    void AirJump() {
        Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 0.0f);
        Player_RigidBody.AddForce(new Vector2(0, AIRJUMP_FORCE), ForceMode2D.Impulse);
    }

    void WallJump(bool left) {
        Player_RigidBody.velocity = new Vector2(0.0f, 0.0f);
        Player_RigidBody.AddForce(new Vector2((left ? -1.0f : 1.0f) * WALLJUMP_FORCE * 0.2f, WALLJUMP_FORCE), ForceMode2D.Impulse);
        lagTime = WALL_JUMP_LAG;
        isHoldingWall = false;
    }

    void Start() {
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        // get onground, onwall, inlag status
        isGrounded = isOnGround();
        isWalledLeft = isOnWallLeft();
        isWalledRight = isOnWallRight();
        isWalled = isWalledLeft || isWalledRight;
        inLag = lagTime > 0.0f;

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        // horizontal movement
        var movementHorizontal = Input.GetAxisRaw("Horizontal");

        isHoldingWall = movementHorizontal > 0 && isWalledRight || movementHorizontal < 0 && isWalledLeft;

        if (!inLag) {
            if (isHoldingWall && false) {
                Player_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Player_RigidBody.velocity.y, -WALL_DROP_SPEED));
            } else {
                if (Player_RigidBody.velocity.x * movementHorizontal > 0) {
                    Player_RigidBody.AddForce(new Vector2(movementHorizontal * MOVEMENT_FORCE, 0.0f) * Mathf.Max(1.0f - Mathf.Pow(Player_RigidBody.velocity.x / MOVEMENT_SPEED, 4.0f), 0.0f), ForceMode2D.Impulse);
                } else {
                    Player_RigidBody.AddForce(new Vector2(movementHorizontal * MOVEMENT_FORCE, 0.0f), ForceMode2D.Impulse);
                }
                //Player_RigidBody.velocity = new Vector2(movementHorizontal * MOVEMENT_SPEED, Player_RigidBody.velocity.y);
            }
        }

        // fastdropping
        var movementVertical = Input.GetAxisRaw("Vertical");
        if (movementVertical < 0 && !inLag) {
            Player_RigidBody.AddForce(new Vector2(0, -FASTDROP_FORCE), ForceMode2D.Impulse);
        }

        // refreshes jump counter if jump counter not locked, and locks jump counter
        if (isGrounded) {
            if (jumps < JUMPS_FROM_GND && !lockGround) {
                jumps = JUMPS_FROM_GND;
                lockGround = true;
            }
        } else if (isHoldingWall && !lockWall) {
            if (jumps < JUMPS_FROM_WALL) {
                jumps = JUMPS_FROM_WALL;
                lockWall = true;
            }
        }

        // jumping
        if (Input.GetButtonDown("Jump") && !inLag) {
            if (isGrounded) {
                Jump();
                jumpButton = true;
            } else {
                if (isHoldingWall) {
                    WallJump(isWalledRight);
                } else if (jumps > 0) {
                    AirJump();
                    jumps--;
                }
            }
            if (isGrounded) lockGround = true;
            if (isHoldingWall) lockWall = true;
        }

        // while jump button is pressed, jumping is true, when jumping is true gravity is reduced
        if (jumpButton && !inLag) {
            Player_RigidBody.AddForce(new Vector2(0.0f, Player_RigidBody.mass * (1.0f - LONGJUMP_GRAV_MULT) * 1.5f * 0.04f), ForceMode2D.Impulse);
            if (Player_RigidBody.velocity.y < 0) {
                jumpButton = false;
            }
        }

        // make jumping false once jump button is released
        if ((Input.GetButtonUp("Jump") || inLag) && jumpButton) {
            jumpButton = false;
        }

        // reduce lag by lag time
        if (inLag) {
            lagTime -= Time.deltaTime;
            if (lagTime < 0.0f) lagTime = 0.0f;
        }

        // debug jumping text
        debugText.text = string.Format("Debug:\nIsGrounded: {0}\nIsHoldingWall: {1}\nJumps: {2}\nInLag: {3}", isGrounded, isHoldingWall, jumps, inLag);

        // press esc to return to title
        if (Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
