using UnityEngine;
using System.Collections.Generic;

public class NavigationUI : MonoBehaviour
{
    public Camera mainCamera;
    public Material pathMaterial;
    public Material startMaterial;
    public Material endMaterial;
    public Material highlightMaterial;

    private MapGenerator mapGenerator;
    private Pathfinder pathfinder;
    private List<GameObject> highlightedObjects = new List<GameObject>();

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
                        ClearHighlights();
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
                highlightedObjects.Add(vertex);
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
                    highlightedObjects.Add(edge);
                }
            }
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject obj in highlightedObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (obj.name.StartsWith("Vertex_"))
                    {
                        renderer.material = Resources.Load<Material>("VertexMaterial");
                    }
                    else if (obj.name.StartsWith("Edge_"))
                    {
                        renderer.material = Resources.Load<Material>("EdgeMaterial");
                    }
                }
            }
        }
        highlightedObjects.Clear();
    }
}
