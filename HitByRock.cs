using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitByRock : MonoBehaviour
{
    public GameObject toDestroy;
    private void OnTriggerEnter(Collider other)
    {
        //gameObject.transform.position = new Vector3 (36, 31, 6);
        //gameObject.transform.rotation = new Quaternion (0,90,0,0);
        gameObject.AddComponent<Rigidbody>();
        Destroy(toDestroy);
    }
}
