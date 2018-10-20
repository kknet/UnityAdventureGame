using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockHitPlayer : MonoBehaviour {

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
            Destroy(gameObject);

        if (collision.gameObject.tag.Equals("Floor"))
            Destroy(gameObject);
    }

}
