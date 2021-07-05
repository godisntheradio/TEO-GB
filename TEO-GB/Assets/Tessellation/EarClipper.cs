using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarClipper : MonoBehaviour
{
    public LinkedList<Point> Points;

    public LinkedList<Point> ConvexPoints;
    public LinkedList<Point> ConcavePoints;
    public LinkedList<Point> EarPoints;

    public PointPlotter Plotter;

    private void Awake()
    {
        Plotter = GetComponent<PointPlotter>();
    }

    public void Run()
    {
        Points = new LinkedList<Point>(Plotter.Points);
        ConvexPoints = new LinkedList<Point>();
        ConcavePoints = new LinkedList<Point>();
        EarPoints = new LinkedList<Point>();
        // ordem de criação dos pontos
        bool counterClockwise = true;

        var i1 = Points.First;


        while (i1 != null)
        {
            var i0 = i1.Previous == null ? Points.Last : i1.Previous;
            var i2 = i1.Next == null ? Points.First : i1.Next;

            var p1 = i0.Value.Position;
            var p2 = i1.Value.Position;
            var p3 = i2.Value.Position;

            var orientation = ComputeOrientation(p1, p2, p3);
            // é concavo
            if (orientation <= 0 && counterClockwise)
                ConcavePoints.AddLast(i1.Value);
            else // se for convexo, o ponto pode ser uma ear tip
            {
                ConvexPoints.AddLast(i1.Value);

                bool containsPointInside = false;
                foreach (var item in Points) // testa esse o triangulo formado por este ponto convexo, o anterior e o proximo contra todos os outros pontos (exceto os que formam esse triangulo) para verificar se é uma orelha
                {
                    if (item.Position == p1 || item.Position == p2 || item.Position == p3)
                        continue;

                    containsPointInside = IsPointInsideTriangle(item.Position, p1, p2, p3);

                    // se achar qualquer ponto dentro já da pra saber que não é uma orelha
                    if (containsPointInside)
                        break;
                }

                // adiciona na lista se nao encontrou nenhum ponto dentro do triangulo
                if (!containsPointInside)
                    EarPoints.AddLast(i1.Value);
            }

            i1 = i1.Next;
        }

        if (ConcavePoints.Count > 0)
            RunConcave();
        else
            RunConvex();

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

        Vector3 centroid = new Vector3(totalX / Points.Count, totalY / Points.Count);

        Plotter.PlotPoint(centroid);
        Plotter.HidePlanes();

    }

    public void RunConcave()
    {
        Debug.Log("é um poligono concavo");
    }
    public static float ComputeOrientation(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
    }

    public static bool IsPointInsideTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float d1, d2, d3;

        d1 = ComputeOrientation(p, p1, p2);
        d2 = ComputeOrientation(p, p2, p3);
        d3 = ComputeOrientation(p, p3, p1);

        bool cw = d1 < 0 && d2 < 0 && d3 < 0;
        bool ccw = d1 > 0 && d2 > 0 && d3 > 0;

        return cw || ccw;
    }
}
