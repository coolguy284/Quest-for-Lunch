using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMovement : MonoBehaviour {
    public float MOVEMENT_FORCE = 0.2f;
    public float MOVEMENT_SPEED = 5.0f;

    public float JUMP_FORCE = 7.5f;
    public float AIRJUMP_FORCE = 5.5f;
    public float WALLJUMP_FORCE = 5.0f;
    public float LONGJUMP_GRAV_MULT = 1f;

    public int JUMPS_FROM_GND = 1;
    public int JUMPS_FROM_WALL = 1;

    public float FASTDROP_FORCE = 0.25f;

    public float WALL_CLIMB_THRESHOLD = -0.3f;

    public float WALL_SLIDE_TIMER = 3.0f;
    public float WALL_DROP_TIMER = 4.0f;
    public float WALL_DROP_SPEED = 1.0f;
    public float PLATFORM_PULLUP_SPEED = 2.0f;
    
    public float WALL_DROPOFF_LAG = 0.5f;
    public float WALL_JUMP_LAG = 0.25f;
    public float PLATFORM_FALL_LAG = 0.5f;

    public GameObject Player;
    BoxCollider2D Player_BoxCollider;
    Rigidbody2D Player_RigidBody;
    public TextMeshProUGUI debugText;

    int jumps = 0; // number of jumps available
    bool lockGround = false; // if true ground cannot refresh jump counter
    bool lockWall = false; // if true wall cannot refresh jump counter
    bool isWallCling = false;
    bool isFastDropping = false;
    float wallClingLagTime = 0.0f; // amount of time that inputs will be ignored due to lag
    bool inWallClingLag = false;
    float wallClingTimer = 0.0f;
    bool ignorePlatform = false;
    bool queuedPlatPullVelReset = false;

    int layerCollisionMask;
    bool isGrounded = false;
    bool isTrueGrounded = false;
    bool isPlatform = false;
    bool isLooseInPlatform = false;
    bool isInPlatform = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool isWalled = false;
    bool isHoldingWall = false;
    bool jumpButton = false; // whether jump button is being held down

    int getLayerCollisionMask() {
        int collisionMask = 0;
        for (int i = 0; i < 32; i++) {
            if (!Physics.GetIgnoreLayerCollision(Player.layer, i)) {
                collisionMask |= 1 << i;
            }
        }
        return collisionMask;
    }

    bool isOnGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y - Player_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Player_BoxCollider.bounds.size.x, layerCollisionMask);
        return groundRaycast.collider != null;
    }

    bool isOnTrueGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y - Player_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Player_BoxCollider.bounds.size.x, LayerMask.GetMask("Default"));
        return groundRaycast.collider != null;
    }

    bool isOnPlatform() {
        var platformRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y - Player_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Player_BoxCollider.bounds.size.x, LayerMask.GetMask("Platform"));
        return platformRaycast.collider != null;
    }

    bool isLooseInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y + Player_BoxCollider.bounds.extents.y + 0.02f), Vector2.down, Player_BoxCollider.bounds.size.y + 0.04f, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + Player_BoxCollider.bounds.extents.x, transform.position.y + Player_BoxCollider.bounds.extents.y + 0.02f), Vector2.down, Player_BoxCollider.bounds.size.y + 0.04f, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x, transform.position.y + Player_BoxCollider.bounds.extents.y), Vector2.down, Player_BoxCollider.bounds.size.y, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + Player_BoxCollider.bounds.extents.x, transform.position.y + Player_BoxCollider.bounds.extents.y), Vector2.down, Player_BoxCollider.bounds.size.y, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isOnWallLeft() {
        var leftWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Player_BoxCollider.bounds.extents.x - 0.02f, transform.position.y - Player_BoxCollider.bounds.extents.y), Vector2.up, Player_BoxCollider.bounds.size.y, LayerMask.GetMask("Default"));
        return leftWallRaycast.collider != null;
    }

    bool isOnWallRight() {
        var rightWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x + Player_BoxCollider.bounds.extents.x + 0.02f, transform.position.y - Player_BoxCollider.bounds.extents.y), Vector2.up, Player_BoxCollider.bounds.size.y, LayerMask.GetMask("Default"));
        return rightWallRaycast.collider != null;
    }

    void Jump() {
        Player_RigidBody.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
    }

    void AirJump() {
        Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 0.0f);
        Player_RigidBody.AddForce(new Vector2(0, AIRJUMP_FORCE), ForceMode2D.Impulse);
    }

    void WallJump(bool left, float strength) {
        Player_RigidBody.velocity = new Vector2(0.0f, 0.0f);
        Player_RigidBody.AddForce(new Vector2((left ? -1.0f : 1.0f) * WALLJUMP_FORCE, WALLJUMP_FORCE) * strength, ForceMode2D.Impulse);
        wallClingLagTime = WALL_JUMP_LAG;
        isHoldingWall = false;
    }

    void StartWallCling(bool climb) {
        isWallCling = true;

        if (climb) {
            Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 10.0f);
        } else {
            Player_RigidBody.velocity = new Vector2(Player_RigidBody.velocity.x, 0.0f);
        }
    }

    void StopWallCling(float lagTime) {
        isWallCling = false;
        wallClingTimer = 0.0f;
        wallClingLagTime = lagTime;
    }

    void StartIgnorePlatform() {
        if (!ignorePlatform) {
            ignorePlatform = true;
            Player.layer = LayerMask.NameToLayer("Ignore Platform");
            layerCollisionMask = getLayerCollisionMask();
        }
    }

    void StopIgnorePlatform() {
        if (ignorePlatform) {
            ignorePlatform = false;
            Player.layer = LayerMask.NameToLayer("Default");
            layerCollisionMask = getLayerCollisionMask();
        }
    }

    void StartFastDrop() {
        isFastDropping = true;
        Player_RigidBody.velocity = new Vector2(0.0f, Player_RigidBody.velocity.y);
    }
    
    void StopFastDrop() {
        isFastDropping = false;
    }

    void Start() {
        Player_BoxCollider = Player.GetComponent<BoxCollider2D>();
        Player_RigidBody = Player.GetComponent<Rigidbody2D>();
        layerCollisionMask = getLayerCollisionMask();
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        // get onground, onwall, inwallclinglag status
        isGrounded = isOnGround();
        isTrueGrounded = isOnTrueGround();
        isPlatform = isOnPlatform();
        isLooseInPlatform = isLooseInsidePlatform();
        isInPlatform = isInsidePlatform();
        isWalledLeft = isOnWallLeft();
        isWalledRight = isOnWallRight();
        isWalled = isWalledLeft || isWalledRight;
        inWallClingLag = wallClingLagTime > 0.0f;

        // get movement
        var movementHorizontal = Input.GetAxisRaw("Horizontal");
        var movementVertical = Input.GetAxisRaw("Vertical");

        isHoldingWall = movementHorizontal > 0 && isWalledRight || movementHorizontal < 0 && isWalledLeft;

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        if (!isWallCling && !isFastDropping) {
            // normal state

            // pull up through platform
            if (isInPlatform && !inWallClingLag && !ignorePlatform) {
                Player_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Player_RigidBody.velocity.y, PLATFORM_PULLUP_SPEED));
                queuedPlatPullVelReset = true;
            } else if (queuedPlatPullVelReset) {
                Player_RigidBody.velocity = new Vector2(0.0f, 0.0f);
                queuedPlatPullVelReset = false;
            }

            // stop ignoring platforms after falling through one enough
            if (ignorePlatform && !isLooseInPlatform) {
                StopIgnorePlatform();
            }

            // horizontal movement
            if (!isInPlatform) {
                if (Player_RigidBody.velocity.x * movementHorizontal > 0) {
                    Player_RigidBody.AddForce(new Vector2(movementHorizontal * MOVEMENT_FORCE, 0.0f) * Mathf.Max(1.0f - Mathf.Pow(Player_RigidBody.velocity.x / MOVEMENT_SPEED, 4.0f), 0.0f), ForceMode2D.Impulse);
                } else {
                    Player_RigidBody.AddForce(new Vector2(movementHorizontal * MOVEMENT_FORCE, 0.0f), ForceMode2D.Impulse);
                }
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

            // establish wall cling
            if (!inWallClingLag) {
                if (isHoldingWall && !isGrounded && !isInPlatform) {
                    if (Player_RigidBody.velocity.y < WALL_CLIMB_THRESHOLD) {
                        // simple cling
                        StartWallCling(false);
                    } else {
                        // climb up
                        StartWallCling(true);
                    }
                }
            }

            // jumping
            if (Input.GetButtonDown("Jump")) {
                if (isGrounded) {
                    if (movementVertical < 0.0f) {
                        // drop through platform
                        if (isPlatform && !isTrueGrounded) {
                            StartIgnorePlatform();
                            wallClingLagTime = PLATFORM_FALL_LAG;
                        }
                    } else {
                        Jump();
                        jumpButton = true;
                    }
                } else {
                    if (movementVertical < 0.0f) {
                        // fastdrop
                        StartFastDrop();
                    } else if (jumps > 0) {
                        // air jump
                        AirJump();
                        jumps--;
                    }
                }
                if (isGrounded) lockGround = true;
                if (isHoldingWall) lockWall = true;
            }

            // while jump button is pressed, jumping is true, when jumping is true gravity is reduced
            if (jumpButton) {
                Player_RigidBody.AddForce(new Vector2(0.0f, Player_RigidBody.mass * (1.0f - LONGJUMP_GRAV_MULT) * 1.5f * 0.04f), ForceMode2D.Impulse);
                if (Player_RigidBody.velocity.y < 0) {
                    jumpButton = false;
                }
            }

            // make jumping false once jump button is released
            if (Input.GetButtonUp("Jump") && jumpButton) {
                jumpButton = false;
            }
        } else if (isWallCling) {
            // wall cling

            if (wallClingTimer < WALL_SLIDE_TIMER) {
                Player_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Player_RigidBody.velocity.y, 0.3f));
            } else if (wallClingTimer < WALL_DROP_TIMER) {
                Player_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Player_RigidBody.velocity.y, -WALL_DROP_SPEED));
            } else {
                StopWallCling(WALL_DROPOFF_LAG);
            }
            
            // jump off wall actions
            if (Input.GetButtonDown("Jump")) {
                if (movementVertical < 0.0f) {
                    // fast drop
                    StopWallCling(0.0f);
                    StartFastDrop();
                } else if (movementHorizontal < 0.0f && isWalledRight || movementHorizontal > 0.0f && isWalledLeft) {
                    // jump away
                    StopWallCling(WALL_JUMP_LAG);
                    WallJump(movementHorizontal < 0.0f, 2.0f);
                } else if (movementHorizontal > 0.0f && isWalledRight || movementHorizontal < 0.0f && isWalledLeft) {
                    // jump towards
                    StopWallCling(WALL_JUMP_LAG);
                    WallJump(movementHorizontal > 0.0f, 1.0f);
                } else {
                    // jump neutral
                    StopWallCling(WALL_JUMP_LAG);
                    WallJump(isWalledRight, 1.0f);
                }
            }

            // disable wall cling if grounded
            if ((isGrounded || !isWalled) && isWallCling) {
                StopWallCling(0.0f);
            }

            wallClingTimer += Time.deltaTime;
        } else if (isFastDropping) {
            // fast dropping
            Player_RigidBody.AddForce(new Vector2(0, -FASTDROP_FORCE), ForceMode2D.Impulse);

            // disable fast drop if grounded
            if (isGrounded || isPlatform) {
                StopFastDrop();
            }
        }

        // reduce lag by lag time
        if (inWallClingLag) {
            wallClingLagTime -= Time.deltaTime;
            if (wallClingLagTime < 0.0f) wallClingLagTime = 0.0f;
        }

        // debug jumping text
        debugText.text = string.Format("IsGrounded: {0}\nIsHoldingWall: {1}\nIsWallCling: {2}\nJumps: {3}\nWallClingLag: {4}\nignorePlatform: {5}", isGrounded, isHoldingWall, isWallCling, jumps, wallClingLagTime, ignorePlatform);

        // press esc to return to title
        if (Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
