using System.Collections;
using UnityEngine;

public class KillTimer : MonoBehaviour
{
    public float SecondsTillKill = 5.0f;

    void Update()
    {
        SecondsTillKill -= Time.deltaTime;
        if (SecondsTillKill <= 0.0f)
            GameObject.Destroy(gameObject);
    }
}