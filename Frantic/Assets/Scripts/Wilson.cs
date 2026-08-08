using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wilson : MonoBehaviour
{
    public PathGroupPreset[] PathGroups;
    public GameObject tilePrefab;
    public int generationSeed;

    [Header("Dimensions")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;


    [Header("Player Spawn")]
    public int playerSpawnX = -1;
    public int playerSpawnY = -1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerSpawnX < 0) { playerSpawnX = UnityEngine.Random.Range(0, width); }
        if (playerSpawnY < 0) { playerSpawnY = UnityEngine.Random.Range(0, height); }

        var pathDirectionsMap = new WilsonMazeGenerator().GenerateMaze(width, height, generationSeed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var vertex = pathDirectionsMap[x, y];

                PathDirections pathDirections = PathDirections.None;
                if (vertex.North) { pathDirections |= PathDirections.Up; }
                if (vertex.South) { pathDirections |= PathDirections.Down; }
                if (vertex.East)  { pathDirections |= PathDirections.Right; }
                if (vertex.West)  { pathDirections |= PathDirections.Left; }

                var pathGroup = GetPathGroupPreset(pathDirections);

                GameObject tile = Instantiate(pathGroup.GetMazeTilePrefab(), new Vector3(x, y, 0), Quaternion.identity);

                if (x == playerSpawnX && y == playerSpawnY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkOrange;
                }
            }
        }
    }

    private PathGroupPreset GetPathGroupPreset(PathDirections pathDirections)
    {
        return PathGroups.First(t => t.MatchExact(pathDirections));
    }



    //// Update is called once per frame
    //void Update()
    //{
    //}
}
