using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBallMovement : MonoBehaviour
{
    private void Start()
    {
        /* Making the Sound Ball completely invisible when the game start */
        GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Function for transport the Sound Ball to a specific position indicated
    /// </summary>
    public void transportSoundBall(Vector3 destinationPoint) {
        transform.position = destinationPoint;
    }
}
