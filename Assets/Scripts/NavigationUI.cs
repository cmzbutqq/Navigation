using UnityEngine;
using System.Collections.Generic;

public class NavigationUI : MonoBehaviour
{
    public Camera mainCamera;
    public Material pathMaterial;
    public Material highlightMaterial;

    private MapGenerator mapGenerator;
    private Pathfinder pathfinder;
    private List<GameObject> highlightedObjects = new List<GameObject>();

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

                    // Highlight vertex
                    HighlightVertex(nearestVertex);

                    // For demo: find path from vertex 0 to clicked vertex
                    if (highlightedObjects.Count == 2)
                    {
                        ClearHighlights();
                        HighlightVertex(0);
                        HighlightVertex(nearestVertex);

                        List<int> path = pathfinder.FindShortestPath(0, nearestVertex);
                        HighlightPath(path);
                    }
                }
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

    void HighlightVertex(int index)
    {
        GameObject vertex = GameObject.Find("Vertex_" + index);
        if (vertex != null)
        {
            Renderer renderer = vertex.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = highlightMaterial;
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
