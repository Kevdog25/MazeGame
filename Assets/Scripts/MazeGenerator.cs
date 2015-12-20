using System;
using System.Collections.Generic;
using UnityEngine;


class MazeGenerator
{
    public int[] Start;
    public int[] End;
    public MazeNode[,] Nodes;

    [SerializeField]
    float MeanderRate = 0.5f;

    int maxWidth;
    int maxHeight;
    int[] playerPos;
    int[][] AllowedDirections;
    Queue<MazeNode> FringeNodes;
    List<MazeNode> CullList;
    List<MazeNode> NewFringe;
    MazeNode PlayerNode;
    const int minMoves = 3;
    const int maxCorridorLength = 2;
    const float expansionProbability = 0.30f;
    //const float koolaidProbability = 0.01f;

    #region Constructor 
    public MazeGenerator(int width, int height)
    {
        maxWidth = width;
        maxHeight = height;
        Nodes = new MazeNode[width, height];
        for(int i = 0; i < width; i++)
            for(int j = 0; j < height; j++)
            {
                Nodes[i, j] = new MazeNode(0);
                Nodes[i, j].Position.x = i;
                Nodes[i, j].Position.y = j;
            }

        AllowedDirections = new int[][] { new int[] {0,1}, new int[] {1,0}, new int[] {0,-1,}, new int[] {-1,0} };
        playerPos = new int[]{0,0};
        Start = new int[2];
        End = new int[2];
        ConnectNodes();
        FringeNodes = new Queue<MazeNode>();
        CullList = new List<MazeNode>();
        NewFringe = new List<MazeNode>();
    }
    #endregion


    public void UpdateMaze(int[] newPlayerPos)
    {
        if (newPlayerPos.Length != 2 || newPlayerPos[0] < 0 || newPlayerPos[0] >= maxWidth
            || newPlayerPos[1] < 0 || newPlayerPos[1] >= maxHeight)
            throw new ArgumentException("Invalid player positon specification.");

        if (newPlayerPos[0] == playerPos[0] && newPlayerPos[1] == playerPos[1])
        {
            //Debug.Log(string.Format("Got the same position twice. [{0},{1}] [{2},{3}]", playerPos[0], playerPos[1], newPlayerPos[0], newPlayerPos[1]));
            //throw new MazeGameException();
        }
        else
        {
            playerPos[0] = newPlayerPos[0];
            playerPos[1] = newPlayerPos[1];
            PlayerNode = Nodes[playerPos[0], playerPos[1]];
            PlayerNode.Previous = null;
            CullPaths();
            GrowPaths();
        }
    }

    /// <summary>
    /// Grows BFS trees from the player node. 
    /// Expands any branches that are below the distance and visibility threshold.
    /// !WARNING! Only handles squares in a 15x15 radius around the player.
    /// </summary>
    void GrowPaths()
    {
        Queue<MazeNode> BFSq = new Queue<MazeNode>();
        BFSq.Enqueue(PlayerNode);
        
        // Initialize the relevant BFS nodes
        int radius = 20;
        for (int i = Math.Max(0, playerPos[0] - radius); i < Math.Min(playerPos[0] + radius, maxWidth); i++)
            for (int j = Math.Max(0, playerPos[1] - radius); j < Math.Min(playerPos[1] + radius, maxHeight); j++)
            {
                Nodes[i, j].Color = 0;
                Nodes[i, j].NPathsOut = 0;
                Nodes[i, j].DistanceSinceTurn = int.MaxValue;
                Nodes[i, j].NMovesAway = int.MaxValue;
                Nodes[i, j].Previous = null;
            }

        PlayerNode.NMovesAway = 0;
        PlayerNode.DistanceSinceTurn = 0;
        PlayerNode.Color = 1;
        PlayerNode.Value = 2;

        // Start the BFS
        while(BFSq.Count > 0)
        {
            MazeNode node = BFSq.Dequeue();
            if(node.NMovesAway == int.MaxValue)
                throw new MazeGameException("Error in distance calc. d:" + node.NMovesAway);

            // Stop at nodes outside of the expansion range.
            if(node.NMovesAway >= minMoves)
            {
                foreach (var adj in node.GetAdjacent())
                {
                    if (adj.Color == 0 && adj.Value > 0)
                    {
                        if (TryUpdateDistance(node, adj))
                        {
                            node.Value = 1;
                            node.NPathsOut++;
                            BFSq.Enqueue(adj);
                            adj.Previous = node;
                            adj.Color = 1;
                            adj.Value = 2;
                        }
                    }
                }
                if (node.Value == 2)
                    FringeNodes.Enqueue(node);

                continue;
            }

            // If not, then look for expansion nodes
            node.Value = 1;

            foreach (var adj in node.GetAdjacent())
            {
                if(TryUpdateDistance(node,adj))
                {
                    // We now should shift adj from its previous path onto this new one.
                    if (adj.Previous != null)
                    {
                        adj.Previous.NPathsOut--;
                        if (adj.Previous.NPathsOut < 0)
                            throw new MazeGameException("Reduced the number of paths out to < 0.");
                    }
                    
                    adj.Previous = node;
                    node.NPathsOut++;
                    BFSq.Enqueue(adj);
                    adj.Color = 1;
                    adj.Value = 2;
                }
            }

            // Also, do not expand nodes too close that can be seen.
            // If it already has paths connected and is close, skip expanding through walls.
            if (node.NMovesAway == 0)
                continue;
            Vector2 currentDir = (node.Position - node.Previous.Position).normalized;
            int availableNodes = 0;
            List<MazeNode> disconnected;
            do
            {
                disconnected = node.GetDisconnected();
                availableNodes = 0;
                while (disconnected.Count > 0)
                {
                    MazeNode dis = disconnected[KUtils.rand.Next(disconnected.Count)];
                    disconnected.Remove(dis);

                    // Enforce that there are no corridors longer than the max length (actually 2* that since they can expand in both directions)
                    if (node.DistanceSinceTurn >= maxCorridorLength) {
                        Vector2 NtoD = (dis.Position - node.Position).normalized;
                        if (Math.Abs(Vector2.Dot(NtoD,currentDir)-1) < 0.5f)
                            continue;
                    }

                    if (dis.Color == 0 && dis.Value == 0)
                    {
                        if (KUtils.rand.NextDouble() < expansionProbability)
                        {
                            node.Connect(dis);
                            dis.Color = 1;
                            dis.Value = 2;
                            dis.Previous = node;
                            TryUpdateDistance(node, dis);
                            BFSq.Enqueue(dis);
                            node.NPathsOut++;
                        }
                        else
                            availableNodes++;
                    }
                }
            } while (availableNodes > 0 && node.NPathsOut < 1);

        }
    }

