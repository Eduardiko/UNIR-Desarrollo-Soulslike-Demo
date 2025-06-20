using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    [SerializeField] Transform target;

    private void LateUpdate()
    {
        Vector3 dir = target.position - transform.position;

        transform.rotation = Quaternion.LookRotation(-dir);
    }
}
