using System.Diagnostics;

namespace MinimalQueues.AwsSqs;

public class Countdown
{
    private TimeSpan _duration;
    private Stopwatch _stopwatch = new Stopwatch();

    public static Countdown StartNew(TimeSpan waitTime)
    {
        var countDown = new Countdown();
        countDown.Start(waitTime);
        return countDown;
    }
    public void Start(TimeSpan duration)
    {
        _duration = duration;
        _stopwatch.Restart();
    }

    public TimeSpan RemainingTime
    {
        get
        {
            var elapsed = _stopwatch.Elapsed;
            return _duration > elapsed ? _duration - elapsed
                : TimeSpan.Zero;
        }
    }

    public bool TimedOut => _stopwatch.Elapsed >= _duration;
}
