using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    [SerializeField] private GameObject[] roomPrefabs; // Assignez room1 et room2 ici
    [SerializeField] private GameObject roomEndPrefab; // La salle finale fixe
    [SerializeField] private int numberOfRooms = 5;

    private void Start()
    {
        GenerateRooms();
    }

    private void GenerateRooms()
    {
        // Position initiale de la première salle
        float startX = 35f;
        float startY = 0f;
        float startZ = 1f;
        float roomOffset = 55f; // Distance entre chaque salle

        // Génère d'abord les salles aléatoires
        for (int i = 0; i < numberOfRooms; i++)
        {
            float xPosition = startX + (i * roomOffset);
            Vector3 roomPosition = new Vector3(xPosition, startY, startZ);

            // Sélectionne aléatoirement une salle parmi les prefabs
            int randomRoomIndex = Random.Range(0, roomPrefabs.Length);
            GameObject roomPrefab = roomPrefabs[randomRoomIndex];

            // Instancie la salle à la position calculée
            GameObject room = Instantiate(roomPrefab, roomPosition, Quaternion.identity);
            room.transform.parent = transform;
            room.name = $"Room_{i + 1}";
        }

        // Ajoute la salle finale après les salles aléatoires
        Vector3 endRoomPosition = new Vector3(startX + (numberOfRooms * roomOffset), startY, startZ);
        GameObject endRoom = Instantiate(roomEndPrefab, endRoomPosition, Quaternion.identity);
        endRoom.transform.parent = transform;
        endRoom.name = "Room_End";
    }

    // Fonction utilitaire pour réinitialiser la génération
    public void RegenerateRooms()
    {
        // Détruit toutes les salles existantes
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Regénère de nouvelles salles
        GenerateRooms();
    }
}