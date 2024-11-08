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
    [SerializeField] private float sprintFOV = 100;
    [SerializeField] private float ScopeFOV = 50f;
    [SerializeField] private float FOVTransitionSpeed = 10f;

    private Camera playerCamera;
    private Rigidbody rb;
    private float verticalRotation = 0f;
    public bool isSprinting;

    private float targetFOV;
    private float currentFOV;

    // R�f�rences � l'Animator d'un autre objet
    [SerializeField] private Animator targetAnimator;  // L'Animator de l'objet � contr�ler (ex: un mod�le ou un autre objet)

    // Distance minimale entre la cam�ra et le joueur pour �viter les collisions
    [SerializeField] private float minCameraDistance = 0.5f;
    [SerializeField] private float maxCameraDistance = 2f; // La distance maximale que la cam�ra peut atteindre du joueur

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>(); // La cam�ra attach�e au joueur

        currentFOV = defaultFOV;
        targetFOV = defaultFOV;
        playerCamera.fieldOfView = defaultFOV;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // V�rifier si le joueur est au sol avec Raycast
        CheckGrounded();

        // Rotation horizontale (le joueur tourne autour de l'axe Y)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Rotation verticale pour la cam�ra
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRotation, maxVerticalRotation);

        // Appliquer la rotation verticale � la cam�ra
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Gestion du sprint
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        float baseFOV = isSprinting ? sprintFOV : defaultFOV; // Choisir le FOV en fonction du sprint

        // V�rifier le clic droit pour activer le "scope"
        if (Input.GetMouseButton(1)) // Maintenir le clic droit pour zoomer
        {
            targetAnimator.SetBool("Scope", true); // Active le scope
            targetFOV = ScopeFOV; // R�duit le FOV pour le scope
        }
        else // Rel�chement du clic droit
        {
            targetAnimator.SetBool("Scope", false); // D�sactive le scope
            targetFOV = baseFOV; // R�tablit le FOV par d�faut ou de sprint
        }

        // Transition fluide du FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, FOVTransitionSpeed * Time.deltaTime);
        playerCamera.fieldOfView = currentFOV;

        // Saut
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // Gestion de la distance de la cam�ra par rapport au joueur pour �viter les collisions
        HandleCameraCollisions();

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void FixedUpdate()
    {
        // R�cup�ration des inputs de d�placement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        Vector3 moveDirection = transform.TransformDirection(movement);

        // Applique le multiplicateur de sprint si n�cessaire
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        rb.AddForce(moveDirection * currentSpeed);

        // Mise � jour des param�tres de l'Animator de l'objet cible
        float speed = movement.magnitude * currentSpeed; // La vitesse totale du joueur
        targetAnimator.SetFloat("Speed", speed);           // Param�tre 'Speed' pour l'animation de d�placement
        targetAnimator.SetBool("IsSprinting", isSprinting); // Param�tre 'IsSprinting' pour le sprint

        // Gestion des animations Idle, Walk, Run
        if (movement.magnitude == 0) // Pas de d�placement -> Idle
        {
            targetAnimator.SetBool("Idle", true);
            targetAnimator.SetBool("Walk", false);
            targetAnimator.SetBool("Run", false);
        }
        else if (isSprinting) // Si le joueur sprinte -> Run
        {
            targetAnimator.SetBool("Idle", false);
            targetAnimator.SetBool("Walk", false);
            targetAnimator.SetBool("Run", true);
        }
        else // Si le joueur marche -> Walk
        {
            targetAnimator.SetBool("Idle", false);
            targetAnimator.SetBool("Walk", true);
            targetAnimator.SetBool("Run", false);
        }

        // Si le joueur est au sol et que l'animation de saut est activ�e, on la d�sactive
        if (isGrounded && targetAnimator.GetBool("Jump"))
        {
            targetAnimator.SetBool("Jump", false); // D�sactive l'animation de saut une fois au sol
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // M�thode pour v�rifier si le joueur est au sol avec un Raycast
    private void CheckGrounded()
    {
        RaycastHit hit;
        // Effectuer un raycast vers le bas � partir de la position du joueur
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            Debug.Log($"Grounded on: {hit.collider.gameObject.name}"); // Pour le d�bogage
        }
        else
        {
            isGrounded = false;
        }

        // Met � jour le param�tre 'isGrounded' dans l'Animator
        targetAnimator.SetBool("isGrounded", isGrounded);

        // Ray de debug pour visualiser le ground check
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    // M�thode de gestion du saut
    private void Jump()
    {
        // R�initialiser la v�locit� verticale avant de sauter
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Enlever toute v�locit� verticale
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Appliquer une impulsion de saut
        targetAnimator.SetBool("Jump", true); // D�clencher l'animation de saut
    }

    // G�re la collision de la cam�ra avec le joueur
    private void HandleCameraCollisions()
    {
        Vector3 directionToCamera = playerCamera.transform.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToCamera, out hit, directionToCamera.magnitude))
        {
            // Si on touche quelque chose entre le joueur et la cam�ra
            if (hit.collider.gameObject != playerCamera.gameObject)
            {
                // La cam�ra est trop proche, ajustons sa position
                float distance = Mathf.Clamp(hit.distance, minCameraDistance, maxCameraDistance);
                playerCamera.transform.position = transform.position + directionToCamera.normalized * distance;
            }
        }
        else
        {
            // Si aucune collision n'est d�tect�e, on remet la cam�ra � la distance maximale
            playerCamera.transform.position = transform.position + directionToCamera.normalized * maxCameraDistance;
        }
    }
}
