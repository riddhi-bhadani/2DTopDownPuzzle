using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    None = -1,
    Lazy,
    Active,
    Patroller
}
public class Enemy : MonoBehaviour
{
    private float m_Speed = 1;
    private SpriteRenderer m_Sprite = null;

    [SerializeField]
    private EnemyType Type = EnemyType.None;

    private void Start()
    {
        m_Sprite = GetComponentInChildren<SpriteRenderer>();

        m_Speed = GameDataContainer.EnemySpeed;

        if (GameDataContainer.IsLastLevelCleared)
        {
            GameDataContainer.EnemySpeed += 2;
            m_Speed = GameDataContainer.EnemySpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            m_Speed = 0;
        }
    }
}
