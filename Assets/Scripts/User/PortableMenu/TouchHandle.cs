using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandle : MonoBehaviour
{
    // Flag to indicate if the screen is being touched
    private bool isTouching = false;

    void Update()
    {
        //// Check if there is any touch input
        //if (Input.touchCount > 0)
        //{
        //    // Get the first touch
        //    Touch touch = Input.GetTouch(0);

        //    // Check the phase of the touch
        //    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
        //    {
        //        if (!isTouching)
        //        {
        //            isTouching = true;
        //            OnTouchStart();
        //        }
        //        OnTouching();
        //    }
        //    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        //    {
        //        isTouching = false;
        //        OnTouchEnd();
        //    }
        //}
        //else
        //{
        //    if (isTouching)
        //    {
        //        isTouching = false;
        //        OnTouchEnd();
        //    }
        //}
    }

    void OnTouchStart()
    {
        Debug.Log("Touch Started");
    }

    void OnTouching()
    {
        Debug.Log("Touching...");
    }

    void OnTouchEnd()
    {
        Debug.Log("Touch Ended");
    }
}
