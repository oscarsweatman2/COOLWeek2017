using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {
    //MINON CONTROL VARIABLES
    public int redminion = 0;
    public int blueminion = 0;
    public int totalminion = 0;
    public int maxred = 70;
    public int maxblue = 70;

    //tower control variables
    public int enemytowercount;
    public int playertowercount;
    public int neturaltowercount;

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        enemytowercount = 0;
        playertowercount = 0;
        neturaltowercount = 0;

        redminion = 0;
        blueminion = 0;
        totalminion = 0;

        towerScript[] towerlist = FindObjectsOfType(typeof(towerScript)) as towerScript[];
        foreach (towerScript tower in towerlist)
        {
            if (tower.m_allegiance == 0)
            {
                neturaltowercount++;
            }
            if (tower.m_allegiance >= 1)
            {
                playertowercount++;
            }
            if (tower.m_allegiance <= -1)
            {
                enemytowercount++;
            }
        }
        Minion[] minionlist = FindObjectsOfType(typeof(Minion)) as Minion[];
        foreach (Minion minions in minionlist)
        {
            totalminion++;
            if (minions.miniononplayerteam == true)
            {
                blueminion++;
            } else
            {
                redminion++;
            }
            
        }
    }
}
