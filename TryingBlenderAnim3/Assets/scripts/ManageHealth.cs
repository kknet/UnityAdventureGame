using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageHealth : MonoBehaviour
{

    public float health;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void decreaseHealth(float decrease)
    {
        health -= decrease;
    }

    public bool isDead()
    {
        return health <= 0f;
    }
}
