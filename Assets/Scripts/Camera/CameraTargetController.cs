using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public Transform player;
    public GameObject indicator;
    public float mouseInfluence = 0.5f;
    public float maxDistance = 5f;
    public float smoothTime = 0.3f;
    
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        indicator.transform.position = mouseWorld;

        Vector3 dir = mouseWorld - player.position;

        if (dir.magnitude > maxDistance)
            dir = dir.normalized * maxDistance;

        Vector3 limitedMousePos = player.position + dir;

        Vector3 targetPos = Vector3.SmoothDamp(transform.position, limitedMousePos, ref velocity, smoothTime);

        targetPos.z = transform.position.z;
        transform.position = targetPos;
    }
}
