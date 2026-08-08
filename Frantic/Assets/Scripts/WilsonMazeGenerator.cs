using System.Collections.Generic;
using System.Linq;

public class WilsonMazeGenerator
{
    public Vertex[,] GenerateMaze(int width, int height, int generationSeed)
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
        notVisited.Remove(notVisited.ElementAt(random.Next(notVisited.Count)));
        while (notVisited.Count > 0)
        {
            Vertex currentVertex = notVisited.ElementAt(random.Next(notVisited.Count));
            Queue<Vertex> currentPath = new Queue<Vertex>();
            currentPath.Enqueue(currentVertex);
            while (notVisited.Contains(currentVertex))
            {
                Vertex.AdjacentReturn adjacentReturn = currentVertex.GetRandomAdjacent();
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
}


public class Vertex
{
    private static readonly PathDirections[] ALL_DIRECTIONS = new PathDirections[]
    {
        PathDirections.Up,
        PathDirections.Down,
        PathDirections.Left,
        PathDirections.Right
    };

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

    public PathDirections Direction { get; set; }
    public bool North { get; set; }
    public bool South { get; set; }
    public bool East { get; set; }
    public bool West { get; set; }

    public void setBoolOnDirectionToTrue(PathDirections direction)
    {
        switch (direction)
        {
            case PathDirections.Up: North = true; break;
            case PathDirections.Down: South = true; break;
            case PathDirections.Right: East = true; break;
            case PathDirections.Left: West = true; break;
        }
    }
    public void setBoolOnOpositeDirectionToTrue(PathDirections direction)
    {
        switch (direction)
        {
            case PathDirections.Up: South = true; break;
            case PathDirections.Down: North = true; break;
            case PathDirections.Right: West = true; break;
            case PathDirections.Left: East = true; break;
        }
    }
    public class AdjacentReturn
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PathDirections DirectionOfMovement { get; set; }
        public AdjacentReturn(int x, int y, PathDirections directionOfMovement)
        {
            X = x;
            Y = y;
            DirectionOfMovement = directionOfMovement;
        }
    }
    public AdjacentReturn GetRandomAdjacent()
    {
        PathDirections randomDirection = ALL_DIRECTIONS[UnityEngine.Random.Range(0, ALL_DIRECTIONS.Length)];

        bool viableAdjacent = false;
        while (!viableAdjacent)
        {
            randomDirection = ALL_DIRECTIONS[UnityEngine.Random.Range(0, ALL_DIRECTIONS.Length)];
            viableAdjacent = true;
            if (X == 0 && randomDirection == PathDirections.Left)
            {
                viableAdjacent = false;
            }
            else if (X == Width - 1 && randomDirection == PathDirections.Right)
            {
                viableAdjacent = false;
            }
            else if (Y == Height - 1 && randomDirection == PathDirections.Up)
            {
                viableAdjacent = false;
            }
            else if (Y == 0 && randomDirection == PathDirections.Down)
            {
                viableAdjacent = false;
            }
        }

        AdjacentReturn adjacentReturn = new AdjacentReturn(X, Y, randomDirection);
        if (randomDirection == PathDirections.Up)
        {
            adjacentReturn.Y++;
            //coordinates[1]--;
        }
        else if (randomDirection == PathDirections.Down)
        {
            adjacentReturn.Y--;
            //coordinates[1]++;
        }
        else if (randomDirection == PathDirections.Right)
        {
            adjacentReturn.X++;
            //coordinates[0]++;
        }
        else if (randomDirection == PathDirections.Left)
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
        value += East ? "E:T" : "E:F";
        value += West ? "W:T" : "W:F";
        return value;
    }
}