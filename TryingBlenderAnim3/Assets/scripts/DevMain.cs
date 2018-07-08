using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMain : MonoBehaviour
{

    #region imports to set in the inspector
    public bool doCombat, doPathfinding, doEnemies;
    public GameObject terrain;
    #endregion

    #region script imports
    InputController inputController;
    DevCellTracking devCellTracking;
    DevCombat devCombat;
    DevCombatReactions devCombatReactions;

    EnemyMain[] enemyMains;

    TrackObstacles trackObstacles;
    MapPathfind mapPathfind;
    ClosestNodes closestNodes;

    public static GameObject Player;
    #endregion

    void Awake()
    {
        Player = gameObject;
        inputController = GetComponent<InputController>();
    }

    void Start()
    {
        inputController.Init();

        if (doPathfinding || doCombat)
            doEnemies = true;

        if (doEnemies)
        {
            enemyMains = GameObject.Find("Enemies").GetComponentsInChildren<EnemyMain>();
            foreach (EnemyMain curEnemyMain in enemyMains)
            {
                if (doCombat)
                    curEnemyMain.setCombat(true);
                if (doPathfinding)
                    curEnemyMain.setPathfinding(true);
                curEnemyMain.Init();
            }
        }

        if (doCombat)
        {
            inputController.combatEnabled = doCombat;
            devCombat = GetComponent<DevCombat>();
            devCombatReactions = GetComponent<DevCombatReactions>();
            devCombat.Init();
            devCombatReactions.Init();
        }

        if (doPathfinding)
        {
            if (terrain == null)
            {
                Debug.LogAssertion("Need to specify terrain in Dev Main Script Public Objects");
            }
            trackObstacles = terrain.GetComponent<TrackObstacles>();
            mapPathfind = terrain.GetComponent<MapPathfind>();
            closestNodes = terrain.GetComponent<ClosestNodes>();
            devCellTracking = GetComponent<DevCellTracking>();

            mapPathfind.Init();
            closestNodes.Init();
            trackObstacles.Init();
            devCellTracking.Init();


            foreach (EnemyMain curEnemyMain in enemyMains)
            {
                curEnemyMain.initCell();
            }
            enemyMains[0].repathAll();
        }
    }

    void Update()
    {
        inputController.FrameUpdate();

        if (doEnemies)
            foreach (EnemyMain curEnemyMain in enemyMains)
                curEnemyMain.FrameUpdate();

        if (doCombat)
        {
            devCombat.FrameUpdate();
            devCombatReactions.FrameUpdate();
        }

        if (doPathfinding)
            devCellTracking.FrameUpdate();
    }

    void FixedUpdate()
    {
        inputController.PhysicsUpdate();        
    }
}
