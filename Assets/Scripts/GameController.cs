using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Direction
{
    Default = 0,
    Left = 1,
    Right = 2,
    Top = 3,
    Bottom = 4
}

public enum CellType
{
    None = -1,
    Start,
    Exit,
    Blocked,
    Coin
}

public class Cell
{
    public int Row;
    public int Column;
    public bool IsOccupied;
    public CellType CellType = CellType.None;
    public Vector3 Position = Vector3.zero;
}

public class GameController : MonoBehaviour
{
    private static float TOP_HEIGHT = 0;
    private static float BOTTOM_HEIGHT = 0;
    private static float CAMERA_SIZE = 5f;
    private const int MINIMUM_BLOCK_RANGE = 4;
    private const int MAXIMUM_BLOCK_RANGE = 6;
    private const float INITIAL_SPEED = 1f;
    private const float CHANGE_DIRECTION_OFFSET = 0.5f;

    private List<Cell> m_Cells = new List<Cell>();

    [SerializeField]
    private Canvas m_Canvas;

    private int m_TotalRows = 6;

    private int m_TotalColumns = 6;

    [SerializeField]
    private Transform m_Parent;

    [SerializeField]
    private GameObject m_Character, m_StartPoint, m_ExitPoint;

    [SerializeField]
    private GameObject m_BlockPrefab, m_CoinPrefab;

    [SerializeField]
    private GameObject[] m_Enemies;

    [SerializeField]
    private Button m_LeftButton;

    [SerializeField]
    private Button m_RightButton;

    [SerializeField]
    private Button m_TopButton;

    [SerializeField]
    private Button m_BottomButton;

    [SerializeField]
    private GameObject m_GameOverPopup, m_LoadNewLevelBtn, m_RestartGameBtn;

    private float m_Left;
    private float m_Right;
    private float m_Top;
    private float m_Bottom;
    private float m_CellWidth;
    private float m_CellHeight;
    private float m_Speed = 1f;

    private bool m_IsLeft = false;
    private bool m_IsRight = false;
    private bool m_IsTop = false;
    private bool m_IsBottom = false;
    private int m_CollectedCoins = 0;

    private bool m_IsIntersectInRow = true;
    private bool m_IsIntersectInColumn = true;
    private Direction m_CurrentDirection = Direction.Default;

    private Cell m_StartPointCell;
    private int m_TotalCoins = 0;

    private void OnEnable()
    {
        Player.CollideWithBlocked += OnBlockerDetected;
        Player.CoinCollected += OnCoinCollection;
        Player.ReachedOnExit += ReachedOnExit;
        Player.CollideWithEnemy += OnCollideWithEnemy;
    }

    private void OnDisable()
    {
        Player.CollideWithBlocked -= OnBlockerDetected;
        Player.ReachedOnExit -= ReachedOnExit;
        Player.CoinCollected -= OnCoinCollection;
        Player.CollideWithEnemy -= OnCollideWithEnemy;
    }

