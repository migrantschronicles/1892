using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCloudsScript : MonoBehaviour
{
    [SerializeField]
    GameObject[] clouds;

    [SerializeField]
    float spawnInterval;

    [SerializeField]
    GameObject endPoint;

    [SerializeField]
    private int orderInLayer = 0;

    Vector3 startPos;


    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        Prewarm();
        Invoke("AttemptSpawn", spawnInterval);

    }

    void SpawnCloud(Vector3 startPos)
    {
        int randomIndex = UnityEngine.Random.Range(0, clouds.Length);
        GameObject cloud = Instantiate(clouds[randomIndex], transform);
        SpriteRenderer[] renderers = cloud.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = orderInLayer;
        }

        float startY = UnityEngine.Random.Range(startPos.y - 11f, startPos.y + 22f);
        cloud.transform.position = new Vector3(startPos.x, startY, startPos.z);

        float scale = UnityEngine.Random.Range(0.3f, 1.7f);
        Vector3 newScale = new Vector3(scale, scale);
        newScale.x *= 1.0f / transform.localScale.x;
        newScale.y *= 1.0f / transform.localScale.y;
        newScale.z *= 1.0f / transform.localScale.z;
        cloud.transform.localScale = newScale;


        float speed = UnityEngine.Random.Range(0.08f, 3.5f);
        cloud.GetComponent<CloudScript>().StartFloating(speed, endPoint.transform.position.x);


    }

    void AttemptSpawn()
    {
        SpawnCloud(startPos);

        Invoke("AttemptSpawn", spawnInterval);
    }

    void Prewarm()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 spawnPos = startPos + Vector3.right * (i * 2);
            SpawnCloud(spawnPos);

        }
    }

}
