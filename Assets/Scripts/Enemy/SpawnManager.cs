using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    // 스폰 포인트별 설정을 인스펙터에서 편하게 하기 위한 구조체
    [System.Serializable]
    public struct SpawnSetting
    {
        [Tooltip("적을 소환할 위치 (맵에 미리 배치한 GameObject/Transform)")]
        public Transform spawnPoint;

        [Tooltip("이 위치에 소환할 적 프리팹 (일반 몹, 보스 등 자유롭게 지정)")]
        public GameObject enemyPrefab;
    }

    [Header("플레이어 설정")]
    public Transform player; // 적들에게 할당할 타겟
    public LayerMask obstacleLayer; // 벽 레이어 (Wall)

    [Header("스폰 목록 설정")]
    [Tooltip("여기에 원하는 만큼 요소를 추가하고 위치와 프리팹을 매칭하세요.")]
    public List<SpawnSetting> spawnList = new List<SpawnSetting>();

    void Start()
    {
        SpawnEntities();
    }

    void SpawnEntities()
    {
        // 설정한 스폰 목록을 순회하며 소환
        foreach (var setting in spawnList)
        {
            // 예외 처리: 스폰 포인트나 프리팹이 비어있다면 건너뜀
            if (setting.spawnPoint == null || setting.enemyPrefab == null)
            {
                Debug.LogWarning("SpawnManager: 스폰 포인트 또는 프리팹이 비어있습니다!");
                continue;
            }

            // 지정된 위치와 회전값으로 적 생성
            Vector3 spawnPos = setting.spawnPoint.position;
            GameObject enemy = Instantiate(setting.enemyPrefab, spawnPos, Quaternion.identity);

            // 일반 적 컴포넌트 설정 (EnemyMove)
            if (enemy.TryGetComponent<Enemy>(out Enemy move))
            {
                move.target = player;
                move.obstacleLayer = obstacleLayer;
            }

            // 보스 컴포넌트 설정 (BossMove)
            if (enemy.TryGetComponent<Boss>(out Boss bossMove))
            {
                bossMove.target = player;
                bossMove.obstacleLayer = obstacleLayer;
            }
        }
    }

    // 개발 편의를 위해 에디터 뷰에서 스폰 위치들을 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var setting in spawnList)
        {
            if (setting.spawnPoint != null)
            {
                // 각 스폰 포인트 위치에 작은 구체와 와이어를 그려줍니다.
                Gizmos.DrawSphere(setting.spawnPoint.position, 0.3f);
            }
        }
    }
}