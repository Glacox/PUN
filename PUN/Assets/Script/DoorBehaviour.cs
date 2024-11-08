using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [Header("Break Settings")]
    [SerializeField] private float velocityThreshold = 10f;  // Vitesse minimum pour casser la porte
    [SerializeField] private float breakForce = 100f;      // Force pour faire voler la porte
    [SerializeField] private float torqueForce = 1f;      // Force de rotation

    [Header("Normal Door Settings")]
    [SerializeField] private float normalOpenForce = 1f;    // Force pour ouverture normale
    private static bool hasFirstDoorBeenBroken = false;

    [Header("Animator Settings")]
    [SerializeField] private Animator doorAnimator; // Référence à l'Animator pour contrôler les animations de la porte

    private Rigidbody rb;
    private bool isDoorBroken = false;
    private bool isFirstDoorLocked = false;

    // Variable globale pour indiquer si la porte est ouverte
    public static bool isDoorOpen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialiser l'état de "DoorOpen" dans l'Animator si disponible
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("DoorOpen", isDoorOpen);
        }

        // Vérifie si c'est la première porte
        PlayerMovement playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDoorBroken)
        {
            float playerVelocity = collision.relativeVelocity.magnitude;

            PlayerMovement playerScript = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerScript == null) return;

            if (playerScript != null && playerScript.DoorsCounter == 0)
            {
                // Verrouille complètement la première porte
                rb.constraints = RigidbodyConstraints.FreezeAll;
                isFirstDoorLocked = true;
            }
            else
            {
                // Configuration normale pour les autres portes
                rb.constraints = RigidbodyConstraints.FreezePosition |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;
            }

            if (playerVelocity > velocityThreshold)
            {
                Debug.Log("BREAK!");
                BreakDoor(collision);

                if (playerScript.DoorsCounter == 0)
                {
                    AudioManager.Instance.StartGameplayMusic();
                }

                playerScript.AddDoorCounter(1);
            }
            else if (!isFirstDoorLocked) // N'applique la force normale que si ce n'est pas la première porte
            {
                // Ouverture normale uniquement pour les portes non verrouillées
                Vector3 forceDirection = transform.position - collision.contacts[0].point;
                forceDirection = Vector3.ProjectOnPlane(forceDirection, Vector3.up).normalized;
                rb.AddForceAtPosition(forceDirection * normalOpenForce, collision.contacts[0].point, ForceMode.Impulse);

                // Définir DoorOpen comme ouvert
                SetDoorOpen(true);
            }
        }
    }

    private void BreakDoor(Collision collision)
    {
        isDoorBroken = true;
        isFirstDoorLocked = false;

        // Déverrouille toutes les contraintes
        rb.constraints = RigidbodyConstraints.None;

        // Direction de l'impact
        Vector3 forceDirection = collision.contacts[0].point - collision.gameObject.transform.position;
        forceDirection = forceDirection.normalized;

        // Applique une force explosive
        rb.AddForce(forceDirection * breakForce, ForceMode.Impulse);

        // Ajoute une rotation aléatoire sur tous les axes
        rb.AddTorque(
            Random.Range(-1f, 1f) * torqueForce,
            Random.Range(-1f, 1f) * torqueForce,
            Random.Range(-1f, 1f) * torqueForce,
            ForceMode.Impulse
        );

        // Définir DoorOpen comme ouvert
        SetDoorOpen(true);
    }

    private void SetDoorOpen(bool open)
    {
        isDoorOpen = open;
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("DoorOpen", isDoorOpen); // Met à jour le paramètre "DoorOpen" dans l'Animator
        }
    }
}
