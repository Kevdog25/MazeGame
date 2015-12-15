using UnityEngine;
using System.Collections;

public class Barrier : MonoBehaviour
{
    BoxCollider col;
    // Use this for initialization
    void Awake()
    {
        col = gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnNodeValueChange(int value)
    {

        if (value > 0)
        {
            col.enabled = false;
        }
        else
        {
            col.enabled = true;
        }
    }
}
