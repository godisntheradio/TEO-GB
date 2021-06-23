using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCulling : MonoBehaviour
{
    
    void Start()
    {

        
    }

    void Update()
    {
        
    }

    public void OnDestroy()
    {
        
    }

    public void OnChange(CullingGroupEvent ev)
    {
        Debug.Log("mudou visibilidade");
    }
}
