using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CallSceneLoaderFase1Part1 : MonoBehaviour
{
    public static event Action OnPart1Fase1Completion;

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
        OnPart1Fase1Completion?.Invoke();
    }
}
