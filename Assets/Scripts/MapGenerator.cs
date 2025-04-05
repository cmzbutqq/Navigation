using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public int vertexCount = 10000;
    public float mapSize = 100f;
    public int minConnections = 2;
    public int maxConnections = 5;
    public float connectionRadius = 10f;

    public GameObject vertexPrefab;
    public GameObject edgePrefab;

    private List<Vector3> vertices = new List<Vector3>();
    private List<List<int>> adjacencyList = new List<List<int>>();

    void Start()
    {
        GenerateVertices();
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

    void ConnectVertices()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            int connections = Random.Range(minConnections, maxConnections + 1);
            connections = Mathf.Min(connections, vertexCount - 1);

            List<int> nearbyIndices = GetNearbyVertices(i, connectionRadius);

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

    List<int> GetNearbyVertices(int index, float radius)
    {
        List<int> nearby = new List<int>();
        Vector3 pos = vertices[index];

        for (int i = 0; i < vertexCount; i++)
        {
            if (i != index && Vector3.Distance(pos, vertices[i]) <= radius)
            {
                nearby.Add(i);
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
        }

        // Create edge objects
        for (int i = 0; i < vertexCount; i++)
        {
            foreach (int j in adjacencyList[i])
            {
                if (j > i) // To avoid duplicate edges
                {
                    Vector3 midpoint = (vertices[i] + vertices[j]) / 2;
                    GameObject edge = Instantiate(edgePrefab, midpoint, Quaternion.identity, transform);
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
