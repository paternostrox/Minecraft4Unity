using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float sensitivity = 5.0f;
    [SerializeField] float height = 2.0f;

    Transform cameraTransform;
    Vector2 mouseLook;
    Vector2 input;

    void Awake()
    {
        cameraTransform = transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!GameController.IsPaused)
        {
            input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            input *= sensitivity;

            mouseLook += input;
            mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

            Quaternion targetRotation = Quaternion.AngleAxis(mouseLook.x, target.transform.up);
            Quaternion cameraRotation = targetRotation * Quaternion.AngleAxis(-mouseLook.y, Vector3.right);

            target.rotation = targetRotation;
            cameraTransform.position = target.transform.position + Vector3.up * height;
            cameraTransform.rotation = cameraRotation;
        }
    }
}