    private void Start()
    {
        m_Speed = INITIAL_SPEED;

        InitCells();
        SetStartPoint();
        SetExitPoint();
        SetCharacterOnStartPoint();
        GenerateBlock();
        GenerateEnemies();
        GenerateCoins();

        m_GameOverPopup.SetActive(false);

        EventTrigger l_LeftDown = m_LeftButton.gameObject.AddComponent<EventTrigger>();
        var l_LeftPointerDown = new EventTrigger.Entry();
        l_LeftPointerDown.eventID = EventTriggerType.PointerDown;
        l_LeftPointerDown.callback.AddListener((e) =>
        {
            if (m_CurrentDirection == Direction.Default)
            {
                m_CurrentDirection = Direction.Left;
            }

            m_Speed = INITIAL_SPEED;

            m_IsLeft = true;
        });
        l_LeftDown.triggers.Add(l_LeftPointerDown);

        EventTrigger l_LeftUp = m_LeftButton.gameObject.AddComponent<EventTrigger>();
        var l_LeftPointerUp = new EventTrigger.Entry();
        l_LeftPointerUp.eventID = EventTriggerType.PointerUp;
        l_LeftPointerUp.callback.AddListener((e) => m_IsLeft = false);
        l_LeftUp.triggers.Add(l_LeftPointerUp);

        EventTrigger l_RightDown = m_RightButton.gameObject.AddComponent<EventTrigger>();
        var l_RightPointerDown = new EventTrigger.Entry();
        l_RightPointerDown.eventID = EventTriggerType.PointerDown;
        l_RightPointerDown.callback.AddListener((e) =>
        {
            if (m_CurrentDirection == Direction.Default)
            {
                m_CurrentDirection = Direction.Right;
            }
            m_Speed = INITIAL_SPEED;

            m_IsRight = true;
        });
        l_RightDown.triggers.Add(l_RightPointerDown);

        EventTrigger l_RightUp = m_RightButton.gameObject.AddComponent<EventTrigger>();
        var l_RightPointerUp = new EventTrigger.Entry();
        l_RightPointerUp.eventID = EventTriggerType.PointerUp;
        l_RightPointerUp.callback.AddListener((e) => m_IsRight = false);
        l_RightUp.triggers.Add(l_RightPointerUp);

        EventTrigger l_TopDown = m_TopButton.gameObject.AddComponent<EventTrigger>();
        var l_TopPointerDown = new EventTrigger.Entry();
        l_TopPointerDown.eventID = EventTriggerType.PointerDown;
        l_TopPointerDown.callback.AddListener((e) =>
        {
            if (m_CurrentDirection == Direction.Default)
            {
                m_CurrentDirection = Direction.Top;
            }
            m_Speed = INITIAL_SPEED;

            m_IsTop = true;
        });
        l_TopDown.triggers.Add(l_TopPointerDown);

        EventTrigger l_TopUp = m_TopButton.gameObject.AddComponent<EventTrigger>();
        var l_TopPointerUp = new EventTrigger.Entry();
        l_TopPointerUp.eventID = EventTriggerType.PointerUp;
        l_TopPointerUp.callback.AddListener((e) => m_IsTop = false);
        l_TopUp.triggers.Add(l_TopPointerUp);

        EventTrigger l_BottomDown = m_BottomButton.gameObject.AddComponent<EventTrigger>();
        var l_BottomPointerDown = new EventTrigger.Entry();
        l_BottomPointerDown.eventID = EventTriggerType.PointerDown;
        l_BottomPointerDown.callback.AddListener((e) =>
        {
            if (m_CurrentDirection == Direction.Default)
            {
                m_CurrentDirection = Direction.Bottom;
            }
            m_Speed = INITIAL_SPEED;

            m_IsBottom = true;
        });
        l_BottomDown.triggers.Add(l_BottomPointerDown);

        EventTrigger l_BottomUp = m_BottomButton.gameObject.AddComponent<EventTrigger>();
        var l_BottomPointerUp = new EventTrigger.Entry();
        l_BottomPointerUp.eventID = EventTriggerType.PointerUp;
        l_BottomPointerUp.callback.AddListener((e) => m_IsBottom = false);
        l_BottomUp.triggers.Add(l_BottomPointerUp);
    }

