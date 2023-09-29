using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera centerEyeAnchor;

    void Update()
    {
        transform.LookAt(centerEyeAnchor.gameObject.transform);
    }
}
