using UnityEngine;

public class FiledOfView : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. 마우스 위치를 월드 좌표로 변환 (2D 평면상 좌표)
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // 2D이므로 Z축 고정

        // 2. 라이트에서 마우스로 향하는 방향 벡터 계산
        Vector2 direction = (Vector2)mousePos - (Vector2)transform.position;

        // 3. Atan2 함수를 사용해 방향을 각도(Degree)로 변환
        // 2D Light는 기본적으로 오른쪽(0도)이 정면입니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. 라이트의 Z축 회전에 계산된 각도 적용
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}