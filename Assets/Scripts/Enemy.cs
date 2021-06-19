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

    private Coroutine m_ActiveEnemyCoroutine, m_ActiveMovementCoroutine, m_PatrollerMovementXCoroutine, m_PatrollerMovementYCoroutine;

    [SerializeField]
    private EnemyType Type = EnemyType.None;

    [SerializeField]
    private Player m_Player = default;

    private Cell m_TargetCell = new Cell();
    int m_randomDirection = -1;
    int m_horizontalDirection = 0, m_verticalDirection = 0;

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

        if (Type == EnemyType.Active)
        {
            ActiveEnemyMovement();
        }
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
        m_ActiveEnemyCoroutine = StartCoroutine(StartActiveEnemyMovement());
    }

    private IEnumerator StartActiveEnemyMovement()
    {
        if (IsBlockedCellByNear(-1) && IsBlockedCellByNear(1)
           && IsBlockedCellByNear(0, -1) && IsBlockedCellByNear(0, 1))
        {
            yield return new WaitForEndOfFrame();
            StopCoroutine(m_ActiveEnemyCoroutine);
        }

        m_ActiveMovementCoroutine = StartCoroutine(ActiveEnemyMovementoroutine());
        yield return new WaitForSeconds(UnityEngine.Random.Range(10, 15));
        m_ActiveEnemySpeed = 0;

        if (m_PatrollerMovementXCoroutine != null)
        {
            StopCoroutine(m_PatrollerMovementXCoroutine);
        }
        if (m_PatrollerMovementYCoroutine != null)
        {
            StopCoroutine(m_PatrollerMovementYCoroutine);
        }
        if (m_ActiveMovementCoroutine != null)
        {
            StopCoroutine(m_ActiveMovementCoroutine);
        }

        yield return new WaitForSeconds(UnityEngine.Random.Range(2, 5));
        m_ActiveEnemySpeed = ACTIVE_ENEMY_SPEED;
        m_ActiveEnemyCoroutine = StartCoroutine(StartActiveEnemyMovement());
    }

    private void PatrollerEnemyMovement()
    {
        SetTargetForMovement();

        if (m_randomDirection == (int)Direction.Bottom || m_randomDirection == (int)Direction.Top)
        {
           m_PatrollerMovementXCoroutine = StartCoroutine(StartMovementY(m_verticalDirection, m_TargetCell.Position));
        }

        if (m_randomDirection == (int)Direction.Left || m_randomDirection == (int)Direction.Right)
        {
            m_PatrollerMovementYCoroutine = StartCoroutine(StartMovementX(m_horizontalDirection, m_TargetCell.Position));
        }
    }

    private IEnumerator FindCell()
    {
        bool isTargetDirectionSelected = false;
        m_randomDirection = -1;

        // check blocker are around the all four direction or not
        m_horizontalDirection = 0; m_verticalDirection = 0;

        //lest find the random direction first, for find the target to movement
        while (!isTargetDirectionSelected)
        {
            yield return new WaitForEndOfFrame(); ;
            m_randomDirection = UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);

            m_horizontalDirection = m_verticalDirection = 0;

            if ((Direction)m_randomDirection == Direction.Left)
            {
                m_horizontalDirection = -1;
            }
            else if ((Direction)m_randomDirection == Direction.Right)
            {
                m_horizontalDirection = 1;
            }
            else if ((Direction)m_randomDirection == Direction.Top)
            {
                m_verticalDirection = 1;
            }
            else if ((Direction)m_randomDirection == Direction.Bottom)
            {
                m_verticalDirection = -1;
            }

            if (!IsBlockedCellByNear(m_horizontalDirection, m_verticalDirection) && !IsOnEdgeOfGrid(m_horizontalDirection, m_verticalDirection))
            {
                isTargetDirectionSelected = true;
            }
        }

        bool isTargetSelected = false;
        Vector2 l_Position = transform.position;

        if (m_randomDirection == (int)Direction.Left)
        {
            while (!isTargetSelected)
            {
                yield return new WaitForEndOfFrame(); ;

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

        if (m_randomDirection == (int)Direction.Right)
        {
            while (!isTargetSelected)
            {
                yield return new WaitForEndOfFrame(); ;

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

        if (m_randomDirection == (int)Direction.Top)
        {
            while (!isTargetSelected)
            {
                yield return new WaitForEndOfFrame(); ;

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

        if (m_randomDirection == (int)Direction.Bottom)
        {
            while (!isTargetSelected)
            {
                yield return new WaitForEndOfFrame(); ;

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
            StartCoroutine(FindCell());
        }
    }

    private IEnumerator ActiveEnemyMovementoroutine()
    {
        StartCoroutine(FindCell());

        while (m_TargetCell == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
            if (m_randomDirection == (int)Direction.Bottom || m_randomDirection == (int)Direction.Top)
            {
                m_PatrollerMovementXCoroutine = StartCoroutine(StartMovementY(m_verticalDirection, m_TargetCell.Position));
            }

            if (m_randomDirection == (int)Direction.Left || m_randomDirection == (int)Direction.Right)
            {
                m_PatrollerMovementYCoroutine = StartCoroutine(StartMovementX(m_horizontalDirection, m_TargetCell.Position));
            }
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


    private void SetTargetForMovement()
    {
        bool isTargetDirectionSelected = false;
        m_randomDirection = -1;

        // check blocker are around the all four direction or not
        if (IsBlockedCellByNear(-1) && IsBlockedCellByNear(1)
           && IsBlockedCellByNear(0, -1) && IsBlockedCellByNear(0, 1))
        {
            return;
        }

        m_horizontalDirection = 0; m_verticalDirection = 0;

        //lest find the random direction first, for find the target to movement
        while (!isTargetDirectionSelected)
        {
            m_randomDirection = UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);

            Debug.Log("direction: "+ m_randomDirection);
            m_horizontalDirection = m_verticalDirection = 0;

            if ((Direction)m_randomDirection == Direction.Left)
            {
                m_horizontalDirection = -1;
            }
            else if ((Direction)m_randomDirection == Direction.Right)
            {
                m_horizontalDirection = 1;
            }
            else if ((Direction)m_randomDirection == Direction.Top)
            {
                m_verticalDirection = 1;
            }
            else if ((Direction)m_randomDirection == Direction.Bottom)
            {
                m_verticalDirection = -1;
            }

            if (!IsBlockedCellByNear(m_horizontalDirection, m_verticalDirection) && !IsOnEdgeOfGrid(m_horizontalDirection, m_verticalDirection))
            {
                isTargetDirectionSelected = true;
                break;
            }
        }

        bool isTargetSelected = false;
        Vector2 l_Position = transform.position;

        if (m_randomDirection == (int)Direction.Left)
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

        if (m_randomDirection == (int)Direction.Right)
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

        if (m_randomDirection == (int)Direction.Top)
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

        if (m_randomDirection == (int)Direction.Bottom)
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
           SetTargetForMovement();
        }
    }

    private IEnumerator StartMovementX(int initialDirection, Vector2 target)
    {
        float l_Speed = 0f;
        if (Type == EnemyType.Patroller)
        {
            l_Speed = m_PatrollerSpeed;
        }

        if (Type == EnemyType.Active)
        {
            l_Speed = m_ActiveEnemySpeed;
        }

        int direction = initialDirection;
        Vector2 l_Position = transform.position;
        Vector2 l_NewTarget = target;

        Vector2 l_UpdatePos = transform.position;

        do
        {
            yield return new WaitForEndOfFrame();

            if (direction == -1)
            {
                l_UpdatePos.x -= Time.deltaTime * l_Speed;
            }
            else if (direction == 1)
            {
                l_UpdatePos.x += Time.deltaTime * l_Speed;
            }
            transform.position = l_UpdatePos;
        } while (Vector2.Distance(transform.position, l_NewTarget) >= 0.1f);


        transform.position = l_NewTarget;
        l_NewTarget = l_Position;
        m_PatrollerMovementXCoroutine = StartCoroutine(StartMovementX (- direction, l_NewTarget));
    }

    private IEnumerator StartMovementY(int initialDirection, Vector2 target)
    {
        float l_Speed = 0f;
        if (Type == EnemyType.Patroller)
        {
            l_Speed = m_PatrollerSpeed;
        }

        if (Type == EnemyType.Active)
        {
            l_Speed = m_ActiveEnemySpeed;
        }

        int direction = initialDirection;
        Vector2 l_Position = transform.position;
        Vector2 l_NewTarget = target;

        Vector2 l_UpdatePos = transform.position;

        do
        {
            yield return new WaitForEndOfFrame();

            if (direction == -1)
            {
                l_UpdatePos.y -= Time.deltaTime * l_Speed;
            }
            else if (direction == 1)
            {
                l_UpdatePos.y += Time.deltaTime * l_Speed;
            }
            transform.position = l_UpdatePos;
        } while (Vector2.Distance(transform.position, l_NewTarget) >= 0.1f);


        transform.position = l_NewTarget;
        l_NewTarget = l_Position;
        m_PatrollerMovementYCoroutine = StartCoroutine(StartMovementY(-direction, l_NewTarget));
    }
}
