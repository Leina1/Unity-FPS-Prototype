using UnityEngine;
using UnityEngine.InputSystem;

public class PickUp : MonoBehaviour
{
    public Material highlightMaterial;
    private Material[] originalMaterials;
    private MeshRenderer[] meshRenderers;

    public GameObject weaponPrefab;
    public float lookRange = 3f;

    private bool isLookedAt = false;
    private Camera playerCam;
    private PlayerShooting player;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMaterials = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            originalMaterials[i] = meshRenderers[i].material;
        }

        player = FindFirstObjectByType<PlayerShooting>();
        if (player != null)
        {
            playerCam = player.GetComponentInChildren<Camera>();
        }
    }

    void Update()
    {
        if (playerCam == null) return;

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if (hit.collider.GetComponentInParent<PickUp>() == this)
            {
                if (!isLookedAt)
                    SetLookedAt(true);

                return;
            }
        }

        if (isLookedAt)
            SetLookedAt(false);
    }

    void SetLookedAt(bool lookedAt)
    {
        isLookedAt = lookedAt;

        if (lookedAt)
        {
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.material = highlightMaterial;
            }
        }
        else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }

    public void OnPickUp()
    {
        if (!isLookedAt) return;

        player.OnDrop(); // Nếu đang cầm vũ khí, thả vũ khí hiện tại trước khi nhặt vũ khí mới

        // Giữ nguyên Position, Rotation và Scale đã căn bên phải trong Prefab AK47
        GameObject newWeapon = Instantiate(weaponPrefab, player.gunHolder);
        player.gun = newWeapon.GetComponent<Gun>();

        Destroy(gameObject);
    }
}