using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundClampOnStart : MonoBehaviour {

    public float Padding = 1.0f;

	// Use this for initialization
	void Start () {

        Vector3 pos = this.transform.position;
        pos = VoxelWorld.Inst.GroundClamp(pos);
        pos.y += Padding;
        this.transform.position = pos;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
