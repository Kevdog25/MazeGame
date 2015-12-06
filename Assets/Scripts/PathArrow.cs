using UnityEngine;
using System.Collections;

public class PathArrow : MonoBehaviour {
    
    public MazeNode Parent;
    Rigidbody rigid;
    Vector3 currentDirection = new Vector3(0,0,1);

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPreviousUpdate(MazeNode previous)
    {

        if (Parent == null)
            throw new MazeGameException("Did not initialize PathArrow with a parent.");
        if (previous == null)
        {
            foreach (var rend in gameObject.GetComponentsInChildren<MeshRenderer>())
                rend.enabled = false;
        }
        else
        {
            rigid = gameObject.GetComponent<Rigidbody>();
            foreach (var rend in gameObject.GetComponentsInChildren<MeshRenderer>())
                rend.enabled = true;
            Vector3 other = new Vector3(previous.Position.x*5+2.5f,transform.position.y,previous.Position.y*5+2.5f)-transform.position;
            Quaternion rot = Quaternion.LookRotation(other);
            rigid.MoveRotation(rot);
        }
    }
}
