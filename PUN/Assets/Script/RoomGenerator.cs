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
        // Position initiale de la premi�re salle
        float startX = 35f;
        float startY = 0f;
        float startZ = 1f;
        float roomOffset = 55f; // Distance entre chaque salle

        // G�n�re d'abord les salles al�atoires
        for (int i = 0; i < numberOfRooms; i++)
        {
            float xPosition = startX + (i * roomOffset);
            Vector3 roomPosition = new Vector3(xPosition, startY, startZ);

            // S�lectionne al�atoirement une salle parmi les prefabs
            int randomRoomIndex = Random.Range(0, roomPrefabs.Length);
            GameObject roomPrefab = roomPrefabs[randomRoomIndex];

            // Instancie la salle � la position calcul�e
            GameObject room = Instantiate(roomPrefab, roomPosition, Quaternion.identity);
            room.transform.parent = transform;
            room.name = $"Room_{i + 1}";
        }

        // Ajoute la salle finale apr�s les salles al�atoires
        Vector3 endRoomPosition = new Vector3(startX + (numberOfRooms * roomOffset), startY, startZ);
        GameObject endRoom = Instantiate(roomEndPrefab, endRoomPosition, Quaternion.identity);
        endRoom.transform.parent = transform;
        endRoom.name = "Room_End";
    }

    // Fonction utilitaire pour r�initialiser la g�n�ration
    public void RegenerateRooms()
    {
        // D�truit toutes les salles existantes
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Reg�n�re de nouvelles salles
        GenerateRooms();
    }
}