using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using UnityEngine.SceneManagement;

public class LayoutLearner : MonoBehaviour
{
    private GameObject lastSelectedObject;


    private AudioSource source;

    private VerbalManager_General verbalManager_General;    // The class controls text to speech

    private VibrationManager_Feedback vibrationManagerFb;

    private string objName;

    private string instructions = "Welcome to the Learning Layout Module! This guide will help you build a mental map of the layout through simple touch interactions.Explore and learn room layouts by touching the screen and moving the touch screen from top to bottom, left to right.";
    //private string instructions = "레이아웃 학습 모듈에 오신 것을 환영합니다! 이 가이드는 간단한 터치 상호작용을 통해 레이아웃에 대한 정신적 지도를 만드는 데 도움이 되며, 화면을 터치하고 터치스크린을 위에서 아래로, 왼쪽에서 오른쪽으로 움직여 방 레이아웃을 탐색하고 학습할 수 있습니다.";

    private List<string> ignoreLst = new List<string>()
    {
        "Window", "Floor"
    };

    string mainMenu;                    // the variable stores the mainMenu name

    // Double click interval (seconds)
    public float doubleClickTime = 0.2f;

    // Keep track of the last click or touch
    private float lastClickTime = 0f;

    private void Start()
    {
        mainMenu = "MainMenu";          // initialize the mainMenu variable
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        vibrationManagerFb = GameObject.Find("SoundBall").GetComponent<VibrationManager_Feedback>();

        //SpeakFeedbackVerbal(instructions);
        try
        {
            verbalManager_General.StopSpeak();
            verbalManager_General.Speak(instructions);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error exists when open the Menu: " + e);
        }
    }

    void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            DetectTouchDoubleTap();

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // Radiate from the camera position
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // If the ray hits the object
                if (Physics.Raycast(ray, out hit))
                {
                    // Get the Selected Object
                    GameObject selectedObject = hit.collider.gameObject;

                    // Print the Selected object name
                    Debug.Log("Tapped on: " + selectedObject.name);
                    if (selectedObject == lastSelectedObject)
                        return;

                    // Reset the last selected object color
                    if (lastSelectedObject != null)
                    {
                        source = lastSelectedObject.GetComponent<AudioSource>();
                        if (source)
                            source.Stop();
                        MMVibrationManager.StopAllHaptics();
                        Renderer lastRenderer = lastSelectedObject.GetComponent<Renderer>();
                        if (lastRenderer != null)
                        {
                            lastRenderer.material.color = Color.white;
                        }
                    }


                    // Change the currently selected object color
                    source = selectedObject.GetComponent<AudioSource>();
                    if (source)
                        source.Play();

                    string name = selectedObject.transform.parent.name;
                    if (!ignoreLst.Contains(name))
                    {
                        objName = name;
                        SpeakFeedbackVerbal(name);
                        MMVibrationManager.ContinuousHaptic(1f, 1f, 0.1f, HapticTypes.Failure, this, true);
                    }
                    Renderer objectRenderer = selectedObject.GetComponent<Renderer>();
                    if (objectRenderer != null)
                    {
                        objectRenderer.material.color = Color.red;
                    }

                    // Records the currently selected object
                    lastSelectedObject = selectedObject;
                }
            }
        }
        else
        {
            if (lastSelectedObject != null)
            {
                source = lastSelectedObject.GetComponent<AudioSource>();
                if (source)
                    source.Stop();
            }
        }
#endif

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
           
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            
            if (Physics.Raycast(ray, out hit))
            {
                
                GameObject selectedObject = hit.collider.gameObject;

                
                Debug.Log("Clicked on: " + selectedObject.name);

                
                if (lastSelectedObject != null)
                {
                    source = lastSelectedObject.GetComponent<AudioSource>();
                    if(source)
                        source.Stop();
                    Renderer lastRenderer = lastSelectedObject.GetComponent<Renderer>();
                    if (lastRenderer != null)
                    {
                        lastRenderer.material.color = Color.white;
                    }
                }

                
                source = selectedObject.GetComponent<AudioSource>();
                if (source)
                    source.Play();
                SpeakFeedbackVerbal(selectedObject.transform.parent.name);
                Debug.LogError("=========Speek  " + selectedObject.gameObject.name);
                Renderer objectRenderer = selectedObject.GetComponent<Renderer>();
                if (objectRenderer != null)
                {
                    objectRenderer.material.color = Color.red;
                }

                
                lastSelectedObject = selectedObject;
            }
        }
#endif
    }

    /// <summary>
    /// The function uses TextToSpeech function from UAP Accessibility Plugin to speak a text
    /// </summary>
    public void SpeakFeedbackVerbal(string text)
    {
        // if hit any new object and the name callout of another object is
        // not finished, we manually cut that callout
        StopVerbalFeedback();

        // play the new callout
        if (!UAP_AccessibilityManager.IsSpeaking())
        {
            UAP_AccessibilityManager.Say(text, true, true, UAP_AudioQueue.EInterrupt.All);
        }
    }


    /// <summary>
    /// The function will stop any verbal speech which is playing by using UAP
    /// </summary>
    public void StopVerbalFeedback()
    {
        if (UAP_AccessibilityManager.IsSpeaking())
            UAP_AccessibilityManager.StopSpeaking();
    }


    private void DetectMouseDoubleClick()
    {
        // Detect the start of the click
        if (Input.GetMouseButtonDown(0))
        {
            // Get current time
            float currentTime = Time.time;

            // Check the time interval between clicks
            if (currentTime - lastClickTime <= doubleClickTime)
            {
                SceneJumpHelper.ResetThenSwitchScene(mainMenu);      // do all the resets needed then back to the main menu
            }

            // Update the time of last click
            lastClickTime = currentTime;
        }
    }

    private void DetectTouchDoubleTap()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Detect the start of the touch
            if (touch.phase == TouchPhase.Began)
            {
                // Get current time
                float currentTime = Time.time;

                // Check the time interval between touches
                if (currentTime - lastClickTime <= doubleClickTime)
                {
                    // double click detected
                    SceneManager.LoadScene(mainMenu);
                }

                // Update the time of last touch
                lastClickTime = currentTime;
            }
        }
    }
}