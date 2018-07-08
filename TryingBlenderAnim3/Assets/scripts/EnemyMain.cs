using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMain : MonoBehaviour
{

    private bool doCombat = false, doPathfinding = false;

    EnemyAI enemyAI;
    ManageHealth manageHealth;
    AStarMovement aStarMovement;
    EnemyCombatAI enemyCombatAI;
    EnemyCombatReactions enemyCombatReactions;

    // Use this for initialization
    public void Init()
    {
        if (doPathfinding)
        {
            enemyAI = GetComponent<EnemyAI>();
            aStarMovement = GetComponent<AStarMovement>();
            enemyAI.Init();
            aStarMovement.Init();
        }
        if (doCombat)
        {
            manageHealth = GetComponent<ManageHealth>();
            enemyCombatAI = GetComponent<EnemyCombatAI>();
            enemyCombatReactions = GetComponent<EnemyCombatReactions>();
            enemyCombatAI.Init();
            enemyCombatReactions.Init();
        }
    }

    public void initCell()
    {
        enemyAI.initCell();
    }

    public void repathAll()
    {
        enemyAI.repathAll();
    }

    // Update is called once per frame
    public void FrameUpdate()
    {

        if (doPathfinding)
        {
            enemyAI.FrameUpdate();
        }

        if (doCombat)
        {
            enemyCombatAI.FrameUpdate();
            enemyCombatReactions.FrameUpdate();
        }
    }

    public void setCombat(bool _doCombat)
    {
        doCombat = _doCombat;
    }

    public void setPathfinding(bool _doPathfinding)
    {
        doPathfinding = _doPathfinding;
    }

}
