using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner_sc : MonoBehaviour
{
    public GameObject[] car;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnCars());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Cars()
    {
        int rand = Random.Range(0, car.Length);
        float randXPos = Random.Range(-2.1f, 2.1f);
        Instantiate(car[rand], new Vector3(randXPos, transform.position.y, transform.position.z), Quaternion.Euler(0,0,90));
    }
    IEnumerator SpawnCars()
    {
        while(true)
        {
            yield return new WaitForSeconds(2f);
            Cars();
        }
    }
}
