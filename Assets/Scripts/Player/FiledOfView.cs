using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        RotateTowardsMouse();
    }

    void RotateTowardsMouse()
    {
        // 1. 카메라에서 마우스 위치를 향하는 광선(Ray)을 생성합니다.
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 2. 캐릭터와 라이트가 위치한 평면(Z=0)을 정의합니다.
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        float rayDistance;

        // 3. 광선이 평면과 만나는 지점이 있는지 확인합니다.
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // 4. 광선과 평면이 만나는 정확한 월드 좌표를 가져옵니다.
            Vector3 mouseWorldPos = ray.GetPoint(rayDistance);

            // 5. 라이트 위치에서 마우스 위치로 향하는 방향 벡터를 계산합니다.
            Vector2 direction = new Vector2(
                mouseWorldPos.x - transform.position.x,
                mouseWorldPos.y - transform.position.y
            );

            // 6. Atan2 함수를 사용해 방향을 각도(Degree)로 변환합니다.
            // 2D Light는 기본적으로 오른쪽(0도)이 정면입니다.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 7. 라이트의 Z축 회전에 계산된 각도를 적용합니다.
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}