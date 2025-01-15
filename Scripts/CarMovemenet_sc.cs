using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CarMovemenet_sc : MonoBehaviour
{
    public Transform transform;
    public float speed = 4f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= new Vector3(0, speed * Time.deltaTime, 0);
        if(transform.position.y <= -6)
        {
            Destroy(gameObject);
        }
    }
}
