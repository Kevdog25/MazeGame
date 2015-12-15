using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GameController : MonoBehaviour {
    
    #region Serialized Fields
    [Header("Maze Dimensions.")]
    [SerializeField]
    int Width;
    [SerializeField]
    int Length;
    [Header("Gameplay")]
    [SerializeField]
    bool VisibleWalls;
    [Header("Maze Room Dimensions.")]
    [SerializeField]
    float RoomWidth;
    [SerializeField]
    float NodeSeparation;
    [SerializeField]
    float NodeHeight;
    [SerializeField]
    float WallWidth;
    [Header("Prefab Connections.")]
    [SerializeField]
    GameObject MainMenu;
    [SerializeField]
    GameObject MainCamera;
    [SerializeField]
    GameObject Player;
    [SerializeField]
    GameObject Wall;
    [SerializeField]
    GameObject Floor;
    [SerializeField]
    GameObject Path;
    [SerializeField]
    GameObject Arrow;
    [SerializeField]
    GameObject InvisibleBarrier;
    [Header("Debug Settings")]
    public bool Debug;
    public void setDebug(bool v)
    {
        Debug = v;
    }
    #endregion

    MazeGenerator maze;
    GameObject WallParent;
    GameObject FloorParent;
    GameObject PathParent;
    bool paused = false;
    bool playing = false;
    int[] start;
    int[] end;
	// Use this for initialization
	void Awake () {
	}
	
    /// <summary>
    /// Start playing the game.
    /// </summary>
    /// <param name="size"></param>
    public void Play(int size = 0)
    {
        UnityEngine.Debug.Log("Starting game with size: " + size);
        if(size != 0)
        {
            Width = size;
            Length = size;
        }
        playing = true;
        MainCamera.transform.SetParent(Player.transform);
        Player.GetComponent<PlayerController>().Paused = false;
        MainMenu.SetActive(false);
        maze = new MazeGenerator(Width, Length);
        start = new int[] { 0, 0 };
        end = new int[] { Width - 2, Length - 2 };
        SetUpBlocks();
        maze.SetStartEnd(start, end);
        Player.transform.position = ToGameBoard(start) + new Vector3(0,3,0);
    }

    public void Reset()
    {
        playing = false;
        Player.GetComponent<PlayerController>().Paused = true;
        MainMenu.SetActive(true);
        Destroy(PathParent);
        Destroy(WallParent);
        Destroy(FloorParent);
    }

	// Update is called once per frame
	void Update () {
        if (!playing)
            return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
        if (!paused)
        {
            int[] playerPos = ToMazeArray(Player.transform.position);
            if(playerPos[0] == end[0] && playerPos[1] == end[1])
            {
                Reset();
            }
            try
            {
                maze.UpdateMaze(ToMazeArray(Player.transform.position));
            }
            catch(MazeGameException ex)
            {
                Player.GetComponent<PlayerController>().Paused = true;
                throw;
            }
        }
    }

    void TogglePause()
    {
        paused = !paused;
        Cursor.visible = !Cursor.visible;
        Time.timeScale = !paused ? 1 : 0;
    }

    /// <summary>
    /// This is a mess. Don't open if you don't have to.
    /// </summary>
    void SetUpBlocks()
    {
        WallParent = new GameObject();
        FloorParent = new GameObject();
        PathParent = new GameObject();
        WallParent.name = "Walls";
        FloorParent.name = "Floors";
        PathParent.name = "Paths";

        // Set up all the walls. There are a bunch.
        // Puts 4 walls around each room.
        for(int i = 0; i < Width; i++)
            for(int j = 0; j < Length; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    int dy = k % 2 == 0 ? 1 - k : 0;
                    int dx = k % 2 == 1 ? 2 - k : 0;

                    var block = Instantiate(Wall);
                    Transform trans = block.transform;
                    trans.position = Vector3.zero;
                    trans.position += new Vector3(0, NodeHeight / 2 + 0.5f, RoomWidth/2-WallWidth/2);
                    trans.localScale = new Vector3(RoomWidth, NodeHeight, WallWidth);
                    trans.RotateAround(Vector3.zero,Vector3.up,k*90);
                    trans.position += ToGameBoard(maze.Nodes[i, j].Position);

                    block.transform.SetParent(WallParent.transform);

                    MazeWall mazeWall = block.GetComponent<MazeWall>();
                    mazeWall.IsVisible = VisibleWalls;
                    maze.Nodes[i, j].AddValueListener(mazeWall.OnNodeValueChange);
                    if (0 <= i + dx && i + dx < Width && 0 <= j + dy && j + dy < Length)
                    {
                        mazeWall.ListenForDisconnect(maze.Nodes[i, j], maze.Nodes[i + dx, j + dy]);
                        maze.Nodes[i + dx, j + dy].AddValueListener(mazeWall.OnNodeValueChange);
                    }
                    else
                    {
                        mazeWall.ListenForDisconnect(maze.Nodes[i, j], null);
                    }
                }
            }

        // Set up floors paths and arrows at each node.
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Length; j++)
            {
                var block = Instantiate(Floor);
                Transform trans = block.transform;
                trans.position = ToGameBoard(maze.Nodes[i, j].Position);
                trans.localScale = new Vector3(RoomWidth, 1, RoomWidth);
                block.transform.SetParent(FloorParent.transform);

                if(i == end[0] && j == end[1])
                    block.GetComponent<MeshRenderer>().material.color = Color.red;
                if( i == start[0] && j == start[1])
                    block.GetComponent<MeshRenderer>().material.color = Color.blue;

                MazeFloor floorComp = block.GetComponent<MazeFloor>();
                floorComp.SetDebugMode(Debug);
                maze.Nodes[i, j].AddValueListener(floorComp.OnNodeValueChange);

                // Set up the paths between rooms.
                for (int k = 0; k < 2; k++)
                {
                    int dy = k % 2 == 0 ? 1 - k : 0;
                    int dx = k % 2 == 1 ? 2 - k : 0;

                    if (!(0 <= i + dx && i + dx < Width && 0 <= j + dy && j + dy < Length))
                        continue;

                    var path = Instantiate(Path);
                    trans = path.transform;
                    trans.position = Vector3.zero;
                    trans.position += new Vector3(0, 0, NodeSeparation / 2);
                    trans.RotateAround(Vector3.zero, Vector3.up, k * 90);
                    trans.position += ToGameBoard(maze.Nodes[i, j].Position);
                    path.transform.SetParent(PathParent.transform);

                    MazePath mazePath = path.GetComponent<MazePath>();
                    mazePath.IsVisible = VisibleWalls;
                    mazePath.Length = NodeSeparation - RoomWidth;
                    mazePath.Height = NodeHeight;
                    mazePath.WallWidth = WallWidth;
                    mazePath.PathWidth = RoomWidth;
                    maze.Nodes[i, j].AddValueListener(mazePath.OnNodeValueChange);

                    mazePath.ListenForDisconnect(maze.Nodes[i, j], maze.Nodes[i + dx, j + dy]);
                    maze.Nodes[i + dx, j + dy].AddValueListener(mazePath.OnNodeValueChange);
                    
                }
                

                // Arrows only exist in debug mode.
                if (Debug)
                {
                    GameObject arrow = Instantiate(Arrow, ToGameBoard(maze.Nodes[i, j].Position) + new Vector3(0, 10, 0), Quaternion.identity) as GameObject;
                    PathArrow pathArrow = arrow.GetComponent<PathArrow>();
                    pathArrow.gameController = this;
                    pathArrow.Parent = maze.Nodes[i, j];
                    maze.Nodes[i, j].onPreviousChange += pathArrow.OnPreviousUpdate;
                }
                else if(VisibleWalls)
                {
                    block = Instantiate(Floor);
                    trans = block.transform;
                    trans.position = ToGameBoard(maze.Nodes[i, j].Position) + new Vector3(0,NodeHeight+1,0);
                    trans.localScale = new Vector3(RoomWidth, 1, RoomWidth);
                    block.transform.SetParent(FloorParent.transform);

                    if (i == end[0] && j == end[1])
                        block.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (i == start[0] && j == start[1])
                        block.GetComponent<MeshRenderer>().material.color = Color.blue;

                    floorComp = block.GetComponent<MazeFloor>();
                    floorComp.SetDebugMode(Debug);
                    maze.Nodes[i, j].AddValueListener(floorComp.OnNodeValueChange);
                }

            }
    }

    /// <summary>
    /// Transforms from node array space to real game space.
    /// </summary>
    /// <param name="pos">Index/Position in node space.</param>
    /// <returns>Position in real space</returns>
    public Vector3 ToGameBoard(int[] pos)
    {
        Vector3 realPos = new Vector3();
        realPos.y = 0;
        realPos.x = (pos[0]) * NodeSeparation + 0.5f * RoomWidth;
        realPos.z = (pos[1]) * NodeSeparation + 0.5f * RoomWidth;
        return realPos;
    }

    /// <summary>
    /// Transforms from node array space to real game space.
    /// </summary>
    /// <param name="pos">Index/Position in node space.</param>
    /// <returns>Position in real space</returns>
    public Vector3 ToGameBoard(Vector2 pos)
    {
        Vector3 realPos = new Vector3();
        realPos.y = 0;
        realPos.x = pos.x * NodeSeparation + 0.5f * RoomWidth;
        realPos.z = pos.y * NodeSeparation + 0.5f * RoomWidth;
        return realPos;
    }

    public int[] ToMazeArray(Vector3 realPos)
    {
        int[] pos = new int[2] {0,0};
        pos[0] = (int)Math.Round(realPos.x / NodeSeparation);
        pos[1] = (int)Math.Round(realPos.z / NodeSeparation);
        return pos;
    }
}
