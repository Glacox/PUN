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
    [SerializeField] private float groundCheckDistance = 0.2f; // Distance du Raycast pour vérifier si le joueur est au sol

    private bool isGrounded; // Vérifie si le joueur est au sol
    private Camera playerCamera;
    private Rigidbody rb;
    private Transform playerRotation; // Partie qui tourne verticalement (ex. tête)
    private float verticalRotation = 0f;
    public bool isSprinting;

    private float targetFOV;
    private float currentFOV;

    // Références à l'Animator d'un autre objet
    [SerializeField] private Animator targetAnimator;  // L'Animator de l'objet à contrôler (ex: un modèle ou un autre objet)

    // Tête du joueur
    [SerializeField] private Transform headTransform; // Transform de la tête du personnage à suivre par la caméra
    [SerializeField] private float headFollowSpeed = 10f; // Vitesse à laquelle la tête suit la caméra

    // Distance minimale entre la caméra et le joueur pour éviter les collisions
    [SerializeField] private float minCameraDistance = 0.5f;
    [SerializeField] private float maxCameraDistance = 2f; // La distance maximale que la caméra peut atteindre du joueur

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRotation = transform.GetChild(0); // La partie qui fait tourner la tête (ou l'objet Parent)
        playerCamera = GetComponentInChildren<Camera>(); // La caméra attachée à la tête

        currentFOV = defaultFOV;
        targetFOV = defaultFOV;
        playerCamera.fieldOfView = defaultFOV;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Vérifier si le joueur est au sol avec Raycast
        CheckGrounded();

        // Rotation horizontale (le joueur tourne autour de l'axe Y)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Rotation verticale pour la tête (affecte la tête et la caméra)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRotation, maxVerticalRotation);

        // Rotation de la tête indépendamment des animations
        if (headTransform != null)
        {
            headTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Rotation de la tête
        }

        // La caméra suit la tête, mais n'est pas influencée par l'animation
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Garde la caméra liée à la tête

        // Faire suivre la tête du joueur par la caméra de manière fluide
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

        // Gestion de la distance de la caméra par rapport au joueur pour éviter les collisions
        HandleCameraCollisions();
    }

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

    }

    private void FixedUpdate()
    {
        // Récupération des inputs de déplacement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        Vector3 moveDirection = transform.TransformDirection(movement);

        // Applique le multiplicateur de sprint si nécessaire
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        rb.AddForce(moveDirection * currentSpeed);

        // Mise à jour des paramètres de l'Animator de l'objet cible
        float speed = movement.magnitude * currentSpeed; // La vitesse totale du joueur
        targetAnimator.SetFloat("Speed", speed);           // Paramètre 'Speed' pour l'animation de déplacement
        targetAnimator.SetBool("IsSprinting", isSprinting); // Paramètre 'IsSprinting' pour le sprint

        // Gestion des animations Idle, Walk, Run
        if (movement.magnitude == 0) // Pas de déplacement -> Idle
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

        // Si le joueur est au sol et que l'animation de saut est activée, on la désactive
        if (isGrounded && targetAnimator.GetBool("Jump"))
        {
            targetAnimator.SetBool("Jump", false); // Désactive l'animation de saut une fois au sol
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Méthode pour vérifier si le joueur est au sol avec un Raycast
    private void CheckGrounded()
    {
        RaycastHit hit;
        // Effectuer un raycast vers le bas à partir de la position du joueur
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            Debug.Log($"Grounded on: {hit.collider.gameObject.name}"); // Pour le débogage
        }
        else
        {
            isGrounded = false;
        }

        // Ray de debug pour visualiser le ground check
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    // Méthode de gestion du saut
    private void Jump()
    {
        // Réinitialiser la vélocité verticale avant de sauter
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Enlever toute vélocité verticale
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Appliquer une impulsion de saut
        targetAnimator.SetBool("Jump", true); // Déclencher l'animation de saut
    }

    // Gère la collision de la caméra avec le joueur
    private void HandleCameraCollisions()
    {
        Vector3 directionToCamera = playerCamera.transform.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToCamera, out hit, directionToCamera.magnitude))
        {
            // Si on touche quelque chose entre le joueur et la caméra
            if (hit.collider.gameObject != playerCamera.gameObject)
            {
                // La caméra est trop proche, ajustons sa position
                float distance = Mathf.Clamp(hit.distance, minCameraDistance, maxCameraDistance);
                playerCamera.transform.position = transform.position + directionToCamera.normalized * distance;
            }
        }
        else
        {
            // Si aucune collision n'est détectée, on remet la caméra à la distance maximale
            playerCamera.transform.position = transform.position + directionToCamera.normalized * maxCameraDistance;
        }
    }
}
