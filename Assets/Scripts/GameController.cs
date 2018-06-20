using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    List<int>[] objGridOpen;
    List<int>[] objGridClosed;

	void Start () {
        initMap();

	}
	
	void Update () {
		
	}

    void initMap()
    {
        objGridOpen = new List<int>[10];
        for (int i = -5; i < 5; i++)
        {
            objGridOpen[i] = new List<int>(new int[] { i * 10, i * 10 + 1, i * 10 + 2, i * 10 + 3, i * 10 + 4, i * 10 + 5, i * 10 + 6, i * 10 + 7, i * 10 + 8, i * 10 + 9});
        }
        objGridClosed = new List<int>[10];
        for (int i = 0; i < 10; i++)
        {
            objGridClosed[i] = new List<int>();
        }

        InvokeRepeating("refillWorld", 0.0f, 20.0f);
    }

    void refillWorld()
    {
        for(int i = 0; i < 10; i++)
        {
            while(objGridOpen[i].Count > 5)
            {
                int resNo = Random.Range(0, 100);
                int index = Random.Range(0, objGridOpen[i].Count-1);

                if(resNo <= 5)
                {
                    // pos = objGridOpen[i][index]
                } else if (resNo <= 10)
                {

                } else if (resNo  <= 55)
                {

                } else
                {

                }
            }
        }
    }
}
