using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int vertexCount = 10000;
    public float mapSize = 100f;
    public int minConnections = 2;
    public int maxConnections = 5;
    public float gridCellSize = 10f;

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
    private List<Edge> allEdges = new List<Edge>();

    void Start()
    {
        GenerateVertices();
        BuildGrid();
        ConnectVerticesWithMST();
        AddAdditionalEdges();
        VisualizeMap();
    }

    struct Edge
    {
        public int from;
        public int to;
        public float length;

        public Edge(int f, int t, float l)
        {
            from = f;
            to = t;
            length = l;
        }
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

    void ConnectVerticesWithMST()
    {
        // Kruskal's algorithm for MST
        List<Edge> edges = new List<Edge>();

        // Generate all possible edges within gridCellSize
        for (int i = 0; i < vertexCount; i++)
        {
            List<int> nearby = GetNearbyVertices(i);
            foreach (int j in nearby)
            {
                if (j > i) // Avoid duplicates
                {
                    float dist = Vector3.Distance(vertices[i], vertices[j]);
                    edges.Add(new Edge(i, j, dist));
                }
            }
        }

        // Sort edges by length
        edges.Sort((a, b) => a.length.CompareTo(b.length));

        // Union-Find data structure
        int[] parent = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++) parent[i] = i;

        int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);
        void Union(int x, int y) => parent[Find(x)] = Find(y);

        // Build MST
        foreach (Edge e in edges)
        {
            if (Find(e.from) != Find(e.to))
            {
                Union(e.from, e.to);
                adjacencyList[e.from].Add(e.to);
                adjacencyList[e.to].Add(e.from);
                allEdges.Add(e);
            }
        }
    }

    void AddAdditionalEdges()
    {
        // Add additional edges while avoiding intersections
        List<Edge> potentialEdges = new List<Edge>();

        // Collect potential edges
        for (int i = 0; i < vertexCount; i++)
        {
            List<int> nearby = GetNearbyVertices(i);
            foreach (int j in nearby)
            {
                if (j > i && !adjacencyList[i].Contains(j))
                {
                    float dist = Vector3.Distance(vertices[i], vertices[j]);
                    potentialEdges.Add(new Edge(i, j, dist));
                }
            }
        }

        // Sort by length (prefer shorter connections)
        potentialEdges.Sort((a, b) => a.length.CompareTo(b.length));

        // Add edges that don't intersect existing ones
        foreach (Edge e in potentialEdges)
        {
            if (adjacencyList[e.from].Count < maxConnections &&
                adjacencyList[e.to].Count < maxConnections)
            {
                if (!DoesEdgeIntersectExisting(e))
                {
                    adjacencyList[e.from].Add(e.to);
                    adjacencyList[e.to].Add(e.from);
                    allEdges.Add(e);
                }
            }
        }
    }

    bool DoesEdgeIntersectExisting(Edge newEdge)
    {
        Vector3 a1 = vertices[newEdge.from];
        Vector3 a2 = vertices[newEdge.to];

        foreach (Edge existingEdge in allEdges)
        {
            Vector3 b1 = vertices[existingEdge.from];
            Vector3 b2 = vertices[existingEdge.to];

            // Skip if edges share a vertex
            if (newEdge.from == existingEdge.from || newEdge.from == existingEdge.to ||
                newEdge.to == existingEdge.from || newEdge.to == existingEdge.to)
            {
                continue;
            }

            if (LineSegmentsIntersect(a1, a2, b1, b2))
            {
                return true;
            }
        }
        return false;
    }

    bool LineSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Check if two line segments intersect in 2D (XZ plane)
        Vector2 a1 = new Vector2(p1.x, p1.z);
        Vector2 a2 = new Vector2(p2.x, p2.z);
        Vector2 b1 = new Vector2(p3.x, p3.z);
        Vector2 b2 = new Vector2(p4.x, p4.z);

        float d1 = Direction(b1, b2, a1);
        float d2 = Direction(b1, b2, a2);
        float d3 = Direction(a1, a2, b1);
        float d4 = Direction(a1, a2, b2);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
        {
            return true;
        }

        return false;
    }

    float Direction(Vector2 pi, Vector2 pj, Vector2 pk)
    {
        return (pk - pi).x * (pj - pi).y - (pj - pi).x * (pk - pi).y;
    }

    List<int> GetNearbyVertices(int index)
    {
        List<int> nearby = new List<int>();
        Vector3 pos = vertices[index];
        Vector2Int centerCoord = GetGridCoordinate(pos);

        // Check current cell and adjacent 8 cells
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
        foreach (Edge e in allEdges)
        {
            GameObject edge = Instantiate(edgePrefab, transform);
            edge.name = $"Edge_{e.from}_{e.to}";

            Vector3 direction = vertices[e.to] - vertices[e.from];
            float distance = direction.magnitude;

            edge.transform.localScale = new Vector3(0.1f, 0.1f, distance);
            edge.transform.rotation = Quaternion.LookRotation(direction);
            edge.transform.position = vertices[e.from] + direction * 0.5f;
        }
    }

    public List<Vector3> GetVertices() => vertices;
    public List<List<int>> GetAdjacencyList() => adjacencyList;
}
