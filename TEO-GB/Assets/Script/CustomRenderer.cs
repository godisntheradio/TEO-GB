using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{
    GameObject Container { get => gameObject; }
    List<GameObject> toDraw;

    [SerializeField]
    Mesh mesh = default;

    [SerializeField]
    Material material = default;

    #region Wireframe

    Mesh wireCube = null;
    [SerializeField]
    Material wireMaterial = default;
    Matrix4x4[] wireMatrices;

    #endregion

    QuadTree qt;

    private void Awake()
    {
        toDraw = new List<GameObject>();
        wireCube = BuildWireCube();

    }
    void Start()
    {

        foreach (Transform item in Container.transform)
            toDraw.Add(item.gameObject);


        Bounds rootBound = new Bounds();
        foreach (var item in toDraw)
        {
            rootBound.Encapsulate(item.GetComponent<MeshRenderer>().bounds);
        }

        qt = new QuadTree(new Bounds(rootBound.center, rootBound.size));

        foreach (var item in toDraw)
        {
            qt.Insert(item.transform);
        }
        var list = new List<Matrix4x4>();

        qt.GetMatrices(ref list);

        wireMatrices = list.ToArray();
    }


    void Update()
    {

        //Graphics.DrawMeshInstanced(mesh, 0, material, matrices);

        Graphics.DrawMeshInstanced(wireCube, 0, wireMaterial, wireMatrices, wireMatrices.Length);

        
    }

    public Mesh BuildWireCube()
    {
        var mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
        };
        Vector2[] uv = new Vector2[vertices.Length];
        int[] indices = new int[]
        {
          0,1,
          1,2,
          2,3,
          3,0,
          4,5,
          5,6,
          6,7,
          7,4,
          0,4,
          1,5,
          2,6,
          3,7
        };


        mesh.SetVertices(vertices);
        mesh.uv = uv;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        return mesh;
    }
}
