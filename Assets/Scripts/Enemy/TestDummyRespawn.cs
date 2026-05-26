using UnityEngine;

public class TestDummyRespawn : MonoBehaviour
{
    [SerializeField] private GameObject testDummyPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnTestDummy();
        }
    }

    public void SpawnTestDummy()
    {
        Instantiate(testDummyPrefab, transform.position, Quaternion.identity);
        testDummyPrefab.SetActive(true);
    }
}
