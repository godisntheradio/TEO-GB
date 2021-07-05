using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{
    GameObject Container { get => gameObject; }
    List<GameObject> toDraw;

    //[SerializeField]
    //Mesh mesh = default;

    //[SerializeField]
    //Material material = default;

    #region Wireframe

    [SerializeField]
    Material wireMaterial = default;

    LineRenderer lineRenderer;

    #endregion

    QuadTree qt;
    BST<HorizonData> Horizon;

    private void Awake()
    {
        
        toDraw = new List<GameObject>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        //wireCube = BuildWireCube();
        lineRenderer.material = wireMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

    }
    void Start()
    {
        Horizon = new BST<HorizonData>();

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

        //wireMatrices = list.ToArray();
    }

    
    void Update()
    {

        //Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
        //Graphics.DrawMeshInstanced(wireCube, 0, wireMaterial, wireMatrices, wireMatrices.Length);

        Horizon = new BST<HorizonData>();

        foreach (var item in toDraw)
        {
            var cameraPos = Camera.main.transform.position;
            Vector3 top = new Vector3(item.transform.position.x, item.transform.position.y + (item.transform.localScale.y / 2), item.transform.position.z);
            Vector3 bottom = new Vector3(item.transform.position.x, item.transform.position.y - (item.transform.localScale.y / 2), item.transform.position.z);
            Vector2 projectedTop = Camera.main.WorldToScreenPoint(top);
            Vector2 projectedBottom = Camera.main.WorldToScreenPoint(bottom);
            var dist = Vector3.Distance(cameraPos, top);

            item.GetComponent<MeshRenderer>().bounds.GetProjectedLowestAndHighestX(Camera.main, out Vector3 lowestX, out Vector3 highestX);
            int key = (int)(highestX.x - lowestX.x); 
            Horizon.Insert(key, new HorizonData(dist, projectedBottom.y, projectedTop.y, lowestX.x, highestX.x, key));
            
        }

        HorizonDataPostOrder(Horizon, out List<Vector3> points);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        //Debug.Log($"x: {Input.mousePosition.x}, y {Input.mousePosition.y}");
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

    public void HorizonDataPostOrder(BST<HorizonData> bst, out List<Vector3> points)
    {
        points = new List<Vector3>();
        GetDataPostOrder(bst.Root, ref points);
    }

    public void GetDataPostOrder(BSTNode<HorizonData> node, ref List<Vector3> points)
    {
        if (node == null)
            return;

        GetDataPostOrder(node.LNode, ref points);

        GetDataPostOrder(node.RNode, ref points);

        const float depth = 5.0f;

        points.Add(Camera.main.ScreenToWorldPoint(new Vector3(node.Data.maxMaskX, node.Data.minMaskHeight, depth)));
        points.Add(Camera.main.ScreenToWorldPoint(new Vector3(node.Data.maxMaskX, node.Data.maxMaskHeight, depth)));
        points.Add(Camera.main.ScreenToWorldPoint(new Vector3(node.Data.minMaskX, node.Data.maxMaskHeight, depth)));
        points.Add(Camera.main.ScreenToWorldPoint(new Vector3(node.Data.minMaskX, node.Data.minMaskHeight, depth)));
    }
}

public class HorizonData
{
    public float distance;
    public float minMaskHeight;
    public float maxMaskHeight;
    public float minMaskX;
    public float maxMaskX;
    public float range;

    public HorizonData()
    {
    }

    public HorizonData(float distance, float minMaskHeight, float maxMaskHeight, float minMaskX, float maxMaskX, float range)
    {
        this.distance = distance;
        this.minMaskHeight = minMaskHeight;
        this.maxMaskHeight = maxMaskHeight;
        this.minMaskX = minMaskX;
        this.maxMaskX = maxMaskX;
        this.range = range;
    }
}

public static class BoundsExtensionMethods
{
    public static void GetProjectedLowestAndHighestX(this Bounds bounds, Camera camera, out Vector3 lowestX, out Vector3 highestX)
    {
        float y = bounds.center.y + bounds.extents.y;
        float xr = bounds.center.x + bounds.extents.x;
        float xl = bounds.center.x - bounds.extents.x;
        float zr = bounds.center.z + bounds.extents.z;
        float zl = bounds.center.z - bounds.extents.z;

        Vector3[] points = new[]
        {
            camera.WorldToScreenPoint(new Vector3(xr, y, zl)),
            camera.WorldToScreenPoint(new Vector3(xl, y, zl)),
            camera.WorldToScreenPoint(new Vector3(xr, y, zr)),
            camera.WorldToScreenPoint(new Vector3(xl, y, zr))
        };

        lowestX = Vector3.positiveInfinity;
        highestX = Vector3.negativeInfinity;

        foreach (var item in points)
        {
            if (item.x < lowestX.x)
                lowestX = item;
            else if (item.x > highestX.x)
                highestX = item;
        }
    }
}