using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RandomGeneratePrefab : MonoBehaviour
{
    GameObject[] prefabs;
    GameObject prefab;
    Vector3 prefabPosition;

    Random rnd = new Random();
    int[,] Position = new int[10,10];
    int tens;
    int ones;


     void Start()
    {
        /* A list to store all the prefabs for generating later */
        prefabs = new GameObject[4];

        /* An empty gameObject in the scene which stores all the prefab gameObjects */
        GameObject prefabRepo = GameObject.Find("PrefabRepository");

        /* Adding prefabs from repository to the "prefabs" list in the script */
        /* Adjust the size of "prefabs" */
        for (int i = 0; i < prefabs.Length; ++i)
        {
            prefabs[i] = prefabRepo.transform.GetChild(i).gameObject;
            prefabs[i].transform.localScale = new Vector3(0.7f,0.7f,0.7f);
        }

        /*In this scene, only special prefabs are required to randomly generate, the position of the existing objects*/
        /*should be marked in the Postion(2D array)*/
        Position[6, 7] = 1;
        Position[6, 8] = 1;
        Position[6, 9] = 1;
        Position[7, 0] = 1;
        Position[8, 0] = 1;
        Position[9, 0] = 1;
        Position[7, 2] = 1;
        Position[7, 3] = 1;
        Position[7, 7] = 1;
        Position[7, 8] = 1;
        Position[7, 9] = 1;
        Position[8, 7] = 1;
        Position[8, 8] = 1;
        Position[8, 9] = 1;
        Position[9, 7] = 1;
        Position[9, 8] = 1;
        Position[9, 9] = 1;

        for (int i = 0; i < prefabs.Length; ++i)
        {
            GenerateObject(i);
        }

        /*These codes are only used for test to guarantee the prefabs are created in the right position*/
        //prefabPosition = new Vector3(-7, 0.5f, -8);
        //prefab = Instantiate(prefabs[0], prefabPosition, Quaternion.Euler(0, 90, 0));
    }


    /*Based on the amount of prefabs need to be generated, get the random number the create the prefabs*/
    /*In order to keep the certain distance between each prefabs, when one prefab is created, the small pieces of*/
    /*around this prefab should also be marked "already occupied"*/
    void GenerateObject(int number)
    {
        CreateRandomNum();
        while (Position[tens, ones] == 1)
        {
            CreateRandomNum();
        }
        Position[tens, ones] = 1;

        
        if (tens == 0 && ones == 0)
        {
            Position[tens, ones + 1] = 1;
            Position[tens + 1, ones] = 1;
            Position[tens + 1, ones + 1] = 1;
        }

        if (tens == 0 && ones >0 && ones <9)
        {
            Position[tens, ones - 1] = 1;
            Position[tens, ones + 1] = 1;
            Position[tens + 1, ones] = 1;
            Position[tens + 1, ones + 1] = 1;
        }

        if (tens > 0 && tens < 9 && ones == 0)
        {
            Position[tens - 1, ones] = 1;
            Position[tens, ones + 1] = 1;
            Position[tens + 1, ones] = 1;
            Position[tens + 1, ones + 1] = 1;
        }

        if (tens > 0 && tens < 9 && ones >0 && ones <9)
        {
            Position[tens - 1, ones] = 1;
            Position[tens, ones - 1] = 1;
            Position[tens - 1, ones - 1] = 1;
            Position[tens, ones + 1] = 1;
            Position[tens + 1, ones] = 1;
            Position[tens + 1, ones + 1] = 1;
        }


        prefabPosition = new Vector3(-1 * tens, 0.5f, -1 * ones);
        prefab = Instantiate(prefabs[number], prefabPosition, Quaternion.Euler(0, 90, 0));

    }

    /*This function are used to get the random number*/
    void CreateRandomNum()
    {
        int randomNum = rnd.Next(1,100);
        tens = randomNum / 10;
        ones = randomNum % 10;

        /*ONlY FOR TEST*/
        //Debug.Log("randomNum: "+ randomNum);
        //Debug.Log("tens: " + tens);
        //Debug.Log("ones: " + ones);
    }

}
