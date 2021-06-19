using System;
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
    private const float LAZY_ENEMY_SPEED = 0.5f;

    private float m_Speed = 1;
    private SpriteRenderer m_Sprite = null;
    private Cell m_Cell = null;
    private List<Cell> m_Cells = new List<Cell>();

    [SerializeField]
    private EnemyType Type = EnemyType.None;

    [SerializeField]
    private Player m_Player = default;
    private float m_LazyEnemySpeed = 0;

    private void OnEnable()
    {
        Player.CollideWithEnemy += OnCollideWithEnemy;
    }

    private void OnDisable()
    {
        Player.CollideWithEnemy -= OnCollideWithEnemy;
    }

    private void OnCollideWithEnemy()
    {
        m_Speed = 0f;
        m_LazyEnemySpeed = 0f;
    }

    private void Start()
    {
        m_LazyEnemySpeed = LAZY_ENEMY_SPEED;

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
            m_LazyEnemySpeed = 0f;
        }
    }

    public void SetInitialCell(Cell cell, List<Cell> allCells)
    {
        m_Cell = cell;
        m_Cells = allCells;
    }

    private void Update()
    {
        if (Type == EnemyType.Lazy)
        {
            LazyEnemyMovement();
        }
    }

    private void LazyEnemyMovement()
    {
        Vector2 l_PlayerPos = m_Player.transform.position;
        Vector2 l_CurrentPos = transform.position;

        Vector2 l_Direction = l_PlayerPos - l_CurrentPos;

        if (Mathf.Abs(l_Direction.x) <= m_Cell.CellWidth * 2f && Mathf.Abs(l_Direction.y) <= 0.1f)
        {
            if (l_Direction.x < 0f && !IsBlockedCellByNear(-1))
            {
                // move enemy into left direction 
                l_CurrentPos.x -= Time.deltaTime * m_LazyEnemySpeed;
                this.transform.position = l_CurrentPos;
            }
            else if(l_Direction.x > 0f && !IsBlockedCellByNear(1))
            {
                // move enemy into right direction 
                l_CurrentPos.x += Time.deltaTime * m_LazyEnemySpeed;
                this.transform.position = l_CurrentPos;
            }
        }
        else if (Mathf.Abs(l_Direction.y) <= m_Cell.CellHeight * 2f && Mathf.Abs(l_Direction.x) <= 0.1f)
        {
            if (l_Direction.y < 0f && !IsBlockedCellByNear(0, -1))
            {
                // move enemy into bottom direction
                l_CurrentPos.y -= Time.deltaTime * m_LazyEnemySpeed;
                this.transform.position = l_CurrentPos;
            }
            else if(l_Direction.y > 0f && !IsBlockedCellByNear(0, 1))
            {
                // move enemy into top direction
                l_CurrentPos.y += Time.deltaTime * m_LazyEnemySpeed;
                this.transform.position = l_CurrentPos;
            }
        }
        else
        {
            // stop enemy movement and set on cell's center position
        }
    }

    private bool IsBlockedCellByNear(int horiZontalDirection = 0, int verticalDirection = 0)
    {
        bool isBlockerDetected = false;

        List<Cell> blockerCells = m_Cells.FindAll(x => x.CellType == CellType.Blocked);

        for (int i = 0; i < blockerCells.Count; i++)
        {
            Cell cell = blockerCells[i];

            Vector2 direction = cell.Position - transform.position;
            float distance =  Vector2.Distance(cell.Position, transform.position);
            if (horiZontalDirection == -1) //want to check blocker in left direction
            {
                if (direction.x < 0 && Mathf.Abs(direction.y) <= 0.1f && Mathf.Abs(direction.x) <= m_Cell.CellWidth)
                {
                    return true;
                }
            }
            else if(horiZontalDirection == 1) //want to check blocker in right direction
            {
                if (direction.x > 0 && Mathf.Abs(direction.y) <= 0.1f && Mathf.Abs(direction.x) <= m_Cell.CellWidth)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < blockerCells.Count; i++)
        {
            Cell cell = blockerCells[i];

            Vector2 direction = cell.Position - transform.position;
            float distance = Vector2.Distance(cell.Position, transform.position);

            if (verticalDirection == -1) //want to check blocker in bottom direction
            {
                if (direction.y < 0 && Mathf.Abs(direction.x) <= 0.1f && Mathf.Abs(direction.y) <= m_Cell.CellHeight)
                {
                    return true;
                }
            }
            else if (verticalDirection == 1) //want to check blocker in top direction
            {
                if (direction.y > 0 && Mathf.Abs(direction.x) <= 0.1f && Mathf.Abs(direction.y) <= m_Cell.CellHeight)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ActiveEnemyMovement()
    {
    }

    private void PatrollerEnemyMovement()
    {

    }
}
