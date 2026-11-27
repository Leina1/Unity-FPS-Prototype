using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 50f;
    public Transform cam;

    [Header("Camera Recoil (Sấy leo nóc & Tự hồi khi ngưng)")]
    public float recoilSnappiness = 22f;      // Độ giật nảy lên mỗi viên đạn
    public float recoilReturnSpeed = 5f;      // Tốc độ tâm tự hạ về vị trí cũ SAU KHI ngừng bắn
    public float recoveryDelay = 0.15f;       // Độ trễ sau viên đạn cuối mới bắt đầu hồi tâm
    public float maxVerticalRecoil = 85f;     // Cho phép giật thẳng lên tận nóc (85 độ)

    private float xRotation = 0f;
    private Vector2 lookInput;

    private Vector2 currentRecoil;
    private Vector2 targetRecoil;
    private float lastShotTime;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseAndRecoil();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Gun.cs gọi hàm này mỗi khi bắn 1 viên đạn
    public void AddRecoil(float verticalRecoil, float horizontalRecoil)
    {
        lastShotTime = Time.time;

        // Cộng dồn thẳng lên tới tận nóc (85 độ) nếu không ghìm
        targetRecoil.x = Mathf.Clamp(targetRecoil.x + verticalRecoil, 0f, maxVerticalRecoil);
        targetRecoil.y += Random.Range(-horizontalRecoil, horizontalRecoil);
    }

    void HandleMouseAndRecoil()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // 1. Khi người chơi kéo chuột xuống (mouseY < 0) lúc tâm đang bị giật lên:
        // Trừ trực tiếp vào targetRecoil.x để ghìm nòng súng xuống
        if (mouseY < 0f && targetRecoil.x > 0f)
        {
            targetRecoil.x += mouseY; // mouseY âm nên làm giảm độ giật
            if (targetRecoil.x < 0f)
            {
                // Phần kéo dư ra mới làm cúi đầu xuống
                xRotation -= targetRecoil.x;
                targetRecoil.x = 0f;
            }
        }
        else
        {
            xRotation -= mouseY;
        }

        // 2. CHỈ tự động hồi tâm về vị trí cũ KHI ĐÃ NGỪNG BẮN (qua khoảng recoveryDelay)
        if (Time.time - lastShotTime > recoveryDelay)
        {
            targetRecoil = Vector2.Lerp(targetRecoil, Vector2.zero, recoilReturnSpeed * Time.deltaTime);
        }

        // Làm mượt chuyển động camera theo targetRecoil
        currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, recoilSnappiness * Time.deltaTime);

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        float finalXRotation = Mathf.Clamp(xRotation - currentRecoil.x, -90f, 90f);

        cam.localRotation = Quaternion.Euler(finalXRotation, currentRecoil.y, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}