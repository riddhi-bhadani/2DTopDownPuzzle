using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Action CoinCollected;
    public static Action CollideWithEnemy;
    public static Action ReachedOnExit;
    public static Action CollideWithBlocked;

    public Cell InitialCell = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            CoinCollected?.Invoke();
        }

        if (collision.gameObject.tag == "Enemy")
        {
            CollideWithEnemy?.Invoke();
        }
       
        if (collision.gameObject.tag == "Block")
        {
            CollideWithBlocked?.Invoke();
        }

        if (collision.gameObject.tag == "Exit")
        {
            ReachedOnExit?.Invoke();
        }
    }
}
