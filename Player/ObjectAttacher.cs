using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAttacher : MonoBehaviour
{
    public Transform modelRoot;
    public Transform leftHandBone;
    public Transform rightHandBone;
    public GameObject leftHandObject;
    public GameObject rightHandObject;

    void LateUpdate()
    {
        // Update left hand object position and rotation
        leftHandObject.transform.position = modelRoot.TransformPoint(leftHandBone.localPosition);
        leftHandObject.transform.rotation = modelRoot.rotation * leftHandBone.localRotation;

        // Update right hand object position and rotation
        rightHandObject.transform.position = modelRoot.TransformPoint(rightHandBone.localPosition);
        rightHandObject.transform.rotation = modelRoot.rotation * rightHandBone.localRotation;
    }
}
