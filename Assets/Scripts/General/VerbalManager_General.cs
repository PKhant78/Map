using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class VerbalManager_General : MonoBehaviour
{
    ObjectsActivenessController activenessController;       // a class which can be used to control the activeness status of interable objects in the scene


    /// <summary>
    /// Modifying the speaking related Setting in Start()
    /// </summary>
    private void Start()
    {
        activenessController = new ObjectsActivenessController();
    }


    /// <summary>
    /// The function uses TextToSpeech function from UAP Accessibility Plugin to speak a text
    /// </summary>
    public void Speak(string text)
    {
        /* Use Try...Finally to prevent potential "Process..." error from StopSpeak() */
        try
        {
            StopSpeak();
        }
        finally
        {
            if (!UAP_AccessibilityManager.IsSpeaking())
                UAP_AccessibilityManager.Say(text, false, true, UAP_AudioQueue.EInterrupt.All);
        }
    }


    /// <summary>
    /// The function will stop any verbal speech which is playing by using UAP
    /// </summary>
    public void StopSpeak()
    {
        if (UAP_AccessibilityManager.IsSpeaking())
            UAP_AccessibilityManager.StopSpeaking();
    }


    /// <summary>
    /// Check if the UAP is speaking
    /// </summary>
    public bool IsSpeaking()
    {
        return UAP_AccessibilityManager.IsSpeaking();
    }


    /// <summary>
    /// Function wait for tiny amount of time before checking UAP isSpeaking
    /// to avoid the case that the UAP is not triggered yet, but speech text is sent
    /// (isSpeaking)
    /// </summary>
    public async Task<bool> IsSeaking_Improved()
    {
        /* Wait for a tiny amount of time */
        float timer = 0f;
        float timeThreshold = 0.1f;
        while (timer < timeThreshold)
        {
            timer += 1f * Time.deltaTime;
            await Task.Yield();
        }

        /* Return isSpeaking boolean value */
        return IsSpeaking();
    }


    /// <summary>
    /// 
    /// The function speaks a text passed in by user. Wait for the speech to end
    ///
    /// [Parameters]
    /// 1. text ===> the text user want to speak
    /// 
    /// </summary>
    public async Task<bool> SpeakAndWait(string text)
    {
        /* Speak the text provided */
        Speak(text);

        /* When the text just send to verbalManager_General, UAP hasn't start to speak yet. 
         * In that case, code will exit the "while" immediately. Thus I added some timer delay here.
         * This ensure the text arrives at the UAP text-to-speech speaker */
        float timer = 0f;
        float timeThreshold = 0.1f;
        while (IsSpeaking() == true || timer < timeThreshold)
        {
            timer += 1f * Time.deltaTime;
            await Task.Yield();
        }

        /* Need to return something, becasue can't await a void function in another funtion */
        return true;
    }


    /// <summary>
    /// 
    /// The function speaks a text passed in by user. Wait for the speech to end
    /// and then call a callback function passed in.
    ///
    /// [Parameters]
    /// 1. text ===> the text user want to speak
    /// 2. callback ===> the callback function to implement after speech is finished
    ///
    /// [Example of calling this function]
    /// verbalManager_General.SpeakWaitAndCallback("hey, how are you!", () => {
    ///     /* Code for actions to do after the speech is finished */
    /// })
    /// 
    /// </summary>
    public async void SpeakWaitAndCallback(string text, Action callback)
    {
        /* Speak and wait for the speaking to finish */
        await SpeakAndWait(text);

        /* Run the callback function */
        callback();
    }


    /// <summary>
    /// Function read out loud a sentence after disabling interactable objects in the scene,
    /// so it makes sure this speech won't be interrupted by name callout due to cane hits any objects.
    ///
    /// [Example Usage]
    /// It can be used after the cane hits an specific objects, and you want to give a sentence of verbal
    /// instruction to the user. You want the user to listen the full content of your instruction. So you
    /// can use this function to avoid user's cane hits another object and break your instruction speech.
    /// </summary>
    public void SpeakWithoutDisturb(string text)
    {
        /* disable all the objects in the scene so the speaking won't be disturbed */
        activenessController.DisableOtherObjs();

        /* Speak the text and implement actions in a callback function*/
        SpeakWaitAndCallback(text, () => {
            /* After finishing speaking, activate objects */
            activenessController.EnableOtherObjs();
        });
    }


    /// <summary>
    /// Function waits the previous speech to end then speak new text passed as parameter
    /// For example: the function is used in "StartInstructionSystem()" function of "InstructionManager" class
    /// </summary>
    public async void WaitThenSpeak(string text)
    {
        /* If any speech is ongoing ===> wait for it to finish */
        while (IsSpeaking())
        {
            await Task.Yield();
        }

        /* Speak the new content */
        Speak(text);
    }


    /// <summary>
    /// Function speak texts from a list one-by-one. It will take a short pause after speaking each of them
    /// before speaking the next
    /// </summary>
    /// <param name="beginWaitTimeFloat"> Waiting time (seconds) before starting to read the 1st text in textList </param>
    /// <param name="textList"> A list of texts to read </param>
    /// <param name="gapTimeFloat"> The gap time (seconds) between reading each text in the list </param>
    /// <param name="gapAfterLast"> A boolean variable indicates whether waiting for a gap time after reading the last text of the list </param>
    /// <returns> Function returns TRUE when the whole function is done </returns>
    public async Task<bool> SpeakWithGapTime(float beginWaitTimeFloat, List<string> textList, float gapTimeFloat, bool hasGapAfterLast)
    {
        /* Transform the time provided by user of this function 
         * "Task.Delay()" function takes number of milliseconds as parameter.
         * If users provides "3", they means waiting for 3 seconds ===> it equals to 3000 milliseconds.
         * We need to transform the time into milliseconds before providing to "Task.Delay()" function. */
        int beginWaitTime = (int)Math.Ceiling(beginWaitTimeFloat * 1000);
        int gapTime = (int)Math.Ceiling(gapTimeFloat * 1000);

        /* Wait for X seconds before start to speak with gap time */
        await Task.Delay(beginWaitTime);

        /* Speaking each text from textList with a gapTime between them */
        int length = textList.Count;
        for (int i = 0; i < length; ++i)
        {
            Speak(textList[i]);

            if (i < length - 1)
                await Task.Delay(gapTime);
            else if (i == length - 1 && hasGapAfterLast)
                await Task.Delay(gapTime);
        }

        return true;
    }


    /// <summary>
    /// Function speaks the countdown like 5...4...3...2...1.
    /// </summary>
    /// <param name="countDownSeconds"> How many seconds you want to countdown </param>
    /// <param name="descending"> Variable indicates if you want to read the countdown in descending or ascending order </param>
    /// <returns></returns>
    public async Task<bool> SpeakCountdown(int countDownSeconds, bool descending = true)
    {
        /* Prepare a list of number for the countdown*/
        List<string> countdownList = new();

        /* Generating the countdown numbers */
        for (int i = countDownSeconds; i > 0; --i)
            countdownList.Add(i.ToString());

        /* Reverse the countdown numbers in the list if needed */
        if (!descending)
            countdownList.Reverse();

        /* Speak the countdown */
        await SpeakWithGapTime(0.5f, countdownList, 1f, true);

        /* Return TRUE when the countdown ends.
         * Other function which calls this function doesn't need to recive variable from it.
         * However, this return is needed to enable "await" this function in other functions */
        return true;
    }

}

