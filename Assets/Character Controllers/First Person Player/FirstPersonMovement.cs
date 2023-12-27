//movement from tutorial: https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment

//better jump tutorial: https://www.youtube.com/watch?v=7KiK0Aqtmzc


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
//using Toolbelt_OJ;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    [field:ReadOnlyField, SerializeField] private float moveSpeed = 25f;
    [field:ReadOnlyField, SerializeField] private float currentSpeed = 0f;

    [SerializeField] private float baseSpeed = 25f;
    [SerializeField] private float runSpeed = 40f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    //[SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private bool canRun;
    [SerializeField] private float stamina = 5f;
    private float maxStamina = 5f;
    [SerializeField] private float staminaDecreaseRate = 1.25f, staminaIncreaseRate = 0.75f;
    [SerializeField] private Transform orientation;


    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;
    [SerializeField] private bool canJump = true;

    [Header("Key Bindings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool isGrounded;

    [Header("Audio & UI")]

    [SerializeField] private List<AudioClip> footstepSounds, jumpSounds;
    AudioSource audioSource;
    [SerializeField] private AudioSource staminaSource;

    [SerializeField] private Vector2 walkingPitchRange;
    [SerializeField] private Vector2 runningPitchRange;

    [Header("Animation")]
    [SerializeField] private GameObject model;
    private Animator animator;


    //[SerializeField] private Slider staminaBar;


    private float horizontalInput, verticalInput;

    private Vector3 moveDirection;

    private Rigidbody rb;

    private Vector3 lastPos;

    private AudioClip lastFootstep;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        audioSource = GetComponent<AudioSource>();
        maxStamina = stamina;
        animator = model.GetComponent<Animator>();

        //if (!canRun)
        //{
        //    if(staminaBar != null && staminaBar.gameObject.activeSelf)
        //    {
        //        staminaBar.gameObject.SetActive(false);
        //    }
        //}
        //else
        //{
        //    baseSpeed = moveSpeed;
        //    maxStamina = stamina;
        //    staminaBar.maxValue = maxStamina;
        //}
    }

    private void Update()
    {
        //Check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.5f, whatIsGround);
        animator.SetBool("isGrounded", isGrounded);

        MovementInput();

        //ControlSpeed();

        //Apply drag
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);

        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        currentSpeed = Mathf.Round(rb.velocity.magnitude);

        animator.SetFloat("x", horizontalInput, 0.1f, Time.deltaTime);
        animator.SetFloat("y", verticalInput, 0.1f, Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(jumpKey) && canJump && isGrounded)
        {
            Jump();
            canJump = false;
        }

       
        //Better Jump Physics
        if (rb.velocity.y < 0) // if we are falling...
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            animator.SetBool("isJumping", false);
        }
        //else if (rb.velocity.y > 0 && !Input.GetKey(jumpKey)) // lower jump depending on duration jump key is held
        //{
        //    rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        //    animator.SetBool("isJumping", false);
        //}

        if (isGrounded)
        {
            Invoke(nameof(ResetJump), jumpCooldown);

            if (canRun)
            {
                Sprint();
            }
        }
    }

    void MovePlayer()
    {
        
        // Get forward movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);

        if (currentSpeed > 1f && !audioSource.isPlaying && isGrounded)
        {
            animator.SetBool("isWalking", true);

            audioSource.volume = Random.Range(0.25f, 0.35f);
            audioSource.pitch = Random.Range(walkingPitchRange.x, walkingPitchRange.y);

            AudioClip footstep = AvoidRepeatedFootstepAudio();
            audioSource.PlayOneShot(footstep);

            lastFootstep = footstep;
        }
        else if (currentSpeed <= 0f)
            animator.SetBool("isWalking", false);

    }

    private AudioClip AvoidRepeatedFootstepAudio()
    {
        List<AudioClip> validSounds = new List<AudioClip>();
        validSounds.AddRange(footstepSounds);

        validSounds.Remove(lastFootstep);

        AudioClip footstep = validSounds[Random.Range(0, validSounds.Count)];

        //if (footstep == lastFootstep) footstep = AvoidRepeatedFootstepAudio();

        return footstep;

    }

    //void ControlSpeed()
    //{
    //    Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    //    //limit velocity
    //    if (flatVelocity.magnitude > moveSpeed)
    //    {
    //        Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
    //        rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
    //    }
    //}

    private void Jump() //fix jump sounds
    {
        //reset y velocity
        if (canJump)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);

            //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Count)]);
        }
    }

    private bool ResetJump()
    {
        return canJump = true;
    }

    private void Sprint()
    {

        if (Input.GetKey(runKey) && stamina >= 0f && !staminaSource.isPlaying)
        {

            moveSpeed = runSpeed;
            stamina -= Time.deltaTime * staminaDecreaseRate;
            audioSource.pitch = Random.Range(runningPitchRange.x, runningPitchRange.y);

            animator.SetBool("isRunning", true);


        }
        else
        {
            moveSpeed = baseSpeed;

            animator.SetBool("isRunning", false);

            if (stamina <= 0f && !staminaSource.isPlaying)
                staminaSource.Play();

            RegenerateStamina();
        }


        //staminaBar.value = stamina;
    }

    private void RegenerateStamina()
    {
        if (stamina <= maxStamina)
        {
            stamina += Time.deltaTime * staminaIncreaseRate;
        }
    }
}
