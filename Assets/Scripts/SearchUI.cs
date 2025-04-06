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

        // 获取鼠标点击的世界坐标
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            searchCenter = hit.point;
            searchCenter.y = 0; // 确保在地平面上

            // 创建坐标标记
            CreateMarker(searchCenter);

            // 显示输入提示
            Debug.Log($"Searching near {searchCenter.ToString("F2")}");

            // 执行搜索和可视化
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
        // 获取所有顶点
        List<Vector3> vertices = mapGenerator.GetVertices();

        // 计算距离并排序，转换为VertexData列表
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

        // 计算最远距离作为范围半径
        float maxDistance = sortedVertices.Max(x => x.Distance);

        // 创建范围圆
        CreateRangeCircle(searchCenter, maxDistance);

        // 高亮顶点和边
        HighlightResults(sortedVertices);

        // 等待一段时间
        yield return new WaitForSeconds(highlightDuration);

        // 恢复原状
        RestoreOriginalAppearance();
    }
    // 修改方法签名，使用VertexData代替dynamic
    void HighlightResults(List<VertexData> vertices)
    {
        // 存储原始颜色并应用高亮
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

        // 高亮连接这些顶点的边
        List<List<int>> adjacencyList = mapGenerator.GetAdjacencyList();
        HashSet<string> processedEdges = new HashSet<string>();

        foreach (var vertex in vertices)
        {
            foreach (int neighbor in adjacencyList[vertex.Index])
            {
                // 确保只处理一次
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
        // 恢复顶点颜色
        foreach (var kvp in originalVertexColors)
        {
            string vertexName = $"Vertex_{kvp.Key}";
            GameObject vertexObj = GameObject.Find(vertexName);

            // 检查对象是否存在且未被销毁
            if (vertexObj != null && vertexObj.activeInHierarchy)
            {
                Renderer renderer = vertexObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = kvp.Value;
                }
            }
        }

        // 恢复边颜色
        foreach (var kvp in originalEdgeColors)
        {
            string edgeName = $"Edge_{kvp.Key}";
            GameObject edgeObj = GameObject.Find(edgeName);

            // 检查对象是否存在且未被销毁
            if (edgeObj != null && edgeObj.activeInHierarchy)
            {
                Renderer renderer = edgeObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = kvp.Value;
                }
            }
        }

        // 清理
        originalVertexColors.Clear();
        originalEdgeColors.Clear();

        // 安全销毁标记物体
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
