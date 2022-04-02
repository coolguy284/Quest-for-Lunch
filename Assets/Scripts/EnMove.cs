using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EnMove : MonoBehaviour {
    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    GameObject Player;
    public TextMeshProUGUI debugText;

    float MOVEMENT_FORCE = 0.3f;
    float MOVEMENT_SPEED = 5.0f;

    float JUMP_FORCE = 7.5f;
    float AIRJUMP_FORCE = 5.5f;
    float WALLJUMP_FORCE = 5.0f;
    float LONGJUMP_GRAV_MULT = 1f;

    int JUMPS_FROM_GND = 1;
    int JUMPS_FROM_WALL = 1;

    float FASTDROP_FORCE = 0.5f;

    float WALL_CLIMB_THRESHOLD = -0.3f;

    float WALL_SLIDE_TIMER = 3.0f;
    float WALL_DROP_TIMER = 4.0f;
    float WALL_DROP_SPEED = 1.0f;
    float PLATFORM_PULLUP_SPEED = 8.0f;

    float WALL_DROPOFF_LAG = 0.5f;
    float WALL_JUMP_LAG = 0.25f;
    float PLATFORM_FALL_LAG = 0.5f;

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
    bool isPlayer = false;
    bool isAlive = false;
    bool isGrounded = false;
    bool isTrueGrounded = false;
    bool isPlatform = false;
    bool isLooseInPlatform = false;
    bool isInPlatform = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool isWalled = false;
    bool isHoldingWall = false;

    public class Inputs {
        public float horizontal = 0.0f;
        public float vertical = 0.0f;
        public bool jump = false;
        public bool jumpRelease = false;
        bool _jumpDone = false;
        bool _jumpReleaseDone = false;
        public bool jumpHeld = false;

        public void Update() {
            if (jumpHeld) {
                _jumpReleaseDone = false;
                jumpRelease = false;
                if (!_jumpDone && !jump) {
                    jump = true;
                    _jumpDone = true;
                } else {
                    jump = false;
                }
            } else {
                _jumpDone = false;
                jump = false;
                if (!_jumpReleaseDone && !jumpRelease) {
                    jumpRelease = true;
                    _jumpReleaseDone = true;
                } else {
                    jumpRelease = false;
                }
            }
        }
    }
    
    Inputs inputs = new Inputs();

    bool jumpButton = false; // whether jump button is being held down

    int getLayerCollisionMask() {
        int collisionMask = 0;
        for (int i = 0; i < 32; i++) {
            if (!Physics.GetIgnoreLayerCollision(Self.layer, i)) {
                collisionMask |= 1 << i;
            }
        }
        return collisionMask;
    }

    bool isOnGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x, transform.position.y - Self_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Self_BoxCollider.bounds.size.x, layerCollisionMask & (~LayerMask.GetMask("Entity")));
        return groundRaycast.collider != null;
    }

    bool isOnTrueGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x, transform.position.y - Self_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Self_BoxCollider.bounds.size.x, LayerMask.GetMask("Default"));
        return groundRaycast.collider != null;
    }

    bool isOnPlatform() {
        var platformRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x, transform.position.y - Self_BoxCollider.bounds.extents.y - 0.02f), Vector2.right, Self_BoxCollider.bounds.size.x, LayerMask.GetMask("Platform"));
        return platformRaycast.collider != null;
    }

    bool isLooseInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x, transform.position.y + Self_BoxCollider.bounds.extents.y + 0.02f), Vector2.down, Self_BoxCollider.bounds.size.y + 0.04f, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x, transform.position.y + Self_BoxCollider.bounds.extents.y + 0.02f), Vector2.down, Self_BoxCollider.bounds.size.y + 0.04f, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x, transform.position.y + Self_BoxCollider.bounds.extents.y), Vector2.down, Self_BoxCollider.bounds.size.y, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x, transform.position.y + Self_BoxCollider.bounds.extents.y), Vector2.down, Self_BoxCollider.bounds.size.y, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isOnWallLeft() {
        var leftWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x - Self_BoxCollider.bounds.extents.x - 0.02f, transform.position.y - Self_BoxCollider.bounds.extents.y), Vector2.up, Self_BoxCollider.bounds.size.y, LayerMask.GetMask("Default"));
        return leftWallRaycast.collider != null;
    }

    bool isOnWallRight() {
        var rightWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x + Self_BoxCollider.bounds.extents.x + 0.02f, transform.position.y - Self_BoxCollider.bounds.extents.y), Vector2.up, Self_BoxCollider.bounds.size.y, LayerMask.GetMask("Default"));
        return rightWallRaycast.collider != null;
    }

    void Jump() {
        Self_RigidBody.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
    }

    void AirJump() {
        Self_RigidBody.velocity = new Vector2(Self_RigidBody.velocity.x, 0.0f);
        Self_RigidBody.AddForce(new Vector2(0, AIRJUMP_FORCE), ForceMode2D.Impulse);
    }

    void WallJump(bool left, float strength) {
        Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
        Self_RigidBody.AddForce(new Vector2((left ? -1.0f : 1.0f) * WALLJUMP_FORCE, WALLJUMP_FORCE) * strength, ForceMode2D.Impulse);
        wallClingLagTime = WALL_JUMP_LAG;
        isHoldingWall = false;
    }

    void StartWallCling(bool climb) {
        isWallCling = true;

        if (climb) {
            Self_RigidBody.velocity = new Vector2(Self_RigidBody.velocity.x, 10.0f);
        } else {
            Self_RigidBody.velocity = new Vector2(Self_RigidBody.velocity.x, 0.0f);
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
            Self.layer = LayerMask.NameToLayer("Ignore Platform");
            layerCollisionMask = getLayerCollisionMask();
        }
    }

    void StopIgnorePlatform() {
        if (ignorePlatform) {
            ignorePlatform = false;
            Self.layer = LayerMask.NameToLayer("Entity");
            layerCollisionMask = getLayerCollisionMask();
        }
    }

    void StartFastDrop() {
        isFastDropping = true;
        Self_RigidBody.velocity = new Vector2(0.0f, Self_RigidBody.velocity.y);
    }
    
    void StopFastDrop() {
        isFastDropping = false;
    }

    void updateInput() {
        if (isPlayer) {
            // take inputs from user
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.jumpHeld = Input.GetButton("Jump");
        } else {
            // calculate inputs of entity
            var relPlayerPos = Player.transform.position - Self.transform.position;
            if (Mathf.Abs(relPlayerPos.x) < 5.0f && Mathf.Abs(relPlayerPos.y) < 1.0f) {
                if (relPlayerPos.x > 1.0f) {
                    inputs.horizontal = 1.0f;
                } else if (relPlayerPos.x < -1.0f) {
                    inputs.horizontal = -1.0f;
                } else {
                    inputs.horizontal = 0.0f;
                }
            } else {
                inputs.horizontal = 0.0f;
            }
            inputs.vertical = 0.0f;
            inputs.jumpHeld = false;
        }
        inputs.Update();
    }

    void Start() {
        Self = this.gameObject;
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        Player = GetComponent<EnMain>().Player;
        layerCollisionMask = getLayerCollisionMask();
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        // update state variables
        isPlayer = GetComponent<EnMain>().isPlayer;
        isAlive = GetComponent<EnHealth>().alive;
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
        updateInput();

        isHoldingWall = inputs.horizontal > 0 && isWalledRight || inputs.horizontal < 0 && isWalledLeft;

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        if (isAlive) {
            if (!isWallCling && !isFastDropping) {
                // normal state

                // pull up through platform
                if (isInPlatform && !inWallClingLag && !ignorePlatform) {
                    Self_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Self_RigidBody.velocity.y, PLATFORM_PULLUP_SPEED));
                    queuedPlatPullVelReset = true;
                } else if (queuedPlatPullVelReset) {
                    Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
                    queuedPlatPullVelReset = false;
                }

                // stop ignoring platforms after falling through one enough
                if (ignorePlatform && !isLooseInPlatform) {
                    StopIgnorePlatform();
                }

                // horizontal movement
                if (!isInPlatform || inWallClingLag) {
                    if (Self_RigidBody.velocity.x * inputs.horizontal > 0) {
                        Self_RigidBody.AddForce(new Vector2(inputs.horizontal * MOVEMENT_FORCE, 0.0f) * Mathf.Max(1.0f - Mathf.Pow(Self_RigidBody.velocity.x / MOVEMENT_SPEED, 4.0f), 0.0f), ForceMode2D.Impulse);
                    } else {
                        Self_RigidBody.AddForce(new Vector2(inputs.horizontal * MOVEMENT_FORCE, 0.0f), ForceMode2D.Impulse);
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
                        if (Self_RigidBody.velocity.y < WALL_CLIMB_THRESHOLD) {
                            // simple cling
                            StartWallCling(false);
                        } else {
                            // climb up
                            StartWallCling(true);
                        }
                    }
                }

                // jumping
                if (inputs.jump) {
                    if (isGrounded) {
                        if (inputs.vertical < 0.0f) {
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
                        if (inputs.vertical < 0.0f) {
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
                    Self_RigidBody.AddForce(new Vector2(0.0f, Self_RigidBody.mass * (1.0f - LONGJUMP_GRAV_MULT) * 1.5f * 0.04f), ForceMode2D.Impulse);
                    if (Self_RigidBody.velocity.y < 0) {
                        jumpButton = false;
                    }
                }

                // make jumping false once jump button is released
                if (inputs.jumpRelease && jumpButton) {
                    jumpButton = false;
                }
            } else if (isWallCling) {
                // wall cling

                if (wallClingTimer < WALL_SLIDE_TIMER) {
                    Self_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Self_RigidBody.velocity.y, 0.3f));
                } else if (wallClingTimer < WALL_DROP_TIMER) {
                    Self_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Self_RigidBody.velocity.y, -WALL_DROP_SPEED));
                } else {
                    StopWallCling(WALL_DROPOFF_LAG);
                }
                
                // jump off wall actions
                if (inputs.jump) {
                    if (inputs.vertical < 0.0f) {
                        // fast drop
                        StopWallCling(0.0f);
                        StartFastDrop();
                    } else if (inputs.horizontal < 0.0f && isWalledRight || inputs.horizontal > 0.0f && isWalledLeft) {
                        // jump away
                        StopWallCling(WALL_JUMP_LAG);
                        WallJump(inputs.horizontal < 0.0f, 2.0f);
                    } else if (inputs.horizontal > 0.0f && isWalledRight || inputs.horizontal < 0.0f && isWalledLeft) {
                        // jump towards
                        StopWallCling(WALL_JUMP_LAG);
                        WallJump(inputs.horizontal > 0.0f, 1.0f);
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
                Self_RigidBody.AddForce(new Vector2(0, -FASTDROP_FORCE), ForceMode2D.Impulse);

                // disable fast drop if grounded
                if (isGrounded || isPlatform) {
                    StopFastDrop();
                }
            }
        }

        // reduce lag by lag time
        if (inWallClingLag) {
            wallClingLagTime -= Time.deltaTime;
            if (wallClingLagTime < 0.0f) wallClingLagTime = 0.0f;
        }

        // debug jumping text
        if (isPlayer) debugText.text = string.Format("IsGrounded: {0}\nIsHoldingWall: {1}\nIsWallCling: {2}\nJumps: {3}\nWallClingLag: {4:0.000}\nIgnorePlatform: {5}\nJump: {6}\nJumpRelease: {7}\nJumpHeld: {8}", isGrounded, isHoldingWall, isWallCling, jumps, wallClingLagTime, ignorePlatform, inputs.jump, inputs.jumpRelease, inputs.jumpHeld);

        // press esc to return to title
        if (isPlayer && Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
