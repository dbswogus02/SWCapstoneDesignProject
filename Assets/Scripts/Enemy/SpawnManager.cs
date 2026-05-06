using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("소환할 프리팹")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public Transform player; // 적들에게 할당할 타겟

    [Header("소환 설정")]
    public int enemyCount = 5;
    public Vector2 spawnRangeMin; // 예: x: -10, y: -10
    public Vector2 spawnRangeMax; // 예: x: 10, y: 10
    public LayerMask obstacleLayer; // 벽 레이어 (Wall)

    void Start()
    {
        SpawnEntities();
    }

    void SpawnEntities()
    {
        // 일반 적 소환
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPos = GetRandomSafePosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // 생성된 적에게 플레이어 타겟 설정
            if (enemy.TryGetComponent<EnemyMove>(out EnemyMove move))
            {
                move.target = player;
                move.obstacleLayer = obstacleLayer;
            }
        }

        // 보스 소환
        Vector2 bossPos = GetRandomSafePosition();
        GameObject boss = Instantiate(bossPrefab, bossPos, Quaternion.identity);
        if (boss.TryGetComponent<BossMove>(out BossMove bossMove))
        {
            bossMove.target = player;
            bossMove.obstacleLayer = obstacleLayer;
        }
    }

    Vector2 GetRandomSafePosition()
    {
        Vector2 randomPos = Vector2.zero;
        bool isSafe = false;
        int attempts = 0;

        // 벽과 겹치지 않는 위치를 찾을 때까지 반복 (최대 100번 시도)
        while (!isSafe && attempts < 100)
        {
            float x = Random.Range(spawnRangeMin.x, spawnRangeMax.x);
            float y = Random.Range(spawnRangeMin.y, spawnRangeMax.y);
            randomPos = new Vector2(x, y);

            // 해당 위치에 반지름 0.5 정도의 원 안에 장애물이 없는지 체크
            Collider2D hit = Physics2D.OverlapCircle(randomPos, 0.5f, obstacleLayer);
            if (hit == null)
            {
                isSafe = true;
            }
            attempts++;
        }

        return randomPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        // 중심점 계산
        Vector3 center = new Vector3((spawnRangeMin.x + spawnRangeMax.x) / 2, (spawnRangeMin.y + spawnRangeMax.y) / 2, 0);
        // 크기 계산
        Vector3 size = new Vector3(spawnRangeMax.x - spawnRangeMin.x, spawnRangeMax.y - spawnRangeMin.y, 1);

        // 사각형 그리기
        Gizmos.DrawWireCube(center, size);
    }

}