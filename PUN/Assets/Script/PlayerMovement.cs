using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.7f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxVerticalRotation = 80f; // Limite de rotation verticale

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    [Header("FOV Settings")]
    [SerializeField] private float defaultFOV = 65f;
    [SerializeField] private float sprintFOV = 90f;
    [SerializeField] private float FOVTransitionSpeed = 10f;

    private Camera playerCamera;
    private Rigidbody rb;
    private Transform playerRotation; // L'objet qui va pivoter verticalement
    private float verticalRotation = 0f;
    public bool isSprinting;

    private float targetFOV;
    private float currentFOV;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRotation = transform.GetChild(0); // Le GameObject PlayerRotation
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentFOV = defaultFOV;
        targetFOV = defaultFOV;
        playerCamera.fieldOfView = defaultFOV;
    }

    private void Update()
    {
        // Rotation horizontale (tourne tout le joueur)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Rotation verticale (tourne uniquement la partie sup�rieure)
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRotation, maxVerticalRotation);
        playerRotation.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Gestion du sprint
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        targetFOV = isSprinting ? sprintFOV : defaultFOV;

        // Transition fluide du FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, FOVTransitionSpeed * Time.deltaTime);
        playerCamera.fieldOfView = currentFOV;

        CheckGrounded();
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

    }

    private void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        Vector3 moveDirection = transform.TransformDirection(movement);

        // Applique le multiplicateur de sprint si n�cessaire
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        rb.AddForce(moveDirection * currentSpeed);
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheckGrounded()
    {
        RaycastHit hit;
        // Position de d�part l�g�rement sur�lev�e
        Vector3 startPos = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(startPos, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer))
        {
            isGrounded = true;
            Debug.Log($"Grounded on: {hit.collider.gameObject.name}");
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        // Reset la v�locit� Y avant de sauter
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}