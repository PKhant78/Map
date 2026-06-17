using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ArSetting : MonoBehaviour
{

#if UNITY_IOS

    /// <summary>
    /// Everytime, when the scene is started, the AR Session will be reset immediately!
    ///
    /// [Explanation]
    /// the ResetArSession() is called in "BackToMainMenu.cs" as well, which means placing avatar back to
    /// origin before exit to the main menu. However, within X secs of "async gap", the user might move the
    /// by accident. To ensure the avatar's position and facing direction is 100% as we designed, we can
    /// call this ResetArSession() again when a new game scene is opened.
    ///
    /// [Extra Explanation]
    /// Why don't we only do Ar Session reset here? Because when the ar session is reset, the avatar will
    /// be dragged from previous position to the origin we designed. On the way of dragging, the avatar
    /// can trigger the objects around ===> this can trigger unwanted feedback playing and waypoint hit.
    /// Thus, we move avatar back to origin before closing the previous game scene, and make minor position
    /// and facing direction adjustment by calling reset function again when a new scene is awake.
    /// 
    /// </summary>

    void Awake()
    {
        ResetArSession();
    }

    /// <summary>
    /// Function resets AR Session. So the Avatar will been placed back to origin point!
    /// </summary>

    public void ResetArSession()
    {
        ARSession aRSession = GameObject.Find("AR Session").GetComponent<ARSession>();
        aRSession.Reset();
        Debug.Log("AR Session Has been reset");
    }

#endif

}
