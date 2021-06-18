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
    [SerializeField]
    private EnemyType Type = EnemyType.None;
}
