using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnMove : MonoBehaviour {
    #region Variables

    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    GameObject Player;
    TextMeshProUGUI debugText;

    float MOVEMENT_FORCE = 15.0f;
    float MOVEMENT_SPEED = 5.0f;

    float JUMP_FORCE = 7.5f;
    float AIRJUMP_FORCE = 5.5f;
    float WALLJUMP_FORCE = 5.0f;
    float LONGJUMP_GRAV_MULT = 1f;

    int JUMPS_FROM_GND = 1;
    int JUMPS_FROM_WALL = 1;

    float DODGE_FORCE = 7.0f;
    float FASTDROP_FORCE = 30.0f;

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
    [HideInInspector]
    public bool isNormalState = true;
    [HideInInspector]
    public bool isWallCling = false;
    [HideInInspector]
    public bool isFastDropping = false;
    float wallClingLagTime = 0.0f; // amount of time that the wall cannot be clinged to
    float wallClingTimer = 0.0f;
    [HideInInspector]
    public float inputLagTime = 0.0f; // amount of time that inputs will be ignored
    bool ignorePlatform = false;
    bool queuedPlatPullVelReset = false;
    float trueGravityScale = 0.0f;
    bool facingRight = true;

    int layerCollisionMask;
    bool isPlayer = false;
    bool alive = false;
    bool isGrounded = false;
    bool isTrueGrounded = false;
    bool isPlatform = false;
    bool isLooseInPlatform = false;
    bool isInPlatform = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool isWalled = false;
    bool isHoldingWall = false;
    
    bool jumpButtonDaemon = false; // whether jump button is being held down, but becomes false once falling
    [HideInInspector]
    public bool fastDropStoppedFrame  = false; // true only on the frame that fastdropping is stopped, used for fastdrop attack

    #endregion

    #region Input

    public class Inputs {
        public float horizontal = 0.0f;
        public float vertical = 0.0f;
        public bool jump = false;
        public bool jumpRelease = false;
        bool _jumpDone = false;
        bool _jumpReleaseDone = false;
        public bool jumpHeld = false;
        public bool dodge = false;
        public bool attackMelee = false;
        public bool attackRanged = false;
        public bool attackTele = false;
        
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
    
    public Inputs inputs = new Inputs();

    #endregion

    #region Getter Functions

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

    #endregion

    #region Movement and State Change Functions

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
        fastDropStoppedFrame = true;
    }

    #endregion

    #region State Update Functions

    void updateInput() {
        if (alive && inputLagTime == 0.0f) {
            if (isPlayer) {
                // take inputs from user
                inputs.horizontal = Input.GetAxisRaw("Horizontal");
                inputs.vertical = Input.GetAxisRaw("Vertical");
                inputs.horizontal = Mathf.Abs(inputs.horizontal) < 0.35 ? 0 : Mathf.Sign(inputs.horizontal);
                inputs.vertical = Mathf.Abs(inputs.vertical) < 0.35 ? 0 : Mathf.Sign(inputs.vertical);
                inputs.jumpHeld = Input.GetButton("Jump");
                inputs.dodge = Input.GetButtonDown("Submit");
                inputs.attackMelee = Input.GetButtonDown("Fire1");
                inputs.attackRanged = Input.GetButtonDown("Fire2");
                inputs.attackTele = Input.GetButtonDown("Fire3");
            } else {
                // calculate inputs of entity
                var relPlayerPos = Player.transform.position - Self.transform.position;
                if (Mathf.Abs(relPlayerPos.x) < 5.0f && Mathf.Abs(relPlayerPos.y) < 1.0f) {
                    if (relPlayerPos.x > 1.0f) {
                        inputs.horizontal = 1.0f;
                        inputs.attackMelee = false;
                    } else if (relPlayerPos.x < -1.0f) {
                        inputs.horizontal = -1.0f;
                        inputs.attackMelee = false;
                    } else {
                        inputs.horizontal = 0.0f;
                        inputs.attackMelee = true;
                    }
                } else {
                    inputs.horizontal = 0.0f;
                    inputs.attackMelee = false;
                }
                inputs.vertical = 0.0f;
                inputs.jumpHeld = false;
                inputs.dodge = false;
                inputs.attackRanged = false;
                inputs.attackTele = false;
            }
        } else {
            inputs.horizontal = 0.0f;
            inputs.vertical = 0.0f;
            inputs.jumpHeld = false;
            inputs.dodge = false;
            inputs.attackMelee = false;
            inputs.attackRanged = false;
            inputs.attackTele = false;
        }
        inputs.Update();
        if (inputs.horizontal > 0.0f) {
            facingRight = true;
        } else if (inputs.horizontal < 0.0f) {
            facingRight = false;
        }
    }

    void updateState() {
        isPlayer = GetComponent<EnMain>().isPlayer;
        alive = GetComponent<EnHealth>().alive;
        isGrounded = isOnGround();
        isTrueGrounded = isOnTrueGround();
        isPlatform = isOnPlatform();
        isLooseInPlatform = isLooseInsidePlatform();
        isInPlatform = isInsidePlatform();
        isWalledLeft = isOnWallLeft();
        isWalledRight = isOnWallRight();
        isWalled = isWalledLeft || isWalledRight;
        isHoldingWall = alive && (inputs.horizontal > 0 && isWalledRight || inputs.horizontal < 0 && isWalledLeft);
    }

    #endregion

    #region Main Functions

    void Start() {
        Self = this.gameObject;
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        Player = GameObject.Find("Player");
        debugText = GameObject.Find("Debug Text").GetComponent<TextMeshProUGUI>();
        trueGravityScale = Self_RigidBody.gravityScale;
        layerCollisionMask = getLayerCollisionMask();
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        // update state and movement
        updateInput();
        updateState();

        if (fastDropStoppedFrame) {
            fastDropStoppedFrame = false;
        }

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        isNormalState = !isWallCling && !isFastDropping;

        if (alive) {
            if (isNormalState) {
                // normal state

                // hover very slightly above ground in order to not get stuck on 0m ledges between ground and platform
                if (isGrounded && inputs.horizontal != 0.0f) {
                    Self_RigidBody.velocity += new Vector2(0.0f, 0.1f);
                }

                // pull up through platform
                if (isInPlatform && wallClingLagTime == 0.0f && !ignorePlatform) {
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
                if (!isInPlatform || wallClingLagTime > 0.0f) {
                    if (Self_RigidBody.velocity.x * inputs.horizontal > 0) {
                        Self_RigidBody.AddForce(new Vector2(inputs.horizontal * MOVEMENT_FORCE, 0.0f) * Mathf.Max(1.0f - Mathf.Pow(Self_RigidBody.velocity.x / MOVEMENT_SPEED, 4.0f), 0.0f), ForceMode2D.Force);
                    } else {
                        Self_RigidBody.AddForce(new Vector2(inputs.horizontal * MOVEMENT_FORCE, 0.0f), ForceMode2D.Force);
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
                if (wallClingLagTime == 0.0f && !ignorePlatform) {
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
                        if (inputs.vertical < 0.0f && isPlatform && !isTrueGrounded) {
                            // drop through platform
                            StartIgnorePlatform();
                            wallClingLagTime = PLATFORM_FALL_LAG;
                        } else {
                            Jump();
                            jumpButtonDaemon = true;
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
                if (jumpButtonDaemon) {
                    Self_RigidBody.gravityScale = trueGravityScale * LONGJUMP_GRAV_MULT;
                    if (Self_RigidBody.velocity.y < 0) {
                        jumpButtonDaemon = false;
                    }
                } else if (Self_RigidBody.gravityScale != trueGravityScale) {
                    Self_RigidBody.gravityScale = trueGravityScale;
                }

                // make jumping false once jump button is released
                if (inputs.jumpRelease && jumpButtonDaemon) {
                    jumpButtonDaemon = false;
                }

                // dodging
                if (inputs.dodge) {
                    Self_RigidBody.AddForce(new Vector2(facingRight ? DODGE_FORCE : -DODGE_FORCE, 0.0f), ForceMode2D.Impulse);
                    GetComponent<EnHealth>().invulnTime = GetComponent<EnHealth>().DODGE_INVULN;
                    inputLagTime = GetComponent<EnHealth>().DODGE_INVULN;
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
                Self_RigidBody.AddForce(new Vector2(0, -FASTDROP_FORCE), ForceMode2D.Force);

                // disable fast drop if grounded
                if (isGrounded || isPlatform) {
                    StopFastDrop();
                }
            }
        }

        // reduce lag time by time passed
        if (wallClingLagTime > 0.0f) {
            wallClingLagTime -= Time.deltaTime;
            if (wallClingLagTime < 0.0f) wallClingLagTime = 0.0f;
        }

        if (inputLagTime > 0.0f) {
            inputLagTime -= Time.deltaTime;
            if (inputLagTime < 0.0f) inputLagTime = 0.0f;
        }

        // debug text
        if (isPlayer) debugText.text = string.Format("IsGrounded: {0}\nIsHoldingWall: {1}\nIsWallCling: {2}\nJumps: {3}\nWallClingLag: {4:0.000}\nInputLag: {5:0.000}\nIgnorePlatform: {6}\nHorz: {7}\nVert: {8}\nJump: {9}", isGrounded, isHoldingWall, isWallCling, jumps, wallClingLagTime, inputLagTime, ignorePlatform, inputs.horizontal, inputs.vertical, inputs.jumpHeld);
    }

    #endregion
}
