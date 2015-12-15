using UnityEngine;
using System.Collections;

public class MazeWall : MonoBehaviour {

    MazeNode N1;
    MazeNode N2;
    MeshRenderer rend;
    BoxCollider coll;

    bool isVisible;
    public bool IsVisible
    {
        get { return isVisible; }
        set
        {
            rend.enabled = value;
            isVisible = value;
        }
    }

    // Use this for initialization
    void Awake () {
        coll = gameObject.GetComponent<BoxCollider>();
        rend = gameObject.GetComponent<MeshRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void OnNodeValueChange(int value)
    {
        if (N1 != null && N2 != null)
        {
            if (N1.isDisconnected(N2) && (N1.Value != 0))
            {
                coll.enabled = true;
                rend.enabled = true && isVisible;
            }
            else
            {
                coll.enabled = false;
                rend.enabled = false;
            }
        }
        else if (N1 != null)
        {
            coll.enabled = N1.Value != 0;
            rend.enabled = N1.Value != 0 && isVisible;
        }
        else if (N2 != null)
        {
            coll.enabled = N2.Value != 0;
            rend.enabled = N2.Value != 0 && isVisible;
        }
    }

    public void ListenForDisconnect(MazeNode n1, MazeNode n2)
    {
        N1 = n1;
        N2 = n2;
    }
}
