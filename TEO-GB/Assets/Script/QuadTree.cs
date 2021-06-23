using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    const int CAPACITY = 1;
    Vector4 BASE_COLOR = new Color(1, 0.4f, 0.39f, 1.0f);

    Bounds boundary;

    // top left
    QuadTree TL;
    // top right
    QuadTree TR;
    // bottom left
    QuadTree BL;
    // bottom right
    QuadTree BR;

    List<Transform> objects;

    public QuadTree(Bounds bounds)
    {
        this.boundary = bounds;
        this.TL = null;
        this.TR = null;
        this.BL = null;
        this.BR = null;

        objects = new List<Transform>();
    }

    public bool Insert(Transform transform)
    {
        // testa no chao para o ponto central nunca ficar fora da altura da bounding box
        if (!boundary.Contains(new Vector3(transform.position.x, 0, transform.position.z)))
            return false;

        if (objects.Count < CAPACITY && TL == null)
        {
            objects.Add(transform);
            return true;
        }

        if (TL == null)
            Subdivide(transform);

        if (TL.Insert(transform)) return true;
        if (TR.Insert(transform)) return true;
        if (BL.Insert(transform)) return true;
        if (BR.Insert(transform)) return true;

        return false;
    }
    // passa tambem o transform para criar os bounds 
    private void Subdivide(Transform transform)
    {
        float halfX = (boundary.extents.x / 2) + boundary.center.x;
        float halfXMinus = (-boundary.extents.x / 2) + boundary.center.x;
        float halfZ = (boundary.extents.z / 2) + boundary.center.z;
        float halfZMinus = (-boundary.extents.z / 2) + boundary.center.z;

        var childBounds = BuildChildBoundSize(transform);

        TL = new QuadTree(new Bounds(new Vector3(halfX, transform.position.y, halfZ), childBounds));
        TR = new QuadTree(new Bounds(new Vector3(halfX, transform.position.y, halfZMinus), childBounds));
        BL = new QuadTree(new Bounds(new Vector3(halfXMinus, transform.position.y, halfZ), childBounds));
        BR = new QuadTree(new Bounds(new Vector3(halfXMinus, transform.position.y, halfZMinus), childBounds));
    }

    public List<Transform> QueryRange(Bounds range)
    {
        List<Transform> inRange = new List<Transform>();

        if (!boundary.Intersects(range))
            return inRange;

        for (int i = 0; i < objects.Count; i++)
        {
            if (range.Contains(objects[i].position))
                inRange.Add(objects[i]);
        }

        if (TL == null)
            return inRange;

        inRange.AddRange(TL.QueryRange(range));
        inRange.AddRange(TR.QueryRange(range));
        inRange.AddRange(BL.QueryRange(range));
        inRange.AddRange(BR.QueryRange(range));

        return inRange;
    }

    public Vector3 BuildChildBoundSize(Transform transform)
    {
        return new Vector3(boundary.extents.x, transform.localScale.y, boundary.extents.z);
    }

    public bool IsHigherThanBounds(Transform transform)
    {
        return transform.localScale.y > boundary.size.y;
    }

    public void GetMatrices(ref List<Matrix4x4> matrices)
    {
        matrices.Add(this.GetMatrix());
        
        if (TL != null) TL.GetMatrices(ref matrices);
        if (TR != null) TR.GetMatrices(ref matrices);
        if (BL != null) BL.GetMatrices(ref matrices);
        if (BR != null) BR.GetMatrices(ref matrices);
    }

    public Matrix4x4 GetMatrix()
    {
        return Matrix4x4.TRS(boundary.center, Quaternion.identity, boundary.extents);
    }
}