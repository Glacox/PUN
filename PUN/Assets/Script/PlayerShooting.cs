using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float shootRange = 50f; // Portée du tir
    [SerializeField] private float shootDamage = 10f; // Dégâts infligés
    [SerializeField] private LayerMask shootableLayers; // Couches d'objets que l'on peut tirer
    [SerializeField] private Camera playerCamera; // La caméra qui détermine la direction du tir

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash; // Effet de flash de bouche (optionnel)
    [SerializeField] private GameObject impactEffect; // Effet d'impact (optionnel)

    private void Update()
    {
        // Déclenchement du tir lors d'un clic gauche
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Optionnel : Jouer l'effet de flash de bouche
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Créer un raycast pour détecter les objets dans la direction de la caméra
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootRange, shootableLayers))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Appliquer des dégâts si l'objet touché a un composant "Health"
            Health target = hit.collider.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(shootDamage);
            }

            // Optionnel : Créer un effet d'impact à l'endroit de la collision
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f); // Détruire l'effet après 2 secondes
            }
        }
    }
}
