using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour

{
    #region Variables

    public List<GameObject> gamePiecePrefabList; // reference to the game piece prefab
    public List<Sprite> spriteList;
    public bool isShifting = false;
    private GameObject[,] _gamePieces; // 2D array to hold the game pieces
    public int tileColumnSize;
    public int tileRowSize;
    public int colorCount;
    private float _xPos = 0;
    private float _yPos = 0;
    public List<GameObject> nullGameObjects;
    public bool isDeadLocked = false;

    #endregion

    #region Instance Method //Singleton

    public static GameManager Instance;

    private void InstanceMethod()
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

    #endregion

    void Start()
    {
        StartGame(); //Create game grid and instantiate tiles
    }

    void Update()
    {
        if (!isShifting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // get the position of the mouse click
                Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // cast a ray to see if it hits a game piece
                RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero); //Fires a ray to desired position.
                if (hit.collider != null)
                {
                    GamePiece gamePieceScript = hit.collider.gameObject.GetComponent<GamePiece>();
                    var matchList = CheckMatches(gamePieceScript.xIndex,
                        gamePieceScript.yIndex);

                    if (matchList.Count >= 2) // To avoid exploding single objects 
                    {
                        foreach (var tile in matchList)
                        {
                            tile.GetComponent<SpriteRenderer>().sprite = null;
                            tile.GetComponent<GamePiece>().type = -1;
                        }
                        StopCoroutine(FindNullTiles());//Finding null tiles
                        StartCoroutine(FindNullTiles());
                        TimeManager.Instance.transform.DOMoveX(1, 0.3F).OnComplete(() => 
                        { //To make sure Coroutines finishes their jobs
                            StopCoroutine(FillNullTiles()); //Filling null tiles
                            StartCoroutine(FillNullTiles());
                            TimeManager.Instance.transform.DOMoveX(1, 0.3F).OnComplete(() =>
                            {
                                SpriteChanger(); //Changing sprites to desired ones.
                                if (DeadLockChecker()) RestartGame(); //If deadlock scenario occurs shuffle the board
                            });
                        });
                    }
                }
            }
        }
    }
    private void StartGame()
    {
        _gamePieces = new GameObject[tileRowSize, tileColumnSize];
        // loop through each cell in the grid and instantiate a game piece prefab
        for (int x = 0; x < tileRowSize; x++)
        {
            for (int y = 0; y < tileColumnSize; y++)
            {
                Vector2 position = new Vector2(_xPos, _yPos);
                GameObject gamePiece = Instantiate(gamePiecePrefabList[RandomCounter(colorCount)], position,
                    Quaternion.identity);
                gamePiece.GetComponent<GamePiece>().SetXandY(x, y);
                _gamePieces[x, y] = gamePiece;
                _yPos += 2.15f;
            }
            _xPos += 2.24f;
            _yPos = 0;
        }
        SpriteChanger(); 
        OrderInLayerSetter();//To set order in layer properties.
        if(DeadLockChecker()) RestartGame();
    }
    private void RestartGame()
    {
        Debug.Log("restarted game");
        for (int i = 0; i < tileRowSize; i++)
        {
            for (int j = 0; j < tileColumnSize; j++)
            {
                Destroy(_gamePieces[i,j]);//Destroy all pieces
            }
        }
        _xPos = 0;
        _yPos = 0;
        StartGame();// Instantiate new ones.
    }
    private int RandomCounter(int randomColorCount)
    {
        return Random.Range(0, randomColorCount); 
    }
    private void OrderInLayerSetter()
    {
        var orderInLayerIndex = tileColumnSize;
        for (int j =tileColumnSize-1; j > 0; j--)
        {
            for (int i= 0; i < tileRowSize; i++)
            {
                _gamePieces[i, j].gameObject.GetComponent<SpriteRenderer>().sortingOrder = orderInLayerIndex;
            }
            orderInLayerIndex--;
        }
    }
    private List<GameObject> CheckMatches(int x, int y)
    {
        int tileType = _gamePieces[x, y].GetComponent<GamePiece>().type; // Get the tag of the clicked object
        List<GameObject> matches = new List<GameObject>();
        bool[,] isVisited = new bool[tileRowSize, tileColumnSize]; // Track visited objects
        FindChains(x, y, tileType, matches, isVisited);
        
        return matches;
    }
    private void FindChains(int x, int y, int type, List<GameObject> matches, bool[,] isVisited)
    {
        if (x < 0 || x >= tileRowSize || y < 0 || y >= tileColumnSize || isVisited[x, y])
        {
            return;
        }
        
        isVisited[x, y] = true;

        if (_gamePieces[x, y].GetComponent<GamePiece>().type == type)
        {
            matches.Add(_gamePieces[x, y]);
            // Recursive calls in all four directions
            FindChains(x - 1, y, type, matches, isVisited); // Left
            FindChains(x + 1, y, type, matches, isVisited); // Right
            FindChains(x, y - 1, type, matches, isVisited); // Down
            FindChains(x, y + 1, type, matches, isVisited); // Up
        }
    }
    private void SpriteChanger()
    {
        for (int i = 0; i < tileRowSize; i++)
        {
            for (int j = 0; j <tileColumnSize ; j++)
            {
                var matchList = CheckMatches(i, j);//look every tile's matches
                if (matchList.Count < 5)
                {
                    TileSpriteChanger(3,matchList); //Changing sprites to default type
                }
                else if (5<=matchList.Count && matchList.Count< 8)
                {
                    TileSpriteChanger(0,matchList); //Changing sprites to A type
                }else if (matchList.Count >= 7 && matchList.Count < 10)
                {
                    TileSpriteChanger(1,matchList);  //Changing sprites to B type

                }else if (matchList.Count >= 10)
                {
                    TileSpriteChanger(2,matchList);  //Changing sprites to C type
                }
            }
            isShifting = false;
        }
    }

    private void TileSpriteChanger(int listIndex,List<GameObject> tileMatchList)
    {
        var tileType = tileMatchList[0].GetComponent<GamePiece>().type;
        var tempList = SpriteListReturner(tileType);
        for (int k = 0; k < tileMatchList.Count; k++)
        {
            tileMatchList[k].GetComponent<SpriteRenderer>().sprite = tempList[listIndex];
        }
        
    }
    private List<Sprite> SpriteListReturner(int tileType) //Sprite List returner function.
    {
        switch (tileType)
        {
            case 1:
                return SpriteManager.Instance.blueSpriteList;
            case 2:
                return  SpriteManager.Instance.greenSpriteList;
            case 3:
                return  SpriteManager.Instance.pinkSpriteList;
            case 4:
                return  SpriteManager.Instance.purpleSpriteList;
            case 5:
                return  SpriteManager.Instance.redSpriteList;;
            case 6:
                return  SpriteManager.Instance.yellowSpriteList;;
            default:
                return null;
        }
    }
    private bool DeadLockChecker()
    {
        isDeadLocked = true;
        for (int i = 0; i < tileRowSize; i++)
        {
            for (int j = 0; j < tileColumnSize; j++)
            {
                var matchList = CheckMatches(i, j);
                if (matchList.Count > 1) //If there is one chain that longer than one than there is no deadlock so return the false value.
                {
                    isDeadLocked = false;
                    return isDeadLocked;
                }
            }
        }
        return isDeadLocked;
    }
    private IEnumerator FindNullTiles()
    {
        for (int x = 0; x < tileRowSize; x++)
        {
            for (int y = 0; y < tileColumnSize; y++)
            {
                if (_gamePieces[x, y].GetComponent<SpriteRenderer>().sprite == null) //Finding null tiles.
                {
                    yield return StartCoroutine(ShiftTilesDown(x, y)); //Find null tiles and start the ShiftTilesDown function.
                    break;
                }
            }
        }
    }
    private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = 0.03f)
    {
        isShifting = true; //To make sure that user cant click in shifting process.
        var renders = new List<SpriteRenderer>();
        List<GameObject> gameObjects = new List<GameObject>();
        int nullTileCount = 0;
        
        for (int y = yStart; y < tileColumnSize; y++)
        {
            SpriteRenderer render = _gamePieces[x, y].GetComponent<SpriteRenderer>();

            if (render.sprite == null) nullTileCount++; //If sprite is null increase the nullTileCount.

            renders.Add(render); //Fill the renders list
            gameObjects.Add(_gamePieces[x, y]);
        }
        
        for (int i = 0; i < nullTileCount; i++)
        {
            yield return new WaitForSeconds(shiftDelay); //For animation.
            for (int k = 0; k < renders.Count - 1; k++)
            {
                if (renders[k + 1].sprite == null) continue; //For avoid "single" C case. I will explain in the notes section.

                if (gameObjects[k].GetComponent<GamePiece>().type != -1 &&
                    gameObjects[k + 1].GetComponent<GamePiece>().type != -1) continue; //For avoid "multiple" C case.
                
                renders[k].sprite = renders[k + 1].sprite;
                gameObjects[k].GetComponent<GamePiece>().type = gameObjects[k + 1].GetComponent<GamePiece>().type;   //This for lines are for carrying the null sprite to the top of the grid.
                renders[k + 1].sprite = null;
                gameObjects[k + 1].GetComponent<GamePiece>().type = -1;
            }
        }
    }
    private int SpriteTypeSetter(string spriteName) //This function is for get tileType using spriteName.
    {
        switch (spriteName)
        {
            case "Blue_Default":
                return 1;
            case "Green_Default":
                return 2;
            case "Pink_Default":
                return 3;
            case "Purple_Default":
                return 4;
            case "Red_Default":
                return 5;
            case "Yellow_Default":
                return 6;
            default:
                return 0;
        }
    }
    private IEnumerator FillNullTiles()
    {
        for (int i = 0; i < tileRowSize; i++)
        {
            var k = tileColumnSize - 1;
            while (_gamePieces[i, tileColumnSize - 1].gameObject.GetComponent<GamePiece>().type == -1) //For finding null values in  row.
            {
                _gamePieces[i, tileColumnSize - 1].gameObject.GetComponent<SpriteRenderer>().sprite = spriteList[RandomCounter(colorCount)]; //For setting random sprite.
                _gamePieces[i, tileColumnSize - 1].gameObject.GetComponent<GamePiece>().type =
                    SpriteTypeSetter(_gamePieces[i, tileColumnSize - 1].gameObject.GetComponent<SpriteRenderer>().sprite
                        .name); //For setting type to the new sprite.
                k = tileColumnSize - 1;
                while ( k!=0 &&_gamePieces[i, k - 1].gameObject.GetComponent<GamePiece>().type == -1 ) 
                {
                 //This lines are for filling empty grids.
                    yield return new WaitForSeconds(0.03f);
                    var tempType = _gamePieces[i, k - 1].gameObject.GetComponent<GamePiece>().type;
                    var tempSprite = _gamePieces[i, k - 1].gameObject.GetComponent<SpriteRenderer>().sprite;
                    _gamePieces[i, k - 1].gameObject.GetComponent<GamePiece>().type =
                        _gamePieces[i, k].gameObject.GetComponent<GamePiece>().type;
                    _gamePieces[i, k - 1].gameObject.GetComponent<SpriteRenderer>().sprite =
                        _gamePieces[i, k].gameObject.GetComponent<SpriteRenderer>().sprite;
                    _gamePieces[i, k].gameObject.GetComponent<GamePiece>().type = tempType;
                    _gamePieces[i, k].gameObject.GetComponent<SpriteRenderer>().sprite = tempSprite;
                    k--;
                }
            }
        }
    }
  
}