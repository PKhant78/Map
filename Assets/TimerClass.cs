using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private GameObject owner;
    private double duration;
    private Action callback;

    MonoBehaviourHook hook;
    public class MonoBehaviourHook : MonoBehaviour
    {
        public Action tick;
        private void Update()
        {
            if (tick != null)
                tick();
        }
    }

    public static void After(GameObject owner, double duration, Action callback)
    {
        Timer timer = new Timer(owner, duration, callback);
        owner.AddComponent<MonoBehaviourHook>().tick = timer.Tick;
    }

    public Timer(GameObject owner, double duration, Action callback)
    {
        this.owner = owner;
        this.duration = duration;
        this.callback = callback;
    }

    private void Tick()
    {
        if(Completed())
        {
            callback();
            UnityEngine.Object.Destroy(owner.GetComponent<MonoBehaviourHook>());

            return;
        }
    }

    public bool Completed()
    {
        return this.duration <= 0f;
    }
}
