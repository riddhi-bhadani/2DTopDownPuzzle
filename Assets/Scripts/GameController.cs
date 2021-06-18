using System.Collections;
using System.Collections.Generic;
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

public class GameController : MonoBehaviour
{
    private static float TOP_HEIGHT = 0;
    private static float BOTTOM_HEIGHT = 0;
    private static float CAMERA_SIZE = 5f;

    [SerializeField]
    private Canvas m_Canvas;

    private int m_TotalRows = 6;

    private int m_TotalColumns = 6;

    [SerializeField]
    private Transform m_Parent;

    [SerializeField]
    private GameObject m_Character, m_StartPoint, m_ExitPoint;

    [SerializeField]
    private Button m_LeftButton;

    [SerializeField]
    private Button m_RightButton;

    [SerializeField]
    private Button m_TopButton;

    [SerializeField]
    private Button m_BottomButton;

    [SerializeField]
    private GameObject m_ObjectType1;

    [SerializeField]
    private GameObject m_ObjectType2;

    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private GameObject m_GameOverPopup;

    [SerializeField]
    private GameObject m_RowAndColSelectionPopup;

    [SerializeField]
    private Slider m_RowSlider;

    [SerializeField]
    private Text m_RowText;

    [SerializeField]
    private Slider m_ColumnsSlider;

    [SerializeField]
    private Text m_ColumnsText;

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
    private int m_Type1Count = 0;
    private int m_Type2Count = 0;
    private int m_Type1Collect = 0;
    private int m_Type2Collect = 0;
    private float m_CurrentSpeed = 1f;

    private bool m_IsIntersectInRow = true;
    private bool m_IsIntersectInColumn = true;
    private Direction m_CurrentDirection = Direction.Default;

    private void OnEnable()
    {
        Player.Type1ObjectCollected += Type1ObjectCollect;
        Player.Type2ObjectCollected += Type2ObjectCollect;
    }

    private void OnDisable()
    {
        Player.Type1ObjectCollected -= Type1ObjectCollect;
        Player.Type2ObjectCollected -= Type2ObjectCollect;
    }

