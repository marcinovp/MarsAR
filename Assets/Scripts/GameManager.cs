using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    
	void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
