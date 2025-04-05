using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    private MapGenerator mapGenerator;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    public List<int> FindShortestPath(int start, int end)
    {
        List<Vector3> vertices = mapGenerator.GetVertices();
        List<List<int>> adjacencyList = mapGenerator.GetAdjacencyList();

        // Dijkstra's algorithm
        Dictionary<int, float> distances = new Dictionary<int, float>();
        Dictionary<int, int> previous = new Dictionary<int, int>();
        List<int> unvisited = new List<int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            distances[i] = i == start ? 0f : Mathf.Infinity;
            previous[i] = -1;
            unvisited.Add(i);
        }

        while (unvisited.Count > 0)
        {
            // Find unvisited node with smallest distance
            int current = -1;
            float minDistance = Mathf.Infinity;

            foreach (int node in unvisited)
            {
                if (distances[node] < minDistance)
                {
                    minDistance = distances[node];
                    current = node;
                }
            }

            if (current == -1 || current == end) break;

            unvisited.Remove(current);

            // Update neighbors
            foreach (int neighbor in adjacencyList[current])
            {
                float edgeLength = Vector3.Distance(vertices[current], vertices[neighbor]);
                float alt = distances[current] + edgeLength;

                if (alt < distances[neighbor])
                {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                }
            }
        }

        // Reconstruct path
        List<int> path = new List<int>();
        int currentPathNode = end;

        while (previous[currentPathNode] != -1)
        {
            path.Insert(0, currentPathNode);
            currentPathNode = previous[currentPathNode];
        }

        if (path.Count > 0 || start == end)
        {
            path.Insert(0, start);
        }

        return path;
    }
}
