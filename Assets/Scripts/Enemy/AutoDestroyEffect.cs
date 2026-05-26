using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [Header("이펙트 최적화 설정")]
    [Tooltip("이펙트가 화면에 머무른 뒤 자동으로 삭제될 시간(초)입니다.")]
    public float lifetime = 0.5f;

    void Start()
    {
        // 게임이 실행되면 설정한 lifetime(초) 뒤에 자기 자신 오브젝트를 완전 삭제 처리하여 최적화합니다.
        Destroy(gameObject, lifetime);
    }
}