using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    public int m_allegiance;
    //public bool m_isMine;
   // public bool m_isNeutral;
    //public bool m_isTheirs;
    public bool m_completecontroll;
    //public bool m_completeTheirs;
    public int m_minionSpawnNumber;
    public int m_minionSpawnRate = 5;
  //  public int seconds = 0;
    // Use this for initialization
    public Minion m_spawnMinion;
    //public towerScript m_tower;
    public float spawnTimer = 2;
    public int spawnCount = 1;
    public float levelUpTimer = 2;
   // public int minutes = 0;
    public int m_towerArmor = 0;
    public int m_levelUpCount = 1;

    void Start()
    {
        //if (m_allegiance >= -2 && m_allegiance <= 2) {
        //    m_isNeutral = true;
        //}
        //else {
        //    m_isNeutral = false;

        //}
        //if (m_allegiance < -2) {
        //    m_isTheirs = true;
        //}
        //else {
        //    m_isTheirs = false;

        //}
        //if (m_allegiance > 2) {
        //    m_isMine = true;
        //}
        //else {
        //    m_isMine = false;

        //}
        //if (m_allegiance == 5) {
        //    m_completecontroll = true;
        //}
        //else {
        //    m_completecontroll = false;

        //}
        //if (m_allegiance == -5) {
        //    m_completeTheirs = true;
        //}
        //else {
        //    m_completeTheirs = false;
        //}
        spawnTimer = m_minionSpawnRate;
        levelUpTimer = m_levelUpCount;
        
    }
    // Update is called once per frame
    void Update()
    {
        //if (m_allegiance >= -2 && m_allegiance <= 2) {
        //    m_isNeutral = true;
        //}
        //else {
        //    m_isNeutral = false;
        //}
        //if (m_allegiance < -2) {
        //    m_isTheirs = true;
        //}
        //else {
        //    m_isTheirs = false;
        //}
        //if (m_allegiance > 2) {
        //    m_isMine = true;
        //}
        //else {
        //    m_isMine = false;
        //}
        //if (m_allegiance == 5) {
        //    m_completecontroll = true;
        //}
        //else {
        //    m_completecontroll = false;
        //}
        //if (m_allegiance == -5) {
        //    m_completeTheirs = true;
        //}
        //else {
        //    m_completeTheirs = false;
        //}
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0)
        {
            
            //Debug.Log("Wave " + spawnCount + " Spawned!");
            spawnTimer = m_minionSpawnRate;
            for (int i = 0; i < 5; i++)
            {
                int minionNum = GameObject.FindObjectsOfType<Minion>().Length;
                if (minionNum < 10)
                {
                    if (m_spawnMinion != null)
                    {
                        Minion spawnedMinion = GameObject.Instantiate(m_spawnMinion, this.transform.position + new Vector3(Random.Range(1, 10), Random.Range(1, 5), 0), Quaternion.identity);
                    }
                }
            }
            spawnCount += 1;
            if (spawnCount % 2 == 0)
            {
                //Debug.Log("Tower level up!");
            }
        }
        //levelUpTimer += Time.deltaTime;
        //seconds += (int)Time.deltaTime;
        //seconds = (int)levelUpTimer % 60;
        //minutes = (int)levelUpTimer / 60;
        levelUpTimer -= Time.deltaTime;
        if (levelUpTimer <= 0) {
            levelUpTimer = m_levelUpCount;
            levelUp();
        }
        //if (seconds == 10 && m_allegiance > 0) {
        //    Debug.Log("Tower level 1");
        //    GameObject.Instantiate(m_tower, new Vector3(15, 8, 10), Quaternion.identity);
        //}
        //if (seconds == 20) {
        //    Debug.Log("Tower level 2");
        //}
        //if (seconds == 30) {
        //    Debug.Log("Tower level 3");
        //}
        //if (seconds == 45) {
        //    Debug.Log("Tower level 4");
        //}
        //if (minutes == 1 ) {
        //    Debug.Log("Tower level 5");
        //}
    }
    void levelUp()
    {
        if (m_allegiance > 0)
        {
            m_allegiance += 5;
        }
        else if (m_allegiance < 0)
        {
            m_allegiance -= 5;
        }
        else if (m_allegiance == 0)
        {
            m_allegiance += 0;
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
        return Mathf.Abs(m_allegiance / 5);
    }
}
