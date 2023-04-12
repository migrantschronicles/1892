using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGeneratorScript : MonoBehaviour
{
    [SerializeField]
    GameObject[] clouds;

    [SerializeField] 
    float spawnInterval;

    [SerializeField]
    GameObject endPoint;

    [SerializeField]
    private int orderInLayer = 50;

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
        foreach(SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = orderInLayer;
        }

        float startY = UnityEngine.Random.Range(startPos.y - 1f, startPos.y + 2.7f);
        cloud.transform.position = new Vector3(startPos.x,startY,startPos.z);

        float scale = UnityEngine.Random.Range(0.1f, 0.5f);
        Vector3 newScale = new Vector3(scale, scale);
        newScale.x *= 1.0f / transform.localScale.x;
        newScale.y *= 1.0f / transform.localScale.y;
        newScale.z *= 1.0f / transform.localScale.z;
        cloud.transform.localScale = newScale;


        float speed = UnityEngine.Random.Range(0.15f, 0.6f);
        cloud.GetComponent<CloudScript>().StartFloating(speed, endPoint.transform.position.x);
    
        
    }

    void AttemptSpawn()
    {
        SpawnCloud(startPos);

        Invoke("AttemptSpawn", spawnInterval);
    }

    void Prewarm()
    {
        for (int i = 0; i < 9; i++)
        {
            Vector3 spawnPos = startPos + Vector3.right * (i * 2);
            SpawnCloud(spawnPos);

        }         
    }
}
