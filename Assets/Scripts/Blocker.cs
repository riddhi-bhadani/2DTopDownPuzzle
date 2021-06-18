using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    private SpriteRenderer m_Sprite = null;
    private Vector2 m_Size = Vector2.zero;

    private void Start()
    {
        m_Sprite = GetComponentInChildren<SpriteRenderer>();
        m_Size = m_Sprite.sprite.rect.size;
    }

    public void SetScale(float m_CellWidth, float m_CellHeight)
    {
        m_CellWidth *= 100;
        m_CellHeight *= 100;

        m_Sprite = GetComponentInChildren<SpriteRenderer>();
        m_Size = m_Sprite.sprite.rect.size;

        Vector3 l_Scale = new Vector3(m_CellWidth/m_Size.x, m_CellHeight/m_Size.y);
        this.transform.localScale = l_Scale;
    }
}
