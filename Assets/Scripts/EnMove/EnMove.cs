using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnMove : MonoBehaviour {
    #region Variables

    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    Rigidbody2D Self_RigidBody;
    EnMain EnMainInst;
    GameObject Player;
    public Animator animator;
    public TextMeshProUGUI DebugText;

    float MOVEMENT_FORCE = 15.0f;
    float MOVEMENT_SPEED = 5.0f;

    float JUMP_FORCE = 7.5f;
    float AIRJUMP_FORCE = 5.5f;
    float WALLJUMP_FORCE = 5.0f;
    float LONGJUMP_GRAV_MULT = 1f;

    int JUMPS_FROM_GND = 1;
    int JUMPS_FROM_WALL = 1;

    float DODGE_SPEED = 7.0f;
    float FASTDROP_FORCE = 30.0f;

    float WALL_CLIMB_THRESHOLD = -0.3f;

    float WALL_SLIDE_TIMER = 3.0f;
    float WALL_DROP_TIMER = 4.0f;
    float WALL_DROP_SPEED = 1.0f;
    float PLATFORM_PULLUP_SPEED = 8.0f;

    float WALL_DROPOFF_LAG = 1.0f;
    float WALL_JUMP_LAG = 0.8f;
    float PLATFORM_FALL_LAG = 0.8f;
    float DODGE_LAG = 1.0f;

    int jumps = 0; // number of jumps available
    bool lockGround = false; // if true ground cannot refresh jump counter
    bool lockWall = false; // if true wall cannot refresh jump counter
    [HideInInspector]
    public bool isNormalState = true;
    [HideInInspector]
    public bool isWallCling = false;
    [HideInInspector]
    public bool isFastDropping = false;
    float dodgeLagTime = 0.0f;

    float wallClingLagTime = 0.0f; // amount of time that the wall cannot be clinged to
    float wallClingTimer = 0.0f;
    bool wallLetGo = false; // if wall has been involuntarily let go, resets if touch ground or air jump
    [HideInInspector]
    public float inputLagTime = 0.0f; // amount of time that inputs will be ignored
    [HideInInspector]
    public bool inAttack = false; // inputs will also be ignored if currently attacking
    [HideInInspector]
    public string attackType = "";
    bool ignorePlatform = false;
    bool queuedPlatPullVelReset = false;
    float trueGravityScale = 0.0f;
    [HideInInspector]
    public bool facingRight = true;

    bool isPlayer = false;
    bool alive = false;
    [HideInInspector]
    public bool isGrounded = false;
    bool isTrueGrounded = false;
    bool isPlatform = false;
    bool isLooseInPlatform = false;
    bool isPLooseInPlatform = false;
    bool isInPlatform = false;
    bool useLooseForL = false; // once isinplatform is true this switches to islooseinplatform until that is false
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool isWalled = false;
    bool isHoldingWall = false;
    [HideInInspector]
    public float inAirTime = 0.0f;
    
    [HideInInspector]
    public bool platformPullUp = false; // currently should be pulling up through platform
    bool jumpButtonDaemon = false; // whether jump button is being held down, but becomes false once falling
    [HideInInspector]
    public bool fastDropStoppedFrame  = false; // true only on the frame that fastdropping is stopped, used for fastdrop attack
    bool looseInPlatDaemon = false;
    float PinputsHorizontal = 0.0f;

    #endregion

    #region Getter Functions

    bool isOnGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentXBot, transform.position.y - EnMainInst.bounds.extentY - EnMainInst.bounds.extraGap), Vector2.right, EnMainInst.bounds.sizeXBot, LayerMask.GetMask("Default", "Platform"));
        return groundRaycast.collider != null;
    }

    bool isOnTrueGround() {
        var groundRaycast = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentXBot, transform.position.y - EnMainInst.bounds.extentY - EnMainInst.bounds.extraGap), Vector2.right, EnMainInst.bounds.sizeXBot, LayerMask.GetMask("Default"));
        return groundRaycast.collider != null;
    }

    bool isOnPlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX, transform.position.y - EnMainInst.bounds.extentY - EnMainInst.bounds.extraGap), Vector2.down, 3.0f, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX, transform.position.y - EnMainInst.bounds.extentY - EnMainInst.bounds.extraGap), Vector2.down, 3.0f, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isLooseInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX, transform.position.y + EnMainInst.bounds.extentY + EnMainInst.bounds.extraGap), Vector2.down, EnMainInst.bounds.sizeY + EnMainInst.bounds.extraSizeGap, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX, transform.position.y + EnMainInst.bounds.extentY + EnMainInst.bounds.extraGap), Vector2.down, EnMainInst.bounds.sizeY + EnMainInst.bounds.extraSizeGap, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isInsidePlatform() {
        var platformRaycastL = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX, transform.position.y + EnMainInst.bounds.extentY), Vector2.down, EnMainInst.bounds.sizeYBot, LayerMask.GetMask("Platform"));
        var platformRaycastR = Physics2D.Raycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX, transform.position.y + EnMainInst.bounds.extentY), Vector2.down, EnMainInst.bounds.sizeYBot, LayerMask.GetMask("Platform"));
        return platformRaycastL.collider != null || platformRaycastR.collider != null;
    }

    bool isOnWallLeft() {
        var leftWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x - EnMainInst.bounds.extentX - EnMainInst.bounds.extraGap, transform.position.y + EnMainInst.bounds.extentY), Vector2.down, EnMainInst.bounds.sizeYBot, LayerMask.GetMask("Default"));
        return leftWallRaycast.collider != null;
    }

    bool isOnWallRight() {
        var rightWallRaycast = Physics2D.Raycast(new Vector2(transform.position.x + EnMainInst.bounds.extentX + EnMainInst.bounds.extraGap, transform.position.y + EnMainInst.bounds.extentY), Vector2.down, EnMainInst.bounds.sizeYBot, LayerMask.GetMask("Default"));
        return rightWallRaycast.collider != null;
    }

    #endregion

    #region Movement and State Change Functions

    void Jump() {
        Self_RigidBody.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
    }

    void AirJump() {
        if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
            Self_RigidBody.velocity = new Vector2(Self_RigidBody.velocity.x, 0.0f);
        Self_RigidBody.AddForce(new Vector2(0, AIRJUMP_FORCE), ForceMode2D.Impulse);
    }

    void WallJump(bool left, float strength) {
        if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
        Self_RigidBody.AddForce(new Vector2((left ? -1.0f : 1.0f) * WALLJUMP_FORCE, WALLJUMP_FORCE) * strength, ForceMode2D.Impulse);
        wallClingLagTime = WALL_JUMP_LAG;
        isHoldingWall = false;
    }

    void StartWallCling(bool climb) {
        isWallCling = true;

        if (climb) {
            if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
                Self_RigidBody.velocity = new Vector2(0.0f, 10.0f);
        } else {
            StartHaltState();
        }
    }

    void StopWallCling(float lagTime) {
        isWallCling = false;
        wallClingTimer = 0.0f;
        wallClingLagTime = lagTime;
        if (Self_RigidBody.bodyType == RigidbodyType2D.Static) {
            StopHaltState();
        }
    }

    void StartIgnorePlatform() {
        if (!ignorePlatform) {
            ignorePlatform = true;
            looseInPlatDaemon = true;
            Self.layer = LayerMask.NameToLayer("Ignore Platform");
        }
    }

    void StopIgnorePlatform() {
        if (ignorePlatform) {
            ignorePlatform = false;
            Self.layer = isPlayer ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Enemy");
        }
    }

    void StartFastDrop() {
        isFastDropping = true;
        if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
            Self_RigidBody.velocity = new Vector2(0.0f, Self_RigidBody.velocity.y);
    }
    
    void StopFastDrop() {
        isFastDropping = false;
        fastDropStoppedFrame = true;
        if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic)
            Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
    }

    public void StartHaltState() {
        Self_RigidBody.bodyType = RigidbodyType2D.Static;
    }

    public void StopHaltState() {
        Self_RigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void StartAttack(string attackTypeVal) {
        inAttack = true;
        attackType = attackTypeVal;
        if (attackTypeVal == "Weapon Plus") return; // TEMPORARY
        var prefix = isPlayer ? "Player" : "Enemy";
        var animation = prefix + "_Attack" + attackType;
        if (animator.HasState(0, Animator.StringToHash(animation))) animator.Play(animation);
        else animator.Play(isPlayer ? "Player_AttackSword" : "Enemy_AttackBasic1");
    }
    
    public void StopAttack() {
        inAttack = false;
        var prefix = isPlayer ? "Player" : "Enemy";
        animator.Play(prefix + "_Idle");
    }

    #endregion

    #region State Update Functions

    void updateInput() {
        if (alive && inputLagTime == 0.0f && !inAttack && !isInPlatform) {
            if (isPlayer) {
                // take inputs from user
                EnMainInst.inputs.horizontal = Input.GetAxisRaw("Horizontal");
                EnMainInst.inputs.vertical = Input.GetAxisRaw("Vertical");
                EnMainInst.inputs.horizontal = Mathf.Abs(EnMainInst.inputs.horizontal) < 0.35f ? 0.0f : Mathf.Sign(EnMainInst.inputs.horizontal);
                EnMainInst.inputs.vertical = Mathf.Abs(EnMainInst.inputs.vertical) < 0.35f ? 0.0f : Mathf.Sign(EnMainInst.inputs.vertical);
                EnMainInst.inputs.jumpHeld = Input.GetButton("Jump");
                EnMainInst.inputs.dodge = Input.GetButtonDown("Submit");
                EnMainInst.inputs.attack1 = Input.GetButtonDown("Fire1");
                EnMainInst.inputs.attack2 = Input.GetButtonDown("Fire2");
                EnMainInst.inputs.attackTele = Input.GetButtonDown("Fire3");
                EnMainInst.inputs.pickupItem = Input.GetKeyDown(KeyCode.I);
                EnMainInst.inputs.dropItem = Input.GetKeyDown(KeyCode.K);
            } else {
                // calculate inputs of entity
                var relPlayerPos = Player.transform.position - Self.transform.position;
                if (Mathf.Abs(relPlayerPos.x) < 5.0f && Mathf.Abs(relPlayerPos.y) < 1.0f &&
                    Physics2D.Raycast(
                        new Vector2(transform.position.x, transform.position.y + EnMainInst.bounds.extentY * 0.8f),
                        relPlayerPos,
                        relPlayerPos.magnitude,
                        LayerMask.GetMask("Default", "Platform")
                    ).collider == null) {
                    if (relPlayerPos.x > 1.0f) {
                        EnMainInst.inputs.horizontal = 1.0f;
                        EnMainInst.inputs.attack1 = false;
                    } else if (relPlayerPos.x < -1.0f) {
                        EnMainInst.inputs.horizontal = -1.0f;
                        EnMainInst.inputs.attack1 = false;
                    } else {
                        if (relPlayerPos.x > 0.0f && !facingRight) {
                            EnMainInst.inputs.horizontal = 1.0f;
                        } else if (relPlayerPos.x < 0.0f && facingRight) {
                            EnMainInst.inputs.horizontal = -1.0f;
                        } else {
                            EnMainInst.inputs.horizontal = 0.0f;
                        }
                        EnMainInst.inputs.attack1 = true;
                    }
                } else {
                    EnMainInst.inputs.horizontal = 0.0f;
                    EnMainInst.inputs.attack1 = false;
                }
                EnMainInst.inputs.vertical = 0.0f;
                EnMainInst.inputs.jumpHeld = false;
                EnMainInst.inputs.dodge = false;
                EnMainInst.inputs.attack2 = false;
                EnMainInst.inputs.attackTele = false;
                EnMainInst.inputs.pickupItem = false;
                EnMainInst.inputs.dropItem = false;
            }
        } else {
            EnMainInst.inputs.horizontal = 0.0f;
            EnMainInst.inputs.vertical = 0.0f;
            EnMainInst.inputs.jumpHeld = false;
            EnMainInst.inputs.dodge = false;
            EnMainInst.inputs.attack1 = false;
            EnMainInst.inputs.attack2 = false;
            EnMainInst.inputs.attackTele = false;
            EnMainInst.inputs.pickupItem = false;
            EnMainInst.inputs.dropItem = false;
        }
        EnMainInst.inputs.Update();
    }

    void updateState() {
        isPlayer = EnMainInst.isPlayer;
        alive = GetComponent<EnHealth>().alive;
        isGrounded = isOnGround();
        isTrueGrounded = isOnTrueGround();
        isPlatform = isOnPlatform();
        isLooseInPlatform = isLooseInsidePlatform();
        isInPlatform = isInsidePlatform();
        isWalledLeft = isOnWallLeft();
        isWalledRight = isOnWallRight();
        isWalled = isWalledLeft || isWalledRight;
        isHoldingWall = alive && (EnMainInst.inputs.horizontal > 0 && isWalledRight || EnMainInst.inputs.horizontal < 0 && isWalledLeft);
    }

    #endregion

    #region Main Functions

    void Start() {
        Self = this.gameObject;
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        Self_RigidBody = GetComponent<Rigidbody2D>();
        EnMainInst = GetComponent<EnMain>();
        Player = GameObject.Find("Player");
        trueGravityScale = Self_RigidBody.gravityScale;
        jumps = JUMPS_FROM_GND;
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();

        updateInput();
        
        if (EnMainInst.inputs.horizontal > 0.0f) {
            facingRight = true;
        } else if (EnMainInst.inputs.horizontal < 0.0f) {
            facingRight = false;
        }

        if (!facingRight) {
            transform.localScale = new Vector3(-1.0f, transform.localScale.y, transform.localScale.z);
        } else {
            transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
        }

        updateState();

        if (!isGrounded) {
            inAirTime += Time.deltaTime;
        } else {
            inAirTime = 0.0f;
        }

        if (fastDropStoppedFrame) {
            fastDropStoppedFrame = false;
        }

        if (looseInPlatDaemon && isPLooseInPlatform && !isLooseInPlatform) {
            looseInPlatDaemon = false;
        }

        // unlock ground/wall jump counter reset
        if (!isGrounded && lockGround) lockGround = false;
        if (!isWalled && lockWall) lockWall = false;

        isNormalState = !isWallCling && !isFastDropping;

        if (alive) {
            // add dodge lag when invulnerability disappears
            if (GetComponent<EnHealth>().dodgeInvulnTime == 0.0f && GetComponent<EnHealth>().pastDodgeInvulnTime > 0.0f) {
                dodgeLagTime = DODGE_LAG;
            }

            // stop ignoring platforms after falling through one enough
            if (ignorePlatform && !looseInPlatDaemon) {
                StopIgnorePlatform();
            }

            if (!useLooseForL && isInPlatform) {
                useLooseForL = true;
            } else if (useLooseForL && !isLooseInPlatform) {
                useLooseForL = false;
            }

            if (isNormalState) {
                // normal state

                if (isGrounded && wallLetGo) wallLetGo = false;

                // pull up through platform
                platformPullUp = useLooseForL && !ignorePlatform;
                if (isPlayer) {
                    if (platformPullUp) {
                        Self_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Self_RigidBody.velocity.y, PLATFORM_PULLUP_SPEED));
                        queuedPlatPullVelReset = true;
                    } else if (queuedPlatPullVelReset) {
                        Self_RigidBody.velocity = new Vector2(0.0f, 0.0f);
                        wallClingLagTime = PLATFORM_FALL_LAG;
                        queuedPlatPullVelReset = false;
                    }
                }

                // horizontal movement
                if (!EnMainInst.haltMotion && (!isInPlatform || wallClingLagTime > 0.0f) && GetComponent<EnHealth>().dodgeInvulnTime == 0.0f && !isHoldingWall) {
                    if (Self_RigidBody.velocity.x * EnMainInst.inputs.horizontal > 0) {
                        Self_RigidBody.AddForce(new Vector2(EnMainInst.inputs.horizontal * MOVEMENT_FORCE, 0.0f) * Mathf.Max(1.0f - Mathf.Pow(Self_RigidBody.velocity.x / MOVEMENT_SPEED, 4.0f), 0.0f), ForceMode2D.Force);
                    } else {
                        Self_RigidBody.AddForce(new Vector2(EnMainInst.inputs.horizontal * MOVEMENT_FORCE, 0.0f), ForceMode2D.Force);
                    }
                }

                // kickback when stop moving
                if ((PinputsHorizontal > 0.5f && EnMainInst.inputs.horizontal < 0.5f || PinputsHorizontal < -0.5f && EnMainInst.inputs.horizontal > -0.5f) && Self_RigidBody.bodyType == RigidbodyType2D.Dynamic) {
                    Self_RigidBody.velocity = new Vector2(0.0f, Self_RigidBody.velocity.y);
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
                if (isPlayer && wallClingLagTime == 0.0f && !ignorePlatform && isHoldingWall && !isGrounded && !isInPlatform && !wallLetGo) {
                    if (Self_RigidBody.velocity.y < WALL_CLIMB_THRESHOLD) {
                        // simple cling
                        StartWallCling(false);
                    } else {
                        // climb up
                        StartWallCling(true);
                    }
                }

                // jumping
                if (EnMainInst.inputs.jump) {
                    if (isGrounded) {
                        if (EnMainInst.inputs.vertical < 0.0f && isPlatform && !isTrueGrounded) {
                            // drop through platform
                            StartIgnorePlatform();
                            wallClingLagTime = PLATFORM_FALL_LAG;
                        } else {
                            Jump();
                            jumpButtonDaemon = true;
                            
                            // halt horizontal velocity if velocity is from a dodge
                            if (GetComponent<EnHealth>().dodgeInvulnTime != 0.0f) {
                                Self_RigidBody.velocity = new Vector2(0.0f, Self_RigidBody.velocity.y);
                            }
                        }
                    } else {
                        if (EnMainInst.inputs.vertical < 0.0f) {
                            // fastdrop
                            StartFastDrop();
                        } else if (jumps > 0) {
                            // air jump
                            AirJump();
                            jumps--;
                            if (wallLetGo) wallLetGo = false;
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
                if (EnMainInst.inputs.jumpRelease && jumpButtonDaemon) {
                    jumpButtonDaemon = false;
                }

                // dodging
                if (EnMainInst.inputs.dodge && dodgeLagTime == 0.0f) {
                    if (isGrounded) {
                        // dodge on ground completely horizontal
                        Self_RigidBody.velocity = new Vector2(facingRight ? DODGE_SPEED : -DODGE_SPEED, Self_RigidBody.velocity.y);
                    } else {
                        // dodge in air sends more downward
                        Self_RigidBody.velocity = new Vector2(facingRight ? DODGE_SPEED : -DODGE_SPEED, -DODGE_SPEED * 0.4f);
                    }
                    GetComponent<EnHealth>().dodgeInvulnTime = GetComponent<EnHealth>().DODGE_INVULN;
                    dodgeLagTime = DODGE_LAG;
                }
            } else if (isWallCling) {
                // wall cling

                if (wallClingTimer < WALL_SLIDE_TIMER) {
                    if (Self_RigidBody.velocity.y > 0.0f) {
                        Self_RigidBody.velocity = new Vector2(0.0f, Self_RigidBody.velocity.y);
                    } else if (Self_RigidBody.bodyType == RigidbodyType2D.Dynamic) {
                        StartHaltState();
                    }
                } else if (wallClingTimer < WALL_DROP_TIMER) {
                    if (Self_RigidBody.bodyType == RigidbodyType2D.Static) {
                        StopHaltState();
                    }
                    Self_RigidBody.velocity = new Vector2(0.0f, Mathf.Max(Self_RigidBody.velocity.y, -WALL_DROP_SPEED));
                } else {
                    StopWallCling(WALL_DROPOFF_LAG);
                    wallLetGo = true;
                }
                
                // jump off wall actions
                if (EnMainInst.inputs.jump) {
                    if (EnMainInst.inputs.vertical < 0.0f) {
                        // fast drop
                        StopWallCling(0.0f);
                        StartFastDrop();
                    } else if (EnMainInst.inputs.horizontal < 0.0f && isWalledRight || EnMainInst.inputs.horizontal > 0.0f && isWalledLeft) {
                        // jump away
                        StopWallCling(WALL_JUMP_LAG);
                        WallJump(EnMainInst.inputs.horizontal < 0.0f, 2.0f);
                    } else if (EnMainInst.inputs.horizontal > 0.0f && isWalledRight || EnMainInst.inputs.horizontal < 0.0f && isWalledLeft) {
                        // jump towards
                        StopWallCling(WALL_JUMP_LAG);
                        WallJump(EnMainInst.inputs.horizontal > 0.0f, 1.0f);
                    } else {
                        // jump neutral
                        StopWallCling(WALL_JUMP_LAG);
                        WallJump(isWalledRight, 1.0f);
                    }
                }

                // disable wall cling if grounded
                if ((isGrounded || !isWalled) && isWallCling || isInPlatform) {
                    StopWallCling(0.0f);
                }

                wallClingTimer += Time.deltaTime;
            } else if (isFastDropping) {
                // fast dropping
                Self_RigidBody.AddForce(new Vector2(0, -FASTDROP_FORCE), ForceMode2D.Force);

                // disable fast drop if grounded
                if (isGrounded) {
                    StopFastDrop();
                }
            }

            // cancel dodge invulnerability for any input
            if ((EnMainInst.inputs.jump || EnMainInst.inputs.attack1 || EnMainInst.inputs.attack2 || EnMainInst.inputs.attackTele) && GetComponent<EnHealth>().dodgeInvulnTime != 0.0f) {
                GetComponent<EnHealth>().dodgeInvulnTime = 0.0f;
            }
        }

        // reduce lag time by time passed
        if (inputLagTime > 0.0f) {
            inputLagTime -= Time.deltaTime;
            if (inputLagTime < 0.0f) inputLagTime = 0.0f;
        }

        if (wallClingLagTime > 0.0f) {
            wallClingLagTime -= Time.deltaTime;
            if (wallClingLagTime < 0.0f) wallClingLagTime = 0.0f;
        }

        if (dodgeLagTime > 0.0f) {
            dodgeLagTime -= Time.deltaTime;
            if (dodgeLagTime < 0.0f) dodgeLagTime = 0.0f;
        }

        // debug text
        if (isPlayer) DebugText.text = string.Format("IsGrounded: {0}\nIsPlatform: {1}\nIsHoldingWall: {2}\nIsWallCling: {3}\nIsInPlatform: {4}\nJumps: {5}\nInputLag: {6:0.000}\nInAttack: {7}\nWallClingLag: {8:0.000}\nDodgeLag: {9:0.000}\nIgnorePlatform: {10}\nInAirTime: {11:0.000}\nHorz: {12}\nVert: {13}\nJump: {14}\nHaltMotion: {15}", isGrounded, isPlatform, isHoldingWall, isWallCling, isInPlatform, jumps, inputLagTime, inAttack, wallClingLagTime, dodgeLagTime, ignorePlatform, inAirTime, EnMainInst.inputs.horizontal, EnMainInst.inputs.vertical, EnMainInst.inputs.jumpHeld, EnMainInst.haltMotion);
    }

    void LateUpdate() {
        isPLooseInPlatform = isLooseInPlatform;
        PinputsHorizontal = EnMainInst.inputs.horizontal;
    }

    #endregion
}
