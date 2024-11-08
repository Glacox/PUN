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
    [SerializeField] private float FOVTransitionSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;  // La force du saut
    [SerializeField] private LayerMask groundLayer; // Le Layer des surfaces solides sur lesquelles le joueur peut sauter
    [SerializeField] private float groundCheckDistance = 0.2f; // Distance du Raycast pour v�rifier si le joueur est au sol

    private bool isGrounded; // V�rifie si le joueur est au sol
    private Camera playerCamera;
    private Rigidbody rb;
    private Transform playerRotation; // Partie qui tourne verticalement (ex. t�te)
    private float verticalRotation = 0f;
    public bool isSprinting;

    private float targetFOV;
    private float currentFOV;

    // R�f�rences � l'Animator d'un autre objet
    [SerializeField] private Animator targetAnimator;  // L'Animator de l'objet � contr�ler (ex: un mod�le ou un autre objet)

    // T�te du joueur
    [SerializeField] private Transform headTransform; // Transform de la t�te du personnage � suivre par la cam�ra
    [SerializeField] private float headFollowSpeed = 10f; // Vitesse � laquelle la t�te suit la cam�ra

    // Distance minimale entre la cam�ra et le joueur pour �viter les collisions
    [SerializeField] private float minCameraDistance = 0.5f;
    [SerializeField] private float maxCameraDistance = 2f; // La distance maximale que la cam�ra peut atteindre du joueur

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRotation = transform.GetChild(0); // La partie qui fait tourner la t�te (ou l'objet Parent)
        playerCamera = GetComponentInChildren<Camera>(); // La cam�ra attach�e � la t�te

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

        // Rotation verticale pour la t�te (affecte la t�te et la cam�ra)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRotation, maxVerticalRotation);

        // Rotation de la t�te ind�pendamment des animations
        if (headTransform != null)
        {
            headTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Rotation de la t�te
        }

        // La cam�ra suit la t�te, mais n'est pas influenc�e par l'animation
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Garde la cam�ra li�e � la t�te

        // Faire suivre la t�te du joueur par la cam�ra de mani�re fluide
        if (headTransform != null)
        {
            Vector3 targetHeadPosition = playerCamera.transform.position;
            headTransform.position = Vector3.Lerp(headTransform.position, targetHeadPosition, headFollowSpeed * Time.deltaTime);
        }

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

        // Gestion de la distance de la cam�ra par rapport au joueur pour �viter les collisions
        HandleCameraCollisions();
    }

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
