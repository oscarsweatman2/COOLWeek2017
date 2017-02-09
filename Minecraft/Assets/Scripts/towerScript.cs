using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    public Minion.Allegiance m_allegiance;
    public int m_minionSpawnNumber;
    public int m_minionSpawnRate = 5;
    // Use this for initialization
    public Minion m_spawnMinion;
    public float spawnTimer = 2;
    public int spawnCount = 1;
    public int m_towerArmor = 5;
    void Start()
    {
        
        spawnTimer = m_minionSpawnRate;
    }
    // Update is called once per frame
    void Update()
    {
        
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {

            //Debug.Log("Wave " + spawnCount + " Spawned!");
            Minion[] minionlist = GameObject.FindObjectsOfType<Minion>();
            List<Minion> enemyMinionList = new List<Minion>();
            List<Minion> playerMinionList = new List<Minion>();


            foreach (Minion minion in minionlist)
            {
                if(minion.miniononplayerteam)
                {
                    playerMinionList.Add(minion);
                }
                else
                {
                    enemyMinionList.Add(minion);
                }
            }



                spawnTimer = m_minionSpawnRate;
            for (int i = 0; i < 5; i++) {
                int minionNum = GameObject.FindObjectsOfType<Minion>().Length;
                if (minionNum < 10 && m_spawnMinion != null) {
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
