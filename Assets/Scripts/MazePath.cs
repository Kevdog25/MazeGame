using UnityEngine;
using System.Collections.Generic;

public class MazePath : MonoBehaviour {

    [SerializeField]
    GameObject LeftWall;
    [SerializeField]
    GameObject RightWall;
    [SerializeField]
    GameObject Floor;
    [SerializeField]
    GameObject Ceiling;

    MeshRenderer[] rends;
    BoxCollider[] colls;
    MeshRenderer floorRend;

    MazeNode N1;
    MazeNode N2;

    Vector3 position;
    public Vector3 Position
    {
        get
        {
            return position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }

    float length;
    public float Length
    {
        get
        {
            return length;
        }
        set
        {
            transform.localScale =  new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z*value/length);
            length = value;
        }
    }

    float pathWidth;
    public float PathWidth
    {
        get
        {
            return pathWidth;
        }
        set
        {
            Vector3 scale = transform.localScale;
            Vector3 scaleW = LeftWall.transform.localScale;
            transform.localScale = new Vector3(value,scale.y, scale.z);
            LeftWall.transform.localScale = new Vector3(scaleW.x * pathWidth / value, scaleW.y, scaleW.z);
            RightWall.transform.localScale = new Vector3(scaleW.x * pathWidth / value, scaleW.y, scaleW.z);
            LeftWall.transform.localPosition = new Vector3(-(value - wallWidth)/ (2 * value), LeftWall.transform.localPosition.y, 0);
            RightWall.transform.localPosition = new Vector3((value - wallWidth) / (2* value), RightWall.transform.localPosition.y, 0);
            pathWidth = value;
        }
    }

    float wallWidth;
    public float WallWidth
    {
        get
        {
            return wallWidth;
        }
        set
        {
            Vector3 scaleW = LeftWall.transform.localScale;
            Vector3 scale = transform.localScale;
            LeftWall.transform.localScale = new Vector3(scaleW.x * value/wallWidth,scaleW.y,scaleW.z);
            RightWall.transform.localScale = new Vector3(scaleW.x*value/wallWidth, scaleW.y, scaleW.z);
            LeftWall.transform.localPosition -= new Vector3((value-wallWidth) / (2 * scale.x), 0,0);
            RightWall.transform.localPosition += new Vector3((value - wallWidth) / (2 * scale.x), 0, 0);
            wallWidth = value;
        }
    }

    float height;
    public float Height
    {
        get
        {
            return height;
        }
        set
        {
            Vector3 scale = LeftWall.transform.localScale;
            LeftWall.transform.localScale = new Vector3(scale.x, value , scale.z);
            RightWall.transform.localScale = new Vector3(scale.x, value , scale.z);
            LeftWall.transform.localPosition += new Vector3(0, (value - height) / 2, 0);
            RightWall.transform.localPosition += new Vector3(0, (value - height) / 2, 0);
            height = value;
        }
    }

    bool isVisible;
    public bool IsVisible
    {
        get { return isVisible; }
        set
        {
            isVisible = value;
            LeftWall.GetComponent<MeshRenderer>().enabled = isVisible;
            RightWall.GetComponent<MeshRenderer>().enabled = isVisible;
            Ceiling.GetComponent<MeshRenderer>().enabled = isVisible;
        }
    }

	// Use this for initialization
	void Awake () {
        length = transform.localScale.z;
        height = LeftWall.transform.localScale.y;
        pathWidth = transform.localScale.x;
        wallWidth = transform.localScale.x * RightWall.transform.localScale.x;
        floorRend = Floor.GetComponent<MeshRenderer>();
        rends = gameObject.GetComponentsInChildren<MeshRenderer>();
        colls = gameObject.GetComponentsInChildren<BoxCollider>();
	}

    public void OnNodeValueChange(int value)
    {
        if (N1 != null && N2 != null)
        {
            if (N1.isDisconnected(N2) || (N1.Value == 0 && N2.Value == 0))
            {
                foreach (var rend in rends)
                    rend.enabled = false;
                foreach (var coll in colls)
                    coll.enabled = false;
            } 
            else if (N1.isConnected(N2))
            {
                foreach (var rend in rends)
                    rend.enabled = true && isVisible;
                floorRend.enabled = true;
                foreach (var coll in colls)
                    coll.enabled = true;
            }
        }
    }

    public void ListenForDisconnect(MazeNode n1, MazeNode n2)
    {
        N1 = n1;
        N2 = n2;
    }
}
