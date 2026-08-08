using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Map : MonoBehaviour
{
    public PathGroupPreset[] PathGroups;
    public GameObject tilePrefab;
    public GameObject navMesh;
    public GameObject dummy;
    public GameObject playerDummy;

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

        var xScale = transform.localScale.x;
        var yScale = transform.localScale.y;

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

                GameObject tile = Instantiate(mazeTilePrefab, new Vector3(x * xScale, y * yScale, 0), Quaternion.identity, this.transform);

                if (x == playerSpawnX && y == playerSpawnY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkOrange;
                }
            }
        }

        playerDummy.transform.position = new Vector3(xScale * playerSpawnX, yScale * playerSpawnY, 0);

        var navsurface = navMesh.GetComponent<NavMeshPlus.Components.NavMeshSurface>();
        navsurface.BuildNavMesh();

        var agent = dummy.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.SetDestination(new Vector3(xScale * playerSpawnX, yScale * playerSpawnY, 0));
    }

    private PathGroupPreset GetPathGroupPreset(PathDirections pathDirections)
    {
        return PathGroups.First(t => t.MatchExact(pathDirections));
    }
}
