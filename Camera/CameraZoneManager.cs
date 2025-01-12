using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneManager : MonoBehaviour
{
    [SerializeField] private CameraZoneTrigger[] zones;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        // Auto-find all zones in children if not set
        if (zones == null || zones.Length == 0)
        {
            zones = GetComponentsInChildren<CameraZoneTrigger>();
        }
    }
}
