using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class WayPointManager : MonoBehaviour
{
    NavigationManager navManager;           // Manager in charge of navigation system => will update the "collide with Avatar status" to NavigationManager
    NavSpeakerManager speakerManager;       // Manager takes care of NavSpeaker => will turn off its navigational sound when playing success music from wayPoint

    AudioSource audioSource;                // The audioSource which carried by the wayPoint

    bool hitUser;                           // The variable track if the wayPoint hits the user avatar or not. Only the first hit counts


    /// <summary>
    /// Awake is called before all any other functions in Unity execution sequence
    ///
    /// [Detail]
    /// For all wayPoints, we want them to be disabled when scene started, and when they
    /// awake. So we call "SetActive" function in awake to disable all of them. They will
    /// be activated by "NavigationManager" when it's needed
    /// 
    /// </summary>
    private void Awake()
    {
        /* Get the script of navigation manager */
        navManager = transform.root.GetComponent<NavigationManager>();

        /* Get the script of navSpeaker manager from its own navigation system */
        speakerManager = transform.root.Find("NavSpeaker").GetComponent<NavSpeakerManager>();

        /* Get the AudioSource on wayPoint */
        audioSource = GetComponent<AudioSource>();

        /* Set the wayPoint itself to inactive */
        transform.gameObject.SetActive(false);
    }


    /// <summary>
    /// Function called everytime when the wayPoint, which it attached to, become active
    /// </summary>
    private void OnEnable()
    {
        /* Set waypoint to "never ever hit an user" status whenever been newly activated */
        hitUser = false;

        /* Grab the navigation speaker to current waypoint and let it play sound */
        GrabNavSpeakerAndPlay();
    }


    /// <summary>
    /// When a wayPoint collided with Avatar's body, it will set "pointShouldBeActive"
    /// variable in "NavigationManager.cs" to False. Then the "NavigationManager" will
    /// know it's time to deactivate this wayPoint and activate the next (if available)
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!hitUser)                                                                   // if the wayPoint never hits user avatar
        {
            Collider collider = collision.collider;                                     // get the collider

            if (collider.transform.root.name == "User" && collider.name == "Body")      // if collided with Avatar's body
            {
                hitUser = true;                                                         // turn the hitUser boolean variable to TRUE, so it won't detect duplicated hits

                speakerManager.StopNavAudio();                                          // stop ongoing navigational music on NavSpeaker
                audioSource.Play();                                                     // playing success music from the wayPoint to tell the user they reach a waypoint
                StartCoroutine(WaitForSound());                                         // wait for success music to finish before update status to navigation manager
            }
        }
    }


    /// <summary>
    /// Wait the wayPoint to finish playing the success sounds, before 
    /// </summary>
    private IEnumerator WaitForSound()
    {
        while (audioSource.isPlaying)                           // wait for the success music to play before taking other action
        {
            yield return null;
        }

        navManager.NextPoint();                                 // once collided with Avatar's body, ask the Navigation Manager to set the current point to inactive, and set the next point to active
    }


    /// <summary>
    /// Function grabs the navSpeaker to the current waypoint and play the audio in it
    /// </summary>
    void GrabNavSpeakerAndPlay()
    {
        Vector3 position = transform.position;                  // get position of current waypoint
        speakerManager.TransportNavSpeaker(position);           // transport the NavSpeaker to new wayPoint
        speakerManager.PlayNavAudio();                          // play navigation audio
    }


    /// <summary>
    /// Function does things-to-do after triggering a way point
    /// Will be used in "Way Point Tutorial" scene
    /// </summary>
    public async Task<bool> AfterTriggerWayPoint()
    {
        /* stop ongoing navigational music on NavSpeaker */
        speakerManager.StopNavAudio();

        /* Playing success music from the wayPoint after triggering */
        audioSource.Play();

        /* Wait for the success music to play before taking other action */
        while (audioSource && audioSource.isPlaying)
            await Task.Yield();

        /* Return true when the process above is done */
        return true;
    }

}

