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

        var pathDirectionsMap = GenerateMaze();

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

                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                tile.GetComponent<SpriteRenderer>().sprite = pathGroup.GetTileSprite();

                if (x == playerSpawnX && y == playerSpawnY)
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.darkOrange;
                }
            }
        }
    }


    private Vertex[,] GenerateMaze()
    {
        System.Random random = new System.Random(generationSeed);
        Vertex[,] maze = new Vertex[width, height];
        HashSet<Vertex> notVisited = new HashSet<Vertex>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vertex newVertex = new Vertex(i, j, width, height);
                maze[i, j] = newVertex;
                notVisited.Add(newVertex);
            }
        }
        Vertex firstVertex = notVisited.ElementAt(random.Next(notVisited.Count));
        notVisited.Remove(firstVertex);
        while (notVisited.Count > 0)
        {
            Vertex currentVertex = notVisited.ElementAt(random.Next(notVisited.Count));
            Queue<Vertex> currentPath = new Queue<Vertex>();
            currentPath.Enqueue(currentVertex);
            while (notVisited.Contains(currentVertex))
            {
                Vertex.AdjacentReturn adjacentReturn = currentVertex.giveAdjacent(random.Next());
                currentVertex.Direction = adjacentReturn.DirectionOfMovement;
                currentVertex = maze[adjacentReturn.X, adjacentReturn.Y];
                int firstInstance = -1;
                for (int i = 0; i < currentPath.Count; i++)
                {
                    if (currentPath.ElementAt(i).Equals(currentVertex))
                    {
                        firstInstance = i;
                        break;
                    }
                }
                if (firstInstance != -1)
                {
                    currentPath = new Queue<Vertex>(currentPath.Take(firstInstance + 1).ToArray());
                }
                else
                {
                    currentPath.Enqueue(currentVertex);
                }
            }
            Vertex firstVertexOnPath = currentPath.Dequeue();
            notVisited.Remove(firstVertexOnPath);
            firstVertexOnPath.setBoolOnDirectionToTrue(firstVertexOnPath.Direction);
            Vertex previousVertex = firstVertexOnPath;
            while (currentPath.Count > 1)
            {
                Vertex localVertex = currentPath.Dequeue();
                notVisited.Remove(localVertex);
                localVertex.setBoolOnOpositeDirectionToTrue(previousVertex.Direction);
                localVertex.setBoolOnDirectionToTrue(localVertex.Direction);
                previousVertex = localVertex;
            }
            Vertex lastVertexOnPath = currentPath.Dequeue();
            lastVertexOnPath.Direction = previousVertex.Direction;
            notVisited.Remove(lastVertexOnPath);
            lastVertexOnPath.setBoolOnOpositeDirectionToTrue(previousVertex.Direction);
        }
        return maze;
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

internal class Vertex
{
    public Vertex(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    public int X { get; set; }
    public int Y { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public enum Directions
    {
        north,
        south,
        east,
        west

    };
    public Directions Direction { get; set; }
    public bool North { get; set; }
    public bool South { get; set; }
    public bool East { get; set; }
    public bool West { get; set; }

    public void setBoolOnDirectionToTrue(Directions direction)
    {
        switch (direction)
        {
            case Directions.north:
                North = true;
                break;
            case Directions.south:
                South = true;
                break;
            case Directions.east:
                East = true;
                break;
            case Directions.west:
                West = true;
                break;
        }
    }
    public void setBoolOnOpositeDirectionToTrue(Directions direction)
    {
        switch (direction)
        {
            case Directions.north:
                South = true;
                break;
            case Directions.south:
                North = true;
                break;
            case Directions.east:
                West = true;
                break;
            case Directions.west:
                East = true;
                break;
        }
    }
    public class AdjacentReturn
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Directions DirectionOfMovement { get; set; }
        public AdjacentReturn(int x, int y, Directions directionOfMovement)
        {
            X = x;
            Y = y;
            DirectionOfMovement = directionOfMovement;
        }
    }
    public AdjacentReturn giveAdjacent(int randomSeed)
    {
        System.Random random = new System.Random(randomSeed);
        Directions[] allDirections = (Directions[])Enum.GetValues(typeof(Directions));
        Directions randomDirection = (Directions)allDirections.GetValue(random.Next(4));
        bool viableAdjacent = false;
        while (!viableAdjacent)
        {
            randomDirection = (Directions)allDirections.GetValue(random.Next(4));
            viableAdjacent = true;
            if (X == 0 && randomDirection == Directions.west)
            {
                viableAdjacent = false;
            }
            else if (X == Width - 1 && randomDirection == Directions.east)
            {
                viableAdjacent = false;
            }
            else if (Y == Height - 1 && randomDirection == Directions.north)
            {
                viableAdjacent = false;
            }
            else if (Y == 0 && randomDirection == Directions.south)
            {
                viableAdjacent = false;
            }
        }
        ;
        AdjacentReturn adjacentReturn = new AdjacentReturn(X, Y, randomDirection);
        if (randomDirection == Directions.north)
        {
            adjacentReturn.Y++;
            //coordinates[1]--;
        }
        else if (randomDirection == Directions.south)
        {
            adjacentReturn.Y--;
            //coordinates[1]++;
        }
        else if (randomDirection == Directions.east)
        {
            adjacentReturn.X++;
            //coordinates[0]++;
        }
        else if (randomDirection == Directions.west)
        {
            adjacentReturn.X--;
            //coordinates[0]--;
        }
        return adjacentReturn;
    }
    public override string ToString()
    {
        string value = "[" + X + "," + Y + "] ";
        value += North ? "N:T" : "N:F";
        value += South ? "S:T" : "S:F";
        value += East  ? "E:T" : "E:F";
        value += West  ? "W:T" : "W:F";
        return value;
    }
}