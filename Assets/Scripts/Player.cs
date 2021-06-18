using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Action Type1ObjectCollected;
    public static Action Type2ObjectCollected;

    private BoxCollider2D m_BoxCollider2D;
    public Cell InitialCell = null;

    private void Start()
    {
        m_BoxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        Vector2 l_Origin = m_BoxCollider2D.bounds.center;
        Vector2 l_Size = m_BoxCollider2D.bounds.size;

        Collider2D l_Type1Collider = Physics2D.OverlapBox(l_Origin, l_Size, 0, LayerMask.GetMask("Object"));
        if (l_Type1Collider != null && l_Type1Collider.gameObject.activeSelf)
        {
            Object l_Object = l_Type1Collider.GetComponent<Object>();
            if (l_Object != null)
            {
                if (l_Object.GetObjectType() == ObjectType.Type1)
                {
                    Type1ObjectCollected?.Invoke();
                    l_Object.SetAsCollect();
                }
                else if (l_Object.GetObjectType() == ObjectType.Type2)
                {
                    Type2ObjectCollected?.Invoke();
                    l_Object.SetAsCollect();
                }
            }

        }
    }
}
