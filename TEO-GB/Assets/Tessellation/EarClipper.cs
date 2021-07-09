using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
public class EarClipper : MonoBehaviour
{
    public LinkedList<Point> Points;

    public LinkedList<Point> ConvexPoints;
    public LinkedList<Point> ConcavePoints;
    public LinkedList<Point> EarPoints;

    bool IsCounterClockwise;

    public PointPlotter Plotter;

    #region Mesh properties

    Mesh PolygonMesh;
    Matrix4x4 PolygonMeshMatrix;
    public Material PolygonMeshMaterial;

    #endregion

    #region UI

    public GameObject ConvexUIList;
    public GameObject ConcaveUIList;
    public GameObject EarUIList;

    #endregion

    private void Awake()
    {
        Plotter = GetComponent<PointPlotter>();
    }

    private void Update()
    {
        if (PolygonMesh != null)
        {
            Graphics.DrawMesh(PolygonMesh, PolygonMeshMatrix, PolygonMeshMaterial, 0);
        }

    }

    public void Run()
    {
        Points = new LinkedList<Point>(Plotter.Points);

        if (Points.Count < 3)
        {
            Debug.Log("precisa de pelo menos 3 vertices");
            return;
        }

        ConvexPoints = new LinkedList<Point>();
        ConcavePoints = new LinkedList<Point>();
        EarPoints = new LinkedList<Point>();

        // ordem de criação dos pontos
        IsCounterClockwise = Plotter.IsCounterClockwise;

        var i1 = Points.First;


        while (i1 != null)
        {
            var i0 = i1.Previous == null ? Points.Last : i1.Previous;
            var i2 = i1.Next == null ? Points.First : i1.Next;

            var p1 = i0.Value.Position;
            var p2 = i1.Value.Position;
            var p3 = i2.Value.Position;

            // é concavo
            if (!IsConvex(p1, p2, p3))
                ConcavePoints.AddLast(i1.Value);
            else // se for convexo, o ponto pode ser uma ear tip
            {
                ConvexPoints.AddLast(i1.Value);

                // adiciona na lista se nao encontrou nenhum ponto dentro do triangulo
                if (IsEar(p1, p2, p3))
                    EarPoints.AddLast(i1.Value);
            }

            i1 = i1.Next;
        }

        if (ConcavePoints.Count > 0)
            RunConcave();
        else
            RunConvex();

    }

    public bool IsEar(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool containsPointInside = false;
        foreach (var item in Points) // testa esse o triangulo formado por este ponto convexo, o anterior e o proximo contra todos os outros pontos (exceto os que formam esse triangulo) para verificar se é uma orelha
        {
            if (item.Position == p1 || item.Position == p2 || item.Position == p3)
                continue;

            containsPointInside = Math.IsPointInsideTriangle(item.Position, p1, p2, p3);

            // se achar qualquer ponto dentro já da pra saber que não é uma orelha
            if (containsPointInside)
                break;
        }
        return !containsPointInside;
    }

    public bool IsConvex(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var orientation = Math.ComputeOrientation(p1, p2, p3);
        return (orientation <= 0 && IsCounterClockwise) || (orientation >= 0 && !IsCounterClockwise);
    }

