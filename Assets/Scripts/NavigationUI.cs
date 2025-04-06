using UnityEngine;
using System.Collections.Generic;

public class NavigationUI : MonoBehaviour
{
    public Camera mainCamera;
    public Material pathMaterial;
    public Material startMaterial;
    public Material endMaterial;
    public Material defaultVertexMaterial;
    public Material defaultEdgeMaterial;

    private MapGenerator mapGenerator;
    private Pathfinder pathfinder;

    // 只记录当前高亮的对象，不再存储原始材质
    private List<GameObject> highlightedVertices = new List<GameObject>();
    private List<GameObject> currentPathEdges = new List<GameObject>();

    private int? startVertexIndex = null;
    private int? endVertexIndex = null;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        pathfinder = FindObjectOfType<Pathfinder>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.name.StartsWith("Vertex_"))
                {
                    // Find nearest vertex to clicked point
                    Vector3 clickPos = hit.point;
                    int nearestVertex = FindNearestVertex(clickPos);

                    // Set start or end vertex
                    if (!startVertexIndex.HasValue)
                    {
                        // First click - set start vertex
                        ResetAllHighlights();
                        startVertexIndex = nearestVertex;
                        HighlightVertex(nearestVertex, startMaterial);
                        Debug.Log($"Start vertex set to: {nearestVertex}");
                    }
                    else if (!endVertexIndex.HasValue && nearestVertex != startVertexIndex)
                    {
                        // Second click - set end vertex (must be different from start)
                        endVertexIndex = nearestVertex;
                        HighlightVertex(nearestVertex, endMaterial);
                        Debug.Log($"End vertex set to: {nearestVertex}");

                        // Calculate and display path
                        CalculateAndDisplayPath();
                    }
                    else
                    {
                        // Reset selection on third click
                        ResetAllHighlights();
                        startVertexIndex = nearestVertex;
                        endVertexIndex = null;
                        HighlightVertex(nearestVertex, startMaterial);
                        Debug.Log($"Reset selection. New start vertex: {nearestVertex}");
                    }
                }
            }
        }
    }

    void CalculateAndDisplayPath()
    {
        if (startVertexIndex.HasValue && endVertexIndex.HasValue)
        {
            // Reset previous path highlights
            ResetPathHighlight();

            List<int> path = pathfinder.FindShortestPath(startVertexIndex.Value, endVertexIndex.Value);

            if (path != null && path.Count > 0)
            {
                HighlightPath(path);

                // Calculate total distance
                float totalDistance = 0f;
                List<Vector3> vertices = mapGenerator.GetVertices();
                for (int i = 0; i < path.Count - 1; i++)
                {
                    totalDistance += Vector3.Distance(vertices[path[i]], vertices[path[i + 1]]);
                }

                Debug.Log($"Path found! Distance: {totalDistance.ToString("F2")} units");
            }
            else
            {
                Debug.LogWarning("No valid path found between selected vertices!");
            }
        }
    }

    int FindNearestVertex(Vector3 position)
    {
        List<Vector3> vertices = mapGenerator.GetVertices();
        int nearestIndex = 0;
        float minDistance = Vector3.Distance(position, vertices[0]);

        for (int i = 1; i < vertices.Count; i++)
        {
            float distance = Vector3.Distance(position, vertices[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    void HighlightVertex(int index, Material material)
    {
        GameObject vertex = GameObject.Find("Vertex_" + index);
        if (vertex != null)
        {
            Renderer renderer = vertex.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
                highlightedVertices.Add(vertex);
            }
        }
    }

    void HighlightPath(List<int> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            int from = path[i];
            int to = path[i + 1];

            string edgeName = from < to ? $"Edge_{from}_{to}" : $"Edge_{to}_{from}";
            GameObject edge = GameObject.Find(edgeName);

            if (edge != null)
            {
                Renderer renderer = edge.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = pathMaterial;
                    currentPathEdges.Add(edge);
                }
            }
        }
    }

    void ResetPathHighlight()
    {
        // 恢复路径边的默认材质
        foreach (GameObject edge in currentPathEdges)
        {
            if (edge != null)
            {
                edge.GetComponent<Renderer>().material = defaultEdgeMaterial;
            }
        }
        currentPathEdges.Clear();
    }

    void ResetAllHighlights()
    {
        // 恢复所有高亮顶点的默认材质
        foreach (GameObject vertex in highlightedVertices)
        {
            if (vertex != null)
            {
                vertex.GetComponent<Renderer>().material = defaultVertexMaterial;
            }
        }
        highlightedVertices.Clear();

        // 恢复路径边的默认材质
        foreach (GameObject edge in currentPathEdges)
        {
            if (edge != null)
            {
                edge.GetComponent<Renderer>().material = defaultEdgeMaterial;
            }
        }
        currentPathEdges.Clear();

        // 重置起点和终点
        startVertexIndex = null;
        endVertexIndex = null;
    }
}
