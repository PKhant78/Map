/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: April 3rd, 2022                                         */
/* Class: UserMovement_Editor                                    */
/* Purpose:                                                      */
/* The class controls Avatar's movement (translation & rotation) */
/* in Unity Editor. So the team can debug for interaction        */
/* related functions without build to phone. Thus, the following */
/* script only works in the Unity Editor. For "movement control  */
/* on phone", please visit "UserMovement_IOS" class instead.     */
/*****************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserMovement_Editor : MonoBehaviour
{

#if UNITY_EDITOR

    public bool lockTranslation = false;              // an boolean variable allow disabling avatar's movement (rotation still available)

    GameObject gripPoint;
    GameObject head;                                  // will rotate head use keyboard to test spatial sounds in editor
    GameObject body;
    private Animator _animator;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    Vector3 moveStep;

    Vector3 userRot;                                  // For dynamically recording local euler angle (rotation) of user obj (Mouse movement will continuously update its rotation)
    Vector3 gripRot;                                  // ... of grip point obj ...

    public float moveSpeed = 10f;       // speed of user's translation
    public float rotSpeed = 50f;        // speed of user's rotation
    float updatedMoveSpeed;             // for enabling slow down movement

    public float sensitivity = 10f;     // control the sensitivity of mouse in the game


    /// <summary>
    /// Start function initialize variables and setting when using Unity Editor
    /// </summary>
    private void Start()
    {
        MovementSetup();
        AssignAnimationIDs();
    }


    /// <summary>
    /// Update user's movement when using Unity Editor (use Mouse + Keyboard control)
    /// </summary>
    void Update()
    {
        MovementControl();
    }


    /// <summary>
    /// Setups to do specifically for running this program on Unity Editor
    /// </summary>
    void MovementSetup()
    {
        /* Assigning "gripPoint" object */
        gripPoint = transform.Find("GripPoint").gameObject;
        head = transform.Find("Head").gameObject;

        body = transform.Find("Body").gameObject;
        _animator = body.GetComponent<Animator>();
        Debug.LogError("========== animator   " + _animator);



        /* Initialize these Vector 3 with the starting local euler angle (an representation of rotation) in the scene */
        userRot = transform.localEulerAngles;
        gripRot = gripPoint.transform.localEulerAngles;

        /* Lock the mouse into game once start */
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    /// <summary>
    /// Method for controlling user's movement when running the program in Unity Editor
    /// </summary>
    void MovementControl()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Replay"))
        {
            PositionControl();
            RotationControl();
        }

    }


    /// <summary>
    /// Method for controlling user's position when running on Unity Editor
    /// </summary>
    void PositionControl()
    {
        /* Update avatar's movement only when movement is not locked */
        if (!lockTranslation)
        {
            /* Press "Shift" to slow down the move speed when needed */
            if (Input.GetKey(KeyCode.LeftShift))
            {
                updatedMoveSpeed = moveSpeed * 0.1f;
            }
            else { updatedMoveSpeed = moveSpeed; }

            /* Using WASD to control User body's translation */

            moveStep = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                moveStep = Vector3.forward * updatedMoveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveStep = Vector3.back * updatedMoveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveStep = Vector3.left * updatedMoveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveStep = Vector3.right * updatedMoveSpeed * Time.deltaTime;
            }

            transform.Translate(moveStep);

            if (_animator)
            {
                
                _animator.SetBool(_animIDSpeed, (Vector3.Distance(Vector3.zero, moveStep) > 0));
            }


        }
    }


    /// <summary>
    /// Method for controlling user's rotation when running on Unity Editor
    /// </summary>
    void RotationControl()
    {
        /* Using Mouse to control all the rotations 
         *
         * NOTE - Rotation rules:
         * 1. Horizontal rotation is y-axis
         * 2. Forward & Back rotation is x-axis
         * 3. Right to Left Side rotation is z-axis
         *
         */
        userRot.y += Input.GetAxis("Mouse X") * sensitivity;        // Let user's horizontal rotation to be controlled by left & right movement of mouse
        gripRot.x -= Input.GetAxis("Mouse Y") * sensitivity;        // Let gripPoint's forward & back rotation to be controlled by fron & back movement of mouse. We use Why using "-=": For similar reason as above

        if (gripRot.x > 90)
        {
            gripRot.x -= 90;
        }

        transform.localEulerAngles = userRot;                       // Assigning the most updated ROT to user, head, and grip's localRotation
        gripPoint.transform.localEulerAngles = gripRot;

        /* Rotate Avatar' head horizontally to test 3D audio */
        if (Input.GetKey(KeyCode.Q))
        {
            head.transform.localEulerAngles += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            head.transform.localEulerAngles += new Vector3(0, 1, 0);
        }

        /* Rotate Avatar's head vertically */
        if (Input.GetKey(KeyCode.R))
        {
            head.transform.localEulerAngles += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.F))
        {
            head.transform.localEulerAngles += new Vector3(1, 0, 0);
        }
    }



#endif

}

