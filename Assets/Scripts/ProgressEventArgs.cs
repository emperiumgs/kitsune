using System;

public class ProgressEventArgs : EventArgs
{
    public string text;
    public float timer;

    /// <summary>
    /// Defines the new progress bar stats
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="timer">The timer it lasts</param>
    public ProgressEventArgs(string text, float timer)
    {
        this.text = text;
        this.timer = timer;
    }
}
