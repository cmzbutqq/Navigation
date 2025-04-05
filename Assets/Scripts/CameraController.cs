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

        // ȷ����ˮƽ���ƶ�������Y�����
        moveDirection.y = 0;
        moveDirection.Normalize();

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleHeightChange()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // �����µĸ߶ȣ�ȷ������С�����߶�֮��
            float newHeight = transform.position.y - scroll * heightChangeSpeed;
            newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

            // ����ˮƽλ�ò��䣬ֻ�ı�߶�
            transform.position = new Vector3(
                transform.position.x,
                newHeight,
                transform.position.z
            );
        }
    }

    void HandleRotation()
    {
        // ��ʼ��ת
        if (Input.GetMouseButtonDown(1)) // �Ҽ�����
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        // ������ת
        if (Input.GetMouseButtonUp(1)) // �Ҽ��ͷ�
        {
            isRotating = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // ִ����ת
        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // ˮƽ��ת��Χ��Y�ᣩ
            float horizontalRotation = delta.x * rotationSpeed;
            transform.RotateAround(transform.position, Vector3.up, horizontalRotation);

            // ��ֱ��ת��Χ�Ʊ���X�ᣩ
            float verticalRotation = -delta.y * rotationSpeed;
            Vector3 localRight = transform.right;
            localRight.y = 0; // ȷ��ֻ��ˮƽ������ת
            localRight.Normalize();

            // ��������ת�ǶȲ����Ʒ�Χ
            float newAngle = Vector3.Angle(Vector3.up, transform.forward) - verticalRotation;
            if (newAngle > minVerticalAngle && newAngle < maxVerticalAngle)
            {
                transform.RotateAround(transform.position, localRight, verticalRotation);
            }
        }
    }
}
