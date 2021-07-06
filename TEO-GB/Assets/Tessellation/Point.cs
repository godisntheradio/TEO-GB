using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public int Index;
    public Vector3 Position { get => gameObject.transform.position; }

    TextMesh Text;

    MeshRenderer Plane;

    void Start()
    {
        Text = GetComponentInChildren<TextMesh>();
        Plane = GetComponent<MeshRenderer>();
    }

    public void Initialize(int index)
    {
        Text = GetComponentInChildren<TextMesh>();
        Index = index;
        Text.text = Index.ToString();
    }

    public override string ToString()
    {
        return Index.ToString();
    }

    public void Hide()
    {
        if (Plane == null)
            Plane = GetComponent<MeshRenderer>();

        Plane.enabled = false;
    }
}
