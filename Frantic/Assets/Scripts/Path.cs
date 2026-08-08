using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public PathGroupPreset[] PathGroups;
    public GameObject tilePrefab;

    [Header("Dimensions")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;

    [Header("Starting Points")]
    public Wave[] startingPointWaves;
    private float[,] startingPointMap;
    public float startingPointThreshold;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GeneratePath();
    }

    void GeneratePath()
    {
        int WIDTH_MINUS_1 = width - 1;
        int HEIGHT_MINUS_1 = height - 1;
        int totalCellCount = width * height;
        int touchedCellCount = 0;
        bool[,] touchedMap = new bool[width, height];
        PathDirections[,] pathDirectionsMap = new PathDirections[width, height];

        PathGroupPreset allDirectionsPathGroup = null;
        PathGroupPreset noDirectionsPathGroup = null;
        for (int i = 0; i < PathGroups.Length; i++)
        {
            float numberOfConnections = PathGroups[i].GetNumberOfConnections();
            if (numberOfConnections >= 4)
            {
                allDirectionsPathGroup = PathGroups[i];
            }
            else if (numberOfConnections <= 0)
            {
                noDirectionsPathGroup = PathGroups[i];
            }
        }

        var allDirectionsStartingMap = NoiseGenerator.Generate(width, height, scale, startingPointWaves, offset);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allDirectionsStartingMap[x, y] > maxValue) { maxValue = allDirectionsStartingMap[x, y]; }
                if (allDirectionsStartingMap[x, y] < minValue) { minValue = allDirectionsStartingMap[x, y]; }

                if (allDirectionsStartingMap[x, y] >= startingPointThreshold)
                {
                    pathDirectionsMap[x, y] = PathDirections.Up | PathDirections.Down | PathDirections.Left | PathDirections.Right;
                    touchedMap[x, y] = true;
                    touchedCellCount++;

                    GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                    tile.GetComponent<SpriteRenderer>().sprite = allDirectionsPathGroup.GetTileSprite();
                    tile.GetComponent<SpriteRenderer>().color = Color.blue;

                }
            }
        }

        while (touchedCellCount < totalCellCount)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!touchedMap[x, y])
                    {
                        PathDirections requiredConnections = PathDirections.None;
                        PathDirections forbiddenConnections = PathDirections.None;

                        if (x > 0)
                        {
                            if (touchedMap[x - 1, y])
                            {
                                if (pathDirectionsMap[x - 1, y].HasFlag(PathDirections.Right))
                                {
                                    requiredConnections |= PathDirections.Left;
                                }
                                else
                                {
                                    forbiddenConnections |= PathDirections.Left;
                                }
                            }
                        }
                        if (x < WIDTH_MINUS_1)
                        {
                            if (touchedMap[x + 1, y])
                            {
                                if (pathDirectionsMap[x + 1, y].HasFlag(PathDirections.Left))
                                {
                                    requiredConnections |= PathDirections.Right;
                                }
                                else
                                {
                                    forbiddenConnections |= PathDirections.Right;
                                }
                            }
                        }
                        if (y > 0)
                        {
                            if (touchedMap[x, y - 1])
                            {
                                if (pathDirectionsMap[x, y - 1].HasFlag(PathDirections.Up))
                                {
                                    requiredConnections |= PathDirections.Down;
                                }
                                else
                                {
                                    forbiddenConnections |= PathDirections.Down;
                                }
                            }
                        }
                        if (y < HEIGHT_MINUS_1)
                        {
                            if (touchedMap[x, y + 1])
                            {
                                if (pathDirectionsMap[x, y + 1].HasFlag(PathDirections.Down))
                                {
                                    requiredConnections |= PathDirections.Up;
                                }
                                else
                                {
                                    forbiddenConnections |= PathDirections.Up;
                                }
                            }
                        }

                        if ((requiredConnections | forbiddenConnections) > 0)
                        {
                            var pathGroupsTemp = new List<PathGroupPreset>(PathGroups.Length);
                            foreach (var pathGroup in PathGroups)
                            {
                                if (pathGroup.MatchCondition(requiredConnections, forbiddenConnections))
                                {
                                    pathGroupsTemp.Add(pathGroup);
                                }
                            }

                            PathGroupPreset pathGroupToUse;
                            if (pathGroupsTemp.Count > 0)
                            {
                                pathGroupToUse = pathGroupsTemp[Random.Range(0, pathGroupsTemp.Count)];
                            }
                            else
                            {
                                pathGroupToUse = noDirectionsPathGroup;
                            }

                            touchedMap[x, y] = true;
                            touchedCellCount++;
                            pathDirectionsMap[x, y] = pathGroupToUse.ConnectionDirections;

                            GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                            tile.GetComponent<SpriteRenderer>().sprite = pathGroupToUse.GetTileSprite();
                        }
                        else
                        {
                            // Cell has no touched neighbors. Skip it and try again next loop.
                        }
                    }
                }
            }
        }

    }
}
