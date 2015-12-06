using UnityEngine;
using System.Collections;

public class MazeWall : MonoBehaviour {

    MazeNode N1;
    MazeNode N2;
    MeshRenderer rend;
    BoxCollider coll;

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
            if (N1.isDisconnected(N2) && (N1.Value != 0 || N2.Value != 0))
            {
                coll.enabled = true;
                rend.enabled = true;
            }
            else if (N1.isConnected(N2) || (N1.Value == 0 && N2.Value == 0))
            {
                coll.enabled = false;
                rend.enabled = false;
            }
        }
    }

    public void ListenForDisconnect(MazeNode n1, MazeNode n2)
    {
        N1 = n1;
        N2 = n2;
    }
}
