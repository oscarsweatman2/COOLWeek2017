﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    // Use this for initialization
    public Minion.Allegiance m_teamAllegiance;
    public int m_minionSpawnNumber = 3;
    public int m_minionSpawnRate = 5;
    //public Minion m_spawnMinion;
    public Minion m_blueMinion;
    public Minion m_redMinion;
    public float spawnTimer = 2;
    public int spawnCount = 1;
    public int m_towerArmor = 5;
    public float levelUpTimer = 2;
    public int m_levelUpCount = 1;
    

    void Start()
    {
        spawnTimer = m_minionSpawnRate;
        levelUpTimer = m_levelUpCount;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            spawnTimer = m_minionSpawnRate;

            Minion[] minionlist = GameObject.FindObjectsOfType<Minion>();
            List<Minion> enemyMinionList = new List<Minion>();
            List<Minion> playerMinionList = new List<Minion>();

            foreach (Minion minion in minionlist)
            {
                if (minion.miniononplayerteam)
                {
                    playerMinionList.Add(minion);
                }
                else
                {
                    enemyMinionList.Add(minion);
                }
            }
            for (int i = 0; i < m_minionSpawnNumber; i++)
            {
                //int minionNum = GameObject.FindObjectsOfType<Minion>().Length;
                if (m_teamAllegiance == Minion.Allegiance.BLUE)
                {
                    if (m_blueMinion != null)
                    {
                        Minion spawnedBlueMinion = GameObject.Instantiate(m_blueMinion, this.transform.position + new Vector3(Random.Range(1, 10), Random.Range(1, 5), 0), Quaternion.identity);
                    }
                }
                else if (m_teamAllegiance == Minion.Allegiance.RED)
                {
                    if (m_redMinion != null)
                    {
                        Minion spawnedRedMinion = GameObject.Instantiate(m_redMinion, this.transform.position + new Vector3(Random.Range(1, 10), Random.Range(1, 5), 0), Quaternion.identity);
                    }
                }
            }
        }

        levelUpTimer -= Time.deltaTime;
        if (levelUpTimer <= 0)
        {
            levelUpTimer = m_levelUpCount;
            levelUp();
        }
    }
    void levelUp()
    {
        if (m_teamAllegiance != Minion.Allegiance.NEUTRAL)
        {
            m_towerArmor += 5;
        }
        int curLevel = getCurrentLevel();
        if (curLevel == 1)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }
        if (curLevel == 2)
        {
            this.transform.localScale = new Vector3(2, 2, 2);
        }
        if (curLevel == 3)
        {
            this.transform.localScale = new Vector3(3, 3, 3);
        }
        if (curLevel == 4)
        {
            this.transform.localScale = new Vector3(4, 4, 4);
        }
        if (curLevel == 5)
        {
            this.transform.localScale = new Vector3(5, 5, 5);
        }
    }
    public int getCurrentLevel()
    {
        return m_towerArmor / 5 + 1;
    }
    void towerHasBeenAttacked(Minion m)
    {
        //if (m_teamAllegiance == Minion.Allegiance.NEUTRAL)
        //{
        //    //switch to the side of the minion that got destroyed
        //    //if (m.miniononplayerteam == true) 
        //    //{
        //    //   this.m_towerArmor += 1;
        //    //}
        //    //else if (m.miniononplayerteam == false)
        //    //{
        //    //   this.m_towerArmor -= 1;
        //    //}
        
        //}
        //if (m_teamAllegiance == Minion.Allegiance.BLUE)
        //{
        //    if (m.miniononplayerteam == true)
        //    {
        //        //friendly minion, do nothing
        //    }
        //    else
        //    {
        //        //tower lose X armor
        //        this.m_towerArmor -= 1;
        //    }
        //}
        //if (m_teamAllegiance == Minion.Allegiance.RED)
        //{
        //    if (m.miniononplayerteam == true)
        //    {
        //        //friendly minion, do nothing
        //    }
        //    else
        //    {
        //        //tower lose X armor
        //        this.m_towerArmor += 1;
        //    }
        //}
     
    }
}
