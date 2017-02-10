using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelScript : MonoBehaviour {

    public string levelName = "";
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Return))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
        }
	}
}
