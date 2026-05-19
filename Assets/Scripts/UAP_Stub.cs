// Stub for UAP Accessibility Plugin (not installed).
// Delete this file once the real plugin is imported from the Asset Store.
using UnityEngine;

public static class UAP_AudioQueue
{
    public enum EInterrupt { All, None }
}

public static class UAP_AccessibilityManager
{
    private static float _speechRate = 1f;

    public static bool IsSpeaking() => false;
    public static void Say(string text, bool interrupt = true, bool flush = true,
        UAP_AudioQueue.EInterrupt interruptMode = UAP_AudioQueue.EInterrupt.All)
        => Debug.Log("[UAP stub] Say: " + text);
    public static void StopSpeaking() { }
    public static void SetSpeechRate(float rate) => _speechRate = rate;
    public static float GetSpeechRate() => _speechRate;
}
