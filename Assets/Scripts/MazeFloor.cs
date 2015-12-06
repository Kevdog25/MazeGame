using UnityEngine;
using System.Collections;

public class MazeFloor : MonoBehaviour {
    MeshRenderer rend;
    BoxCollider col;
    bool DebugMode = false;
	// Use this for initialization
	void Awake () {
        col = gameObject.GetComponent<BoxCollider>();
        rend = gameObject.GetComponent<MeshRenderer>();
	}

    public void SetDebugMode(bool debug)
    {
        DebugMode = debug;
    }

    public void OnNodeValueChange(int value)
    {
        
        if(value > 0)
        {
            col.enabled = true;
            rend.enabled = true;
        }
        else
        {
            col.enabled = false;
            rend.enabled = false;
        }

        // THe following is only available in debug mode.
        if (!DebugMode)
            return;

        switch(value)
        {
            case 0:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case 1:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
                break;
            case 2:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                break;
            case 3:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
                break;
            default:
                break;
        }

    }
}
