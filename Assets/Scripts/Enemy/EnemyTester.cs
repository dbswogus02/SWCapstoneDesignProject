using UnityEngine;

public class EnemyTester : MonoBehaviour
{
    public Enemy targetEnemy; // 테스트할 적 오브젝트를 여기에 드래그하세요

    void OnGUI()
    {
        // 화면에 버튼을 생성합니다.
        if (GUI.Button(new Rect(10, 10, 150, 50), "Test Attack"))
        {
            targetEnemy.PerformAttack();
        }

        if (GUI.Button(new Rect(10, 70, 150, 50), "Test Damage"))
        {
            targetEnemy.TakeDamage(10); // 정수값을 넣어 호출
        }

        if (GUI.Button(new Rect(10, 130, 150, 50), "Test Die"))
        {
            targetEnemy.Die();
        }
    }
}