using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance;

    public List<Sprite> redSpriteList;
    public List<Sprite> blueSpriteList;
    public List<Sprite> yellowSpriteList;
    public List<Sprite> greenSpriteList;
    public List<Sprite> pinkSpriteList;
    public List<Sprite> purpleSpriteList;
    

    private void InstanceMethod() //Singleton
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Awake()
    {
        InstanceMethod();
    }
}
