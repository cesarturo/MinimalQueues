using System.Diagnostics;

namespace MinimalQueues.AwsSqs;

public struct Countdown
{
    private readonly double _duration;
    private long _start;

    public Countdown(TimeSpan duration)
    {
        _duration = duration.TotalSeconds * Stopwatch.Frequency;
        _start = 0;
    }
    public void Start() => _start = Stopwatch.GetTimestamp();
    public bool TimedOut => (Stopwatch.GetTimestamp() - _start) >= _duration;
}
