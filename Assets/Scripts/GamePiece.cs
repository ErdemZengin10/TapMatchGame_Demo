using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int type;
    public int xIndex;
    public int yIndex;

    public void SetType(int newType)
    {
        type = newType;
    }

    public void SetXandY(int x , int y)
    {
        xIndex = x;
        yIndex = y;
    }

  

   
}
