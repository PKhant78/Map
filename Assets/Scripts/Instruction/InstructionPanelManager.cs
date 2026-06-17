using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionPanelManager : MonoBehaviour
{
    /* Get the InstructionManager to call function for switch instruction */
    InstructionManager instructionManager;

    /* Boolean variable judges if there is next instruction.  
     * Make it a static variable, so its value will be shared across instantiated class on both NextButton and PrevButton */
    static bool hasNext = true;


    private void Start()
    {
        /* Initialize the InstructionManager */
        instructionManager = GameObject.Find("InstructionSystem").GetComponent<InstructionManager>();
    }


    /// <summary>
    /// The action (Instruction Switching) the button takes after hitted by user's cane
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;                                            // the collider object which the button collides with

        if (other.name == "Cane")                                                       // if the button hitted by cane
        {
            if (AccessColliderInfo.GetParentName(transform) == "NextButton")            // if hit NextButton
            {
                if (hasNext)
                    hasNext = instructionManager.ReadNextInstruction();                 // get the next instruction sentence
                else
                    ToQuitInstructionSystem();                                          // call function to quit the instruction session when "endHitCount != 0"
            }
            else if (AccessColliderInfo.GetParentName(transform) == "PrevButton")       // if hit PrevButton
            {
                instructionManager.ReadPrevInstruction();                               // get the previous instruction sentence
                hasNext = true;                                                         // after trying to read previous instruction, we reset "hasNext" to True. So when cane hits the right panel the next time, it will give instruction again, it won't become silent
            }
        }
    }


    /// <summary>
    /// Function to quit instruction system
    /// </summary>
    private void ToQuitInstructionSystem()
    {
        /* Reset variables */
        hasNext = true;

        /* End the instruction system */
        instructionManager.EndInstructionSystem();
    }

}

