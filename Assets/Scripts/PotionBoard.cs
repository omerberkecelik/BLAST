using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PotionBoard : MonoBehaviour
{
    //define the size of the board
    public int width = 6;
    public int height = 8;

    public int conditionA = 5;   // E.g. any group bigger than 5 potions uses icon A
    public int conditionB = 10;  // bigger than 10 uses icon B
    public int conditionC = 15;  // bigger than 15 uses icon C

    //define some spacing for the board
    public float spacingX; 
    public float spacingY;
    
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;

    //2D array of Nodes on this board
    private Node[,] potionBoard;

    public GameObject potionBoardGo;
    public static PotionBoard Instance;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
{
    potionBoard = new Node[width, height];

    spacingX = (float)(width - 1) / 2;
    spacingY = (float)(height - 1) / 2;

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Vector2 position = new Vector2(x - spacingX, y - spacingY);

            // Randomly select a prefab from the array
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            GameObject potionGO = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);

            // Get the Potion component from the instantiated prefab
            Potion potionComponent = potionGO.GetComponent<Potion>();

            // Set grid indices
            potionComponent.SetIndicies(x, y);

            // The prefab already has its potionType set in the Inspector
            Debug.Log($"Instantiated {potionComponent.potionType} at position ({x}, {y})");

            // Store the potion in the Node
            potionBoard[x, y] = new Node(true, potionGO);
        }
    }
    CheckBoard();

}

    /// <summary>
    /// Finds all clusters of connected potions of the same type and removes
    /// any group with 2 or more potions.
    /// Returns true if any potions were removed.
    /// </summary>
    public bool CheckBoard()
    {
        Debug.Log("Checking Board for matches...");

        // Keep track of which cells we've checked
        bool[,] visited = new bool[width, height];
        // A list to store all groups that qualify as a match
        List<List<Potion>> matchingGroups = new List<List<Potion>>();

        // Traverse the entire board
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // If not visited and there's a potion here
                if (!visited[x, y] && potionBoard[x, y]?.potion != null)
                {
                    // Get the potion at (x, y)
                    Potion startPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                    // Find all connected potions of the same type
                    List<Potion> connectedCluster = GetConnectedCluster(x, y, startPotion.potionType, visited);


                    // If cluster is >= 3, we consider it a match
                    if (connectedCluster.Count >= 3)
                    {
                        matchingGroups.Add(connectedCluster);
                    }

                     UpdateIconsForGroup(connectedCluster);
                }
            }
        }
        // 4. Print out info about each matching group
        if (matchingGroups.Count > 0)
        {
            foreach (List<Potion> group in matchingGroups)
            {
                // Just use the first potion's type for the group's color
                if (group.Count > 0)
                {
                    PotionType groupType = group[0].potionType;
                    int groupSize = group.Count;

                    Debug.Log($"Found a cluster of {groupSize} potions of type {groupType}");
                }
            }

            return true; // We found at least one cluster
        }

        Debug.Log("No matches found.");
        return false; // No cluster of size >= 2 found
    
/* 
        // If we found any matching groups, remove them
        if (matchingGroups.Count > 0)
        {
            foreach (List<Potion> group in matchingGroups)
            {
                foreach (Potion p in group)
                {
                    // Destroy the potion GameObject
                    Destroy(p.gameObject);
                    // Also clear the Node reference
                    potionBoard[p.xIndex, p.yIndex] = null;
                }
            }
            return true; // We removed something
        }

        return false; // No matches found */
    }

    /// <summary>
    /// Performs a BFS (or DFS) to find all potions connected to (startX, startY) 
    /// that share the same type. Returns the full cluster as a List.
    /// </summary>
    private List<Potion> GetConnectedCluster(int startX, int startY, PotionType color, bool[,] visited)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Potion> cluster = new List<Potion>();

        // Mark the start as visited
        visited[startX, startY] = true;
        queue.Enqueue(new Vector2Int(startX, startY));

        // BFS
        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            int x = currentPos.x;
            int y = currentPos.y;

            // The potion at the current position
            Potion currentPotion = potionBoard[x, y].potion.GetComponent<Potion>();
            cluster.Add(currentPotion);

            // Check neighbors (up, down, left, right)
            foreach (Vector2Int neighbor in GetNeighbors(x, y))
            {
                int nx = neighbor.x;
                int ny = neighbor.y;

                // Make sure it's valid and not visited
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                {
                    // If there's a potion and it matches the color
                    if (potionBoard[nx, ny]?.potion != null)
                    {
                        Potion neighborPotion = potionBoard[nx, ny].potion.GetComponent<Potion>();
                        if (neighborPotion.potionType == color)
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        return cluster;
    }

    /// <summary>
    /// Returns a list of the 4 orthogonal neighbors of (x, y).
    /// </summary>
    private List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Up
        neighbors.Add(new Vector2Int(x, y + 1));
        // Down
        neighbors.Add(new Vector2Int(x, y - 1));
        // Left
        neighbors.Add(new Vector2Int(x - 1, y));
        // Right
        neighbors.Add(new Vector2Int(x + 1, y));

        return neighbors;
    }

    // Optional result class if you want to store match data
    public class MatchResul
    {
        public List<Position> connectedPotions;
    }

    private void UpdateIconsForGroup(List<Potion> group)
{
    if (group == null || group.Count == 0) return;

    int groupSize = group.Count;

    // Determine the appropriate icon based on the size
    string iconSuffix = "_Default"; // Default icon
    if (groupSize > conditionA) iconSuffix = "_A";
    if (groupSize > conditionB) iconSuffix = "_B";
    if (groupSize > conditionC) iconSuffix = "_C";

    // Update each potion in the group
    foreach (Potion potion in group)
    {
        // Get the base color of the potion (e.g., "Red", "Blue")
        string baseColor = potion.potionType.ToString().Split('_')[0]; // Extract "Red" from "Red_Default"

        // Find the correct prefab with the new icon
        string newIconName = baseColor + iconSuffix; // e.g., "Red_A"
        GameObject newPrefab = GetPrefabByName(newIconName);

        if (newPrefab != null)
        {
            // Replace the potion's appearance by swapping its prefab
            potion.UpdateAppearance(newPrefab);
        }
        else
        {
            Debug.LogError($"No prefab found for {newIconName}");
        }
    }
}
private GameObject GetPrefabByName(string prefabName)
{
    foreach (GameObject prefab in potionPrefabs)
    {
        if (prefab != null && prefab.name == prefabName)
        {
            return prefab;
        }
    }
    return null; // If no matching prefab name is found
}



}
