using System;
using System.Diagnostics;

namespace TurtleRock
{
  public class HighResolutionTimeSpan
  {
    private readonly long _preciseTicks = 0;
    public long Ticks => _preciseTicks;
    
    public HighResolutionTimeSpan(long preciseTicks)
    {
      _preciseTicks = preciseTicks;
    }

    static long GetTicksFromStart()
    {
      return Stopwatch.GetTimestamp() - StartTicks;
    }

    static long TicksToPreciseTicks(long ticks)
    {
      return Stopwatch.IsHighResolution
        ? (long)(ticks * PreciseRatio)
        : ticks;
    }
    
    static long PreciseTicksToTicks(long ticks)
    {
      //return ticks;
      return Stopwatch.IsHighResolution
        ? (long)(ticks * ReversePreciseRatio)
        : ticks;
    }

    protected bool Equals(HighResolutionTimeSpan other)
    {
      return _preciseTicks == other._preciseTicks;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((HighResolutionTimeSpan)obj);
    }

    public override int GetHashCode()
    {
      return _preciseTicks.GetHashCode();
    }

    private static readonly long StartTicks = Stopwatch.GetTimestamp();
    private static readonly double PreciseRatio = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;
    private static readonly double ReversePreciseRatio = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
    public static HighResolutionTimeSpan FromStartTimeSpan => new HighResolutionTimeSpan(GetTicksFromStart());

    public static HighResolutionTimeSpan DeadLine(TimeSpan delay)
    {
      return new HighResolutionTimeSpan(FromStartTimeSpan._preciseTicks + TicksToPreciseTicks(delay.Ticks));
    }

    public static HighResolutionTimeSpan FromTimeSpan(TimeSpan ts)
    {
      return new HighResolutionTimeSpan(TicksToPreciseTicks(ts.Ticks));
    }
    
    public static TimeSpan ToTimeSpan(HighResolutionTimeSpan ts)
    {
      return TimeSpan.FromTicks(PreciseTicksToTicks(ts.Ticks));
    }

    public static bool operator ==(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t2 != null && t1 != null && t1._preciseTicks == t2._preciseTicks;
    }

    public static bool operator !=(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t2 != null && t1 != null && t1._preciseTicks != t2._preciseTicks;
    }

    public static bool operator >(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t1._preciseTicks > t2._preciseTicks;
    }

    public static bool operator <(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t1._preciseTicks < t2._preciseTicks;
    }

    public static bool operator >=(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t1._preciseTicks >= t2._preciseTicks;
    }

    public static bool operator <=(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      return t1._preciseTicks <= t2._preciseTicks;
    }
    
    public static HighResolutionTimeSpan operator -(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      long ticks = t1.Ticks - t2.Ticks;
      return new HighResolutionTimeSpan(ticks);
    }
    
    public static HighResolutionTimeSpan operator +(HighResolutionTimeSpan t1, HighResolutionTimeSpan t2)
    {
      long ticks = t1.Ticks + t2.Ticks;
      return new HighResolutionTimeSpan(ticks);
    }
  }
}
