using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginEnd : MonoBehaviour
{
    public static event Action OnInteracted;
    public static event Action OnSpeedRun;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        OnInteracted?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        OnSpeedRun?.Invoke();
    }
}
