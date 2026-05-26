using UnityEngine;

public class TestDummyRespawn : MonoBehaviour
{
    [SerializeField] private GameObject testDummyPrefab;

    void Update()
    {
        if (GameObject.FindWithTag("Enemy") == null)
        {
            SpawnTestDummy();
        }
    }

    private void Start()
    {
        SpawnTestDummy();
    }

    public void SpawnTestDummy()
    {
        Instantiate(testDummyPrefab, transform.position, Quaternion.identity);
    }
}
