using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

    public Mesh ActiveHealthBar;
    public Mesh DepletedHealthBar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // Rotate the meshes so they always face the camera
        //ActiveHealthBar.
        //ActiveHealthBar.transform.rotation = Camera.main.transform.rotation;
    }

    void ChangeHealthBar(float percentage)
    {

    }
}
