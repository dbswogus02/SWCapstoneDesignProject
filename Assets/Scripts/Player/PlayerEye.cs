using UnityEngine;

public class PlayerEye : MonoBehaviour
{
    void Update()
    {
        RotateTowardsMouse();
    }

    void RotateTowardsMouse()
    {
        // 1. 마우스의 화면 좌표를 월드 좌표로 변환합니다.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. 마우스 위치에서 현재 오브젝트 위치를 빼서 방향 벡터를 구합니다.
        // 2D이므로 z값은 0으로 고정해주는 것이 정확합니다.
        Vector2 direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        // 3. Atan2 함수를 사용해 방향 벡터의 각도(라디안)를 구하고 도(Degree) 단위로 변환합니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. 오브젝트의 회전값을 적용합니다. 
        // 만약 스프라이트의 앞부분이 오른쪽(X축 화살표 방향)을 향하고 있다면 그대로 사용하면 됩니다.
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

