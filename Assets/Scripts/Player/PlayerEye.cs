using UnityEngine;

public class PlayerEye : MonoBehaviour
{
    void Update()
    {
        RotateTowardsMouse();
    }

    void RotateTowardsMouse()
    {
        // 1. 카메라에서 마우스 위치를 향하는 광선(Ray)을 생성합니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 2. 캐릭터가 서 있는 평면(Z=0)을 정의합니다.
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance;

        // 3. 광선이 평면과 만나는지 확인합니다.
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // 4. 만나는 지점(월드 좌표)을 구합니다.
            Vector3 mouseWorldPosition = ray.GetPoint(rayDistance);

            // 5. 캐릭터 위치에서 마우스 위치까지의 방향 벡터를 구합니다.
            Vector2 direction = new Vector2(
                mouseWorldPosition.x - transform.position.x,
                mouseWorldPosition.y - transform.position.y
            );

            // 6. 각도를 계산하여 회전값을 적용합니다.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}