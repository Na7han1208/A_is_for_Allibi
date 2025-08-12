using System;
using UnityEditor.Callbacks;
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
    public float jumpHeight = 10f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 0.7f;
    public float verticalLookLimit = 90f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    [Header("Pickup System")]
    public float pickupRange = 2f;
    public Transform holdPoint;
    public float pickupSmoothness = 10f;

    private GameObject heldObject;
    private Rigidbody heldRb;

    private LayerMask layerMask;
    private bool isHoldingObject = false;

    private float throwForce = 7f;
    private bool isColliding;

    [Header("Shooting")]
    public GameObject dartPrefab;
    public Transform gunPoint;
    public float muzzleVelocity = 5f;

    //Crouch
    private bool isCrouching;
    private float playerHeight = 2f;
    private float crouchHeight = 1f;

    [Header("Inspect")]
    private bool isInspecting = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //hides and locks cursor ^

        layerMask = LayerMask.GetMask("Pickupable", "Player");
    }

    private void Update()
    {
        if (!isInspecting)
        {
            HandleMovement();
            HandleLook();
        }
        HandlePickup();
        HandleInspect();
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

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isHoldingObject)
            {
                RaycastHit hit;
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, layerMask))
                {
                    if (hit.rigidbody != null)
                    {
                        heldObject = hit.collider.gameObject;
                        heldRb = heldObject.GetComponent<Rigidbody>();
                        heldRb.useGravity = false;

                        heldRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                        heldRb.constraints = RigidbodyConstraints.FreezeRotation;

                        heldObject.transform.position = holdPoint.position;
                        heldObject.transform.SetParent(holdPoint);
                        isHoldingObject = true;
                    }
                }
            }
            else
            {
                //Drop Obj
                heldObject.transform.SetParent(null);
                heldRb.useGravity = true;
                heldRb.collisionDetectionMode = CollisionDetectionMode.Discrete;

                heldRb.constraints = RigidbodyConstraints.None;

                heldObject = null;
                heldRb = null;
                isHoldingObject = false;
            }
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (dartPrefab != null && gunPoint != null && context.performed)
        {
            for (int i = 1; i < 100; i++)
            {
                GameObject dart = Instantiate(dartPrefab, gunPoint.position, gunPoint.rotation);
                Rigidbody rb = dart.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(gunPoint.forward * muzzleVelocity);
                    Destroy(dart, 7);
                }
            }
        }
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed && isHoldingObject)
        {
            heldObject.transform.SetParent(null);
            heldRb.useGravity = true;

            //heldRb.isKinematic = false;
            heldRb.constraints = RigidbodyConstraints.None;

            heldRb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            heldObject = null;
            heldRb = null;
            isHoldingObject = false;
        }
    }

    public void OnInspect(InputAction.CallbackContext context)
    {
        if (context.performed && isHoldingObject)
        {
            isInspecting = !isInspecting;

            if (isInspecting)
            {
                holdPoint.localPosition += new Vector3(0f, 0f, 0.5f);
                heldObject.transform.localScale *= 2f;

                moveInput = Vector2.zero;
                lookInput = Vector2.zero;
            }
            else
            {
                holdPoint.localPosition -= new Vector3(0f, 0f, 0.5f);
                heldObject.transform.localScale /= 2f;
            }
        }
    }

    private void LateUpdate()
    {
        if (!isHoldingObject && heldObject != null)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, holdPoint.position, Time.deltaTime * pickupSmoothness);
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        Transform playerTransform = this.transform;
        Vector3 currentScale = transform.localScale;

        if (context.performed)
        { //Start crouching
            controller.height = crouchHeight;
            playerTransform.localScale = currentScale;

            isCrouching = true;
            isSprinting = false;
        }
        else if (context.canceled)
        { //Stop crouching
            controller.height = playerHeight;
            playerTransform.localScale = currentScale;
            isCrouching = false;
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = (float)Math.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // --- Input Handling ---
    public void HandleMovement()
    {
        if (isSprinting) { moveSpeed = sprintSpeed; }
        else if (isCrouching) { moveSpeed = crouchSpeed; }
        else { moveSpeed = walkSpeed; }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

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

    public void HandlePickup()
    {
        if (isHoldingObject)
        {
            float distance = Vector3.Distance(heldObject.transform.position, holdPoint.position);
            if (distance > 0 && !isColliding)
            {
                Vector3 direction = (holdPoint.position - heldObject.transform.position).normalized;
                heldRb.AddForce(direction * distance, ForceMode.Impulse);
                heldRb.linearVelocity = Vector3.zero;
            }
        }
    }

    public void HandleInspect()
    {
        if (isInspecting && heldObject != null)
        {
            float rotateX = lookInput.y * 100f * Time.deltaTime;
            float rotateY = -lookInput.x * 100f * Time.deltaTime;

            heldObject.transform.Rotate(cameraTransform.up, rotateY, Space.World);
            heldObject.transform.Rotate(cameraTransform.right, rotateX, Space.World);
        }     
    }

    //Checks collision for heldObjects
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isHoldingObject && hit.gameObject.layer == LayerMask.NameToLayer("Pickupable"))
        {
            isColliding = true;
            Debug.Log("Colliding");
        }
        else
        {
            isColliding = false;
        }
    }
}

// ~Andy