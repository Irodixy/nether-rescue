using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayAfterChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
