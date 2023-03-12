using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody _playerRigidbody;

    private Animator _rotationAnimator;
    [SerializeField] private Animator _playerAnimator;

    [HideInInspector] public static bool canMove = true;
    [HideInInspector] public bool activateSpeedControl = true;
    private bool canSlide = true;
    private bool isSliding;
    private bool isRunning;
    private bool permaCrouch;

    [Header("Speed Parametres\n")]
    [SerializeField] private float force = 30f;
    [SerializeField] private float groundDrag = 10f;
    [SerializeField] private float airMultiplyer;

    public float horizontalInput;
    public float verticalInput;

    public float x;
    public float y;

    public float lerpSpeed;

    private Vector3 moveDirection;

    [Header("Jump Parametres\n")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float playerHight;

    [SerializeField] private LayerMask whatIsGorund;

    private bool canJump = true;
    [HideInInspector] public bool canSecondJump;
    [HideInInspector] public bool isGrounded = true;

    private Vector3 wallJumpDir;

    private bool canWallJump;

    [Header("Gravity Modifier\n")]
    [SerializeField] private float wallGrav = -1f;
    public float normalGrav = -10f;

    [SerializeField] private AudioSource _sfxAudioSource;
    private AudioSource _playerAudioSource;

    private float oneUnit = 1f;
    private float halfUnit = .5f;
    private float dobleUnit = 2f;
    private float tenthOfUnit = .1f;
    private float normalForceMulti = 10f;
    private float runningForceMulti = 14f;
    private float runningFov = 6f;
    private float wallJumpMulti = .8f;

    void Start()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _rotationAnimator = GetComponent<Animator>();

      

        wallJumpDir = Vector3.forward;

        
    }

    private void Update()
    {
        //Restricting the movement of the player with a boolean
        if (canMove)
        {
            PlayerInput();
            SpeedControl();
            
            if (isGrounded)
            {
                _playerRigidbody.drag = groundDrag;
                Physics.gravity = new Vector3(0, normalGrav, 0);
            }
            else
            {
                _playerRigidbody.drag = oneUnit;
                Physics.gravity = new Vector3(0, normalGrav, 0);
            }
        }
    }

    void FixedUpdate()
    {
        //Restricting the movement of the player with a boolean
        if (canMove)
        {
            MovePlayer();
        }

        //Raycast that checks if the player is touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHight * halfUnit + tenthOfUnit, whatIsGorund);
    }

    private void LateUpdate()
    {
        if (_rotationAnimator.enabled)
        {
            x = Mathf.Lerp(x, horizontalInput, Time.deltaTime * lerpSpeed);
            y = Mathf.Lerp(y, verticalInput, Time.deltaTime * lerpSpeed);

            _rotationAnimator.SetFloat("Horizontal", x);
            _rotationAnimator.SetFloat("Vertical", y);
        }

        if(horizontalInput != 0 || verticalInput != 0)
        {
            _playerAnimator.SetBool("IsRunning", true);
        }
        else
        {
            _playerAnimator.SetBool("IsRunning", false);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            _playerAnimator.SetBool("isJumping", true);
        }

        if(isGrounded && _playerAnimator.GetBool("isJumping"))
        {
            _playerAnimator.SetBool("isJumping", false);
        }

    }

    //All the inputs on whitch the player depends to move arround
    void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Jump if the player is on the ground
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0))
        {
            if (canJump && isGrounded)
            {
                canJump = false;

                JumpMechanic();

                Invoke(nameof(JumpReset), jumpCooldown);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isRunning = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isRunning = false;
        }

        //Player crouching state
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton8))
        {
            if(isGrounded && canSlide)
            {
                canJump = false;
                isSliding = true;
                canSlide = false;
                GetComponent<CapsuleCollider>().height = halfUnit;
                _playerRigidbody.AddForce(Vector3.down * force, ForceMode.Impulse);
            }
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.JoystickButton8))
        {
            if (!permaCrouch)
            {
                GetComponent<CapsuleCollider>().height = dobleUnit;
                canSlide = true;
                isSliding = false;
                canJump = true;
            }
        }
    }

    //All the diferents move speeds of the player depending on if is touching the ground, running, crouching, attatched to a wall, grappling or in free fall
    void MovePlayer()
    {
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if (isGrounded)
        {
            if (isSliding)
            {
                _playerRigidbody.AddForce(moveDirection.normalized * force * dobleUnit, ForceMode.Force);
            }
            else if (isRunning)
            {
                _playerRigidbody.AddForce(moveDirection.normalized * force * runningForceMulti, ForceMode.Force);
            }
            else
            {
                _playerRigidbody.AddForce(moveDirection.normalized * force * normalForceMulti, ForceMode.Force);
            }
        }
        else
        {
            _playerRigidbody.AddForce(moveDirection.normalized * force * (normalForceMulti - dobleUnit) * airMultiplyer, ForceMode.Force);
        }
    }

    //If the player isn't running the players max speed is clamped to a max value
    void SpeedControl()
    {
        if (activateSpeedControl)
        {
            Vector3 flatVel = new Vector3(_playerRigidbody.velocity.x, 0, _playerRigidbody.velocity.z);
            float newForce = force / dobleUnit;

            if (flatVel.magnitude > newForce && !isRunning)
            {
                Vector3 limitedVel = flatVel.normalized * newForce;
                _playerRigidbody.velocity = new Vector3(limitedVel.x, _playerRigidbody.velocity.y, limitedVel.z);
            }
        }
    }

    //When called this function adds an upwards force to the player
    void JumpMechanic()
    {
        _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x, 0f, _playerRigidbody.velocity.z);

        _playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    //When called this function adds an upwards force to the player and a force in the opposite directon of the wall that is collideing
    void WallJumpMechanic()
    {
        _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x, 0f, _playerRigidbody.velocity.z);

        _playerRigidbody.AddForce(wallJumpDir * jumpForce * wallJumpMulti, ForceMode.Impulse);
        _playerRigidbody.AddForce(Vector3.up * jumpForce * wallJumpMulti, ForceMode.Impulse);
    }

    //When called set the ability to jump to true
    void JumpReset()
    {
        canJump = true;
    }

    //Adds a delay to exit the permacrouch state
    IEnumerator ResetPermaCrouch()
    {
        yield return new WaitForSeconds(halfUnit + tenthOfUnit + tenthOfUnit);

        if (!isSliding)
        {
            GetComponent<CapsuleCollider>().height = dobleUnit;
        }
    }
}