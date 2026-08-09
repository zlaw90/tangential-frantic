using UnityEngine;

public class MazeTilePrefab : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite SpriteV1;
    public Sprite SpriteV2;

    [Header("Sprite Weights")]
    public float v1Weight = 0.35f;
    public float v2Weight = 0.65f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        PickRandomSpriteVersion();
    }

    public void PickRandomSpriteVersion()
    {
        var choice = Random.value;
        if (choice <= v1Weight)
        {
            SetSpriteVersion(1);
        }
        else
        {
            SetSpriteVersion(2);
        }
    }

    public void SetSpriteVersion(int version)
    {
        switch (version)
        {
            case 1: GetComponent<SpriteRenderer>().sprite = SpriteV1; break;
            case 2: GetComponent<SpriteRenderer>().sprite = SpriteV2; break;
        }
    }

}