    private void Start()
    {
        InitCells();

        //m_GameOverPopup.SetActive(false);
        //m_RowAndColSelectionPopup.SetActive(true);

        //m_Text.text = "Speed : " + m_CurrentSpeed.ToString() + "X";

        EventTrigger l_LeftDown = m_LeftButton.gameObject.AddComponent<EventTrigger>();
        var l_LeftPointerDown = new EventTrigger.Entry();
        l_LeftPointerDown.eventID = EventTriggerType.PointerDown;
        l_LeftPointerDown.callback.AddListener((e) =>
        {
            if (m_CurrentDirection == Direction.Default)
            {
                m_CurrentDirection = Direction.Left;
            }

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

        for (float x = m_Left; x <= m_Right; x += m_CellWidth)
        {
            DrawLine(new Vector3(x, m_Top, 0), new Vector3(x, m_Bottom, 0));
        }

        for (float y = m_Bottom; y <= m_Top; y += m_CellHeight)
        {
            DrawLine(new Vector3(m_Left, y, 0), new Vector3(m_Right, y, 0));
        }

        SetCharacterOnRandomPlace();
        //SetRandomObjects();
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

    private void SetStartPointRandomOnFirstColumn()
    {
        int l_RandomRow = Random.Range(1, m_TotalRows);
        Vector3 l_Position = FindPositionBaseOnRowAndColumn(l_RandomRow, 1);
        m_Character.transform.position = l_Position;
        //m_Character.GetComponent<Player>().m_InitalRow = l_RandomRow;
        //m_Character.GetComponent<Player>().m_InitalColumn = l_RandomCol;
    }

    private void SetCharacterOnRandomPlace()
    {
        int l_RandomRow = Random.Range(1, m_TotalRows);
        int l_RandomCol = Random.Range(1, m_TotalColumns);
        Vector3 l_Position = FindPositionBaseOnRowAndColumn(l_RandomRow, l_RandomCol);
        m_Character.transform.position = l_Position;
        m_Character.GetComponent<Player>().m_InitalRow = l_RandomRow;
        m_Character.GetComponent<Player>().m_InitalColumn = l_RandomCol;
    }

    private Vector3 FindPositionBaseOnRowAndColumn(int row, int column)
    {
        Vector3 l_Position = Vector3.zero;

        int l_Column = 1;
        for (float x = m_Left + m_CellWidth / 2f; x <= m_Right - m_CellWidth; x += m_CellWidth)
        {
            if (l_Column == column)
            {
                l_Position.x = x;
                break;
            }
            l_Column++;
        }

        int l_Row = 1;
        for (float y = m_Bottom + m_CellHeight / 2f; y <= m_Top - m_CellHeight; y += m_CellHeight)
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

    private void SetRandomObjects()
    {
        Player l_Player = m_Character.GetComponent<Player>();
        m_Type1Count = Random.Range(3, 6);
        m_Type2Count = Random.Range(4, 7);

        List<int> l_Rows = new List<int>();
        List<int> l_Cols = new List<int>();

        int l_RandomRow = Random.Range(1, m_TotalRows);
        int l_RandomCol = Random.Range(1, m_TotalColumns);
        for (int i = 1; i <= m_Type1Count; i++)
        {
            while ((l_Rows.Contains(l_RandomRow) && l_Cols.Contains(l_RandomCol)) || (l_Player.m_InitalRow == l_RandomRow && l_Player.m_InitalColumn == l_RandomCol))
            {
                l_RandomRow = Random.Range(1, m_TotalRows);
                l_RandomCol = Random.Range(1, m_TotalColumns);
            }
            l_Rows.Add(l_RandomRow);
            l_Cols.Add(l_RandomCol);

            Vector3 l_Position = FindPositionBaseOnRowAndColumn(l_RandomRow, l_RandomCol);
            GameObject l_Type = Instantiate(m_ObjectType1);
            l_Type.transform.position = l_Position;
            l_Type.transform.SetParent(m_Parent);
        }

        l_RandomRow = Random.Range(1, m_TotalRows);
        l_RandomCol = Random.Range(1, m_TotalColumns);
        for (int i = 1; i <= m_Type2Count; i++)
        {
            while ((l_Rows.Contains(l_RandomRow) && l_Cols.Contains(l_RandomCol)) || (l_Player.m_InitalRow == l_RandomRow && l_Player.m_InitalColumn == l_RandomCol))
            {
                l_RandomRow = Random.Range(1, m_TotalRows);
                l_RandomCol = Random.Range(1, m_TotalColumns);
            }
            l_Rows.Add(l_RandomRow);
            l_Cols.Add(l_RandomCol);

            Vector3 l_Position = FindPositionBaseOnRowAndColumn(l_RandomRow, l_RandomCol);
            GameObject l_Type = Instantiate(m_ObjectType2);
            l_Type.transform.position = l_Position;
            l_Type.transform.SetParent(m_Parent);
        }
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
        for (float x = m_Left + (m_CellWidth / 2f); x <= m_Right - m_CellWidth; x += m_CellWidth)
        {
            float l_Position = m_Character.transform.position.x;
            if (Mathf.Abs(l_Position - x) <= 0.1f)
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
        for (float y = m_Bottom + (m_CellHeight / 2f); y <= m_Top - m_CellHeight; y += m_CellHeight)
        {
            float l_Position = m_Character.transform.position.y;
            if (Mathf.Abs(l_Position - y) <= 0.1f)
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
        for (float x = m_Left + (m_CellWidth / 2f); x <= m_Right - m_CellWidth; x += m_CellWidth)
        {
            float l_Position = m_Character.transform.position.x;
            if (Mathf.Abs(l_Position - x) <= 0.1f)
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
        for (float y = m_Bottom + (m_CellHeight / 2f); y <= m_Top - m_CellHeight; y += m_CellHeight)
        {
            float l_Position = m_Character.transform.position.y;
            if (Mathf.Abs(l_Position - y) <= 0.1f)
            {
                Vector3 l_CharacterPosition = m_Character.transform.position;
                l_CharacterPosition.y = y;
                m_Character.transform.position = l_CharacterPosition;
                break;
            }
        }
    }

    private void Type1ObjectCollect()
    {
        m_Type1Collect++;
        if (m_Type1Collect == (m_Type1Count / 2))
        {
            m_CurrentSpeed++;
            m_Text.text = "Speed : " + m_CurrentSpeed.ToString() + "X";
            m_Speed += 5;
        }
        AllObjectCollected();
    }

    private void Type2ObjectCollect()
    {
        m_Type2Collect++;

        if (m_Type2Collect == (m_Type2Count / 2))
        {
            m_CurrentSpeed++;
            m_Text.text = "Speed : " + m_CurrentSpeed.ToString() + "X";
            m_Speed += 5;
        }
        AllObjectCollected();
    }

    private void AllObjectCollected()
    {
        if (m_Type1Collect == m_Type1Count && m_Type2Collect == m_Type2Count)
        {
            m_Parent.gameObject.SetActive(false);
            m_GameOverPopup.SetActive(true);

            m_IsLeft = false;
            m_IsRight = false;
            m_IsTop = false;
            m_IsBottom = false;
        }
    }

    public void OnRestartButtonClick()
    {
        SceneManager.LoadScene(0);
    }

    public void SelectColumns()
    {
        m_TotalColumns = (int)m_ColumnsSlider.value;
        m_ColumnsText.text = m_ColumnsSlider.value.ToString();
    }

    public void SelectRows()
    {
        m_TotalRows = (int)m_RowSlider.value;
        m_RowText.text = m_RowSlider.value.ToString();
    }

    public void GameStart()
    {
        m_RowAndColSelectionPopup.SetActive(false);
        InitCells();
    }
}
