using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Default = 0,
    Type1 = 1,
    Type2 = 2
}

public class Object : MonoBehaviour
{
    [SerializeField]
    private ObjectType m_ObjectType;

    [SerializeField]
    private Sprite m_Sprite;

    public void SetAsCollect()
    {
        if (m_ObjectType == ObjectType.Type2)
        {
            gameObject.SetActive(false);
        }
        else if (m_ObjectType == ObjectType.Type1)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = m_Sprite;
        }
        m_ObjectType = ObjectType.Default;
    }

    public ObjectType GetObjectType()
    {
        return m_ObjectType;
    }
}
