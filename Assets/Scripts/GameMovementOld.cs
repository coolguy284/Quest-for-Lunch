using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMovementOld : MonoBehaviour {
    public float MOVEMENT_SPEED = 4.0f;

    public float SHORTJUMP_FORCE = 4.0f;
    public float JUMP_FORCE = 10.0f;
    public float SHORTJUMP_TIME = 0.116f;
    
    public float WALL_JUMP_LAG = 0.25f;

    public int JUMPS_FROM_GND = 1;
    public int JUMPS_FROM_WALL = 1;

    public float FASTDROP_FORCE = 2.0f;

    public float WALL_DROP_SPEED = 1.0f;

    public Rigidbody2D Player_RigidBody;
    public TextMeshProUGUI debugText;

    int jumps = 0; // number of jumps available
    bool jumpButton = false; // whether jump button is being held down
    bool lockGround = false; // if true ground cannot refresh jump counter
    bool lockWall = false; // if true wall cannot refresh jump counter
    float jumpButtonTime = 0.0f; // how long jump button has been held down
    float lagTime = 0.0f; // amount of time that inputs will be ignored due to lag

    bool isOnGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - 0.30f, transform.position.y - 0.34f), Vector2.right, 0.60f);
        var isGrounded = groundRaycast.collider != null;
        return isGrounded;
    }

    bool isOnWallLeft() {
        var leftWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x - 0.34f, transform.position.y - 0.30f), Vector2.up, 0.60f);
        return leftWallRaycast.collider != null;
    }

    bool isOnWallRight() {
        var rightWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x + 0.34f, transform.position.y - 0.30f), Vector2.up, 0.60f);
        return rightWallRaycast.collider != null;
    }

    bool isOnWall() {
        var isWalled = isOnWallLeft() || isOnWallRight();
        return isWalled;
    }

    void ShortJump() {
        Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 0.0f);
        Player_RigidBody.AddForce(new Vector2(0, SHORTJUMP_FORCE), ForceMode2D.Impulse);
    }

    void Jump() {
        if (jumpButton) {
            Player_RigidBody.AddForce(new Vector2(0, JUMP_FORCE - SHORTJUMP_FORCE), ForceMode2D.Impulse);
        } else {
            Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 0.0f);
            Player_RigidBody.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
        }
    }

    void WallJump(bool left) {
        Player_RigidBody.velocity = new Vector2(0.0f, 0.0f);
        Player_RigidBody.AddForce(new Vector2((left ? -1.0f : 1.0f) * JUMP_FORCE * 0.5f, JUMP_FORCE * 0.5f), ForceMode2D.Impulse);
        lagTime = WALL_JUMP_LAG;
    }

    void Start() {
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        // get onground, onwall, inlag status
        bool isGrounded = isOnGround();
        bool isWalledLeft = isOnWallLeft();
        bool isWalledRight = isOnWallRight();
        bool isWalled = isWalledLeft || isWalledRight;
        bool inLag = lagTime > 0.0f;

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        // horizontal movement
        var movementHorizontal = Input.GetAxisRaw("Horizontal");
        if (!inLag) {
            if (movementHorizontal > 0 && isWalledRight || movementHorizontal < 0 && isWalledLeft) {
                Player_RigidBody.velocity = new Vector2(0.0f, -WALL_DROP_SPEED);
            } else {
                Player_RigidBody.velocity = new Vector2(movementHorizontal * MOVEMENT_SPEED, Player_RigidBody.velocity.y);
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
        } else if (isWalled) {
            if (jumps < JUMPS_FROM_WALL && !lockWall) {
                jumps = JUMPS_FROM_WALL;
                lockWall = true;
            }
        }

        // jumping
        if (Input.GetButtonDown("Jump") && !inLag && jumps > 0) {
            if (isGrounded) {
                ShortJump();
                jumpButton = true;
                jumpButtonTime = 0;
            } else {
                if (isWalled) {
                    WallJump(isWalledRight);
                } else {
                    Jump();
                }
            }
            if (!isGrounded && !isWalled) jumps--;
            if (isGrounded) lockGround = true;
            if (isWalled) lockWall = true;
        }

        // while jump button is pressed, jumping is true, if it is true for long enough a shorthop becomes a jump
        if (jumpButton && !inLag) {
            jumpButtonTime += Time.deltaTime;
            if (jumpButtonTime > SHORTJUMP_TIME) {
                Jump();
                jumpButton = false;
            }
        }

        // make jumping false once jump button is released, so that the full jump in the previous code block is cancelled
        if ((Input.GetButtonUp("Jump") || inLag) && jumpButton) {
            jumpButton = false;
        }

        // reduce lag by lag time
        if (inLag) {
            lagTime -= Time.deltaTime;
            if (lagTime < 0.0f) lagTime = 0.0f;
        }

        // debug jumping text
        debugText.text = string.Format("Debug:\nIsGrounded: {0}\nIsWalled: {1}\nJumps: {2}\nInLag: {3}", isGrounded, isWalled, jumps, inLag);

        // press esc to return to title
        if (Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
