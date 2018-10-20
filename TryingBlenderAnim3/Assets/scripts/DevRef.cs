using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevRef : MonoBehaviour
{
    public static GameObject Player;

    void Awake()
    {
        Player = gameObject;
    }
}
