using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraSettings
{
    public string zoneName;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public Vector3 cameraRotation = new Vector3(20, 0, 0); // testing
    //public Vector3 cameraRotation = new Vector3(20, 180, 0);
    public float transitionSpeed = 2f;
}

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private CameraSettings cameraSettings;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<MovimentarJogador>(out var player))
        {
            player.EnterCameraZone(cameraSettings);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<MovimentarJogador>(out var player))
        {
            player.ExitCameraZone();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw trigger zone
        Gizmos.color = gizmoColor;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }

        // Draw camera position and direction
        if (Application.isEditor)
        {
            Gizmos.color = Color.red;
            Vector3 cameraPosition = transform.position + cameraSettings.cameraOffset;
            Gizmos.DrawSphere(cameraPosition, 0.3f);

            // Draw camera direction
            Quaternion cameraRotation = Quaternion.Euler(cameraSettings.cameraRotation);
            Vector3 cameraForward = cameraRotation * Vector3.forward;
            Gizmos.DrawRay(cameraPosition, cameraForward * 2f);
        }
    }
}