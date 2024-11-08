using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [Header("Break Settings")]
    [SerializeField] private float velocityThreshold = 10f;  // Vitesse minimum pour casser la porte
    [SerializeField] private float breakForce = 100f;      // Force pour faire voler la porte
    [SerializeField] private float torqueForce = 1f;      // Force de rotation

    [Header("Normal Door Settings")]
    [SerializeField] private float normalOpenForce = 1f;    // Force pour ouverture normale

    private Rigidbody rb;
    private bool isDoorBroken = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Verrouille tout sauf la rotation Y pour l'ouverture normale
        rb.constraints = RigidbodyConstraints.FreezePosition |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDoorBroken)
        {
            float playerVelocity = collision.relativeVelocity.magnitude;
            Debug.Log($"Vitesse du joueur: {playerVelocity}");

            if (playerVelocity > velocityThreshold)
            {
                Debug.Log("BREAK!");
                BreakDoor(collision);
            }
            else
            {
                // Ouverture normale de la porte
                Vector3 forceDirection = transform.position - collision.contacts[0].point;
                forceDirection = Vector3.ProjectOnPlane(forceDirection, Vector3.up).normalized;
                rb.AddForceAtPosition(forceDirection * normalOpenForce, collision.contacts[0].point, ForceMode.Impulse);
            }
        }
    }

    private void BreakDoor(Collision collision)
    {
        isDoorBroken = true;

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
    }
}