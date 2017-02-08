using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    public int m_allegiance;
    public bool m_isMine;
    public bool m_isNeutral;
    public bool m_isTheirs;
    public bool m_completecontroll;
    public bool m_completeTheirs;
    public int m_minionSpawnNumber;
    public int m_minionSpawnRate = 5;
    // Use this for initialization
    public Minion m_spawnMinion;
    public float spawnTimer = 2;
    public int spawnCount = 1;

    void Start()
    {
        if (m_allegiance >= -2 && m_allegiance <= 2) {
            m_isNeutral = true;
        }
        else {
            m_isNeutral = false;

        }
        if (m_allegiance < -2) {
            m_isTheirs = true;
        }
        else {
            m_isTheirs = false;

        }
        if (m_allegiance > 2) {
            m_isMine = true;
        }
        else {
            m_isMine = false;

        }
        if (m_allegiance == 5) {
            m_completecontroll = true;
        }
        else {
            m_completecontroll = false;

        }
        if (m_allegiance == -5) {
            m_completeTheirs = true;
        }
        else {
            m_completeTheirs = false;
        }
        spawnTimer = m_minionSpawnRate;
    }
    // Update is called once per frame
    void Update()
    {
        if (m_allegiance >= -2 && m_allegiance <= 2) {
            m_isNeutral = true;
        }
        else {
            m_isNeutral = false;

        }
        if (m_allegiance < -2) {
            m_isTheirs = true;
        }
        else {
            m_isTheirs = false;

        }
        if (m_allegiance > 2) {
            m_isMine = true;
        }
        else {
            m_isMine = false;

        }
        if (m_allegiance == 5) {
            m_completecontroll = true;
        }
        else {
            m_completecontroll = false;

        }
        if (m_allegiance == -5) {
            m_completeTheirs = true;
        }
        else {
            m_completeTheirs = false;
        }
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            
            //Debug.Log("Wave " + spawnCount + " Spawned!");
            spawnTimer = m_minionSpawnRate;
            for (int i = 0; i < 5; i++) {
                int minionNum = GameObject.FindObjectsOfType<Minion>().Length;
                if (minionNum < 10) {
                    GameObject.Instantiate(m_spawnMinion, this.transform.position + new Vector3(Random.Range(1, 10), Random.Range(1, 5), 0), Quaternion.identity);
                }
            }
            spawnCount += 1;
            if (spawnCount % 2 == 0)
            {
                //Debug.Log("Tower level up!");
            }

        }
    }
    
}
