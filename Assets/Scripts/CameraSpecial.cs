using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpecial : MonoBehaviour
{
    
    void Start()
    {
        var n = GameManager.Instance.tileRowSize;
        Camera.main.transform.position = new Vector3(Convert.ToSingle(CalculateCamXPos(n)), 5, -10);
        
    }
    private double  CalculateCamXPos(int n)
    {
        var camXPos = (n - 1) * 2.24; //For the camera to average the scene
        return camXPos/2;
    }
    
}
