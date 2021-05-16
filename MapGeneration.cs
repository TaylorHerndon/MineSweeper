using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MapGeneration : MonoBehaviour
{
    private Tilemap myTileMap;
    private int[,,] mineField;

    private int mapSizeX;
    public int mapSizeY;
    public float bombProbability;

    private bool GameOver;

    //Sprites
    public Sprite bombSprite;
    public Sprite unknownSprite;
    public Sprite flagTileSprite;
    public Sprite Tile0Sprite;
    public Sprite Tile1Sprite;
    public Sprite Tile2Sprite;
    public Sprite Tile3Sprite;
    public Sprite Tile4Sprite;
    public Sprite Tile5Sprite;
    public Sprite Tile6Sprite;
    public Sprite Tile7Sprite;
    public Sprite Tile8Sprite;

    //Tiles
    private Tile bombTile;
    private Tile unknownTile;
    private Tile flagTile;
    private Tile Tile0;
    private Tile Tile1;
    private Tile Tile2;
    private Tile Tile3;
    private Tile Tile4;
    private Tile Tile5;
    private Tile Tile6;
    private Tile Tile7;
    private Tile Tile8;

    //For ClearAllConnectedOpenTiles
    bool[,] testedTiles;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 20;
        mapSizeX = mapSizeY * 2;

        myTileMap = GetComponent<Tilemap>(); //Open the tilemap for editing
        mineField = new int[mapSizeX, mapSizeY, 2]; //Set the minefield array dimensions
        testedTiles = new bool[mapSizeX, mapSizeY]; //Set dimensions of testedTiles array
        GameOver = false; //Default GameOver

        //Create scriptable objects for each tile type in order to use the tiles
        bombTile = ScriptableObject.CreateInstance<Tile>();
        unknownTile = ScriptableObject.CreateInstance<Tile>();
        flagTile = ScriptableObject.CreateInstance<Tile>();
        Tile0 = ScriptableObject.CreateInstance<Tile>();
        Tile1 = ScriptableObject.CreateInstance<Tile>();
        Tile2 = ScriptableObject.CreateInstance<Tile>();
        Tile3 = ScriptableObject.CreateInstance<Tile>();
        Tile4 = ScriptableObject.CreateInstance<Tile>();
        Tile5 = ScriptableObject.CreateInstance<Tile>();
        Tile6 = ScriptableObject.CreateInstance<Tile>();
        Tile7 = ScriptableObject.CreateInstance<Tile>();
        Tile8 = ScriptableObject.CreateInstance<Tile>();

        //Assign each tile their specific sprite
        bombTile.sprite = bombSprite;
        unknownTile.sprite = unknownSprite;
        flagTile.sprite = flagTileSprite;
        Tile0.sprite = Tile0Sprite;
        Tile1.sprite = Tile1Sprite;
        Tile2.sprite = Tile2Sprite;
        Tile3.sprite = Tile3Sprite;
        Tile4.sprite = Tile4Sprite;
        Tile5.sprite = Tile5Sprite;
        Tile6.sprite = Tile6Sprite;
        Tile7.sprite = Tile7Sprite;
        Tile8.sprite = Tile8Sprite;

        GenerateMap();
        DrawMineField();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int mousePos = new Vector3Int(Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x),   
                                             Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y), 0);

        if (!GameOver)
        {
            if (Input.GetMouseButtonDown(0)) //Left Click
            {
                switch (mineField[mousePos.x, mousePos.y, 0])
                {
                    case 0: //Selected tile is not a bomb
                        mineField[mousePos.x, mousePos.y, 1] = 1; //Set the current tile as uncovered
                        if (SearchNearbyBombs(mousePos.x, mousePos.y) == 0)
                        {
                            //If tile is a Tile0 then clear all connected open tiles
                            ClearAllConnectedOpenTiles(mousePos.x, mousePos.y);
                            //Large change so redrawing the whole field is allowable
                            DrawMineField();
                        }
                        else
                        {
                            //Tile is not a Tile0 so only update the one tile
                            DrawSpecificTile(new Vector3(mousePos.x, mousePos.y, 0));
                        }
                        break;

                    case 1: //Selected tile is a bomb
                            //Evaluate if the selected tile has been flagged before
                        if (mineField[mousePos.x, mousePos.y, 1] == 2)
                        {
                            //The tile has been flagged as a bomb do not end the game
                        }
                        else
                        {
                            //The tile has not been flagged this is a game over
                            ShowAllTiles();
                            GameOver = true;
                        }
                        break;
                }
            }
            else if (Input.GetMouseButtonDown(1)) //Right Click
            {
                //If right click has been pressed then flag the current tile
                if (mineField[mousePos.x, mousePos.y, 1] == 0)
                {
                    //Set the current tile as flagged
                    mineField[mousePos.x, mousePos.y, 1] = 2;
                    DrawSpecificTile(new Vector3(mousePos.x, mousePos.y, 0));
                }
                else if (mineField[mousePos.x, mousePos.y, 1] == 2)
                {
                    //Unflag the current tile
                    mineField[mousePos.x, mousePos.y, 1] = 0;
                    DrawSpecificTile(new Vector3(mousePos.x, mousePos.y, 0));
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Set all tiles as uncovered
                for (int x = 0; x < mapSizeX; x++)
                {
                    for (int y = 0; y < mapSizeY; y++)
                    {
                        mineField[x, y, 1] = 0;
                    }
                }
                //Set all testedTiles back to false
                for (int x = 0; x < mapSizeX; x++)
                {
                    for (int y = 0; y < mapSizeY; y++)
                    {
                        testedTiles[x,y] = false;
                    }
                }
                //Generate a new map
                GenerateMap();
                //Redraw the Mine Field
                DrawMineField();
                GameOver = false;
            }
        }
    }

    private void GenerateMap()
    {
        //Generate the mineField
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                mineField[x,y,0] = Mathf.FloorToInt(Random.Range(0, 200) / 100f * bombProbability);
            }
        }
    }

    private void DrawSpecificTile(Vector3 tilePosition)
    {
        if (mineField[Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 1] == 0)
        {
            //Tile has not been uncovered yet
            myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), unknownTile);
        }
        else if (mineField[Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 1] == 2)
        {
            //Tile has been flagged
            myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), flagTile);
        }
        else
        {
            //Tile has been uncovered
            if (mineField[Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0] == 1)
            {
                //Tile is a bomb
                myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), bombTile);
            }
            else
            {
                //Tile is not a bomb, search to find out how many bombs are nearby and assign the correct sprite
                switch (SearchNearbyBombs(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y)))
                {
                    case 0:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile0);
                        break;
                    case 1:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile1);
                        break;
                    case 2:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile2);
                        break;
                    case 3:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile3);
                        break;
                    case 4:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile4);
                        break;
                    case 5:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile5);
                        break;
                    case 6:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile6);
                        break;
                    case 7:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile7);
                        break;
                    case 8:
                        myTileMap.SetTile(new Vector3Int(Mathf.FloorToInt(tilePosition.x), Mathf.FloorToInt(tilePosition.y), 0), Tile8);
                        break;
                }
            }
        }
    }

    private void DrawMineField()
    {
        //mineField[x,y,z]
        //x & y are cordinates
        //Z -> 0 stores whether or not the position has a bomb
        //Z -> 1 stores whether or not the position has been uncovered or flagged (0 no, 1 yes, 2 flagged)

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (mineField[x,y,1] == 0)
                {
                    //Tile has not been uncovered yet
                    myTileMap.SetTile(new Vector3Int(x, y, 0), unknownTile);
                }
                else if (mineField[x,y,1] == 2)
                {
                    //Tile has been flagged
                    myTileMap.SetTile(new Vector3Int(x,y,0), flagTile);
                }
                else
                {
                    //Tile has been uncovered
                    if (mineField[x, y, 0] == 1)
                    {
                        //Tile is a bomb
                        myTileMap.SetTile(new Vector3Int(x, y, 0), bombTile);
                    }
                    else
                    {
                        //Tile is not a bomb, search to find out how many bombs are nearby and assign the correct sprite
                        switch (SearchNearbyBombs(x, y))
                        {
                            case 0:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile0);
                                break;
                            case 1:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile1);
                                break;
                            case 2:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile2);
                                break;
                            case 3:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile3);
                                break;
                            case 4:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile4);
                                break;
                            case 5:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile5);
                                break;
                            case 6:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile6);
                                break;
                            case 7:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile7);
                                break;
                            case 8:
                                myTileMap.SetTile(new Vector3Int(x, y, 0), Tile8);
                                break;
                        }
                    }
                }
            }
        }
    }

    private int SearchNearbyBombs(int StartPosX, int StartPosY)
    {
        int nearbyBombs = 0;
        try
        {
            if (mineField[StartPosX + 1, StartPosY, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX + 1, StartPosY + 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX, StartPosY + 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX - 1, StartPosY + 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX - 1, StartPosY, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX - 1, StartPosY - 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX, StartPosY - 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        try
        {
            if (mineField[StartPosX + 1, StartPosY - 1, 0] == 1)
            {
                nearbyBombs += 1;
            }
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
        return nearbyBombs;
    }

    private void ClearAllConnectedOpenTiles(int StartX, int StartY)
    {
        List<Vector2> positionsToTest = new List<Vector2>();
        positionsToTest.Add(new Vector2(StartX, StartY));
        int abortLoop = 0;

        while (positionsToTest.Count > 0 && abortLoop < 20000)
        {
            Debug.Log("Starting Loop");
            abortLoop++;
            if (SearchNearbyBombs(Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y)) == 0)
            {
                Debug.Log("Testing Positon: " + positionsToTest[0] + "  No neighboring bombs");
                //The currently tested tile has no neighboring bombs and can be cleared
                mineField[Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y), 1] = 1;
                testedTiles[Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y)] = true;
                //Because the tested tile has no nearby bombs all tiles nearby can be cleared
                UncoverAllNearbyTiles(positionsToTest[0]);

                //Up
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y + 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x, positionsToTest[0].y + 1));
                        Debug.Log("Added Tile Above");
                    }
                }
                catch { }
                //Up Right
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x + 1), Mathf.FloorToInt(positionsToTest[0].y + 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x + 1, positionsToTest[0].y + 1));
                        Debug.Log("Added Tile Above and Right");
                    }
                }
                catch { }
                //Up Left
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x - 1), Mathf.FloorToInt(positionsToTest[0].y + 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x - 1, positionsToTest[0].y + 1));
                        Debug.Log("Added Tile Above and Left");
                    }
                }
                catch { }
                //Down
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y - 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x, positionsToTest[0].y - 1));
                        Debug.Log("Added Tile Down");
                    }
                }
                catch { }
                //Down Right
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x + 1), Mathf.FloorToInt(positionsToTest[0].y - 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x + 1, positionsToTest[0].y - 1));
                        Debug.Log("Added Tile Down and Right");
                    }
                }
                catch { }
                //Down Left
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x - 1), Mathf.FloorToInt(positionsToTest[0].y - 1)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x - 1, positionsToTest[0].y - 1));
                        Debug.Log("Added Tile Down and Left");
                    }
                }
                catch { }
                //Right
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x + 1), Mathf.FloorToInt(positionsToTest[0].y)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x + 1, positionsToTest[0].y));
                        Debug.Log("Added Tile Right");
                    }
                }
                catch { }
                //Left
                try
                {
                    if (!testedTiles[Mathf.FloorToInt(positionsToTest[0].x - 1), Mathf.FloorToInt(positionsToTest[0].y)])
                    {
                        positionsToTest.Add(new Vector2(positionsToTest[0].x - 1, positionsToTest[0].y));
                        Debug.Log("Added Tile Left");
                    }
                }
                catch { }
                
                positionsToTest.RemoveAt(0);
                Debug.Log("Remaining Positions to test: " + positionsToTest.Count);
            }
            else
            {
                //Tile has bombs nearby remove the option form positionsToTest and mark it as tested
                Debug.Log("Testing Positon: " + positionsToTest[0] + "  Some neighboring bombs");
                testedTiles[Mathf.FloorToInt(positionsToTest[0].x), Mathf.FloorToInt(positionsToTest[0].y)] = true;
                positionsToTest.RemoveAt(0);
            }
        }
        if (abortLoop > 19990)
        {
            Debug.LogError("ClearAllConnectedOpenTiles looped 20000 times so the loop was aborted.");
        }
    }

    private void UncoverAllNearbyTiles(Vector2 postionToClear)
    {   

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x + 1), Mathf.FloorToInt(postionToClear.y), 1] = 1;     //Right
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x + 1), Mathf.FloorToInt(postionToClear.y + 1), 1] = 1; //UpRight   
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x), Mathf.FloorToInt(postionToClear.y + 1), 1] = 1;     //Up
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x - 1), Mathf.FloorToInt(postionToClear.y + 1), 1] = 1; //UpLeft
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x - 1), Mathf.FloorToInt(postionToClear.y), 1] = 1;     //Left
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x - 1), Mathf.FloorToInt(postionToClear.y - 1), 1] = 1; //DownLeft
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x), Mathf.FloorToInt(postionToClear.y - 1), 1] = 1;     //Down
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }

        try
        {
            mineField[Mathf.FloorToInt(postionToClear.x + 1), Mathf.FloorToInt(postionToClear.y - 1), 1] = 1; //DownRight
        }
        catch
        {
            //Do nothing, this is needed incase of outOfBounds exception
        }
    }

    private void ShowAllTiles()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (mineField[x, y, 1] == 0)
                {
                    //Uncover all not uncovered tiles
                    mineField[x, y, 1] = 1;
                }
            }
        }
        DrawMineField();
    }
}