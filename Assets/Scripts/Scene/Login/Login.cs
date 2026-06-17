using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField usernameInput;
    public Button loginButton;
   // FireBaseManager fireBaseManager;

    // Start is called before the first frame update
    void Start()
    {
        usernameInput = GameObject.Find("Canvas/Loginsystem/Username").GetComponent<InputField>();
        loginButton = GameObject.Find("Canvas/Loginsystem/Login").GetComponent<Button>();
       // fireBaseManager = GameObject.Find("FirebaseManager").GetComponent<FireBaseManager>();
        loginButton.onClick.AddListener(login);
    }

    void login()
    {
        Debug.Log("username: "+usernameInput.text);
        //fireBaseManager.setUserName(usernameInput.text);
        SceneManager.LoadScene("MainMenu");
    }
}
