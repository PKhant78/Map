using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBarManager : MonoBehaviour
{
    /// <summary>
    /// Function transport goal bar to a position. Will ignore y-axis
    /// passed by parameter in Vector3
    /// </summary>
    public void TransportGoalBar(Vector3 destination)
    {
        /* Y-axis position of the goal bar which will not be changed */
        float goalBarPosY = transform.position.y;

        /* New position for transferring the goal bar to */
        Vector3 newPosition = new Vector3(destination.x, goalBarPosY, destination.z);

        /* Transfer the goal bar to destination */
        transform.position = newPosition;
    }

}
