using UnityEngine;

public class EnemyTester : MonoBehaviour
{
    public Enemy targetEnemy; // �׽�Ʈ�� �� ������Ʈ�� ���⿡ �巡���ϼ���

    void OnGUI()
    {
        // ȭ�鿡 ��ư�� �����մϴ�.
        if (GUI.Button(new Rect(10, 10, 150, 50), "Test Attack"))
        {
            targetEnemy.PerformAttack();
        }

        if (GUI.Button(new Rect(10, 70, 150, 50), "Test Damage"))
        {
            targetEnemy.TakeDamage(); // �������� �־� ȣ��
        }

        if (GUI.Button(new Rect(10, 130, 150, 50), "Test Die"))
        {
            targetEnemy.Die();
        }
    }
}