    private void InitCells()
    {
        m_Top = CAMERA_SIZE - (TOP_HEIGHT * m_Canvas.transform.localScale.x);
        m_Bottom = -CAMERA_SIZE + (BOTTOM_HEIGHT * m_Canvas.transform.localScale.x);
        m_Right = (CAMERA_SIZE * (float)Screen.width) / (float)Screen.height;
        m_Left = -m_Right;

        m_CellWidth = (m_Right * 2f) / m_TotalColumns;
        m_CellHeight = (Mathf.Abs(m_Top) + Mathf.Abs(m_Bottom)) / m_TotalRows;

        if (m_CellWidth >= m_CellHeight)
        {
            m_CellWidth = m_CellHeight;
            m_Right = (m_CellWidth * m_TotalColumns) / 2f;
            m_Left = -m_Right;
        }
        else
        {
            m_CellHeight = m_CellWidth;
            m_Top = (m_CellHeight * m_TotalRows) / 2f;
            m_Bottom = -m_Top;
        }

        //draw Grid 6x6
        for (float x = m_Left; x <= m_Right; x += m_CellWidth)
        {
            DrawLine(new Vector3(x, m_Top, 0), new Vector3(x, m_Bottom, 0));
        }

        for (float y = m_Bottom; y <= m_Top; y += m_CellHeight)
        {
            DrawLine(new Vector3(m_Left, y, 0), new Vector3(m_Right, y, 0));
        }

        //define Cells
        for (int row = 0; row < m_TotalRows; row++)
        {
            for (int col = 0; col < m_TotalColumns; col++)
            {
                Cell cell = new Cell();
                cell.Row = row + 1;
                cell.Column = col + 1;
                cell.IsOccupied = false;
                cell.Position = FindPositionBaseOnRowAndColumn(cell.Row, cell.Column);
                cell.CellType = CellType.None;
                m_Cells.Add(cell);
            }
        }
    }

    private void SetStartPoint()
    {
        // Here, We set Start point randomly only on First Column
        int l_RandomRow = Random.Range(1, m_TotalRows + 1);
        Cell startCell = m_Cells.Single(cell => cell.Row == l_RandomRow && cell.Column == 1);
        startCell.IsOccupied = true;
        startCell.CellType = CellType.Start;
        m_StartPoint.transform.position = startCell.Position;

       SpriteRenderer sprite = m_StartPoint.GetComponentInChildren<SpriteRenderer>();
       Vector2 size = sprite.sprite.rect.size;

        Vector3 l_Scale = new Vector3((m_CellWidth * 100) / size.x, (m_CellHeight * 100)/ size.y);
        m_StartPoint.transform.localScale = l_Scale;

        m_StartPointCell = startCell;
    }

    private void SetExitPoint()
    {
        // Here, We set Exit point randomly only on Last Column
        int l_RandomRow = Random.Range(1, m_TotalRows + 1);
        Cell exitCell = m_Cells.Single(cell => cell.Row == l_RandomRow && cell.Column == m_TotalColumns);
        exitCell.IsOccupied = true;
        exitCell.CellType = CellType.Exit;
        m_ExitPoint.transform.position = exitCell.Position;

        Exit exit = m_ExitPoint.GetComponent<Exit>();
        exit.SetScale(m_CellWidth, m_CellHeight);
    }

    private void SetCharacterOnStartPoint()
    {
        //Set Player on Start Cell
        m_Character.transform.position = m_StartPointCell.Position; ;
        Player player = m_Character.GetComponent<Player>();
        player.InitialCell = m_StartPointCell;
    }

    private void GenerateBlock()
    {
        int l_TotalBlockToGenerate = Random.Range(MINIMUM_BLOCK_RANGE, MAXIMUM_BLOCK_RANGE + 1);

        for (int i = 0; i < l_TotalBlockToGenerate; i++)
        {
           Cell cell = GetRandomCell();

            if (cell != null)
            {
                GameObject block = Instantiate(m_BlockPrefab, m_Parent);
                Blocker blocker = block.GetComponent<Blocker>();
                block.transform.position = cell.Position;
                cell.IsOccupied = true;
                cell.CellType = CellType.Blocked;
                blocker.SetScale(m_CellWidth, m_CellHeight);
            }
        }
    }

    private void GenerateEnemies()
    {
        for (int i = 0; i < m_Enemies.Length; i++)
        {
            Cell cell = GetRandomCell();

            if (cell != null)
            {
                m_Enemies[i].transform.position = cell.Position;
            }
        }
    }

