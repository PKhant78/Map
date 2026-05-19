using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class BackToMainMenu : MonoBehaviour
{
    float time;                         // varialbe tracks time elapsed since the script is activated
    float timeDelay;                    // time gap needed before allowing do something
    string mainMenu;                    // the variable stores the mainMenu name


    private void Start()
    {
        time = 0f;                      // when scene started, time variable is 0
        timeDelay = 1f;                 // set required time gap

        mainMenu = "MainMenu";          // initialize the mainMenu variable
    }


    void Update()
    {
        // update time variable
        time += 1f * Time.deltaTime;

        // back to main menu on different platform
        PhoneBackToMain();
        EditorBackToMain();
    }


    /// <summary>
    /// Function controls returning from a game scene to main menu on phone
    ///
    /// [Brief]
    /// In the script below, only when script is activated (entered the scene) for more than X seconds,
    /// we will allow screen touch detection and exit the scene! Otherwise, the user will quickly jump
    /// between the main menu and game scene without entering
    ///
    /// [Explanation]
    /// This is because in accessibility mode, the user need to double-tap to enter a game scene from the 
    /// main menu. If we immediately allow user to perform double-tap to exit the game scene, the system will 
    /// accidentally catch tap from previous entering selection, and it will pop the user out of game scene right a way.
    ///
    /// </summary>
    void PhoneBackToMain()
    {
        if (time > timeDelay && Input.touchCount > 0)
        {
            if (Input.GetTouch(0).tapCount == 1 || Input.GetTouch(0).tapCount == 0)
            {
                print("single tap");
                return;
            }

            if (Input.GetTouch(0).tapCount == 2)
            {
                print("double tap back to main menu");
                SceneJumpHelper.ResetThenSwitchScene(mainMenu);      // do all the resets needed then back to the main menu
            }
        }
    }


    /// <summary>
    /// Function controls returning from a game scene to main menu on Unity Editor
    /// </summary>
    void EditorBackToMain()
    {
        if (time > timeDelay && Input.GetKey(KeyCode.B))
        {
            print("pressed B to return");
            SceneJumpHelper.ResetThenSwitchScene(mainMenu);          // do all the resets needed then back to the main menu
        }

    }

}


