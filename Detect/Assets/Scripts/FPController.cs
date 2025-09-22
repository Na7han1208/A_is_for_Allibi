using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FPController : MonoBehaviour
{
    //---------------- Variables ----------------

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 3f;
    private float moveSpeed;
    public float gravity = -9.81f;
    private bool isSprinting = false;

    public float jumpHeight = 10f;
    public float jumpGravityMultiplier = 5f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.5f;
    public float controllerSensitivity = 2f;
    public float verticalLookLimit = 90f;
    public bool usingGamepad;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    public float verticalRotation = 0f;

    [Header("Pickup System")]
    public Transform holdPoint;
    public float pickupStrength = 200f;
    public float pickupDamping = 20f;
    public float pickupRange = 2f;

    public GameObject heldObject;
    public Rigidbody heldRb;

    private LayerMask layerMask;
    private bool isHoldingObject = false;

    public float throwForce = 7f;
    private bool isColliding;

    public GameObject Foxy;
    private bool FoxyDialogueDone = false;

    [Header("PickupHighlight")]
    private GameObject currentHighlighted;
    private Material originalMaterial;
    private Material outlineMateial;

    public float outlineWidth;
    private bool isHighlighting;
    public Color outlineColor = Color.white;

    [Header("Shooting")]
    public GameObject dartPrefab;
    public Transform gunPoint;
    public float muzzleVelocity = 5f;

    [Header("Crouch")]
    private bool isCrouching;
    private float playerHeight = 2f;
    private float crouchHeight = 1f;

    public Volume postProcessVolume;
    private UnityEngine.Rendering.Universal.Vignette vignette;
    public float crouchVignetteIntensity = 0.4f;
    public float normalVignetteIntensity = 0.2f;
    private Coroutine vignetteRoutine;


    [Header("Inspect")]
    public bool isInspecting = false;
    public float inspectSizeMult = 3f;

    [SerializeField] ParticleSystem successParticles;

    [Header("PickupOutline")]
    public Material outlineMaterial;
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    private bool puzzleActive = false;


    //---------------- Unity Events ----------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //hides and locks cursor ^

        layerMask = LayerMask.GetMask("Pickupable");
    }

    private void Start()
    {
        //SoundManager.Instance.PlayComplex("MainMusic", this.transform);
        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out vignette);
    }

    private void Update()
    {
        Debug.Log(FindFirstObjectByType<PlayerInput>().currentActionMap);

        if (!isInspecting && !puzzleActive)
        {
            HandleMovement();
            HandleLook();
        }
        HandleInspect();
        HandleHighlight();
    }

    private void FixedUpdate()
    {
        HandlePickup();
    }

    // ---------------- Input Actions ----------------
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();

        if (context.control.device is Gamepad)
        {
            usingGamepad = true;
        }
        else if (context.control.device is Mouse)
        {
            usingGamepad = false;
        }
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, layerMask))
            {
                ClearHighlight();
                
                // case: combo lock
                CombinationLock lockUI = hit.collider.GetComponentInChildren<CombinationLock>();
                if (lockUI != null)
                {
                    lockUI.ShowPuzzle();
                    Debug.Log("Opened combo lock UI");

                    moveInput = Vector2.zero;
                    lookInput = Vector2.zero;
                    return;
                }

                // case: tracing puzzle
                TracingPuzzle tracingPuzzle = hit.collider.GetComponent<TracingPuzzle>();
                if (tracingPuzzle != null)
                {
                    tracingPuzzle.ShowPuzzle();
                    moveInput = Vector2.zero;
                    lookInput = Vector2.zero;

                    return;
                }

                // case: music box puzzle
                MusicBoxPuzzle musicBoxPuzzle = hit.collider.GetComponentInChildren<MusicBoxPuzzle>();
                if (musicBoxPuzzle != null)
                {
                    musicBoxPuzzle.ShowPuzzle();
                    moveInput = Vector2.zero;
                    lookInput = Vector2.zero;

                    heldObject = musicBoxPuzzle.transform.parent.gameObject;
                    HintManager.Instance.TriggerPickupDialogue(heldObject);
                    heldObject = null;

                    return;
                }

                // case: foxy
                if (hit.collider.CompareTag("Foxy") && !FoxyDialogueDone)
                {
                    Debug.Log("FOXY TALKS");
                    SoundManager.Instance.PlayComplex("FoxyDialogue", Foxy.transform);

                    FindFirstObjectByType<TutorialHelper>().pickedUp = true;
                    FindFirstObjectByType<TutorialHelper>().ToggleInteraction(false);

                    hit.collider.gameObject.layer = 0;
                    heldObject = null;
                    FoxyDialogueDone = true;
                    return;
                }

                // case: normal pickup
                if (!isHoldingObject && hit.rigidbody != null)
                {
                    heldObject = hit.collider.gameObject;
                    heldRb = heldObject.GetComponent<Rigidbody>();
                    heldRb.useGravity = false;

                    heldRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    heldRb.constraints = RigidbodyConstraints.FreezeRotation;
                    heldRb.interpolation = RigidbodyInterpolation.Interpolate;

                    heldObject.layer = LayerMask.NameToLayer("HeldObject"); //this doesnt collide with player layer

                    isHoldingObject = true;

                    HintManager.Instance.TriggerPickupDialogue(heldObject);
                }
            }
            else if (isHoldingObject)
            {
                DropObject();
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

    public void DropObject()
    {
        if (heldObject == null || isInspecting) return;

        heldRb.useGravity = true;
        heldRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        heldRb.interpolation = RigidbodyInterpolation.None;
        heldRb.constraints = RigidbodyConstraints.None;
        heldObject.layer = LayerMask.NameToLayer("Pickupable");

        heldObject = null;
        heldRb = null;
        isHoldingObject = false;
        isInspecting = false;
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed && isHoldingObject && !isInspecting)
        {
            heldObject.transform.SetParent(null);
            heldRb.useGravity = true;

            //heldRb.isKinematic = false;
            heldRb.constraints = RigidbodyConstraints.None;
            heldRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            heldRb.interpolation = RigidbodyInterpolation.None;
            heldObject.layer = LayerMask.NameToLayer("Pickupable");

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
                heldObject.transform.localScale *= inspectSizeMult;

                heldRb.isKinematic = true;

                moveInput = Vector2.zero;
                lookInput = Vector2.zero;
            }
            else
            {
                heldRb.isKinematic = false;
                holdPoint.localPosition -= new Vector3(0f, 0f, 0.5f);
                heldObject.transform.localScale /= inspectSizeMult;
            }
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

            StartVignetteLerp(crouchVignetteIntensity);
            //vignette.intensity.value = crouchVignetteIntensity;
        }
        else if (context.canceled)
        { //Stop crouching
            controller.height = playerHeight;
            playerTransform.localScale = currentScale;
            isCrouching = false;

            StartVignetteLerp(normalVignetteIntensity);
            //vignette.intensity.value = normalVignetteIntensity;
        }
    }

    private void StartVignetteLerp(float target)
    {
        if (vignetteRoutine != null)
        {
            StopCoroutine(vignetteRoutine);
        }
        vignetteRoutine = StartCoroutine(LerpVignette(target));
    }

    private System.Collections.IEnumerator LerpVignette(float target)
    {
        float start = vignette.intensity.value;
        float t = 0f;

        while (t < 0.7f)
        {
            t += Time.deltaTime * 4;
            vignette.intensity.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        //vignette.intensity.value = target;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        /*
        if (context.performed && !isCrouching)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
        */
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = (float)Math.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnQuit(InputAction.CallbackContext context)
    {
        Application.Quit(0);
    }

    // ---------------- Logic ----------------
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

        if (!controller.isGrounded)
        {
            velocity.y += gravity * jumpGravityMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleLook()
    {
        if (puzzleActive) return;

        float sensitivity = usingGamepad ? controllerSensitivity*50f : mouseSensitivity;

        float mouseX;
        float mouseY;

        if (usingGamepad)
        {
            mouseX = lookInput.x * sensitivity * Time.deltaTime;
            mouseY = lookInput.y * sensitivity * Time.deltaTime;
        }
        else
        {
            mouseX = lookInput.x * sensitivity; // no deltaTime for mouse
            mouseY = lookInput.y * sensitivity;
        }

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void HandleInspect()
    {
        float sensitivity = usingGamepad ? controllerSensitivity : mouseSensitivity;
        if (isInspecting && heldObject != null)
        {
            float rotateX = lookInput.y * 120f * sensitivity * Time.deltaTime;
            float rotateY = -lookInput.x * 120f * sensitivity * Time.deltaTime;

            heldObject.transform.Rotate(cameraTransform.up, rotateY, Space.World);
            heldObject.transform.Rotate(cameraTransform.right, rotateX, Space.World);
        }
    }

    public void HandlePickup()
    {
        if (isHoldingObject && heldObject != null)
        {
            Vector3 toTarget = holdPoint.position - heldRb.position;
            Vector3 force = toTarget * pickupStrength - heldRb.linearVelocity * pickupDamping;
            heldRb.AddForce(force * Time.fixedDeltaTime, ForceMode.VelocityChange);
            if (Vector3.Distance(heldObject.transform.position, holdPoint.transform.position) > 5)
            {
                heldObject.transform.position = holdPoint.transform.position;
            } //if object gets too far away tp it back into hold point
        }
    }

    public void HandleHighlight()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, layerMask))
        {
            GameObject target = hit.collider.gameObject;

            // if we are looking at a new object
            if (currentHighlighted != target)
            {
                ClearHighlight();
                currentHighlighted = target;

                if (!originalMaterials.ContainsKey(currentHighlighted)) // store the original materials
                    originalMaterials[currentHighlighted] = currentHighlighted.GetComponent<Renderer>().materials;

                //if (!originalScales.ContainsKey(currentHighlighted)) // store the original sizes
                //    originalScales[currentHighlighted] = currentHighlighted.transform.localScale;

                //currentHighlighted.transform.localScale = originalScales[currentHighlighted] * 0.92f;

                // apply outline material
                var mats = new Material[originalMaterials[currentHighlighted].Length + 1];
                for (int i = 0; i < originalMaterials[currentHighlighted].Length; i++)
                    mats[i] = originalMaterials[currentHighlighted][i];

                mats[mats.Length - 1] = outlineMaterial;

                currentHighlighted.GetComponent<Renderer>().materials = mats;
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    private void ClearHighlight()
    {
        if (currentHighlighted != null)
        {
            if (originalMaterials != null && originalMaterials.ContainsKey(currentHighlighted)) // restore materials
                currentHighlighted.GetComponent<Renderer>().materials = originalMaterials[currentHighlighted];

            //if (originalScales != null && originalScales.ContainsKey(currentHighlighted)) // restore scale
            //    currentHighlighted.transform.localScale = originalScales[currentHighlighted];

            currentHighlighted = null;
        }
    }

    //Checks collision for heldObjects 
    // **NOTE PART OF FIX NOT YET COMPLETE
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isHoldingObject && hit.gameObject.layer == LayerMask.NameToLayer("Pickupable"))
        {
            isColliding = true;
            //Debug.Log("Colliding");
        }
        else
        {
            isColliding = false;
        }
    }

    public void PlaySuccessParticles()
    {
        successParticles.Play();
    }

    public void SetPuzzleActive(bool active)
    {
        puzzleActive = active;
    }
}

/*
    _
.__(.)<
 \___)

*/