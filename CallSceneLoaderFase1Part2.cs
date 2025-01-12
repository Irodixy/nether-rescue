using UnityEngine;
using System;

public class CallSceneLoaderFase1Part2 : MonoBehaviour
{
    public static event Action OnPart1Fase2Completion;

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
        OnPart1Fase2Completion?.Invoke();
    }
}