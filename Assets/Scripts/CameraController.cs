using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float heightChangeSpeed = 5f;
    public float minHeight = 5f;
    public float maxHeight = 500f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;
    public float minVerticalAngle = 0f;
    public float maxVerticalAngle = 89f;

    private Vector3 lastMousePosition;
    private float currentXRotation;
    private float currentYRotation;

    void Update()
    {
        HandleMovement();
        HandleHeightChange();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // WASD 控制水平移动
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

        // 确保只在XZ平面移动
        moveDirection.y = 0;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    void HandleHeightChange()
    {
        // 鼠标滚轮控制高度
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= scroll * heightChangeSpeed * 10f;
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
            transform.position = newPosition;
        }
    }

    void HandleRotation()
    {
        // 按住右键旋转视角
        if (Input.GetMouseButton(1))
        {
            // 初始化鼠标位置
            if (Input.GetMouseButtonDown(1))
            {
                lastMousePosition = Input.mousePosition;
                currentXRotation = transform.eulerAngles.x;
                currentYRotation = transform.eulerAngles.y;
            }
            else
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                // 计算旋转量
                currentYRotation += delta.x * rotationSpeed * Time.deltaTime;
                currentXRotation -= delta.y * rotationSpeed * Time.deltaTime;
                currentXRotation = Mathf.Clamp(currentXRotation, minVerticalAngle, maxVerticalAngle);

                // 应用旋转
                transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
            }
        }
    }

    // 辅助方法：确保角度在-180到180范围内
    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}
