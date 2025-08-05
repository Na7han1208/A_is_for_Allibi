using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 3f;
    private float moveSpeed;
    public float gravity = -9.81f;
    private bool isSprinting;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 0.7f;
    public float verticalLookLimit = 90f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    //Pickup System
    private LayerMask layerMask;
    private float pickupInput;
    private bool isHoldingObject = false;

    //Crouch
    private bool isCrouching;
    private float crouchSize = 0.5f;

    //Crawling
    private bool isCrawling;
    private float crawlSize = 0.2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //hides and locks cursor ^

        layerMask = LayerMask.GetMask("Pickupable", "Player");
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleLook();
    }

    // --- Input Reads ---
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    //BUG - this method is run 3 times every key press
    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pickupInput = context.ReadValue<float>();
            Debug.Log("DEBUG: " + pickupInput);
            Debug.Log("DEBUG : HandlePickup Run");
            
            if (!isHoldingObject)
            {
                Debug.Log("DEBUG: Raycast drawn");
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
                {
                    Debug.Log("HIT");
                    isHoldingObject = true;
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                }
                else
                {
                    Debug.Log("MISS");
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                }
            } 
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        Transform playerTransform = this.transform;
        Vector3 currentScale = transform.localScale;

        if (context.performed) //Start crouching
        {
            currentScale.y = crouchSize;
            playerTransform.localScale = currentScale;
            isCrouching = true;
            isSprinting = false;
        }
        else if (context.canceled) //Stop crouching
        {
            currentScale.y = 1;
            playerTransform.localScale = currentScale;
            isCrouching = false;
        }
    }

    public void OnCrawl(InputAction.CallbackContext context)
    {
        Transform playerTransform = this.transform;
        Vector3 currentScale = transform.localScale;

        if (context.performed) //Start crawling
        {
            currentScale.y = crawlSize;
            playerTransform.localScale = currentScale;
            isCrawling = true;
        }
        else if (context.canceled) //Stop crawling
        {
            currentScale.y = 1;
            playerTransform.localScale = currentScale;
            isCrawling = false;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && !isCrouching)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }

    // --- Input Handling ---
    public void HandleMovement()
    {
        if(isSprinting)         {moveSpeed = sprintSpeed;}
        else if(isCrouching)    {moveSpeed = crouchSpeed;}
        else                    {moveSpeed = walkSpeed;}

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}

// ~Andy