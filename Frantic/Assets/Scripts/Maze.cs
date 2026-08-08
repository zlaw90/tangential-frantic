using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public PathGroupPreset[] PathGroups;
    public GameObject tilePrefab;

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
        var pathDirectionsMap = GenerateMaze();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PathDirections pathDirections = pathDirectionsMap[x, y];
                var pathGroup = GetPathGroupPreset(pathDirections);

                GameObject tile = Instantiate(pathGroup.GetMazeTilePrefab(), new Vector3(x, y, 0), Quaternion.identity);

                if (x == playerSpawnX && y == playerSpawnY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkOrange;
                }
            }
        }
    }


    private PathDirections[,] GenerateMaze()
    {
        int WIDTH_MINUS_ONE = width - 1;
        int HEIGHT_MINUS_ONE = height - 1;
        var ALL_DIRECTIONS = new List<PathDirections>(4)
        {
            PathDirections.Up,
            PathDirections.Down,
            PathDirections.Left,
            PathDirections.Right
        };

        if (playerSpawnX < 0) { playerSpawnX = Random.Range(0, width); }
        if (playerSpawnY < 0) { playerSpawnY = Random.Range(0, height); }

        bool[,] touchedMap = new bool[width, height];
        PathDirections[,] pathMap = new PathDirections[width, height];

        int cursorX = playerSpawnX;
        int cursorY = playerSpawnY;

        var stack = new Stack<(int x, int y)>(width * height);
        stack.Push((cursorX, cursorY));
        touchedMap[cursorX, cursorY] = true;

        PathDirections unvisitedNeighbors;

        do
        {
            unvisitedNeighbors = PathDirections.None;

            if (cursorX > 0 && !touchedMap[cursorX - 1, cursorY])
            {
                unvisitedNeighbors |= PathDirections.Left;
            }
            if (cursorX < WIDTH_MINUS_ONE && !touchedMap[cursorX + 1, cursorY])
            {
                unvisitedNeighbors |= PathDirections.Right;
            }
            if (cursorY > 0 && !touchedMap[cursorX, cursorY - 1])
            {
                unvisitedNeighbors |= PathDirections.Down;
            }
            if (cursorY < HEIGHT_MINUS_ONE && !touchedMap[cursorX, cursorY + 1])
            {
                unvisitedNeighbors |= PathDirections.Up;
            }

            if (unvisitedNeighbors == PathDirections.None)
            {
                // This cell is done, pop it off the stack and update the cursor to the next cell
                stack.Pop();
                if (stack.Count > 0)
                {
                    (cursorX, cursorY) = stack.Peek();
                }
            }
            else
            {
                // Choose one of the unvisited neighbors at random
                var unvisitedNeighborDirections = ALL_DIRECTIONS.Where(t => unvisitedNeighbors.HasFlag(t)).ToList();
                var randomDirection = unvisitedNeighborDirections[Random.Range(0, unvisitedNeighborDirections.Count)];

                // Update the mapping in the current cell to account for the neighbor
                pathMap[cursorX, cursorY] |= randomDirection;

                // Update the cursor position to the chosen neighbor
                // Update the neighbor's mapping to account for its link to our now-previous cell
                switch (randomDirection)
                {
                    case PathDirections.Up:
                        cursorY++;
                        pathMap[cursorX, cursorY] |= PathDirections.Down;
                        break;
                    case PathDirections.Down:
                        cursorY--;
                        pathMap[cursorX, cursorY] |= PathDirections.Up;
                        break;
                    case PathDirections.Right:
                        cursorX++;
                        pathMap[cursorX, cursorY] |= PathDirections.Left;
                        break;
                    case PathDirections.Left:
                        cursorX--;
                        pathMap[cursorX, cursorY] |= PathDirections.Right;
                        break;
                    default:
                        throw new System.Exception("Illegal path direction in map generation");
                }

                // Mark new cell as touched and push it onto the stack
                stack.Push((cursorX, cursorY));
                touchedMap[cursorX, cursorY] = true;
            }

        } while (stack.Count > 0);

        return pathMap;

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
