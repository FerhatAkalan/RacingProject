using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CoinSpawner_sc : MonoBehaviour
{
    public GameObject coinPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CoinSpawner());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CoinSpawn()
    {
        float rand = Random.Range(-2.1f, 2.1f);
        Instantiate(coinPrefab, new Vector3(rand, transform.position.y, transform.position.z), Quaternion.identity);
    }
    IEnumerator CoinSpawner()
    {
        while(true)
        {
            int time = Random.Range(3, 10);
            yield return new WaitForSeconds(time);
            CoinSpawn();
        }
    }
}
