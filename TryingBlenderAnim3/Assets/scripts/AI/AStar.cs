//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AStar : MonoBehaviour
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStar : MonoBehaviour
{
    public class Node : StablePriorityQueueNode
    {
        int x;
        int y;
        public float f;
        public float g;
        public bool walkable;
        public Vector3 pos;
        public float obstacleCost;
        int ownerID;

        public Node(int x, int y, Vector3 pos)
        {
            obstacleCost = 0;
            this.x = x;
            this.y = y;
            this.pos = pos;
            this.ownerID = -1;
        }

        public List<Node> getNeighbors(Node[,] grid, AStar aStar)
        {
            List<Node> neighbors = new List<Node>();

            if (aStar.legal(x, y - 1))
                neighbors.Add(grid[x, y - 1]);

            if (aStar.legal(x, y + 1))
                neighbors.Add(grid[x, y + 1]);

            if (aStar.legal(x - 1, y - 1))
                neighbors.Add(grid[x - 1, y - 1]);

            if (aStar.legal(x - 1, y + 1))
                neighbors.Add(grid[x - 1, y + 1]);

            if (aStar.legal(x + 1, y - 1))
                neighbors.Add(grid[x + 1, y - 1]);

            if (aStar.legal(x + 1, y + 1))
                neighbors.Add(grid[x + 1, y + 1]);

            if (aStar.legal(x - 1, y))
                neighbors.Add(grid[x - 1, y]);

            if (aStar.legal(x + 1, y))
                neighbors.Add(grid[x + 1, y]);

            return neighbors;
        }

        public float euclidHeuristic(Node other)
        {
            return Vector3.Distance(pos, other.pos);
        }

        public void setFull(int ownerID)
        {
            this.ownerID = ownerID;
        }

        public void setEmpty()
        {
            this.ownerID = -1;
        }

        public bool hasOtherOwner(int ownerID)
        {
            return this.ownerID != -1 && this.ownerID != ownerID;
        }

    }

    public List<Collider> Obstacles;
    public Transform forwardLeft, forwardRight, backLeft, backRight;

    List<Node> playerWaypoints;
    Dictionary<GameObject, Node> reservedPos;
    Queue<KeyValuePair<Vector3, GameObject>> requestedUpdates;
    Vector3 lastPlayerPos;
    List<Node> walkables;
    Node[,] grid;
    IDictionary<Node, Node> nodeParents;
    Dictionary<GameObject, Stack<Node>> debugPaths;
    Dictionary<GameObject, Node> prevNodes;

    int xLength;
    int zLength;
    float nodeSize = 1.5f;

    const bool useCounter = false;
    const int counterMax = 1000;

    void Awake()
    {
        if (!enabled)
            return;
        debugPaths = new Dictionary<GameObject, Stack<Node>>();
        prevNodes = new Dictionary<GameObject, Node>();
        playerWaypoints = new List<Node>();
        reservedPos = new Dictionary<GameObject, Node>();
        lastPlayerPos = transform.position;
        requestedUpdates = new Queue<KeyValuePair<Vector3, GameObject>>();

        setUpEmptyGrid();
        updateObstacles();
        setObstacleNodesUnwalkable();
        walkables = getWalkableNodes();
        StartCoroutine(makePaths());
    }

    private void Update()
    {
        updateObstacles();
    }

    public void moveDirection(Vector3 goal, GameObject agent)
    {
        requestedUpdates.Enqueue(new KeyValuePair<Vector3, GameObject>(goal, agent));
    }

    IEnumerator makePaths()
    {
        while (true)
        {
            if (requestedUpdates.Count > 0)
            {
                KeyValuePair<Vector3, GameObject> updateObj = requestedUpdates.Dequeue();
                Vector3 goal = updateObj.Key;
                GameObject agent = updateObj.Value;
                if (agent != null)
                {
                    EnemyAI enemyScript = agent.GetComponentInParent<EnemyAI>();
                    int ownerId = enemyScript.enemyID;
                    bool targetIsPlayer = enemyScript.TargetIsPlayer;
                    //bool targetIsPlayer = false;

                    Vector3 moveDir = Vector3.zero;
                    bool shouldUpdatePath = !enemyScript.inAttack();
                    if(shouldUpdatePath)
                        moveDir = updatePath(goal, agent, ownerId, targetIsPlayer);
                    enemyScript.setMoveDirection(moveDir);
                }
            }
            yield return new WaitForSeconds(0.02f);
        }
    }

    private Vector3 updatePath(Vector3 goal, GameObject agent, int ownerID, bool goalIsPlayer)
    {
        float dist = Vector3.Distance(goal, agent.transform.position);
        bool farFromPlayer = dist > 10f;
        Node goalNode = null;
        if (goalIsPlayer && farFromPlayer)
        {
            if (reservedPos.ContainsKey(agent))
            {
                goalNode = reservedPos[agent];
            }
            else
            {
                foreach (Node wp in playerWaypoints)
                {
                    if (wp.walkable && !wp.hasOtherOwner(ownerID))
                    {
                        goalNode = wp;
                        goalNode.setFull(ownerID);
                        reservedPos.Add(agent, goalNode);
                        break;
                    }
                }
                if (goalNode == null)
                    return Vector3.zero;
            }
            //Debug.Log("Goal Pos " + goalNode.pos);
        }
        else
        {
            Vector2Int ind = indices(goal);
            if (!legal(ind.x, ind.y))
            {
                Debug.Log("Not legal indices for start or goal nodes");
                return Vector3.zero;
            }
            goalNode = grid[ind.x, ind.y];
        }


        Stack<Node> path = shortestPath(agent, goalNode, ownerID);

        if (debugPaths.ContainsKey(agent))
            debugPaths[agent] = path;
        else
            debugPaths.Add(agent, path);

        Vector2Int ai = indices(agent.transform.position);
        if (!legal(ai.x, ai.y))
        {
            Debug.Log("Not legal indices for agent cur position!");
            return Vector3.zero;
        }
        Node currNode = grid[ai.x, ai.y];
        currNode.setFull(ownerID);
        if (prevNodes.ContainsKey(agent))
        {
            if (currNode != prevNodes[agent])
            {
                prevNodes[agent].setEmpty();
                prevNodes[agent] = currNode;
            }
        }
        else
        {
            prevNodes.Add(agent, currNode);
        }

        if (path.Count > 0)
        {
            Node node = path.Pop();
            if (node != null)
            {
                if (node.hasOtherOwner(ownerID))
                    return Vector3.zero;
                else
                    return node.pos - agent.transform.position;
            }
        }
        return Vector3.zero;
    }

    public Stack<Node> shortestPath(GameObject agentObj, Node goal, int ownerID)
    {
        float startTime = Time.realtimeSinceStartup;
        nodeParents = new Dictionary<Node, Node>();

        Vector2Int startIndices = indices(agentObj.transform.position);

        if (!legal(startIndices.x, startIndices.y))
        {
            Debug.Log("Not legal indices for start or goal nodes");
            return new Stack<Node>();
        }

        Node start = grid[startIndices.x, startIndices.y];

        Node result = shortestPathInternal(start, goal, ownerID);

        if (result != null && result.Equals(goal))
            return traceBackFromGoal(start, goal, ownerID, agentObj);
        else
            return new Stack<Node>();
    }

    private Node shortestPathInternal(Node start, Node goal, int ownerID)
    {
        SimplePriorityQueue<Node, float> nodeQueue = new SimplePriorityQueue<Node, float>();
        HashSet<Node> visited = new HashSet<Node>();
        foreach (Node node in walkables)
        {
            node.g = float.MaxValue;
            node.f = float.MaxValue;
        }
        start.g = 0;
        start.f = 0 + start.euclidHeuristic(goal);
        nodeQueue.Enqueue(start, start.f);
        int count = 0;
        while (nodeQueue.Count > 0)
        {
            if (useCounter)
            {
                count += 1;
                if (count > counterMax)
                    return start;
            }


            Node curNode = nodeQueue.Dequeue();

            if (curNode.Equals(goal))
                return goal;
            visited.Add(curNode);

            List<Node> successors = curNode.getNeighbors(grid, this);
            foreach (Node successor in successors)
            {
                if (!successor.walkable) { continue; }
                if (successor.hasOtherOwner(ownerID)) { continue; }
                if (visited.Contains(successor)) { continue; }

                float curScore = curNode.g + Vector3.Distance(curNode.pos, successor.pos) + successor.obstacleCost;
                if (curScore < successor.g)
                {
                    nodeParents[successor] = curNode;
                    successor.g = curScore;
                    successor.f = successor.g + successor.euclidHeuristic(goal);
                    if (!nodeQueue.Contains(successor))
                        nodeQueue.Enqueue(successor, successor.f);
                    else
                        nodeQueue.UpdatePriority(successor, successor.f);
                }
            }
        }

        return start;
    }

    private Stack<Node> traceBackFromGoal(Node start, Node goal, int ownerID, GameObject agentObj)
    {
        Stack<Node> path = new Stack<Node>();
        Node curNode = goal;
        while (!curNode.Equals(start))
        {
            path.Push(curNode);
            curNode = nodeParents[curNode];
            //if (curNode.hasOtherOwner(ownerID)) 
            //curNode.setFull(ownerID);
        }
        return path;
    }

    public bool legalPositionInGrid(Vector3 position, int ownerID)
    {
        Vector2Int ind = indices(position);
        return legal(ind.x, ind.y) && !grid[ind.x, ind.y].hasOtherOwner(ownerID);
    }

    public bool legal(int x, int z)
    {
        return x >= 0 && z >= 0 && x < xLength && z < zLength &&
            grid[x, z].walkable;
    }


    private void setUpEmptyGrid()
    {
        xLength = Mathf.RoundToInt((backRight.position.x - backLeft.position.x) / nodeSize);
        zLength = Mathf.RoundToInt((forwardLeft.position.z - backLeft.position.z) / nodeSize);
        grid = new Node[xLength, zLength];
        for (int x = 0; x < xLength; ++x)
        {
            for (int z = 0; z < zLength; ++z)
            {
                grid[x, z] = new Node(x, z, pos(x, z));
            }
        }
    }

    private void updateObstacles()
    {
        List<Collider> obstaclesLeft = new List<Collider>();
        for (int i = 0; i < Obstacles.Count; ++i)
        {
            if (Obstacles[i] != null)
                obstaclesLeft.Add(Obstacles[i]);
        }

        //if(Obstacles.Count != obstaclesLeft.Count)
        //{
        Obstacles = obstaclesLeft;
        setObstacleNodesUnwalkable();
        walkables = getWalkableNodes();
        //}
    }

    private void setObstacleNodesUnwalkable()
    {
        for (int x = 0; x < xLength; ++x)
        {
            for (int z = 0; z < zLength; ++z)
            {
                grid[x, z].walkable = true;
                grid[x, z].obstacleCost = 0;
            }
        }

        foreach (Collider obj in Obstacles)
        {
            if (!obj.gameObject.activeInHierarchy)
                continue;

            int xStart = Mathf.RoundToInt((obj.bounds.min.x - backLeft.position.x) / nodeSize);
            int xEnd = xStart + Mathf.RoundToInt((obj.bounds.max.x - obj.bounds.min.x) / nodeSize);
            int zStart = Mathf.RoundToInt((obj.bounds.min.z - backLeft.position.z) / nodeSize);
            int zEnd = zStart + Mathf.RoundToInt((obj.bounds.max.z - obj.bounds.min.z) / nodeSize);

            for (int x = xStart; x <= xEnd; ++x)
            {
                for (int z = zStart; z <= zEnd; ++z)
                {
                    if (legal(x, z))
                        grid[x, z].walkable = false;

                }
            }
        }

        for (int x = 0; x < xLength; ++x)
        {
            for (int z = 0; z < zLength; ++z)
            {
                if (!grid[x, z].walkable)
                {
                    List<Node> neighbors = grid[x, z].getNeighbors(grid, this);
                    foreach (Node node in neighbors)
                    {
                        node.obstacleCost = 1f;
                    }
                }

            }
        }

        bool updateTargets = false;
        if (Vector3.Distance(transform.position, lastPlayerPos) > 2f)
        {
            playerWaypoints.Clear();
            foreach (Node node in reservedPos.Values)
            {
                node.setEmpty();
            }
            reservedPos.Clear();
            lastPlayerPos = transform.position;
            updateTargets = true;
        }

        for (int x = 0; x < xLength; ++x)
        {
            for (int z = 0; z < zLength; ++z)
            {
                if (updateTargets && grid[x, z].walkable)
                {
                    float playerDist = Vector3.Distance(grid[x, z].pos, transform.position);
                    if (playerDist > 4f && playerDist < 5f)
                    {
                        playerWaypoints.Add(grid[x, z]);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            //Debug.Log("Got here");
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    if (!grid[x, z].walkable)
                        Gizmos.DrawSphere(grid[x, z].pos, 0.2f);
                }
            }
        }

        if (debugPaths != null)
        {
            foreach (GameObject agent in debugPaths.Keys)
            {
                foreach (Node next in debugPaths[agent])
                    Gizmos.DrawCube(next.pos, Vector3.one * 0.2f);
            }
        }
    }

    #region helpers
    private List<Node> getWalkableNodes()
    {
        List<Node> w = new List<Node>();
        for (int x = 0; x < xLength; ++x)
        {
            for (int z = 0; z < zLength; ++z)
            {
                if (grid[x, z].walkable)
                    w.Add(grid[x, z]);
            }
        }
        return w;
    }

    private Vector2Int indices(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt((pos.x - backLeft.position.x) / nodeSize),
                            Mathf.RoundToInt((pos.z - backLeft.position.z) / nodeSize));
    }

    private Vector3 pos(int xIndex, int zIndex)
    {
        return new Vector3(backLeft.position.x + (xIndex * nodeSize),
                           0f,
                           backLeft.position.z + (zIndex * nodeSize));
    }

    private float powerCalculation(float distance)
    {
        return Mathf.Pow(distance, -.00f);
    }
    #endregion
}
