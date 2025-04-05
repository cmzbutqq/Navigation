using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int vertexCount = 10000;
    public float mapSize = 100f;
    public int minConnections = 2;
    public int maxConnections = 5;
    public float gridCellSize = 10f; // 替换原来的connectionRadius

    [Header("Visualization Settings")]
    public bool showGridColors = true;
    public Color gridColorA = Color.white;
    public Color gridColorB = Color.gray;

    [Header("Prefabs")]
    public GameObject vertexPrefab;
    public GameObject edgePrefab;

    private List<Vector3> vertices = new List<Vector3>();
    private List<List<int>> adjacencyList = new List<List<int>>();
    private Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();

    void Start()
    {
        GenerateVertices();
        BuildGrid();
        ConnectVertices();
        VisualizeMap();
    }

    void GenerateVertices()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-mapSize, mapSize),
                0,
                Random.Range(-mapSize, mapSize)
            );
            vertices.Add(position);
            adjacencyList.Add(new List<int>());
        }
    }

    void BuildGrid()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2Int gridCoord = GetGridCoordinate(vertices[i]);
            if (!grid.ContainsKey(gridCoord))
            {
                grid[gridCoord] = new List<int>();
            }
            grid[gridCoord].Add(i);
        }
    }

    Vector2Int GetGridCoordinate(Vector3 position)
    {
        int x = Mathf.FloorToInt((position.x + mapSize) / gridCellSize);
        int z = Mathf.FloorToInt((position.z + mapSize) / gridCellSize);
        return new Vector2Int(x, z);
    }

    void ConnectVertices()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            int connections = Random.Range(minConnections, maxConnections + 1);
            connections = Mathf.Min(connections, vertexCount - 1);

            List<int> nearbyIndices = GetNearbyVertices(i);

            while (adjacencyList[i].Count < connections && nearbyIndices.Count > 0)
            {
                int randomIndex = Random.Range(0, nearbyIndices.Count);
                int j = nearbyIndices[randomIndex];

                if (!adjacencyList[i].Contains(j) && i != j)
                {
                    adjacencyList[i].Add(j);
                    adjacencyList[j].Add(i);
                }

                nearbyIndices.RemoveAt(randomIndex);
            }
        }
    }

    List<int> GetNearbyVertices(int index)
    {
        List<int> nearby = new List<int>();
        Vector3 pos = vertices[index];
        Vector2Int centerCoord = GetGridCoordinate(pos);

        // 检查当前网格和相邻的8个网格
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int checkCoord = centerCoord + new Vector2Int(x, y);
                if (grid.TryGetValue(checkCoord, out List<int> cellVertices))
                {
                    foreach (int i in cellVertices)
                    {
                        if (i != index && Vector3.Distance(pos, vertices[i]) <= gridCellSize)
                        {
                            nearby.Add(i);
                        }
                    }
                }
            }
        }

        return nearby;
    }

    void VisualizeMap()
    {
        // Create vertex objects
        for (int i = 0; i < vertexCount; i++)
        {
            GameObject vertex = Instantiate(vertexPrefab, vertices[i], Quaternion.identity, transform);
            vertex.name = "Vertex_" + i;

            if (showGridColors)
            {
                Vector2Int gridCoord = GetGridCoordinate(vertices[i]);
                bool isAlternate = (gridCoord.x + gridCoord.y) % 2 == 0;
                vertex.GetComponent<Renderer>().material.color = isAlternate ? gridColorA : gridColorB;
            }
        }

        // Create edge objects
        for (int i = 0; i < vertexCount; i++)
        {
            foreach (int j in adjacencyList[i])
            {
                if (j > i) // To avoid duplicate edges
                {
                    GameObject edge = Instantiate(edgePrefab, transform);
                    edge.name = $"Edge_{i}_{j}";

                    // Scale and rotate the edge to connect the vertices
                    Vector3 direction = vertices[j] - vertices[i];
                    float distance = direction.magnitude;

                    edge.transform.localScale = new Vector3(0.1f, 0.1f, distance);
                    edge.transform.rotation = Quaternion.LookRotation(direction);
                    edge.transform.position = vertices[i] + direction * 0.5f;
                }
            }
        }
    }

    public List<Vector3> GetVertices() => vertices;
    public List<List<int>> GetAdjacencyList() => adjacencyList;
}