    private void GenerateCoins()
    {
        foreach (var cell in m_Cells)
        {
            if (!cell.IsOccupied)
            {
               GameObject coin =  Instantiate(m_CoinPrefab, m_Parent);
               coin.transform.position = cell.Position;
               cell.IsOccupied = true;
               cell.CellType = CellType.Coin;
               m_TotalCoins++;
            }
        }
    }

    private Cell GetRandomCell()
    {
        bool isRandomCellPicked = false;
        while (!isRandomCellPicked)
        {
            int l_RandomIndex = Random.Range(0, m_Cells.Count);
            if (!m_Cells[l_RandomIndex].IsOccupied)
            {
                Cell cell = m_Cells[l_RandomIndex];
                isRandomCellPicked = true;
                return cell;
            }
        }

        return null;
    }

    private void DrawLine(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject l_Line = new GameObject();
        LineRenderer lineRenderer = l_Line.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.material.color = Color.black;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.sortingOrder = -20;
        l_Line.transform.SetParent(m_Parent);
    }

    private Vector3 FindPositionBaseOnRowAndColumn(int row, int column)
    {
        Vector3 l_Position = Vector3.zero;

        int l_Column = 1;
        for (float x = m_Left + m_CellWidth / 2f; x <= m_Right + m_CellWidth; x += m_CellWidth)
        {
            if (l_Column == column)
            {
                l_Position.x = x;
                break;
            }
            l_Column++;
        }

        int l_Row = 1;
        for (float y = m_Bottom + m_CellHeight / 2f; y <= m_Top + m_CellHeight; y += m_CellHeight)
        {
            if (l_Row == row)
            {
                l_Position.y = y;
                break;
            }
            l_Row++;
        }

        return l_Position;
    }
    
    private void Update()
    {
        if (m_CurrentDirection == Direction.Default)
        {
            return;
        }

        if (m_IsIntersectInRow && m_IsIntersectInColumn)
        {
            if (m_IsIntersectInRow && m_IsIntersectInColumn)
            {
                if (m_IsRight && m_CurrentDirection != Direction.Right)
                {
                    m_CurrentDirection = Direction.Right;
                    m_IsRight = false;
                }
                else if (m_IsLeft && m_CurrentDirection != Direction.Left)
                {
                    m_CurrentDirection = Direction.Left;
                    m_IsLeft = false;
                }
                else if (m_IsTop && m_CurrentDirection != Direction.Top)
                {
                    m_CurrentDirection = Direction.Top;
                    m_IsTop = false;
                }
                else if (m_IsBottom && m_CurrentDirection != Direction.Bottom)
                {
                    m_CurrentDirection = Direction.Bottom;
                    m_IsBottom = false;
                }
            }
            else if (m_IsIntersectInColumn)
            {
                if (m_IsBottom && m_CurrentDirection == Direction.Top)
                {
                    m_CurrentDirection = Direction.Bottom;
                    m_IsBottom = false;
                }
                else if (m_IsTop && m_CurrentDirection == Direction.Bottom)
                {
                    m_CurrentDirection = Direction.Top;
                    m_IsTop = false;
                }
            }
            else if (m_IsIntersectInRow)
            {
                if (m_IsLeft && m_CurrentDirection == Direction.Right)
                {
                    m_CurrentDirection = Direction.Left;
                    m_IsLeft = false;
                }
                else if (m_IsRight && m_CurrentDirection == Direction.Left)
                {
                    m_CurrentDirection = Direction.Right;
                    m_IsRight = false;
                }
            }
        }

        if (m_CurrentDirection == Direction.Left)
        {
            SetInColumn();
            Vector3 l_Position = m_Character.transform.position;
            l_Position.x -= Time.deltaTime * m_Speed;
            if (m_Left < l_Position.x && m_Right > l_Position.x)
            {
            }
            else
            {
                l_Position.x = m_Right;
            }
            m_Character.transform.position = l_Position;
        }
        else if (m_CurrentDirection == Direction.Right)
        {
            SetInColumn();
            Vector3 l_Position = m_Character.transform.position;
            l_Position.x += Time.deltaTime * m_Speed;
            if (m_Left < l_Position.x && m_Right > l_Position.x)
            {
            }
            else
            {
                l_Position.x = m_Left;
            }
            m_Character.transform.position = l_Position;
        }
        else if (m_CurrentDirection == Direction.Top)
        {
            SetInRow();
            Vector3 l_Position = m_Character.transform.position;
            l_Position.y += Time.deltaTime * m_Speed;
            if (m_Bottom < l_Position.y && m_Top > l_Position.y)
            {
            }
            else
            {
                l_Position.y = m_Bottom;
            }
            m_Character.transform.position = l_Position;
        }
        else if (m_CurrentDirection == Direction.Bottom)
        {
            SetInRow();
            Vector3 l_Position = m_Character.transform.position;
            l_Position.y -= Time.deltaTime * m_Speed;
            if (m_Bottom < l_Position.y && m_Top > l_Position.y)
            {
            }
            else
            {
                l_Position.y = m_Top;
            }
            m_Character.transform.position = l_Position;
        }

        SetIntersectInColumn();
        SetIntersectInRow();
    }

