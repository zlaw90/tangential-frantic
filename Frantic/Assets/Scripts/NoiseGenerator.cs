using UnityEngine;

public class NoiseGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="width">width of the noise map</param>
    /// <param name="height">height of the noise map</param>
    /// <param name="scale">overall scale so we can zoom in or out if needed</param>
    /// <param name="waves">array of different waves to generate the noise map</param>
    /// <param name="offset">horizontal and vertical offset if needed</param>
    /// <returns></returns>
    public static float[,] Generate(int width, int height, float scale, Wave[] waves, Vector2 offset)
    {
        // create the noise map
        float[,] noiseMap = new float[width, height];

        // loop through each cell in the noise map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // calculate the sample positions
                float samplePosX = (float)x * scale + offset.x;
                float samplePosY = (float)y * scale + offset.y;

                float normalization = 0.0f;
                foreach (Wave wave in waves)
                {
                    // sample the perlin noise taking into consideration amplitude and frequency
                    noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(samplePosX * wave.frequency + wave.seed, samplePosY * wave.frequency + wave.seed);
                    normalization += wave.amplitude;
                }
                // normalize the value
                noiseMap[x, y] /= normalization;
            }
        }
        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}