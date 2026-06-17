using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserTranslationController : MonoBehaviour
{
    /* Variables related to adjusting user's translation */
    UserMovement_Editor userMovement_Editor;
    UserMovement_IOS userMovement_IOS;
    bool editorOriginalLockChoice;                    // original choice of "user movement lock" in Unity Editor
    bool iosOriginalLockChoice;                       // original choice of "user movement lock" on IOS platform


    // Awake is the first function called in an Unity execution loop
    void Awake()
    {
        /* Initialize variables which controls user movement */
        userMovement_Editor = GameObject.Find("User").GetComponent<UserMovement_Editor>();
        userMovement_IOS = GameObject.Find("User").GetComponent<UserMovement_IOS>();
    }


    /// <summary>
    /// Function temporarily freezes user avatar's translation (can't move but can rotate)
    /// </summary>
    public void PauseUserTranslation()
    {
        /* Save the original choice of user translation lock, then lock the user */
#if UNITY_EDITOR
        editorOriginalLockChoice = userMovement_Editor.lockTranslation;
        userMovement_Editor.lockTranslation = true;
#endif

#if UNITY_IOS
        iosOriginalLockChoice = userMovement_IOS.lockTranslation;
        userMovement_IOS.lockTranslation = true;
#endif
    }


    /// <summary>
    /// Function temporarily unfreezes user avatar's translation
    /// </summary>
    public void ResumeUserTranslation()
    {
        /* Reset choice of user translation lock to what they were before */
#if UNITY_EDITOR
        userMovement_Editor.lockTranslation = editorOriginalLockChoice;
#endif

#if UNITY_IOS
        userMovement_IOS.lockTranslation = iosOriginalLockChoice;
#endif
    }

}
