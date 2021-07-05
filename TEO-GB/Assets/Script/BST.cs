using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BST<T>
{
    public BSTNode<T> Root;
    public BST()
    {
        
    }

    public BST(int key)
    {
        Root = new BSTNode<T>(key);
    }

    public BST(int key, T data)
    {
        Root = new BSTNode<T>(key, data);
    }

    public void Insert(int key, T data)
    {
        if (Root == null)
        {
            Root = new BSTNode<T>(key, data);
            return;
        }

        BSTNode<T> current = Root;
        BSTNode<T> parent = null;

        bool found = false;

        while (!found)
        {
            parent = current;

            // vai para a esquerda
            if (key < parent.Key)
            {
                current = current.LNode;

                if (current == null)
                {
                    parent.LNode = new BSTNode<T>(key, data);
                    found = true;
                }
            }
            else // vai para a direita
            {
                current = current.RNode;

                if (current == null)
                {
                    parent.RNode = new BSTNode<T>(key, data);
                    found = true;
                }
            }
        }
    }
}

public class BSTNode<T>
{
    public int Key { get => key; }
    public T Data { get => data; }
    public bool IsLeaf { get => RNode == null && LNode == null; }

    public BSTNode<T> RNode;
    public BSTNode<T> LNode;

    private int key;
    private T data;

    public BSTNode(int key)
    {
        this.key = key;
        this.RNode = null;
        this.LNode = null;
    }

    public BSTNode(int key, T data)
    {
        this.key = key;
        this.data = data;
        this.RNode = null;
        this.LNode = null;
    }

}