using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int vertexCount = 10000;
    public float mapSize = 100f;
    [Tooltip("Minimum connections per vertex (recommend 1 for MST)")]
    public int minConnections = 1; // 改为1确保MST基础连通性
    public int maxConnections = 4;
    public float gridCellSize = 5f;

    [Header("Optimization")]
    [Tooltip("Prevent edge intersections (performance heavy)")]
    public bool preventIntersections = true;
    [Tooltip("Max attempts per vertex for non-intersecting edges")]
    public int maxAttempts = 3;

    [Header("Visualization")]
    public bool showGridColors = true;
    public Color gridColorA = Color.white;
    public Color gridColorB = Color.gray;

    [Header("Prefabs")]
    public GameObject vertexPrefab;
    public GameObject edgePrefab;

    private List<Vector3> vertices;
    private List<List<int>> adjacencyList;
    private Dictionary<Vector2Int, List<int>> grid;
    private List<Edge> allEdges;
    private int gridSizeX, gridSizeZ;

    void Start()
    {
        vertices = new List<Vector3>(vertexCount);
        adjacencyList = new List<List<int>>(vertexCount);
        grid = new Dictionary<Vector2Int, List<int>>();
        allEdges = new List<Edge>();

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
    }

    void GenerateVertices()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            vertices.Add(new Vector3(
                Random.Range(-mapSize, mapSize),
                0,
                Random.Range(-mapSize, mapSize)
            ));
            adjacencyList.Add(new List<int>(maxConnections));
        }
    }

    void BuildGrid()
    {
        gridSizeX = Mathf.CeilToInt(2 * mapSize / gridCellSize);
        gridSizeZ = gridSizeX;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2Int coord = GetGridCoordinate(vertices[i]);
            if (!grid.TryGetValue(coord, out List<int> cell))
            {
                cell = new List<int>(10); // 预分配小容量
                grid[coord] = cell;
            }
            cell.Add(i);
        }
    }

    Vector2Int GetGridCoordinate(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((pos.x + mapSize) / gridCellSize),
            Mathf.FloorToInt((pos.z + mapSize) / gridCellSize)
        );
    }

    void ConnectVerticesWithMST()
    {
        // 1. 收集所有可能的边（网格加速）
        List<Edge> edges = new List<Edge>(vertexCount * 3);
        HashSet<long> edgeHashes = new HashSet<long>();

        for (int i = 0; i < vertexCount; i++)
        {
            foreach (int j in GetNearbyVertices(i))
            {
                if (j > i)
                {
                    long hash = ((long)Mathf.Min(i, j) << 32) | (long)Mathf.Max(i, j);
                    if (edgeHashes.Add(hash)) // 避免重复
                    {
                        edges.Add(new Edge
                        {
                            from = i,
                            to = j,
                            length = Vector3.Distance(vertices[i], vertices[j])
                        });
                    }
                }
            }
        }

        // 2. Kruskal算法（优化版）
        edges.Sort((a, b) => a.length.CompareTo(b.length));

        int[] parent = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++) parent[i] = i;

        int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);
        void Union(int x, int y) => parent[Find(x)] = Find(y);

        int connectedComponents = vertexCount;
        foreach (var e in edges)
        {
            if (Find(e.from) != Find(e.to))
            {
                Union(e.from, e.to);
                AddEdge(e.from, e.to);
                connectedComponents--;

                if (connectedComponents == 1) break; // 提前退出
            }
        }
    }

    void AddAdditionalEdges()
    {
        // 按随机顺序尝试添加边，避免模式化
        int[] vertexOrder = Enumerable.Range(0, vertexCount).ToArray();
        Shuffle(vertexOrder);

        foreach (int i in vertexOrder)
        {
            if (adjacencyList[i].Count >= maxConnections) continue;

            List<int> candidates = GetNearbyVertices(i)
                .Where(j => j != i &&
                    adjacencyList[j].Count < maxConnections &&
                    !adjacencyList[i].Contains(j))
                .ToList();

            int attempts = 0;
            while (attempts < maxAttempts && candidates.Count > 0 &&
                  adjacencyList[i].Count < maxConnections)
            {
                int randomIndex = Random.Range(0, candidates.Count);
                int j = candidates[randomIndex];
                candidates.RemoveAt(randomIndex);

                Edge newEdge = new Edge { from = i, to = j };
                if (!preventIntersections || !DoesEdgeIntersectExisting(newEdge))
                {
                    AddEdge(i, j);
                    break;
                }
                attempts++;
            }
        }
    }

    void AddEdge(int a, int b)
    {
        // 确保 from < to
        int from = Mathf.Min(a, b);
        int to = Mathf.Max(a, b);

        // 检查是否已存在该边（避免重复）
        if (!adjacencyList[from].Contains(to))
        {
            adjacencyList[from].Add(to);
            adjacencyList[to].Add(from);
            allEdges.Add(new Edge
            {
                from = from,
                to = to,
                length = Vector3.Distance(vertices[from], vertices[to])
            });
        }
    }


    bool DoesEdgeIntersectExisting(Edge newEdge)
    {
        Vector3 a1 = vertices[newEdge.from];
        Vector3 a2 = vertices[newEdge.to];

        // 只检查可能相交的网格区域
        Vector2Int minGrid = GetGridCoordinate(new Vector3(
            Mathf.Min(a1.x, a2.x) - gridCellSize, 0,
            Mathf.Min(a1.z, a2.z) - gridCellSize));
        Vector2Int maxGrid = GetGridCoordinate(new Vector3(
            Mathf.Max(a1.x, a2.x) + gridCellSize, 0,
            Mathf.Max(a1.z, a2.z) + gridCellSize));

        for (int x = minGrid.x; x <= maxGrid.x; x++)
        {
            for (int z = minGrid.y; z <= maxGrid.y; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                if (grid.TryGetValue(coord, out List<int> cellVertices))
                {
                    foreach (int v in cellVertices)
                    {
                        foreach (int u in adjacencyList[v])
                        {
                            if (u > v) // 每条边只检查一次
                            {
                                Vector3 b1 = vertices[v];
                                Vector3 b2 = vertices[u];

                                if (LineSegmentsIntersect(a1, a2, b1, b2))
                                    return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    List<int> GetNearbyVertices(int index)
    {
        List<int> result = new List<int>(8);
        Vector2Int center = GetGridCoordinate(vertices[index]);

        for (int x = Mathf.Max(0, center.x - 1); x <= Mathf.Min(gridSizeX - 1, center.x + 1); x++)
        {
            for (int z = Mathf.Max(0, center.y - 1); z <= Mathf.Min(gridSizeZ - 1, center.y + 1); z++)
            {
                if (grid.TryGetValue(new Vector2Int(x, z), out List<int> cell))
                {
                    foreach (int i in cell)
                    {
                        if (i != index && Vector3.Distance(vertices[index], vertices[i]) <= gridCellSize)
                            result.Add(i);
                    }
                }
            }
        }
        return result;
    }

    // 辅助函数
    void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    bool LineSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector2 a1 = new Vector2(p1.x, p1.z);
        Vector2 a2 = new Vector2(p2.x, p2.z);
        Vector2 b1 = new Vector2(p3.x, p3.z);
        Vector2 b2 = new Vector2(p4.x, p4.z);

        float d1 = (b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x);
        float d2 = (b2.x - b1.x) * (a2.y - b1.y) - (b2.y - b1.y) * (a2.x - b1.x);
        if (d1 * d2 >= 0) return false;

        float d3 = (a2.x - a1.x) * (b1.y - a1.y) - (a2.y - a1.y) * (b1.x - a1.x);
        float d4 = (a2.x - a1.x) * (b2.y - a1.y) - (a2.y - a1.y) * (b2.x - a1.x);
        return d3 * d4 < 0;
    }

    void VisualizeMap()
    {
        // 顶点实例化
        for (int i = 0; i < vertexCount; i++)
        {
            GameObject vertex = Instantiate(vertexPrefab, vertices[i], Quaternion.identity, transform);
            vertex.name = $"Vertex_{i}";

            if (showGridColors)
            {
                var coord = GetGridCoordinate(vertices[i]);
                vertex.GetComponent<Renderer>().material.color =
                    (coord.x + coord.y) % 2 == 0 ? gridColorA : gridColorB;
            }
        }

        // 边实例化
        foreach (Edge e in allEdges)
        {
            GameObject edge = Instantiate(edgePrefab, transform);
            edge.name = $"Edge_{e.from}_{e.to}";

            Vector3 dir = vertices[e.to] - vertices[e.from];
            edge.transform.position = vertices[e.from] + dir * 0.5f;
            edge.transform.rotation = Quaternion.LookRotation(dir);
            edge.transform.localScale = new Vector3(0.1f, 0.1f, dir.magnitude);
        }
    }

    public List<Vector3> GetVertices() => vertices;
    public List<List<int>> GetAdjacencyList() => adjacencyList;
}
