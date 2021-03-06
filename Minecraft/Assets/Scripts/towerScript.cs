﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    public static List<towerScript> AllTowers = new List<towerScript>();

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


    // Mesh types
    public GameObject[] redMeshes = new GameObject[3];
    public GameObject[] blueMeshes = new GameObject[3];
    public GameObject neutralMesh;

    public Material redFlagMaterial = null;
    public Material blueFlagMaterial = null;
    public Material neutralFlagMaterial = null;

    void Start()
    {
        AllTowers.Add(this);

        spawnTimer = m_minionSpawnRate;
        levelUpTimer = m_levelUpCount;

        //Setup flag material based on team
        UpdateFlagMaterial();
    }

    private void OnDestroy()
    {
        AllTowers.Remove(this);
    }

    void UpdateFlagMaterial()
    {
        MeshRenderer towerMeshRenderer = GetComponent<MeshRenderer>();
        MeshRenderer[] myMeshRenderers = GetComponentsInChildren<MeshRenderer>();

       for(int i = 0; i < myMeshRenderers.Length; ++i)
        {
            MeshRenderer meshRenderer = myMeshRenderers[i];

            // Skip the mesh if it is the parent mesh (don't want to update the tower, just the flag)
            if (towerMeshRenderer == meshRenderer)
                continue;

            switch(m_teamAllegiance)
            {
                case Minion.Allegiance.RED:
                    meshRenderer.material = redFlagMaterial;
                    break;
                case Minion.Allegiance.BLUE:
                    meshRenderer.material = blueFlagMaterial;
                    break;
                case Minion.Allegiance.NEUTRAL:
                    meshRenderer.material = neutralFlagMaterial;
                    break;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            spawnTimer = m_minionSpawnRate;
            
            List<Minion> enemyMinionList = new List<Minion>();
            List<Minion> playerMinionList = new List<Minion>();

            foreach (Minion minion in Minion.AllMinions)
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
                if (m_teamAllegiance == Minion.Allegiance.BLUE)
                {
                    if (m_blueMinion != null && Gameplay.Inst.canSpawnMinion(m_teamAllegiance))
                    {
                        GameObject.Instantiate(m_blueMinion, this.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)), Quaternion.identity);
                        Gameplay.Inst.notifyOfNewSpawn(m_teamAllegiance);
                    }
                }
                else if (m_teamAllegiance == Minion.Allegiance.RED)
                {
                    if (m_redMinion != null && Gameplay.Inst.canSpawnMinion(m_teamAllegiance))
                    {
                        GameObject.Instantiate(m_redMinion, this.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)), Quaternion.identity);
                        Gameplay.Inst.notifyOfNewSpawn(m_teamAllegiance);
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
        MeshFilter myMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer myMeshRenderer = GetComponent<MeshRenderer>();

        GameObject[] teamMeshArray = (m_teamAllegiance == Minion.Allegiance.RED) ? redMeshes : blueMeshes;

        // Increase tower armor
        if (m_teamAllegiance != Minion.Allegiance.NEUTRAL)
        {
            m_towerArmor += 5;

            // Clamp tower armor if it is too high
            int maxTowerLevel = 3;
            int towerArmorPerLevel = 5;
            int maxTowerArmorLevel = (maxTowerLevel * towerArmorPerLevel) - 1;
            if (m_towerArmor > maxTowerArmorLevel)
            {
                m_towerArmor = maxTowerArmorLevel;
            }
        }

        // Change mesh for tower based on level and current team
        if (m_teamAllegiance == Minion.Allegiance.NEUTRAL)
        {
            myMeshFilter.mesh = neutralMesh.GetComponent<MeshFilter>().sharedMesh;
            myMeshRenderer.materials = neutralMesh.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else
        {
            int curLevel = getCurrentLevel();
            GameObject choice = teamMeshArray[curLevel - 1];

            myMeshFilter.mesh = choice.GetComponent<MeshFilter>().sharedMesh;
            myMeshRenderer.materials = choice.GetComponent<MeshRenderer>().sharedMaterials;
        }

        // Change the color of the tower's flag based on the current team
        UpdateFlagMaterial();
    }
    public int getCurrentLevel()
    {
        if (m_towerArmor <= 5)
            return 1;
        else if (m_towerArmor <= 10)
            return 2;
        else if (m_towerArmor <= 15)
            return 3;
        else if (m_towerArmor <= 20)
            return 4;
        else
            return 5;
    }

    public int getMaxTowerArmor()
    {
        return getCurrentLevel() * 5;
    }

    void handleAllegianceSwap()
    {
        m_towerArmor = 0;

        // Change the mesh for the tower based on the current level
        levelUp();
    }

    public void towerHasBeenAttacked(Minion m)
    {
        // Handle different behaviors based on tower allegiance
        switch (m_teamAllegiance)
        {
            case Minion.Allegiance.NEUTRAL:
                {
                    // Any team will reduce the tower's armor
                    m_towerArmor -= 1;

                    if (m_towerArmor < 0)
                    {
                        if (m.miniononplayerteam)
                        {
                            m_teamAllegiance = Minion.Allegiance.BLUE;
                            handleAllegianceSwap();
                        }
                        else
                        {
                            m_teamAllegiance = Minion.Allegiance.RED;
                            handleAllegianceSwap();
                        }
                    }
                }
                break;
            case Minion.Allegiance.BLUE:
                {
                    // Only red minions can reduce a blue tower's armor
                    if (m.miniononplayerteam == false)
                    {
                        m_towerArmor -= 1;
                    }

                    if (m_towerArmor <= 0)
                    {
                        m_teamAllegiance = Minion.Allegiance.RED;
                        handleAllegianceSwap();
                    }
                }
                break;
            case Minion.Allegiance.RED:
                {
                    // Only blue minions can reduce a red tower's armor
                    if (m.miniononplayerteam == true)
                    {
                        m_towerArmor -= 1;
                    }

                    if (m_towerArmor <= 0)
                    {
                        m_teamAllegiance = Minion.Allegiance.BLUE;
                        handleAllegianceSwap();
                    }
                }
                break;
        }
    }
}
