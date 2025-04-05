using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float heightChangeSpeed = 5f;
    public float minHeight = 5f;
    public float maxHeight = 50f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f;
    public float minVerticalAngle = 20f;
    public float maxVerticalAngle = 80f;

    private Vector3 lastMousePosition;
    private bool isRotating = false;

    void Update()
    {
        HandleMovement();
        HandleHeightChange();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right;

        // 确保在水平面移动，忽略Y轴分量
        moveDirection.y = 0;
        moveDirection.Normalize();

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleHeightChange()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // 计算新的高度，确保在最小和最大高度之间
            float newHeight = transform.position.y - scroll * heightChangeSpeed;
            newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

            // 保持水平位置不变，只改变高度
            transform.position = new Vector3(
                transform.position.x,
                newHeight,
                transform.position.z
            );
        }
    }

    void HandleRotation()
    {
        // 开始旋转
        if (Input.GetMouseButtonDown(1)) // 右键按下
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        // 结束旋转
        if (Input.GetMouseButtonUp(1)) // 右键释放
        {
            isRotating = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 执行旋转
        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // 水平旋转（围绕Y轴）
            float horizontalRotation = delta.x * rotationSpeed;
            transform.RotateAround(transform.position, Vector3.up, horizontalRotation);

            // 垂直旋转（围绕本地X轴）
            float verticalRotation = -delta.y * rotationSpeed;
            Vector3 localRight = transform.right;
            localRight.y = 0; // 确保只在水平面上旋转
            localRight.Normalize();

            // 计算新旋转角度并限制范围
            float newAngle = Vector3.Angle(Vector3.up, transform.forward) - verticalRotation;
            if (newAngle > minVerticalAngle && newAngle < maxVerticalAngle)
            {
                transform.RotateAround(transform.position, localRight, verticalRotation);
            }
        }
    }
}