    /// <summary>
    /// Trims edge nodes that are out of bounds along their predicessor paths.
    /// </summary>
    void CullPaths()
    {
        CullList.Clear();
        while(FringeNodes.Count > 0)
        {
            MazeNode tailNode = FringeNodes.Dequeue();
            // The grow algorithm sucks and sometimes puts a single node multiple times.
            if (tailNode.Previous == null)
                continue;
            //int nTurns = CountTurns(tailNode);
            if (tailNode.NPathsOut == 0 && tailNode.NMovesAway > minMoves+1 && tailNode.Previous.NMovesAway != 0)
            {
                tailNode.Previous.NPathsOut--;
                foreach (var node in tailNode.GetAdjacent())
                {
                    if (node.Value == 1)
                    {
                        FringeNodes.Enqueue(node);
                        node.Value = 2;
                    }
                }
                tailNode.Reset();
            }
        }
    }

    /// <summary>
    /// Counts all the turns while following the predicessors of the start node.
    /// Does not handle cycles and will loop indefinitely.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    int CountTurns(MazeNode node)
    {
        if (node == null)
            throw new ArgumentException("Cannot find turns from null node");

        MazeNode node1 = node;
        int nTurns = 0;

        while (node1.Previous != null && node1.Previous.Previous != null)
        {
            MazeNode node2 = node1.Previous;

            int[] node1Pos = new int[] { (int)node1.Position.x, (int)node1.Position.y };
            //int[] node2Pos = new int[] { (int)node2.Position.x, (int)node2.Position.y };
            int[] node3Pos = new int[] { (int)node2.Previous.Position.x, (int)node2.Previous.Position.y };

            if (!(Math.Abs(node3Pos[0] - node1Pos[0]) == 2 ||
                Math.Abs(node3Pos[1] - node1Pos[1]) == 2))
                nTurns++;

            if (nTurns > 20)
                throw new MazeGameException("High number of turns. Likely a closed loop");
            node1 = node1.Previous;
        }

        return nTurns;
    }

    public void SetStartEnd(int[] start, int[] end)
    {
        if (0 > start[0] || start[0] >= maxWidth ||
            0 > start[1] || start[1] >= maxHeight)
            throw new ArgumentException(String.Format("Start point out of bounds. [{0},{1}] Bounds: [{2},{3}]", start[0], start[1], maxWidth, maxHeight));
        if (0 > end[0] || end[0] >= maxWidth ||
            0 > end[1] || end[1] >= maxHeight)
            throw new ArgumentException(String.Format("End point out of bounds. [{0},{1}] Bounds: [{2},{3}]", end[0], end[1], maxWidth, maxHeight));

        Start[0] = start[0];
        Start[1] = start[1];
        End[0] = end[0];
        End[1] = end[1];
        playerPos[0] = start[0];
        playerPos[1] = start[1];
        PlayerNode = Nodes[playerPos[0], playerPos[1]];
        GenerateFixedPath(start, end);
        Nodes[start[0], start[1]].Value = 2;
        GrowPaths();
    }

