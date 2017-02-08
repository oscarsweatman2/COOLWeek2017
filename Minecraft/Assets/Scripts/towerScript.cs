using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerScript : MonoBehaviour
{
    public int m_allegiance;
    public bool m_ismine;
    public bool m_isneutral;
    public bool m_istheirs;
    public bool m_completecontroll;
    public bool m_completetheirs;
    public int m_minionSpawnNumber;
    public int m_minionSpawnRate;
    // Use this for initialization
    void Start()
    {
        if (m_allegiance > -2 && m_allegiance < 2)
        {
            m_isneutral = true;
        }
        else
        {
            m_isneutral = false;

        }
        if (m_allegiance < -2)
        {
            m_istheirs = true;
        }
        else
        {
            m_istheirs = false;

        }
        if (m_allegiance > 2)
        {
            m_ismine = true;
        }
        else
        {
            m_ismine = false;

        }
        if (m_allegiance == 5)
        {
            m_completecontroll = true;
        }
        else
        {
            m_completecontroll = false;

        }
    }
    // Update is called once per frame
    void Update()
    {
        if (m_allegiance > -2 && m_allegiance < 2)
        {
            m_isneutral = true;
        }
        else
        {
            m_isneutral = false;

        }
        if (m_allegiance < -2)
        {
            m_istheirs = true;
        }
        else
        {
            m_istheirs = false;

        }
        if (m_allegiance > 2)
        {
            m_ismine = true;
        }
        else
        {
            m_ismine = false;

        }
        if (m_allegiance == 5)
        {
            m_completecontroll = true;
        }
        else
        {
            m_completecontroll = false;

        }
        if (m_allegiance == 5)
        {
            m_completetheirs = true;
        }
        else
        {
            m_completetheirs = false;

        }


    }

}
