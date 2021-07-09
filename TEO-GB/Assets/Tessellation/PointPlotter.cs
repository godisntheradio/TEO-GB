using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointPlotter : MonoBehaviour
{
    public GameObject PointObject;
    [Range(3, 50)]
    public int depth = 10;

    public LinkedList<Point> Points;

    private const string SAFE = "Safe";
    private const string POINT = "Point";

    public bool IsCounterClockwise;

    private LineRenderer Line;
    public Material LineMaterial;

    private bool FinishedPolygon = false;

    public UnityEvent ClosedPolygon;
    public UnityEvent ResetPolygon;

    void Start()
    {
        Points = new LinkedList<Point>();
        Line = gameObject.AddComponent<LineRenderer>();
        Line.material = LineMaterial;
        Line.startWidth = 0.03f;
        Line.endWidth = 0.03f;
        var color = new Color(120, 37, 37);
        Line.startColor = color;
        Line.endColor = color;
        Line.receiveShadows = false;
        Line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

    }

    void Update()
    {
        GameObject HitObject = null;
        HitObject = Ray();
        if (HitObject && !FinishedPolygon)
        {
            switch (HitObject.tag)
            {
                case SAFE:
                    {
                        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
                        PlotPoint(worldPos);
                    }
                    break;
                case POINT:
                    {
                        AddVisualPoint(HitObject.transform.position);
                        FinishedPolygon = true;
                        ClosedPolygon.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private GameObject Ray()
    {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    public void Begin()
    {
        if (!FinishedPolygon)
        {
            // Apenas adiciona na lista da linha para fechar o poligono
            AddVisualPoint(Points.First.Value.transform.position);
            FinishedPolygon = true;
            ClosedPolygon.Invoke();
        }
    }

    public void Restart()
    {
        FinishedPolygon = false;
        ResetPolygon.Invoke();
        Line.enabled = true;
        Line.positionCount = 0;
        Points.Clear();
        foreach (Transform item in gameObject.transform)
        {
            Destroy(item.gameObject);
        }
    }

    public void PlotPoint(Vector3 pos, bool connectToLine = true)
    {
        var point = Instantiate(PointObject, pos, PointObject.transform.rotation, gameObject.transform).GetComponent<Point>();
        if (connectToLine) AddVisualPoint(pos);
        // inicializa parte visual
        point.Initialize(Points.Count);
        // adiciona na lista encadeada
        Points.AddLast(point);

        if (Points.Count == 3)
        {
            IsCounterClockwise = Util.Math.ComputeOrientation(Points.First.Value.Position, Points.First.Next.Value.Position, Points.First.Next.Next.Value.Position) <= 0;
        }
    }

    public void AddVisualPoint(Vector3 pos)
    {
        // adiciona no line renderer a partir do indice 0
        Line.positionCount = Points.Count + 1;
        Line.SetPosition(Points.Count, pos);
    }

    public void HideFirstPhase()
    {
        foreach (var item in Points)
        {
            item.Hide();
        }
        Line.enabled = false;
    }
}