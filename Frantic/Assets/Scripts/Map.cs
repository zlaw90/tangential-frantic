using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public PathGroupPreset[] PathGroups;

    public int generationSeed;
    public float scale = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = new Vector3(scale, scale, 1);
    }

    public void Generate(int width, int height)
    {
        int minimumXDistance = Mathf.FloorToInt((float)width / 4);
        int minimumYDistance = Mathf.FloorToInt((float)height / 4);

        int exitX = Random.Range(0, width);
        int exitY = Random.Range(0, height);

        int playerSpawnX, playerSpawnY;
        do
        {
            playerSpawnX = Random.Range(0, width);
            playerSpawnY = Random.Range(0, height);
        } while (Mathf.Abs(playerSpawnX - exitX) < minimumXDistance || Mathf.Abs(playerSpawnY - exitY) < minimumYDistance);

        Generate(
            width: width, 
            height: height,
            exitX: exitX,
            exitY: exitY,
            playerSpawnX: Random.Range(0, width),
            playerSpawnY: Random.Range(0, height)
        );
    }



    public void Generate(int width, int height, int exitX, int exitY, int playerSpawnX, int playerSpawnY)
    {
        var pathDirectionsMap = new WilsonMazeGenerator().GenerateMaze(width, height, generationSeed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var vertex = pathDirectionsMap[x, y];

                PathDirections pathDirections = PathDirections.None;
                if (vertex.North) { pathDirections |= PathDirections.Up; }
                if (vertex.South) { pathDirections |= PathDirections.Down; }
                if (vertex.East) { pathDirections |= PathDirections.Right; }
                if (vertex.West) { pathDirections |= PathDirections.Left; }

                var pathGroup = GetPathGroupPreset(pathDirections);

                GameObject mazeTilePrefab = pathGroup.GetMazeTilePrefab();

                GameObject tile = Instantiate(mazeTilePrefab, GetCoordinatesFromCellPosition(x, y), Quaternion.identity, this.transform);

                if (x == playerSpawnX && y == playerSpawnY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkGreen;
                }
                if (x == exitX && y == exitY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkOrange;
                }
            }
        }


        var navMesh = FindAnyObjectByType<NavMesh>();
        navMesh.BuildNavMesh();
    }

    private PathGroupPreset GetPathGroupPreset(PathDirections pathDirections)
    {
        return PathGroups.First(t => t.MatchExact(pathDirections));
    }

    public Vector3 GetCoordinatesFromCellPosition(int x, int y)
    {
        return new Vector3(scale * x, scale * y, 0);
    }
}
