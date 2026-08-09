using UnityEngine;

public class NavMesh : MonoBehaviour
{
    private NavMeshPlus.Components.NavMeshSurface navSurface;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        navSurface = GetComponent<NavMeshPlus.Components.NavMeshSurface>();
    }

    // Update is called once per frame
    //void Update()
    //{
    //}

    public void BuildNavMesh()
    {
        navSurface.BuildNavMesh();
    }
    public AsyncOperation BuildNavMeshAsync()
    {
        return navSurface.BuildNavMeshAsync();
    }

}
