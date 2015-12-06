using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]
    int Width;
    [SerializeField]
    int Length;
    [SerializeField]
    float NodeWidth;
    [SerializeField]
    float NodeLength;
    [SerializeField]
    float NodeHeight;
    [SerializeField]
    float WallWidth;
    [SerializeField]
    GameObject Player;
    [SerializeField]
    GameObject Wall;
    [SerializeField]
    GameObject Floor;
    [SerializeField]
    GameObject Arrow;
    [SerializeField]
    bool Debug;

    MazeGenerator maze;
    GameObject WallParent;
    bool paused = false;
	// Use this for initialization
	void Start () {
        maze = new MazeGenerator(Width, Length);
        SetUpWalls();
        int[] start = new int[] { Width/2, Length/2 };
        maze.SetStartEnd(start, new int[] {Width-2,Length-2});
        Player.transform.position = ToGameBoard(start);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
        if (!paused)
        {
            maze.UpdateMaze(ToMazeArray(Player.transform.position));
        }
    }

    void TogglePause()
    {
        Cursor.visible = !Cursor.visible;
        Time.timeScale = !Cursor.visible ? 1 : 0;
        paused = !paused;
    }

    void SetUpWalls()
    {
        WallParent = new GameObject();
        WallParent.name = "Wall Parent";
        for(int i = 0; i < Width; i++)
            for(int j = 0; j < Length-1; j++)
            {
                var block = Instantiate(Wall);
                Transform trans = block.transform;
                trans.position = ToGameBoard(maze.Nodes[i, j].Position);
                trans.position += new Vector3(0, NodeHeight / 2.0f-1, NodeLength/2.0f);
                trans.localScale = new Vector3(NodeWidth, NodeHeight, WallWidth);
                block.transform.SetParent(WallParent.transform);

                MazeWall mazeWall = block.GetComponent<MazeWall>();
                mazeWall.ListenForDisconnect(maze.Nodes[i,j],maze.Nodes[i,j+1]);

                maze.Nodes[i, j].AddValueListener(mazeWall.OnNodeValueChange);
                maze.Nodes[i, j+1].AddValueListener(mazeWall.OnNodeValueChange);

            }

        for (int i = 0; i < Width-1; i++)
            for (int j = 0; j < Length; j++)
            {
                var block = Instantiate(Wall);
                Transform trans = block.transform;
                trans.position = ToGameBoard(maze.Nodes[i, j].Position);
                trans.position += new Vector3(NodeWidth / 2.0f, NodeHeight/2.0f-1, 0);
                trans.localScale = new Vector3(WallWidth, NodeHeight, NodeLength);
                block.transform.SetParent(WallParent.transform);

                MazeWall mazeWall = block.GetComponent<MazeWall>();
                mazeWall.ListenForDisconnect(maze.Nodes[i, j], maze.Nodes[i+1, j]);

                maze.Nodes[i, j].AddValueListener(mazeWall.OnNodeValueChange);
                maze.Nodes[i + 1, j].AddValueListener(mazeWall.OnNodeValueChange);

            }

        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Length; j++)
            {
                var block = Instantiate(Floor);
                Transform trans = block.transform;
                trans.position = ToGameBoard(maze.Nodes[i, j].Position);
                trans.position += new Vector3(0, -1, 0);
                trans.localScale = new Vector3(NodeWidth, 1, NodeLength);
                block.transform.SetParent(WallParent.transform);
                MazeFloor floorComp = block.GetComponent<MazeFloor>();
                floorComp.SetDebugMode(Debug);
                maze.Nodes[i, j].AddValueListener(floorComp.OnNodeValueChange);

                // Arrows only exist in debug mode.
                if (Debug)
                {
                    GameObject arrow = Instantiate(Arrow, ToGameBoard(maze.Nodes[i, j].Position) + new Vector3(0, 10, 0), Quaternion.identity) as GameObject;
                    PathArrow pathArrow = arrow.GetComponent<PathArrow>();
                    pathArrow.Parent = maze.Nodes[i, j];
                    maze.Nodes[i, j].onPreviousChange += pathArrow.OnPreviousUpdate;
                }

            }
    }

    /// <summary>
    /// Transforms from node array space to real game space.
    /// </summary>
    /// <param name="pos">Index/Position in node space.</param>
    /// <returns>Position in real space</returns>
    Vector3 ToGameBoard(int[] pos)
    {
        Vector3 realPos = new Vector3();
        realPos.y = 0;
        realPos.x = (pos[0] + 0.5f) * NodeWidth;
        realPos.z = (pos[1] + 0.5f) * NodeLength;
        return realPos;
    }

    /// <summary>
    /// Transforms from node array space to real game space.
    /// </summary>
    /// <param name="pos">Index/Position in node space.</param>
    /// <returns>Position in real space</returns>
    Vector3 ToGameBoard(Vector2 pos)
    {
        Vector3 realPos = new Vector3();
        realPos.y = 0;
        realPos.x = pos.x * NodeWidth + NodeWidth / 2.0f;
        realPos.z = pos.y * NodeLength + NodeLength / 2.0f;
        return realPos;
    }

    int[] ToMazeArray(Vector3 realPos)
    {
        int[] pos = new int[2] {0,0};
        pos[0] = (int)(realPos.x / NodeWidth);
        pos[1] = (int)(realPos.z / NodeLength);
        return pos;
    }
}
