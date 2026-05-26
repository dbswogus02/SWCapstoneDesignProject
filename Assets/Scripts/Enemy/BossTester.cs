using UnityEngine;

public class BossTester : MonoBehaviour
{
    // 테스트할 Boss 스크립트가 붙은 오브젝트를 인스펙터에서 드래그하세요
    public Boss targetBoss;

    void OnGUI()
    {
        // 화면에 버튼을 생성합니다. (가독성을 위해 y축 간격을 60으로 설정)

        // 1. 공격 테스트 버튼 (새로 추가)
        if (GUI.Button(new Rect(10, 10, 150, 50), "Test Boss Attack"))
        {
            if (targetBoss != null)
            {
                targetBoss.PerformAttack();
            }
            else
            {
                Debug.LogWarning("Target Boss가 할당되지 않았습니다!");
            }
        }

        // 2. 피격 테스트 버튼
        if (GUI.Button(new Rect(10, 70, 150, 50), "Test Boss Damage"))
        {
            if (targetBoss != null)
            {
                targetBoss.TakeDamage(10); // 임의의 데미지 10 전달
            }
            else
            {
                Debug.LogWarning("Target Boss가 할당되지 않았습니다!");
            }
        }

        // 3. 사망 테스트 버튼
        if (GUI.Button(new Rect(10, 130, 150, 50), "Test Boss Die"))
        {
            if (targetBoss != null)
            {
                targetBoss.Die();
            }
            else
            {
                Debug.LogWarning("Target Boss가 할당되지 않았습니다!");
            }
        }
    }
}