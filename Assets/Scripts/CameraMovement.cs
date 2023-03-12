using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform cameraPos;

    void Update()
    {
        //Sets the position of this gameobject to a given position continiously
        transform.position = cameraPos.position;
    }
}
