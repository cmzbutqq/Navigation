using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchUI : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public Camera mainCamera;

    [Header("Visual Settings")]
    public float highlightDuration = 5f;
    public Color highlightColor = Color.yellow;
    public Color edgeHighlightColor = Color.cyan;
    public GameObject coordinateMarkerPrefab;
    public GameObject rangeCirclePrefab;
    [Range(0.1f, 2f)] public float markerScale = 1f;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.Z;
    public KeyCode confirmKey = KeyCode.Return;

    private bool hudActive = false;
    private bool isSearching = false;
    private Vector3 searchCenter;
    private List<GameObject> highlightedObjects = new List<GameObject>();
    private GameObject currentMarker;
    private GameObject currentRangeCircle;
    private Dictionary<int, Color> originalVertexColors = new Dictionary<int, Color>();
    private Dictionary<string, Color> originalEdgeColors = new Dictionary<string, Color>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleHUD();
        }

        if (hudActive && !isSearching && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(StartSearchRoutine());
        }
    }

    void ToggleHUD()
    {
        hudActive = !hudActive;
        Debug.Log($"HUD {(hudActive ? "Enabled" : "Disabled")}");

        if (!hudActive && isSearching)
        {
            CancelSearch();
        }
    }

    IEnumerator StartSearchRoutine()
    {
        isSearching = true;

        // ��ȡ���������������
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            searchCenter = hit.point;
            searchCenter.y = 0; // ȷ���ڵ�ƽ����

            // ����������
            CreateMarker(searchCenter);

            // ��ʾ������ʾ
            Debug.Log($"Searching near {searchCenter.ToString("F2")}");

            // ִ�������Ϳ��ӻ�
            yield return StartCoroutine(SearchAndVisualize());
        }

        isSearching = false;
    }

    void CreateMarker(Vector3 position)
    {
        if (currentMarker != null)
            Destroy(currentMarker);
        currentMarker = Instantiate(coordinateMarkerPrefab, position, coordinateMarkerPrefab.transform.rotation);
    }

    private struct VertexData
    {
        public int Index;
        public Vector3 Position;
        public float Distance;
    }
    IEnumerator SearchAndVisualize()
    {
        // ��ȡ���ж���
        List<Vector3> vertices = mapGenerator.GetVertices();

        // ������벢����ת��ΪVertexData�б�
        List<VertexData> sortedVertices = vertices
            .Select((v, i) => new VertexData
            {
                Index = i,
                Position = v,
                Distance = Vector3.Distance(searchCenter, v)
            })
            .OrderBy(x => x.Distance)
            .Take(100)
            .ToList();

        if (sortedVertices.Count == 0)
            yield break;

        // ������Զ������Ϊ��Χ�뾶
        float maxDistance = sortedVertices.Max(x => x.Distance);

        // ������ΧԲ
        CreateRangeCircle(searchCenter, maxDistance);

        // ��������ͱ�
        HighlightResults(sortedVertices);

        // �ȴ�һ��ʱ��
        yield return new WaitForSeconds(highlightDuration);

        // �ָ�ԭ״
        RestoreOriginalAppearance();
    }
    // �޸ķ���ǩ����ʹ��VertexData����dynamic
    void HighlightResults(List<VertexData> vertices)
    {
        // �洢ԭʼ��ɫ��Ӧ�ø���
        foreach (var vertex in vertices)
        {
            GameObject vertexObj = GameObject.Find($"Vertex_{vertex.Index}");
            if (vertexObj != null)
            {
                Renderer renderer = vertexObj.GetComponent<Renderer>();
                originalVertexColors[vertex.Index] = renderer.material.color;
                renderer.material.color = highlightColor;
                highlightedObjects.Add(vertexObj);
            }
        }

        // ����������Щ����ı�
        List<List<int>> adjacencyList = mapGenerator.GetAdjacencyList();
        HashSet<string> processedEdges = new HashSet<string>();

        foreach (var vertex in vertices)
        {
            foreach (int neighbor in adjacencyList[vertex.Index])
            {
                // ȷ��ֻ����һ��
                string edgeKey = $"{Mathf.Min(vertex.Index, neighbor)}_{Mathf.Max(vertex.Index, neighbor)}";
                if (processedEdges.Add(edgeKey))
                {
                    GameObject edgeObj = GameObject.Find($"Edge_{edgeKey}");
                    if (edgeObj != null)
                    {
                        Renderer renderer = edgeObj.GetComponent<Renderer>();
                        originalEdgeColors[edgeKey] = renderer.material.color;
                        renderer.material.color = edgeHighlightColor;
                        highlightedObjects.Add(edgeObj);
                    }
                }
            }
        }
    }


    void CreateRangeCircle(Vector3 center, float radius)
    {
        if (currentRangeCircle != null)
            Destroy(currentRangeCircle);

        currentRangeCircle = Instantiate(rangeCirclePrefab, center, rangeCirclePrefab.transform.rotation);
        currentRangeCircle.transform.localScale = new Vector3( radius * 2, currentRangeCircle.transform.localScale.y, radius * 2);
    }



    void RestoreOriginalAppearance()
    {
        // �ָ�������ɫ
        foreach (var kvp in originalVertexColors)
        {
            string vertexName = $"Vertex_{kvp.Key}";
            GameObject vertexObj = GameObject.Find(vertexName);

            // �������Ƿ������δ������
            if (vertexObj != null && vertexObj.activeInHierarchy)
            {
                Renderer renderer = vertexObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = kvp.Value;
                }
            }
        }

        // �ָ�����ɫ
        foreach (var kvp in originalEdgeColors)
        {
            string edgeName = $"Edge_{kvp.Key}";
            GameObject edgeObj = GameObject.Find(edgeName);

            // �������Ƿ������δ������
            if (edgeObj != null && edgeObj.activeInHierarchy)
            {
                Renderer renderer = edgeObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = kvp.Value;
                }
            }
        }

        // ����
        originalVertexColors.Clear();
        originalEdgeColors.Clear();

        // ��ȫ���ٱ������
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }

        if (currentRangeCircle != null)
        {
            Destroy(currentRangeCircle);
            currentRangeCircle = null;
        }

        highlightedObjects.Clear();
    }


    void CancelSearch()
    {
        StopAllCoroutines();
        RestoreOriginalAppearance();
        isSearching = false;
    }

    void OnDisable()
    {
        CancelSearch();
    }
}
