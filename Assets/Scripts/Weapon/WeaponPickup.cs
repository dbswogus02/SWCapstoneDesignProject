using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private Transform playerTransform;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupWeapon();
        }
    }

    void PickupWeapon()
    {
        transform.SetParent(playerTransform);

        // 위치/회전 초기화 (필요하면 조정)
        transform.localPosition = new Vector3(0.3f, -0.6f, 0f);
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        Debug.Log("Weapon picked up!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null;
        }
    }
}