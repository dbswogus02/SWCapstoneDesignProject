using UnityEngine;

public class TestDummyRespawn : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        Instantiate(Prefab, transform.position, Quaternion.identity);
    }
}
