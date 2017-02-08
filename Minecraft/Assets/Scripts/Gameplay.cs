using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {
    //variables for time control
    public float timer = 0;
    public int minutes = 0;
    public int seconds = 0;

    //MINON CONTROL VARIABLES
    public int redminon = 0;
    public int blueminon = 0;
    public int greyminon = 0;
    public int maxred = 70;
    public int maxblue = 70;
    public int maxgrey = 14;

    /*
    //tower control variables
    static public int red;
    static public int blue;
    static public int netural;
    */


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //time control variable rules
        timer += Time.deltaTime;
        seconds += (int)Time.deltaTime;
        minutes = (int)timer / 60;
        seconds = (int)timer % 60;
        /*
        towerScript[] towerlist = FindObjectsOfType(typeof(towerScript)) as towerScript[];
        foreach (towerScript obj in towerlist)
        {
            if (obj.m_allegiance > -2 && obj.m_allegiance < 2)
            {
                netural ++;
            }
            if (obj.m_allegiance < -2)
            {
                red++;
            }
            if (obj.m_allegiance > 2)
            {
                blue++;
            }
        }
        */
    }

    //GUI here
    private void OnGUI()
    {
        //onscreen timer
        GUI.Label(new Rect(Screen.width/2-50, 0, 100, 100), minutes+":"+seconds);
        //GUI.Label(new Rect(Screen.width / 2 - 100, 50, 200, 100), "You:" + blue + "No One" + netural + "Them" + red);
    } 
}