    void GenerateFixedPath(int[] start, int[] end)
    {
        MazeNode workingNode = Nodes[start[0], start[1]];
        MazeNode endNode = Nodes[end[0], end[1]];

        while (!workingNode.Equals(endNode))
        {
            workingNode.Value = 1;
            MazeNode nextNode = null;
            Vector2 bestDirection = endNode.Position - workingNode.Position;
            float phi = (float)KUtils.RandGauss(0, Math.PI * (1 - MeanderRate));
            bestDirection = KUtils.Rotate2D(bestDirection, phi);

            float max = -(float)1e10;
            foreach (var node in workingNode.GetAdjacent())
            {
                Vector2 dir = node.Position - workingNode.Position;
                if (node.Value == 0 && Vector2.Dot(dir, bestDirection) > max)
                {
                    nextNode = node;
                    max = Vector2.Dot(dir, bestDirection);
                }
            }

            // This is petty sketchy handling of the DFS search.
            // This disallows for the possibility of some fixed path solutions
            // after certain are tried.
            if (nextNode == null)
            {
                if (workingNode.Previous == null)
                    throw new InvalidOperationException("Fixed path creation method broke really hard.");
                else
                {
                    workingNode.Disconnect(workingNode.Previous);
                    workingNode.Previous.Disconnect(workingNode);
                    workingNode.Value = 0;
                    workingNode = workingNode.Previous;
                }
            }
            else
            {
                nextNode.Previous = workingNode;
                workingNode = nextNode;
            }
        }

        for (int i = 0; i < maxWidth; i++)
            for (int j = 0; j < maxHeight; j++)
                Nodes[i, j].Reset(true);

        for (int i = 0; i < maxWidth; i++)
            for (int j = i % 2; j < maxHeight; j += 2)
                Nodes[i, j].FlipEdges();

        MazeNode temp;
        while (workingNode != null)
        {
            if (workingNode.Previous != null)
                workingNode.Connect(workingNode.Previous);
            workingNode = workingNode.Previous;
        }

        // Clear all previous nodes.
        for (int i = 0; i < maxWidth; i++)
            for (int j = 0; j < maxHeight; j++)
            {
                Nodes[i, j].Value = 0;
                Nodes[i, j].Previous = null;
            }
        SaveState();
    }

    /// <summary>
    /// Find the distance between two positions in some number of dimensions
    /// defined by the taxi-cab metric
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    int Distance(int[] p1, int[] p2)
    {
        if (p1.Length != p2.Length)
            throw new ArgumentException("Cannot find the distance between two positions of differing dimension.");
        int d = 0;
        for (int i = 0; i < p1.Length; i++)
            d += Math.Abs(p1[i]-p2[i]);
        return d;
    }

    /// <summary>
    /// connects all the maze nodes with the ones adjacent defined by allowedDirections
    /// provided.
    /// </summary>
    void ConnectNodes()
    {
        for(int i = 0; i < maxWidth; i++)
            for(int j = 0; j < maxHeight; j++)
            {
                foreach(var dir in AllowedDirections)
                {
                    if (KUtils.CheckRange(i + dir[0], 0, maxWidth) &&
                        KUtils.CheckRange(j + dir[1], 0, maxHeight))
                        Nodes[i, j].Connect(Nodes[i + dir[0], j + dir[1]]);
                }
            }

        for (int i = 0; i < maxWidth; i++)
            for (int j = 0; j < maxHeight; j++)
                Nodes[i, j].SaveState();
    }

    void SaveState()
    {
        for (int i = 0; i < maxWidth; i++)
            for (int j = 0; j < maxHeight; j++)
                Nodes[i, j].SaveState();
    }

    /// <summary>
    /// Makes a guess about the smallest value of n2.Distance based on
    /// the connection to n1.
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <returns></returns>
    bool TryUpdateDistance(MazeNode n1, MazeNode n2)
    {
        // If there cant be a turn yet then n2 is just as accessible as n1.
        if (n1.Previous == null)
        {
            if (n2.NMovesAway > n1.NMovesAway)
            {
                n2.DistanceSinceTurn = 1;
                n2.NMovesAway = n1.NMovesAway;
                return true;
            }
            else
                return false;
        }
        // If there happens to not be a turn, then n2 is still just as accessible as n1.
        if ((int)Math.Round((n1.Previous.Position - n2.Position).magnitude) == 2)
        {
            if(n2.NMovesAway > n1.NMovesAway)
            {
                n2.DistanceSinceTurn = n1.DistanceSinceTurn + 1;
                n2.NMovesAway = n1.NMovesAway;
                return true;
            }
            return false;
        }
        // Else there must be a turn
        if (n2.NMovesAway > n1.NMovesAway + 1)
        {
            n2.DistanceSinceTurn = 1;
            n2.NMovesAway = n1.NMovesAway + n1.DistanceSinceTurn;
            return true;
        }
        else
            return false;
    }
}
