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

public enum Directions
{
    None = -1,
    Left,
    Right,
    Top,
    Bottom
}

public class Enemy : MonoBehaviour
{
    private const int TOTAL_ROW = 6;
    private const int TOTAL_COLUMN = 6;

    private const float LAZY_ENEMY_SPEED = 0.5f;
    private const float ACTIVE_ENEMY_SPEED = 0.7f;
    private const float PATROLLER_ENEMY_SPEED = 0.5f;

    private float m_PatrollerSpeed = 1;
    private float m_ActiveEnemySpeed = 1;
    private float m_LazyEnemySpeed = 0;

    private SpriteRenderer m_Sprite = null;
    private Cell m_Cell = null;
    private List<Cell> m_Cells = new List<Cell>();

    [SerializeField]
    private EnemyType Type = EnemyType.None;

    [SerializeField]
    private Player m_Player = default;

    private Cell m_TargetCell = null;

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
        m_ActiveEnemySpeed = 0f;
        m_LazyEnemySpeed = 0f;
    }

    private void Start()
    {
        m_LazyEnemySpeed = LAZY_ENEMY_SPEED;
        m_ActiveEnemySpeed = ACTIVE_ENEMY_SPEED;
        m_PatrollerSpeed = PATROLLER_ENEMY_SPEED;

        m_ActiveEnemySpeed = GameDataContainer.EnemySpeed;

        if (GameDataContainer.IsLastLevelCleared)
        {
            GameDataContainer.EnemySpeed += 2;
            m_ActiveEnemySpeed = GameDataContainer.EnemySpeed;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            m_ActiveEnemySpeed = 0;
            m_LazyEnemySpeed = 0f;
        }
    }

    public void SetInitialCell(Cell cell, List<Cell> allCells)
    {
        m_Cell = cell;
        m_Cells = allCells;

        if (Type == EnemyType.Patroller)
        {
            PatrollerEnemyMovement();
        }
    }

    private void Update()
    {
        switch (Type)
        {
            case EnemyType.Lazy:
                LazyEnemyMovement();
                break;
            case EnemyType.Active:
                ActiveEnemyMovement();
                break;
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


    private bool IsOnEdgeOfGrid(int horiZontalDirection = 0, int verticalDirection = 0)
    {
        Cell cell = null;

        if (horiZontalDirection == -1)
        {
           cell =  m_Cells.Find(x => x.Column == 1 && Vector2.Distance(transform.position, x.Position) <= 0.1f);
        }

        else if (horiZontalDirection == 1)
        {
             cell = m_Cells.Find(x => x.Column == TOTAL_COLUMN && Vector2.Distance(transform.position, x.Position) <= 0.1f);
        }

        else if (verticalDirection == -1)
        {
             cell = m_Cells.Find(x => x.Row == 1 && Vector2.Distance(transform.position, x.Position) <= 0.1f);
        }

        else if (verticalDirection == 1)
        {
             cell = m_Cells.Find(x => x.Row == TOTAL_ROW && Vector2.Distance(transform.position, x.Position) <= 0.1f);
        }

        if (cell != null)
        {
            return true;
        }

        return false;
    }

    private void ActiveEnemyMovement()
    {
        transform.position += (m_Player.transform.position - transform.position).normalized * Time.deltaTime * m_ActiveEnemySpeed;
    }

    private void PatrollerEnemyMovement()
    {
        bool isTargetDirectionSelected = false;
        int randomDirection = -1;

        // check blocker are around the all four direction or not
        if (IsBlockedCellByNear(-1) && IsBlockedCellByNear(1)
           && IsBlockedCellByNear(0, -1) && IsBlockedCellByNear(0, 1))
        {
            return;
        }

        int horizontalDirection = 0, verticalDirection = 0;

        //lest find the random direction first, for find the target to movement
        while (!isTargetDirectionSelected)
        {
            randomDirection = UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);

            horizontalDirection = verticalDirection = 0;

            if ((Direction)randomDirection == Direction.Left)
            {
                horizontalDirection = -1;
            }
            else if ((Direction)randomDirection == Direction.Right)
            {
                    horizontalDirection = 1;
            }
            else if ((Direction)randomDirection == Direction.Top)
            {
                    verticalDirection = 1;
            }
            else if ((Direction)randomDirection == Direction.Bottom)
            {
                    verticalDirection = -1;
            }

            if (!IsBlockedCellByNear(horizontalDirection, verticalDirection) && !IsOnEdgeOfGrid(horizontalDirection, verticalDirection))
            {
                isTargetDirectionSelected = true;
                break;
            }
        }

        Debug.Log("direction: "+ (Direction)randomDirection);
        bool isTargetSelected = false;
        Vector2 l_Position = transform.position;

        if (randomDirection == (int)Direction.Left)
        {
            while (!isTargetSelected)
            {
                l_Position.x -= m_Cell.CellWidth;
                if (!IsLeftCellEmpty(l_Position))
                {
                    isTargetSelected = true;
                    l_Position.x += m_Cell.CellWidth;
                }
            }

            Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, l_Position) <= 0.1f && x.Column >= 1);
            m_TargetCell = cell;

        }

        if (randomDirection == (int)Direction.Right)
        {
            while (!isTargetSelected)
            {
                l_Position.x += m_Cell.CellWidth;
                if (!IsRightCellEmpty(l_Position))
                {
                    isTargetSelected = true;
                    l_Position.x -= m_Cell.CellWidth;
                }
            }

            Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, l_Position) <= 0.1f && x.Column <= TOTAL_COLUMN);
            m_TargetCell = cell;
        }

        if (randomDirection == (int)Direction.Top)
        {
            while (!isTargetSelected)
            {
                l_Position.y += m_Cell.CellHeight;
                if (!IsTopCellEmpty(l_Position))
                {
                    isTargetSelected = true;
                    l_Position.y -= m_Cell.CellHeight; 
                }
            }


            Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, l_Position) <= 0.1f && x.Row <= TOTAL_ROW);
            m_TargetCell = cell;
        }

        if (randomDirection == (int)Direction.Bottom)
        {
            while (!isTargetSelected)
            {
                l_Position.y -= m_Cell.CellHeight;
                if (!IsBottomCellEmpty(l_Position))
                {
                    isTargetSelected = true;
                    l_Position.y += m_Cell.CellHeight;
                }
            }


            Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, l_Position) <= 0.1f && x.Row >= 1);
            m_TargetCell = cell;
        }

        if (m_TargetCell == null)
        {
            PatrollerEnemyMovement();
        }

        if (randomDirection == (int)Direction.Bottom || randomDirection == (int)Direction.Top)
        {
            StartCoroutine(StartMovementY(verticalDirection, m_TargetCell.Position));
        }

        if (randomDirection == (int)Direction.Left || randomDirection == (int)Direction.Right)
        {
            StartCoroutine(StartMovementX(horizontalDirection, m_TargetCell.Position));
        }

       Debug.Log("target: "+ m_TargetCell.Row +" , "+ m_TargetCell.Column + "direction: "+ (int)randomDirection +" pos: "+ m_TargetCell.Position);
    }

    private bool IsLeftCellEmpty(Vector2 left)
    {
        Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, left) <= 0.1f && x.Column >= 1);
        if (cell != null)
        {
            return true;
        }
        return false;
    }

    private bool IsRightCellEmpty(Vector2 right)
    {
        Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, right) <= 0.1f && x.Column <= TOTAL_COLUMN);
        if (cell != null)
        {
            return true;
        }
        return false;
    }

    private bool IsTopCellEmpty(Vector2 top)
    {
        Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, top) <= 0.1f && x.Row <= TOTAL_ROW);
        if (cell != null)
        {
            return true;
        }
        return false;
    }

    private bool IsBottomCellEmpty(Vector2 bottom)
    {
        Cell cell = m_Cells.Find(x => x.IsOccupied == false && Vector2.Distance(x.Position, bottom) <= 0.1f && x.Row >= 1);
        if (cell != null)
        {
            return true;
        }
        return false;
    }

    private IEnumerator StartMovementX(int initialDirection, Vector2 target)
    {
        int direction = initialDirection;
        Vector2 l_Position = transform.position;
        Vector2 l_NewTarget = target;

        Vector2 l_UpdatePos = transform.position;

        do
        {
            yield return new WaitForEndOfFrame();

            if (direction == -1)
            {
                l_UpdatePos.x -= Time.deltaTime * m_PatrollerSpeed;
            }
            else if (direction == 1)
            {
                l_UpdatePos.x += Time.deltaTime * m_PatrollerSpeed;
            }
            transform.position = l_UpdatePos;
        } while (Vector2.Distance(transform.position, l_NewTarget) >= 0.1f);


        transform.position = l_NewTarget;
        l_NewTarget = l_Position;
        StopAllCoroutines();
        StartCoroutine(StartMovementX (- direction, l_NewTarget));
    }

    private IEnumerator StartMovementY(int initialDirection, Vector2 target)
    {
        int direction = initialDirection;
        Vector2 l_Position = transform.position;
        Vector2 l_NewTarget = target;

        Vector2 l_UpdatePos = transform.position;

        do
        {
            yield return new WaitForEndOfFrame();

            if (direction == -1)
            {
                l_UpdatePos.y -= Time.deltaTime * m_PatrollerSpeed;
            }
            else if (direction == 1)
            {
                l_UpdatePos.y += Time.deltaTime * m_PatrollerSpeed;
            }
            transform.position = l_UpdatePos;
        } while (Vector2.Distance(transform.position, l_NewTarget) >= 0.1f);


        transform.position = l_NewTarget;
        l_NewTarget = l_Position;
        StopAllCoroutines();
        StartCoroutine(StartMovementY(-direction, l_NewTarget));
    }
}
