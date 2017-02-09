using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIscript : MonoBehaviour
{
    //variables for timer
    public float timer = 0;
    public int minutes = 0;
    public int seconds = 0;

    //variables for towers (will change)
    public int enemytowers;
    public int playertowers;
    public int neturaltowers;

    //variables for minion count
    public int enemyminions;
    public int playerminions;
    public int totalminions;

    //power variable
    public int power;

    //did you win
    public bool wongame;
    public bool ongame;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //timer variables
        timer += Time.deltaTime;
        seconds += (int)Time.deltaTime;
        minutes = (int)timer / 60;
        seconds = (int)timer % 60;

        //tower variables
        enemytowers = 0;
        playertowers = 0;
        neturaltowers = 0;

        //minion numbers
        enemyminions = 0;
        playerminions = 0;
        totalminions = 0;

        //pwr variable
        power = 0;

        //check # of towers and minions, also win status
        Gameplay[] controllist = FindObjectsOfType(typeof(Gameplay)) as Gameplay[];
        foreach (Gameplay control in controllist)
        {
            //tower check
            enemytowers = control.enemytowercount;
            playertowers = control.playertowercount;
            neturaltowers = control.neturaltowercount;

            //minion check
            enemyminions = control.redminion;
            playerminions = control.blueminion;
            totalminions = control.totalminion;

            // wincheck
            wongame = control.win;
            ongame = control.gameon;
        }

        //check powerlevel
        Player[] playerlist = FindObjectsOfType(typeof(Player)) as Player[];
        foreach (Player play in playerlist)
        {
            power = play.Energy;
        }
    }

    //GUI here
    private void OnGUI()
    {
        //onscreen timer code
        if (seconds >= 10 && minutes >= 10)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, 0, 100, 100), minutes + ":" + seconds);
        }
        if (seconds < 10 && minutes >= 10)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, 0, 100, 100), minutes + ":0" + seconds);
        }
        if (seconds < 10 && minutes < 10)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, 0, 100, 100), "0" + minutes + ":0" + seconds);
        }
        if (seconds > 10 && minutes < 10)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, 0, 100, 100), "0" + minutes + ":" + seconds);
        }

        //onscreen towercount (temporary)
        GUI.Label(new Rect(Screen.width / 2 - 150, 30, 500, 100), "Enemy: " + enemytowers + " Netural: " + neturaltowers + " Yours: " + playertowers);

        //onscreen minioncount
        GUI.Label(new Rect(Screen.width / 2 - 200, 60, 600, 100), "MINION COUNT -- Enemy: " + enemyminions + " Total: " + totalminions + " Yours: " + playerminions);

        //onscreen powerlevel
        GUI.Label(new Rect(Screen.width / 2 - 50, 80, 100, 100), "BLOCK POWER:" + power);


        //win/loss signal
        if (ongame == false)
        {

            Time.timeScale = (float)0.0;

            if (wongame == true)
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height/2, 100, 100), "WINNER");
            }
            else
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2, 100, 100), "LOSER");
            }
        }
        else
        {
            Time.timeScale = (float)1;
        }
    }
}