    private void SetIntersectInColumn()
    {
        for (float x = m_Left + (m_CellWidth / 2f); x <= m_Right - m_CellWidth/2; x += m_CellWidth)
        {
            float l_Position = m_Character.transform.position.x;
            if (Mathf.Abs(l_Position - x) <= CHANGE_DIRECTION_OFFSET)
            {
                m_IsIntersectInColumn = true;
                break;
            }
            else
            {
                m_IsIntersectInColumn = false;
            }
        }
    }

    private void SetIntersectInRow()
    {
        for (float y = m_Bottom + (m_CellHeight / 2f); y <= m_Top - m_CellHeight/2; y += m_CellHeight)
        {
            float l_Position = m_Character.transform.position.y;
            if (Mathf.Abs(l_Position - y) <= CHANGE_DIRECTION_OFFSET)
            {
                m_IsIntersectInRow = true;
                break;
            }
            else
            {
                m_IsIntersectInRow = false;
            }
        }
    }

    private void SetInRow()
    {
        for (float x = m_Left + (m_CellWidth / 2f); x <= m_Right - m_CellWidth/2; x += m_CellWidth)
        {
            float l_Position = m_Character.transform.position.x;
            if (Mathf.Abs(l_Position - x) <= CHANGE_DIRECTION_OFFSET)
            {
                Vector3 l_CharacterPosition = m_Character.transform.position;
                l_CharacterPosition.x = x;
                m_Character.transform.position = l_CharacterPosition;
                break;
            }
        }
    }

    private void SetInColumn()
    {
        for (float y = m_Bottom + (m_CellHeight / 2f); y <= m_Top - m_CellHeight/2; y += m_CellHeight)
        {
            float l_Position = m_Character.transform.position.y;
            if (Mathf.Abs(l_Position - y) <= CHANGE_DIRECTION_OFFSET)
            {
                Vector3 l_CharacterPosition = m_Character.transform.position;
                l_CharacterPosition.y = y;
                m_Character.transform.position = l_CharacterPosition;
                break;
            }
        }
    }

    private void OnCoinCollection()
    {
        m_CollectedCoins++;
    }

    private void ReachedOnExit()
    {
        if (m_CollectedCoins == m_TotalCoins)
        {
            m_Speed = 0;
            m_Parent.gameObject.SetActive(false);
            m_GameOverPopup.SetActive(true);
            m_LoadNewLevelBtn.SetActive(true);

            GameDataContainer.IsLastLevelCleared = true;
        }
    }

    private void OnCollideWithEnemy()
    {
        // Game Over stuff will be here
        m_Speed = 0;
        m_GameOverPopup.SetActive(true);
        m_RestartGameBtn.SetActive(true);
    }


    private void OnBlockerDetected()
    {
        m_Speed = 0;
    }

    public void OnRestartButtonClick()
    {
        SceneManager.LoadScene(0);
    }
}
