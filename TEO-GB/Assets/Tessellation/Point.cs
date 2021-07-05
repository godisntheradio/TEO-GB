using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    string Desc;
    public Vector3 Position { get => gameObject.transform.position; }

    TextMesh Text;

    MeshRenderer Plane;

    void Start()
    {
        Text = GetComponentInChildren<TextMesh>();
        Plane = GetComponent<MeshRenderer>();
    }

    public void Initialize(string name)
    {
        Text = GetComponentInChildren<TextMesh>();
        Desc = name;
        Text.text = Desc;
    }

    public override string ToString()
    {
        return Desc;
    }

    public void Hide()
    {
        if (Plane == null)
            Plane = GetComponent<MeshRenderer>();

        Plane.enabled = false;
    }
}
