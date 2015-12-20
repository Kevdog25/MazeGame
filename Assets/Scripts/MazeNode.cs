using System;
using UnityEngine;
using System.Collections.Generic;

public class MazeNode
{
    public delegate void OnValueChange(int v);
    public delegate void OnPreviousChange(MazeNode p);
    public delegate void OnConnectChange(MazeNode m1, MazeNode m2);
    public OnConnectChange OnDisconnect;
    public OnConnectChange OnConnect;
    OnValueChange onValueChange;
    public OnPreviousChange onPreviousChange;
    int defaultValue;

    List<MazeNode> defaultAdj;
    List<MazeNode> defaultDis;
    List<MazeNode> Adjacent;
    List<MazeNode> Disconnected;

    public int DistanceSinceTurn;
    public int NMovesAway;
    int color;
    public int Color
    {
        get
        {
            return color;
        }
        set
        {
            color = value;
            //if(onValueChange != null)
            //    onValueChange(value); 
        }
    }
    // Color is used solely for BFS and DFS searching, and is largely a temp variable.
    int value;
    public int Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            if (onValueChange != null)
                onValueChange(value);
        }
    }
    public int NPathsOut = 0;

    public Vector2 Position;

    MazeNode previous = null;
    public MazeNode Previous
    {
        get { return previous; }
        set
        {
            if (value != null && value.Equals(this))
                throw new MazeGameException("Tried to set previous node to self!");
            else
            {
                previous = value;
            }
            if(onPreviousChange != null) onPreviousChange(previous);
        }
    }

    #region Properties

    public int NPaths
    {
        get { return Adjacent.Count; }
        private set { }
    }
    #endregion

    #region Constructor
    public MazeNode(int v, params MazeNode[] adj)
    {
        defaultValue = v;
        Value = v;
        Adjacent = new List<MazeNode>(adj);
        Disconnected = new List<MazeNode>();
        defaultAdj = new List<MazeNode>(Adjacent);
        defaultDis = new List<MazeNode>();
        Position = new Vector2();
    }
    #endregion

    #region OtherMethods
    public bool Connect(MazeNode other)
    {
        bool successful = true;
        if (Adjacent.Contains(other))
            successful = false;
        else
        {
            Adjacent.Add(other);
            Disconnected.Remove(other);
        }

        if (successful)
        {
            other.Connect(this);
            if (OnConnect != null) OnConnect(this,other);
        }

        return successful;
    }

    public bool isConnected(MazeNode other)
    {
        return Adjacent.Contains(other);
    }

    public bool Disconnect(MazeNode other)
    {
        bool successful = Adjacent.Remove(other);
        if (successful)
            if (!Disconnected.Contains(other))
                Disconnected.Add(other);

        if (successful)
        {
            other.Disconnect(this);
            if (OnDisconnect != null) OnDisconnect(this, other);
        }

        return successful;
    }

    public bool isDisconnected(MazeNode other)
    {
        return Disconnected.Contains(other);
    }

    public void FlipEdges()
    {
        List<MazeNode> dis = new List<MazeNode>(Disconnected);
        while (Adjacent.Count > 0)
            Disconnect(Adjacent[0]);
        for (int i = 0; i < dis.Count; i++)
            Connect(dis[i]);
    }

    /// <summary>
    /// Resets the node. Has an option to reset this.Previous or not.
    /// </summary>
    /// <param name="retainPrevious"></param>
    public void Reset(bool retainPrevious = false)
    {
        if (!retainPrevious)
            Previous = null;
        Value = defaultValue;
        Color = 0;
        NPathsOut = 0;
        DistanceSinceTurn = int.MaxValue;
        NMovesAway = int.MaxValue;
        foreach (var node in defaultAdj)
            Connect(node);
        foreach (var node in defaultDis)
            Disconnect(node);
    }

    public List<MazeNode> GetAdjacent()
    {
        return new List<MazeNode>(Adjacent);
    }

    public List<MazeNode> GetDisconnected()
    {
        return new List<MazeNode>(Disconnected);
    }

    public void SaveState()
    {
        defaultValue = Value;
        defaultAdj = new List<MazeNode>(Adjacent);
        defaultDis = new List<MazeNode>(Disconnected);
    }
    
    public void AddValueListener(OnValueChange f)
    {
        onValueChange += f;
    }
    #endregion
}

