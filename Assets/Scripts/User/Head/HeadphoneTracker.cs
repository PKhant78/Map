using System.Collections;
using System.Collections.Generic;
using HearXR;
using UnityEngine;

public class HeadphoneTracker : MonoBehaviour
{
    Transform rotateTarget;                                         // the target which we will use headphone to control its rotation

    private bool motionAvailable;                                   // bool variable indicates whether HeadphoneMotion is available on this device or not
    private bool tracking;                                          // bool variable indicates whether the rotation tracking started or not
    private bool headphonesConnected;                               // bool variable indicates whether headPhone is connected or not
    private Quaternion lastRotation;                                // the last rotation of headphone (initially 0)     
    private Quaternion calibratedOffset;                            // the offset value for calibrate the target rotation (initially 0)


    /// <summary>
    /// Function tries to turn on headphone tracking for adjusting target rotation
    /// </summary>
    public void TryTurnOnTracking(Transform theRotateTarget)
    {
        /* Assign the rotate target */
        rotateTarget = theRotateTarget;

        /* Init lastRotation */
        lastRotation = Quaternion.identity;

        /* Init calibratedOffset (Used to calibrate target rotation) */
        calibratedOffset = Quaternion.identity;

        /* Init HeadphoneMotion. Always call this first */
        HeadphoneMotion.Init();

        /* Check if the device support headphone motion */
        motionAvailable = HeadphoneMotion.IsHeadphoneMotionAvailable();

        /* If the device supports headphone motion, do the following */
        if (motionAvailable)
        {
            /* Set headphones connected text to false when start */
            HandleHeadphoneConnectionChange(false);

            /* Subscribe to the headphones connected/disconnected event */
            HeadphoneMotion.OnHeadphoneConnectionChanged += HandleHeadphoneConnectionChange;

            /* Subscribe to the headphone rotation callback */
            HeadphoneMotion.OnHeadRotationQuaternion += HandleHeadRotationQuaternion;

            /* Start tracking headphone motion */
            HeadphoneMotion.StartTracking();
            tracking = true;
        }
        else
        {
            Debug.Log("Cannot trun on headphone motion tracking, because it's not supported on this device!");
        }
    }


    /// <summary>
    /// Function turns off the tracking for headphone motion
    /// </summary>
    public void TurnOffTracking()
    {
        /* Unsubscribe from the headphones connected/disconnected event */
        HeadphoneMotion.OnHeadphoneConnectionChanged -= HandleHeadphoneConnectionChange;

        /* Unsubscribe from headphone rotation event */
        HeadphoneMotion.OnHeadRotationQuaternion -= HandleHeadRotationQuaternion;

        /* Stop the headphone motion tracking */
        HeadphoneMotion.StopTracking();
        tracking = false;

        /* Remove the rotate target */
        rotateTarget = null;
    }


    /// <summary>
    /// Function receives headphone connection status as a boolean variable and assign it to a class member variable
    /// The function will act as a callback function for "OnHeadphoneConnectionChanged()" in HeadPhoneMotion API
    /// </summary>
    /// <param name="connected">TRUE if connected, FALSE otherwise.</param>
    private void HandleHeadphoneConnectionChange(bool connected)
    {
        headphonesConnected = connected;
    }


