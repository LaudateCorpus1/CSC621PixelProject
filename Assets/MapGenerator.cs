using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{
    // Arena data
    public int arenaRadius;

    // Player data
    public GameObject player;
    private GameObject playerFound;

    //Chest data
    public GameObject chest;
    [Range(0, 100)]
    public int chestSpawnPercentage;

    //Forground Tile Refferences
    public Tile grassLeft;
    public Tile grassMiddle;
    public Tile grassRight;
    public Tile grassSingle;
    public Tile dirtLight;
    public Tile dirtDark;

    //TileMapRefference
    int[,] itemMap;
    public Tilemap tilemap;

    //Map Size
    public int width;
    public int height;

    //Variables control smoothing and map variation
    public int smoothingTarget;
    public int wallCountModifier;

    //Variable control map seeding
    public string seed;
    public bool useRandomSeed;

    //Density of map Gen(effects smoothing)
    [Range(0, 100)]
    public int randomFillPercent;

    //2d map array
    int[,] map;


void generateArena()
    {
        Debug.Log("Running generateArena method.");
        int arenaX = width / 2; // center of generated map
        int arenaY = height / 2;
        int perimeterX;
        int perimeterY;

        // Carve out arena into map
        for (int angle = -180; angle < 180; angle++) // Loop through 360 degree
        {
            for (int radius = arenaRadius; radius >= 0; radius--) // loop through all coordinates for all angles
            {
                perimeterX = (int)(radius * Math.Cos(angle) + arenaX);
                perimeterY = (int)(radius * Math.Sin(angle) + arenaY);

                for (int nextToX = perimeterX - 1; nextToX < perimeterX + 1; nextToX++)
                {
                    for (int nextToY = perimeterY - 1; nextToY < perimeterY + 1; nextToY++)
                    {
                        map[nextToX, nextToY] = 0;
                    }
                }
            }
        }

        int scaleArena = (int)(arenaRadius * 0.28);
        //Build Arena
        for (int x = arenaX - arenaRadius + scaleArena; x < arenaX + arenaRadius - scaleArena; x++){
            for (int y = arenaX - arenaRadius + scaleArena; y < arenaY + arenaRadius - scaleArena; y++)
            {
                if(x == arenaX - arenaRadius + scaleArena || x == arenaX + arenaRadius - scaleArena - 1 || y == arenaY - arenaRadius + scaleArena || y == arenaY + arenaRadius - scaleArena - 1)
                {
                    map[x, y] = 1;
                }
            }
        }
    }

    void Start()
    {
        Debug.Log("Running start method.");
        GenerateMap();
        spawnObjects();
        if(GameObject.Find("player(Clone)") != null)
        {
            GameObject playerFound = GameObject.Find("player(Clone)");
        }
        else
        {
            Debug.Log("player(Clone) NOT FOUND.");
        }

    }

    void GenerateMap() //Runing through all functions needed to generate the map
    {
        Debug.Log("Running GenerateMap method.");
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < smoothingTarget; i++) //SmoothMap exicutes based on the Smoothing target creating softer structures
        {
            SmoothMap();
        }
        generateArena();
        DrawMap();
    }

    void RandomFillMap() //Generate noise based on a seed hash
    {
        Debug.Log("Running RandomFillMap method.");
        if (useRandomSeed == true) //Generate seeded hash. Either Random or preset so terain generation is repeatable
        {
            seed = DateTime.Now.ToString();
            Debug.Log(seed);
        }

        System.Random psuedoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (psuedoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0; //RandomFillPercent controls the density of noise
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int adjacentTitlesCount = countAdjacentTitles(x, y);

                if (adjacentTitlesCount > wallCountModifier)
                {
                    map[x, y] = 1;
                }
                else if (adjacentTitlesCount < wallCountModifier)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int countAdjacentTitles(int mapX, int mapY) //count adjacent tiles to see if a tile is floating or attatched to others
    {
        int adjacentTitlesCount = 0;

        for(int nextToX = mapX - 1; nextToX <= mapX + 1; nextToX++)
        {
            for (int nextToY = mapY - 1; nextToY <= mapY + 1; nextToY++)
            {
                if (nextToX >= 0 && nextToX < width && nextToY >= 0 && nextToY < height)
                {
                    if (nextToX != mapX || nextToY != mapY)
                    {
                        adjacentTitlesCount += map[nextToX, nextToY];
                    }
                    else
                        adjacentTitlesCount++;
                }
            }
        }
        return adjacentTitlesCount;
    }
    
    void DrawMap() //Updates Tilemap and renders array
    {
        Debug.Log("Running PopulateMap method.");
        
        if (map != null)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                for (int y = height - 1; y >= 0 ; y--)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1) //Build border wall
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, dirtDark);
                    }
                    else if (map[x, y] == 0) //Empty Space
                        {
                            Vector3Int p = new Vector3Int(x, y, 0);
                            tilemap.SetTile(p, null);
                        }
                    else if (map[x, y] == 1 && map[x, y + 1] == 0 && map[x - 1, y] == 0) //Check for grass position and place left case tile
                    {
                            Vector3Int p = new Vector3Int(x, y, 0);
                            tilemap.SetTile(p, grassLeft);
                        
                    }
                    else if (map[x, y] == 1 && map[x, y + 1] == 0 && map[x + 1, y] == 0) //Check for grass position and place right case tile
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, grassRight);

                    }
                    else if (map[x, y] == 1 && map[x, y + 1] == 0 && map[x - 1, y] == 1 && map[x + 1, y] == 1) //Check for grass position and place middle case tile
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, grassMiddle);

                    }
                    else if (map[x, y] == 1 && map[x, y + 1] == 0 && map[x - 1, y] == 0 && map[x + 1, y] == 0) //Check for grass position and place single case tile
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, grassSingle);

                    }
                    else if (map[x, y] == 1 && map[x, y + 1] == 1) //If not grass or empty then make dirt
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, dirtLight);
                    }
                    else
                    {
                        Vector3Int p = new Vector3Int(x, y, 0);
                        tilemap.SetTile(p, dirtDark);
                    }
                }
            }
        }
    }

    void spawnObjects() //find a safe location to spawn in an item
    {
        Debug.Log("Running spawnObjects method.");
        
        bool characterHasSpawned = false;
        if (useRandomSeed) //Generate seeded hash. Either Random or preset so loot locations are repeatable
        {
            seed = DateTime.Now.ToString();
        }
        System.Random psuedoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++) // Cycle through entire tileMap
        {
            for (int y = 0; y < height; y++)
            {
                if (x > 1 && x < width - 2 && y < height - 4 && map[x, y] == 1) //check for out of bounds  and allowable tile
                {
                    int aboveSpaceCount = 0;
                    for (int nextToX = x - 1; nextToX < x + 1; nextToX++) // check above 9 tile spaces
                    {
                        for (int nextToY = y + 1; nextToY < y + 4; nextToY++)
                        {
                            if (map[nextToX, nextToY] != 0)
                            {
                                aboveSpaceCount++;
                            }
                        }
                    }

                    if (aboveSpaceCount == 0) // if above space is empty Generate Object
                    {
                        if (characterHasSpawned == false)
                        {
                            Vector3Int p = new Vector3Int(x, y + 1, 0);
                            Instantiate(player, p, Quaternion.identity);
                            characterHasSpawned = true;
                        }
                        else if (psuedoRandom.Next(0, 100) < chestSpawnPercentage)
                    {
                            Vector3Int p = new Vector3Int(x, y + 1, 0);
                            Instantiate(chest, p, Quaternion.identity);
                        }
                        
                    }
                }
            }
        }
    }
}