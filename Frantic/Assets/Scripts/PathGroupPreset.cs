using UnityEngine;

[CreateAssetMenu(fileName = "PathGroup Preset", menuName = "New PathGroup Preset")]
public class PathGroupPreset : ScriptableObject
{
    public Sprite[] tiles;

    public PathDirections ConnectionDirections;

    public Sprite GetTileSprite()
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    /// <summary>
    /// Checks to see if this path group connects in all of the specified directions
    /// and does *not* connect to any extraneous connections.
    /// </summary>
    public bool MatchCondition(PathDirections requiredConnections, PathDirections forbiddenConnections)
    {
        return (ConnectionDirections & requiredConnections) == requiredConnections      // Must contain all required connections
            && (ConnectionDirections & forbiddenConnections) == PathDirections.None;    // Must contain no forbidden connections
    }

    public float GetNumberOfConnections()
    {
        float count = 0;
        if (ConnectionDirections.HasFlag(PathDirections.Up))    { count++; }
        if (ConnectionDirections.HasFlag(PathDirections.Right)) { count++; }
        if (ConnectionDirections.HasFlag(PathDirections.Down))  { count++; }
        if (ConnectionDirections.HasFlag(PathDirections.Left))  { count++; }
        return count;
    }
}

[System.Flags]
public enum PathDirections
{
    None = 0,
    Up = 1 << 0,
    Right = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3
}