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
        // 1. 기존 무기 체크 및 분리
        foreach (Transform child in playerTransform)
        {
            if (child.CompareTag("Weapon"))
            {
                // 부모 해제 (바닥에 떨어뜨리기)
                child.SetParent(null);
                break; // 하나만 처리하면 끝
            }
        }

        // 2. 새 무기 장착
        transform.SetParent(playerTransform);

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