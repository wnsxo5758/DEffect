using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable()
    {
        CameraChange.Register(GetComponent<CinemachineCamera>());   
    }

    private void OnDisable()
    {
        CameraChange.Unregister(GetComponent<CinemachineCamera>());
    }
}
