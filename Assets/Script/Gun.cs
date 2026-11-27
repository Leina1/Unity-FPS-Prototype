using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float reloadTime = 1f;
    public float fireRate = 0.12f;
    public int magSize = 30;

    [Header("References")]
    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public PlayerLook playerLook;
    public GameObject droppedWeapon; // Kéo object Player vào ô này trên Inspector

    [Header("Visual Recoil (Súng giật trên tay)")]
    public float recoilDistance = 0.08f;
    public float recoilKickUp = 2f;
    public float recoilSpeed = 12f;

    [Header("Camera Recoil (Ghìm tâm chuột)")]
    public float cameraVerticalRecoil = 2.2f;   // Độ nảy tâm lên trên mỗi viên
    public float cameraHorizontalRecoil = 0.8f; // Độ lắc tâm sang trái/phải ngẫu nhiên

  

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 reloadRotationOffset = new Vector3(66, 50, 50);

    void Start()
    {
        currentAmmo = magSize;
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;

        // Tự tìm PlayerLook trên object cha nếu bạn quên kéo thả ngoài Inspector
        if (playerLook == null)
        {
            playerLook = GetComponentInParent<PlayerLook>();
        }
    }

    public void Shoot()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            currentAmmo--;

            if (bullet != null && bulletSpawnPoint != null)
            {
                // Đạn bay theo đúng hướng Camera đang chỉa tới (bao gồm cả độ giật tâm)
                Quaternion shootDirection = (playerLook != null && playerLook.cam != null)
                    ? playerLook.cam.rotation
                    : bulletSpawnPoint.rotation;

                Instantiate(bullet, bulletSpawnPoint.position, shootDirection);
            }

            if (weaponFlash != null && bulletSpawnPoint != null)
            {
                GameObject flash = Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation, bulletSpawnPoint);
                Destroy(flash, 0.08f);
            }

            // Đẩy tâm Camera hất lên để người chơi phải kéo chuột ghìm súng
            if (playerLook != null)
            {
                playerLook.AddRecoil(cameraVerticalRecoil, cameraHorizontalRecoil);
            }

            StopCoroutine(nameof(Recoil));
            StartCoroutine(nameof(Recoil));
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        transform.localPosition = initialPosition;

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRotationOffset);
        float halfReloadTime = reloadTime / 2f;
        float t = 0f;

        while (t < halfReloadTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReloadTime);
            yield return null;
        }

        t = 0f;
        while (t < halfReloadTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReloadTime);
            yield return null;
        }

        transform.localRotation = initialRotation;
        currentAmmo = magSize;
        isReloading = false;
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == magSize)
            return;

        StartCoroutine(Reload());
    }

    private IEnumerator Recoil()
    {
        Vector3 recoilTargetPos = initialPosition + new Vector3(0f, 0f, -recoilDistance);
        Quaternion recoilTargetRot = initialRotation * Quaternion.Euler(-recoilKickUp, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed * 2f;
            transform.localPosition = Vector3.Lerp(initialPosition, recoilTargetPos, t);
            transform.localRotation = Quaternion.Slerp(initialRotation, recoilTargetRot, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(recoilTargetPos, initialPosition, t);
            transform.localRotation = Quaternion.Slerp(recoilTargetRot, initialRotation, t);
            yield return null;
        }

        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
    }
    public void Drop()
    {
        if (droppedWeapon != null)
        {
            Transform camTransform = (playerLook != null && playerLook.cam != null) ? playerLook.cam : transform;
            Vector3 dropPos = camTransform.position + camTransform.forward * 0.8f;

            GameObject dropped = Instantiate(droppedWeapon, dropPos, Quaternion.identity);
            if (dropped.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(camTransform.forward * 3f + Vector3.up * 1.5f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}