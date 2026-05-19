using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMovementManage_IOS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string movementControlMode = PlayerPrefs.GetString("MoveControlMode", "Touch");
        if (movementControlMode == "Swing")
        {
            gameObject.AddComponent<InPlaceUserMovement_Swing_IOS>();

        }
        else if(movementControlMode == "Touch")
        {
            gameObject.AddComponent<InPlaceUserMovement_Touch_IOS>();
        }
        else
        {
            Debug.LogError("No Movement Control Mode");
        }
    }
}