    /// <summary>
    /// Function receives headphone rotation as quaternion and use it to modify target object's rotation 
    /// This function will act as a callback function for "OnHeadRotationQuaternion()" in HeadPhoneMotion API
    /// </summary>
    /// <param name="rotation">Headphone rotation</param>
    private void HandleHeadRotationQuaternion(Quaternion rotation)
    {
        /* Match the target object's rotation to the headphone rotation */
        if (calibratedOffset == Quaternion.identity)                                   // if the object rotation has NOT yet been calibrated
        {
            rotateTarget.rotation = rotation;                                          // let the object rotation equal to the airpod rotaiton
        }
        else                                                                           // if the object target rotation has been calibrated (When "calibratedOffset" is not 0)
        {
            /* [Note]
             * Just for record, the official code of dealing the task below is:
             * ===> rotateTarget.rotation = rotation * Quaternion.Inverse(calibratedOffset);
             * ===> This code works when we only use "CalibrateTargetRotation()". Not sure if
             *      it works if we "pass an origin rotation defined by us", which means using the 
             *      "CalibrateTargetRotation_GivenOrigin(Quaternion originRotation)" function.
             *      I still need to grow my knowledge in Quaternion calculation before I can confirm.
             */

            /* Calculate target's new "Euler Rotation" by using Airpod's rotation minus calibratedOffest's rotation */
            Vector3 newTargetRot = EulerArithmeticHelper.SubtractEulerRotation(rotation.eulerAngles, calibratedOffset.eulerAngles);

            /* Apply the "newTargetRot" to the target */
            rotateTarget.rotation = Quaternion.Euler(newTargetRot);
        }

        lastRotation = rotation;                                                       // update the "lastRotation" to record the latest headphone rotation
    }


    /// <summary>
    /// Function helps with setting the target's rotation to (0,0,0)
    /// It lets the calibration offset value equal to last rotation of headphone.
    /// </summary>
    public void CalibrateTargetRotation()
    {
        /* If "the device support Headphone motion" & "headphone is connected" & "the tracking is ongoing"
         * ===> we can let calibratedOffset equal to the headphone rotation
         * ===> after assigning value to "calibratedOffset", the target object's rotationEuler will become (0,0,0) according to logic in "HandleHeadRotationQuaternion()" */
        if (motionAvailable && headphonesConnected && tracking)
        {
            calibratedOffset = lastRotation;
        }
    }


    /// <summary>
    /// 
    /// Function helps with setting the target's rotation to a rotation of given origin.
    ///
    /// [Use Case]
    /// For example, this function can be used to set user' head rotation to the rotation of ARFace (When it's available)
    /// Because, we assume the rotation of user's head should always be consistent with ARFace's rotation
    /// In the case above It lets the calibration offset value equal to "last rotation of headphone" - "rotation of ARFace"
    /// 
    /// </summary>
    public void CalibrateTargetRotation_GivenOrigin(Quaternion originRotation)
    {
        /* If "the device support Headphone motion" & "headphone is connected" & "the tracking is ongoing"
         * ===> we can let calibratedOffset equal to the "headphone rotation" - "rotation of origin provided"
         * ===> so the rotation of target object will become the same as originRotation according to logic in "HandleHeadRotationQuaternion()" */
        if (motionAvailable && headphonesConnected && tracking)
        {
            /* (x,y,z) difference after using Airpod euler rotation minus provided origin euler rotation */
            Vector3 rotDiff = EulerArithmeticHelper.SubtractEulerRotation(lastRotation.eulerAngles, originRotation.eulerAngles);
            //Debug.Log($"$$$ Airpod Rot: {lastRotation.eulerAngles} || ARFace Rot: {originRotation.eulerAngles} ||  Diff Rot: {rotDiff}");

            /* Update the calibratedOffset using the difference calculated above */
            calibratedOffset = Quaternion.Euler(rotDiff);
        }
    }


    /// <summary>
    /// Function undo the calibration by resetting "calibrate offset value" back to 0
    /// </summary>
    public void ResetCalibration()
    {
        /* If "the device support Headphone motion" & "headphone is connected" & 
         * "the tracking is ongoing" & "calibrationOffset is not 0"
         * ===> we can reset the calibrationOffset to 0 */
        if (motionAvailable && headphonesConnected && tracking && calibratedOffset != Quaternion.identity)
        {
            calibratedOffset = Quaternion.identity;
        }
    }


    /// <summary>
    /// Getter and Setter of the rotate target
    /// </summary>
    public Transform RotateTarget
    {
        get { return rotateTarget; }
        set { rotateTarget = value; }
    }

}

