using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    [Header("Health")]
    public float currentHealth;
    public float maxHealth;
    [SerializeField] private float fallDamageMultiplier;
    public bool isTakingDamage;

    public float currentOxygen;
    public float maxOxygen = 100f;
    [SerializeField] private float oxygenDecreaseRate = 1f;
    [SerializeField] private float oxygenRegenRate = 1f;
    [SerializeField] private float drowningDamage = 5f;
    [SerializeField] private float drowningDamageDelay = 3f;
    public bool isDrowning;


    [Header("Movement")]
    public bool characterControllerMovement = true;
    [field: ReadOnlyField] public float speed = 25f;
    [field: ReadOnlyField] public float affectedSpeed = 25f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float proneSpeed = 1f;
    [SerializeField] private float climbingSpeed = 5f;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool isProne;
    [SerializeField] private bool isClimbing;
    [SerializeField] private bool canClimb;
    public bool inWater;
    public bool isSwimming;
    public bool isUnderwater;
    public bool isOnRope = false;
    public float weight;
    public float maxWeight = 50f;
    public float minSpeed = 1f;

    public ParticleSystem waterRipples;

    private Vector3 controllerOriginalCenter;
    private float controllerOriginalHeight;

    private Vector3 colliderOriginalCenter;
    private float colliderOriginalHeight;
    [HideInInspector] public Vector3 lastAnchorPoint;
    [HideInInspector] public Vector3 maxRopePos;

    [field: ReadOnlyField] public Vector3 moveDirection;
    [SerializeField] private Transform orientation;


    [Header("Stamina")]
    [SerializeField] private AudioSource staminaSource;
    [SerializeField] private bool canRun;
     public float stamina = 5f;
     public float maxStamina = 5f;
    [SerializeField] private float staminaDecreaseRate = 1f, staminaIncreaseRate = 0.75f;

    [SerializeField] private float climbingStaminaDecreaseRate = 1f;

    [Header("Jump")]
    [field: ReadOnlyField, SerializeField] private float currentGravScale = 1.0f;
    [SerializeField] private float gravScale = 1.0f;
    [SerializeField] private float underwaterGravScale = 1.0f;
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isGrounded;

    [SerializeField] private float minFallVelocity = -0.1f;
    [SerializeField] private float fallVelocity;
    [SerializeField] private float fallVelocityToTakeDamage;

    [Header("Animation")]
    [SerializeField] private GameObject model;
    private Animator animator;
    private CapsuleCollider capCollider;

    [Header("Audio")]
    [SerializeField] private Vector2 walkPitchRange = new Vector2() { x = 0.8f, y = 1f };
    [SerializeField] private Vector2 walkVolumeRange = new Vector2() { x = 0.45f, y = 0.55f };
    [SerializeField] private Vector2 runPitchRange = new Vector2() { x = 1.1f, y = 1.3f };
    [SerializeField] private Vector2 runVolumeRange = new Vector2() { x = 0.65f, y = 0.75f };
    [SerializeField] private Vector2 crouchPitchRange = new Vector2() { x = 1.1f, y = 1.3f };
    [SerializeField] private Vector2 crouchVolumeRange = new Vector2() { x = 0.65f, y = 0.75f };
    [SerializeField] private List<AudioClip> footstepSounds;
    private AudioClip lastFootstep;
    [SerializeField] private List<AudioClip> jumpSounds;
    [SerializeField] private List<AudioClip> climbingSounds;

    private AudioSource audioSource;


    void Start()
    {
        maxStamina = stamina;
        controller = GetComponent<CharacterController>();
        controllerOriginalCenter = controller.center;
        controllerOriginalHeight = controller.height;

        capCollider = model.transform.GetChild(0).GetComponent<CapsuleCollider>();
        colliderOriginalCenter = capCollider.center;
        colliderOriginalHeight = capCollider.height;

        Cursor.lockState = CursorLockMode.Locked;

        audioSource = GetComponent<AudioSource>();

        animator = model.GetComponent<Animator>();

        waterRipples.Stop();
    }

    void Update()
    {

        if (!isSwimming)
        {
            float yStore = moveDirection.y;
            moveDirection = (orientation.forward * Input.GetAxisRaw("Vertical")) + (orientation.right * Input.GetAxisRaw("Horizontal"));
            moveDirection = moveDirection.normalized * affectedSpeed;
            moveDirection.y = yStore;
        }
        else if (isSwimming && isUnderwater)
        {
            //float yStore = moveDirection.y;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            moveDirection = (ray.direction * Input.GetAxisRaw("Vertical")) + (orientation.right * Input.GetAxisRaw("Horizontal"));
            moveDirection = moveDirection.normalized * affectedSpeed;
            moveDirection.y = moveDirection.y - (weight / 10);
        }
        else if (isSwimming && !isUnderwater)
        {
            float yStore = moveDirection.y;
            moveDirection = (orientation.forward * Input.GetAxisRaw("Vertical")) + (orientation.right * Input.GetAxisRaw("Horizontal"));
            moveDirection = moveDirection.normalized * affectedSpeed;
            //moveDirection.y = yStore;
        }
        


        animator.SetFloat("x", Input.GetAxisRaw("Horizontal"), 0.1f, Time.deltaTime);
        animator.SetFloat("z", Input.GetAxisRaw("Vertical"), 0.1f, Time.deltaTime);
        animator.SetFloat("y", moveDirection.y, 0.1f, Time.deltaTime);

        //isGrounded = controller.isGrounded;

        if (Input.GetButtonDown("Interact") && isClimbing)
        {
            isClimbing = false;
            SetWalkVariables();
        }

        //if the player is grounded...
        if (controller.isGrounded && !isJumping)
        {
            // If the player is moving...
            if (controller.velocity.magnitude > 2f)
            {
                if (animator.GetBool("isRunning"))
                {
                    stamina -= Time.deltaTime * staminaDecreaseRate;
                }
                else
                    animator.SetBool("isWalking", true);

                if (!audioSource.isPlaying)
                {
                    AudioClip footstep = AvoidRepeatedFootstepAudio();
                    audioSource.PlayOneShot(footstep);
                }

                if (inWater)
                {
                    if (!waterRipples.isPlaying)
                    {
                        waterRipples.Play();
                    }
                }
            }
            else if (Mathf.Round(controller.velocity.magnitude) <= 0f)
            {
                animator.SetBool("isWalking", false);

                if (inWater)
                {
                    if (waterRipples.isPlaying)
                    {
                        waterRipples.Stop();
                    }
                }
            }

           

            //if the player is jumping...
            if (Input.GetButtonDown("Jump") && canJump)
            {
                moveDirection.y = jumpForce;

                isJumping = true;

                if (isCrouching)
                {
                    isCrouching = false;
                    SetWalkVariables();
                }

                if (isClimbing)
                {
                    isClimbing = false;
                    SetWalkVariables();
                }

                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Count)]);
            }
        }

        //if the player is falling...
        else if ((!controller.isGrounded || moveDirection.y < 0) && !isOnRope)
        {
            moveDirection.y += (Physics.gravity.y * currentGravScale * Time.deltaTime);

            if (controller.velocity.y < fallVelocity)
            {
                fallVelocity = controller.velocity.y;
            }

            if (isJumping)
                isJumping = false;
        } //else moveDirection.y = minFallVelocity;
        else if (isOnRope)
        {
            moveDirection = (lastAnchorPoint - transform.position) / (lastAnchorPoint - transform.position).magnitude;

            if (transform.position.y <= lastAnchorPoint.y + 0.5f)
            {
                moveDirection.y = Input.GetAxisRaw("Vertical") * climbingSpeed;
            }
            else isOnRope = false;
        }

        // if the player has just landed...
        if ((controller.isGrounded && !isGrounded) || (controller.isGrounded && moveDirection.y < -2f))
        {
            audioSource.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Count)]);

            if (fallVelocity < fallVelocityToTakeDamage)
            {
                StartCoroutine(DecreaseHealth(fallVelocity * fallDamageMultiplier));
            }

            fallVelocity = 0f;
            moveDirection.y = 0f;

            if (isOnRope)
                isOnRope = false;

            if (isJumping)
                isJumping = false;

            isGrounded = true;

            if (!isProne)
                canJump = true;
            else canJump = false;
        }
        animator.SetBool("isJumping", isJumping);

        animator.SetBool("isGrounded", isGrounded);

        if (canRun)
        {
            if (Input.GetButton("Run") && stamina > 0f && !staminaSource.isPlaying)
            {
                SetRunVariables();
            }
            else SetWalkVariables();
        }
        else if (!isCrouching && !isProne) SetWalkVariables();

        //if the player is crouching...
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;

            if (isProne) isProne = false;

            if (!isCrouching)
            {
                SetWalkVariables();
            }
            else
            {
                SetCrouchVariables();
            }
        }
        animator.SetBool("isCrouching", isCrouching);

        //if the player is prone...
        if (Input.GetButtonDown("Prone"))
        {
            isProne = !isProne;

            if (isCrouching) isCrouching = false;

            if (!isProne)
            {
                SetWalkVariables();
            }
            else
            {
                SetProneVariables();
            }
        }
        animator.SetBool("isProne", isProne);

        if (canClimb && Input.GetButtonDown("Interact") && stamina > 0f && !staminaSource.isPlaying)
        {
            SetClimbVariables();
        }
        animator.SetBool("isClimbing", isClimbing);

        if (isClimbing && stamina > 0f)
        {
            moveDirection.y = Input.GetAxisRaw("Vertical") * climbingSpeed;

            if (controller.velocity.magnitude > 2f)
            {
                stamina -= Time.deltaTime * climbingStaminaDecreaseRate;

                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(climbingSounds[Random.Range(0, climbingSounds.Count)]);
            }
        }

        if (isSwimming)
        {
            SetSwimVariables();

            if (isUnderwater)
            {
                currentOxygen -= Time.deltaTime * oxygenDecreaseRate;

                if (currentOxygen <= 0f && !isDrowning)
                {
                    StartCoroutine(DecreaseHealthConsistent(drowningDamage, drowningDamageDelay));
                    isDrowning = true;
                }
            }
            else if (currentOxygen < maxOxygen)
            {
                currentOxygen += Time.deltaTime * oxygenRegenRate;
            }
        }
        else if (currentOxygen < maxOxygen)
        {
            currentOxygen += Time.deltaTime * oxygenRegenRate;
        }

        animator.SetBool("isSwimming", isSwimming);

        affectedSpeed = speed * (maxWeight - weight) / maxWeight;

        if (affectedSpeed <= minSpeed)
        {
            affectedSpeed = minSpeed;
        }


        if (characterControllerMovement != false)
            controller.Move(moveDirection * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Climbable"))
        {
            canClimb = true;
        }

        if (other.CompareTag("Water"))
        {
            inWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Climbable"))
        {
            canClimb = false;

            if (isClimbing)
            {
                isClimbing = false;
            }
        }

        if (other.CompareTag("Water"))
        {
            inWater = false;
        }

    }

    void SetClimbVariables()
    {
        speed = climbingSpeed;

        isClimbing = true;
        canRun = false;

        if (controller.height != controllerOriginalHeight)
        {
            controller.height = controllerOriginalHeight;
            controller.center = controllerOriginalCenter;

            capCollider.height = colliderOriginalHeight;
            capCollider.center = colliderOriginalCenter;
        }

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);

    }

    void SetSwimVariables()
    {
        speed = baseSpeed;
        currentGravScale = underwaterGravScale;

        isSwimming = true;
        canJump = true;
        canRun = false;

        if (controller.height != controllerOriginalHeight)
        {
            controller.height = controllerOriginalHeight;
            controller.center = controllerOriginalCenter;

            capCollider.height = colliderOriginalHeight;
            capCollider.center = colliderOriginalCenter;
        }

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
    }

    void SetWalkVariables()
    {
        speed = baseSpeed;
        currentGravScale = gravScale;

        audioSource.volume = Random.Range(walkVolumeRange.x, walkVolumeRange.y);
        audioSource.pitch = Random.Range(walkPitchRange.x, walkPitchRange.y);
        animator.SetBool("isRunning", false);
        canRun = true;
        canJump = true;


        if (controller.height != controllerOriginalHeight)
        {
            controller.height = controllerOriginalHeight;
            controller.center = controllerOriginalCenter;

            capCollider.height = colliderOriginalHeight;
            capCollider.center = colliderOriginalCenter;
        }

        if (stamina <= 0f && !staminaSource.isPlaying)
            staminaSource.Play();
        if (!isClimbing)
            RegenerateStamina();
    }

    void SetRunVariables()
    {
        speed = runSpeed;
        audioSource.pitch = Random.Range(runPitchRange.x, runPitchRange.y);
        audioSource.volume = Random.Range(runVolumeRange.x, runVolumeRange.y);
        animator.SetBool("isRunning", true);

        canJump = true;
    }

    void SetCrouchVariables()
    {
        speed = crouchSpeed;
        controller.height = controllerOriginalHeight / 2;
        controller.center = Vector3.zero;

        capCollider.height = colliderOriginalHeight / 2;
        capCollider.center = new Vector3(0f, colliderOriginalCenter.y / 2, 0f);

        canRun = false;
        canJump = true;

        audioSource.pitch = Random.Range(crouchPitchRange.x, crouchPitchRange.y);
        audioSource.volume = Random.Range(crouchVolumeRange.x, crouchVolumeRange.y);
    }

    void SetProneVariables()
    {
        speed = proneSpeed;
        controller.height = 0.5f;
        controller.center = Vector3.zero;

        capCollider.height = 0.5f;
        capCollider.center = new Vector3(0f, colliderOriginalCenter.y / 2, 0f);

        canRun = false;
        canJump = false;

        audioSource.pitch = Random.Range(crouchPitchRange.x, crouchPitchRange.y);
        audioSource.volume = Random.Range(crouchVolumeRange.x, crouchVolumeRange.y);

    }

    private void RegenerateStamina()
    {
        if (stamina <= maxStamina)
        {
            stamina += Time.deltaTime * staminaIncreaseRate;
        }
    }

    private AudioClip AvoidRepeatedFootstepAudio()
    {
        List<AudioClip> validSounds = new List<AudioClip>();
        validSounds.AddRange(footstepSounds);

        validSounds.Remove(lastFootstep);

        AudioClip footstep = validSounds[Random.Range(0, validSounds.Count)];

        return footstep;

    }

    public float IncreaseHealth(float increaseAmount)
    {
        return currentHealth = Mathf.Clamp(currentHealth + Mathf.Abs(increaseAmount), currentHealth, maxHealth);
    }

    public void StartDecreaseHealth(float decreaseAmount)
    {
        StartCoroutine(DecreaseHealth(decreaseAmount));
    }

    public IEnumerator DecreaseHealth(float decreaseAmount)
    {
        Debug.Log("Taking " + decreaseAmount + " Damage");
        isTakingDamage = true;
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(decreaseAmount), 0, currentHealth);
        yield return new WaitForSeconds(.5f);

        isTakingDamage = false;
    } 
    
    public IEnumerator DecreaseHealthConsistent(float decreaseAmount, float damageTimeIncrement)
    {
        isTakingDamage = true;
        Debug.Log("Taking " + decreaseAmount + " Damage");
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(decreaseAmount), 0, currentHealth);
        
        yield return new WaitForSeconds(damageTimeIncrement);

        isDrowning = false;
        isTakingDamage = false;

    }
}

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    float healthChangeAmount;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerController controller = (PlayerController)target;
        if (controller == null) return;

        healthChangeAmount = EditorGUILayout.FloatField("Health Change Amount: ", healthChangeAmount);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Increase Health"))
        {
            controller.IncreaseHealth(healthChangeAmount);
        }

        if (GUILayout.Button("Decrease Health"))
        {
    
           controller.StartDecreaseHealth(healthChangeAmount);
        }

        GUILayout.EndHorizontal();
    }
}

