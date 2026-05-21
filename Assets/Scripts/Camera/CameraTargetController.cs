using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public Transform player;
    public float mouseInfluence = 0.5f;
    public float maxDistance = 5f;

    void Update()
    {
        //마우스 → 월드 좌표
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        //거리 제한
        Vector3 dir = mouseWorld - player.position;

        if (dir.magnitude > maxDistance)
            dir = dir.normalized * maxDistance;

        Vector3 limitedMousePos = player.position + dir;

        //중간 위치 계산
        Vector3 targetPos = Vector3.Lerp(player.position, limitedMousePos, mouseInfluence);

        targetPos.z = transform.position.z;

        transform.position = targetPos;
    }
}