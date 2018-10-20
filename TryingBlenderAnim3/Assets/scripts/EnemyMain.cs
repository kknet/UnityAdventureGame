//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyMain : MonoBehaviour
//{

//    private bool doCombat = false, doPathfinding = true;

//    EnemyAI enemyAI;
//    ManageHealth manageHealth;
//    EnemyCombatAI enemyCombatAI;
//    EnemyCombatReactions enemyCombatReactions;

//    // Use this for initialization
//    public void Init()
//    {
//        if (doPathfinding)
//        {
//            enemyAI = GetComponent<EnemyAI>();
//            enemyAI.Init();
//        }
//        if (doCombat)
//        {
//            manageHealth = GetComponent<ManageHealth>();
//            enemyCombatAI = GetComponent<EnemyCombatAI>();
//            enemyCombatReactions = GetComponent<EnemyCombatReactions>();
//            enemyCombatAI.Init();
//            enemyCombatReactions.Init();
//        }
//    }

//    // Update is called once per frame
//    public void Update()
//    {

//        if (doPathfinding)
//        {
//            enemyAI.Update();
//        }

//        if (doCombat)
//        {
//            enemyCombatAI.Update();
//            enemyCombatReactions.Update();
//        }
//    }

//    public void setCombat(bool _doCombat)
//    {
//        doCombat = _doCombat;
//    }

//    public void setPathfinding(bool _doPathfinding)
//    {
//        doPathfinding = _doPathfinding;
//    }

//}