    // ear clipping
    public void RunConvex()
    {
        Debug.Log("é um poligono convexo");
        float totalX = 0;
        float totalY = 0;
        foreach (var item in Points)
        {
            totalX += item.Position.x;
            totalY += item.Position.y;
        }

        Vector3 centroid = new Vector3(totalX / Points.Count, totalY / Points.Count, -0.02f);

        Plotter.PlotPoint(centroid, false);
        Plotter.HideFirstPhase();
        PolygonMesh = BuildTriangleFan(centroid);
        PolygonMeshMatrix = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, Vector3.one);
    }

    public void RunConcave()
    {
        Debug.Log("é um poligono concavo");
        // mostra listas
        UpdateUI();
        ConvexUIList.transform.parent.gameObject.SetActive(true);

        List<int> indices = new List<int>();

        var node = EarPoints.First;

        while(node != null)
        {
            // pega o proximo e o anterior da ear tip baseado na lista de todos os pontos
            var next = GetNext(Points.Find(node.Value));
            var previous = GetPrevious(Points.Find(node.Value));

            // adiciona o triangulo
            indices.Add(node.Value.Index);
            indices.Add(previous.Value.Index);
            indices.Add(next.Value.Index);

            // remove ear tip da lista de pontos
            Points.Remove(node.Value);
            // remove ear
            EarPoints.Remove(node);

            // se sobrar menos que tres pontos não precisa mais fazer o processo
            if (Points.Count < 3) break;

            // reavaliar o proximo e o anterior da ear tip
            bool isNextConvex = ConvexPoints.Contains(next.Value);
            bool isPreviousConvex = ConvexPoints.Contains(previous.Value);

            // triangulo formado pelo proximo
            Vector3 n1 = GetPrevious(next).Value.Position;
            Vector3 n2 = next.Value.Position;
            Vector3 n3 = GetNext(next).Value.Position;

            Vector3 p1 = GetPrevious(previous).Value.Position;
            Vector3 p2 = previous.Value.Position;
            Vector3 p3 = GetNext(previous).Value.Position;

            // se era convexo, continuara convexo
            // se for concavo ele pode ter virado convexo, portanto calcular novamente sua orientacao e a avaliar se é concavo ou convexo
            if (!isNextConvex)
            {
                if (IsConvex(n1, n2, n3))
                {
                    ConcavePoints.Remove(next.Value);
                    ConvexPoints.AddLast(next.Value);
                    isNextConvex = true;
                }
            }

            if (!isPreviousConvex)
            {
                if (IsConvex(p1, p2, p3))
                {
                    ConcavePoints.Remove(previous.Value);
                    ConvexPoints.AddLast(previous.Value);
                    isPreviousConvex = true;
                }
            }

            // se ele o ponto é convexo ou se tornou convexo, reavaliar se ele é uma orelha (pois existe a possibilidade de nao ser mais depois da remoção da orelha mais acima)
            var isNextEar = IsEar(n1, n2, n3);
            var isPreviousEar = IsEar(p1, p2, p3);
            if (isNextConvex && isNextEar)
            {
                if (!EarPoints.Contains(next.Value))
                    EarPoints.AddFirst(next.Value);
            } 
            else if (!isNextEar)
            {
                if (EarPoints.Contains(next.Value))
                    EarPoints.Remove(next.Value);
            }

            if (isPreviousConvex && isPreviousEar)
            {
                if (!EarPoints.Contains(previous.Value))
                    EarPoints.AddLast(previous.Value);
            }
            else if (!isPreviousEar)
            {
                if (EarPoints.Contains(previous.Value))
                    EarPoints.Remove(previous.Value);
            }

            // passar para o primeiro da lista
            node = EarPoints.First;
        }

        Plotter.HideFirstPhase();
        PolygonMesh = BuildTriangleMesh(ref indices);
        PolygonMeshMatrix = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, Vector3.one);
    }

    public LinkedListNode<Point> GetPrevious(LinkedListNode<Point> node)
    {
        return node.Previous != null ? node.Previous : Points.Last;
    }

    public LinkedListNode<Point> GetNext(LinkedListNode<Point> node)
    {
        return node.Next != null ? node.Next : Points.First;
    }
   
    public Mesh BuildTriangleFan(Vector3 centroid)
    {
        Mesh mesh = new Mesh();
        int count = Points.Count + 1;
        Vector3[] vertices = new Vector3[count];
        Vector2[] uvs = new Vector2[count];
        int[] indices = new int[count * 3];

        int i = 0;
        int j = 0;
        vertices[i] = centroid;
        i++;
        foreach (var item in Points)
        {
            vertices[i] = item.Position;
            uvs[i] = item.Position;
            indices[j] = 0;
            indices[j + 1] = i + 1 >= count ? 1 : i + 1;
            indices[j + 2] = i;

            i++;
            j+= 3;
        }



        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }

    public Mesh BuildTriangleMesh(ref List<int> listIndices)
    {
        Mesh mesh = new Mesh();
        int count = Plotter.Points.Count;
        Vector3[] vertices = new Vector3[count];
        Vector2[] uvs = new Vector2[count];
        int[] indices = listIndices.ToArray();

        int i = 0;

        foreach (var item in Plotter.Points)
        {
            vertices[i] = item.Position;
            uvs[i] = item.Position;
            i++;
        }



        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }

    public void Restart()
    {
        if (PolygonMesh != null) PolygonMesh.Clear();
        PolygonMesh = null;

        ConvexUIList.transform.parent.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        string convexResult = "凸: ";
        string concaveResult = "凹: ";
        string earResult = "Ear: ";

        foreach (var item in ConvexPoints)
        {
            convexResult += item.Index.ToString() + " ";
        }

        foreach (var item in ConcavePoints)
        {
            concaveResult += item.Index.ToString() + " ";
        }

        foreach (var item in EarPoints)
        {
            earResult += item.Index.ToString() + " ";
        }

        ConvexUIList.GetComponent<UnityEngine.UI.Text>().text = convexResult;
        ConcaveUIList.GetComponent<UnityEngine.UI.Text>().text = concaveResult;
        EarUIList.GetComponent<UnityEngine.UI.Text>().text = earResult;
    }
}
