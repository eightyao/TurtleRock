using System;
using System.Threading;
using NUnit.Framework;

namespace TurtleRock.Test.Loop
{
  public class LoopTest
  {
    [Test]
    public void AddRunnableTest()
    {
      TurtleRock.Loop loop = new TurtleRock.Loop();
      loop.Start();
      loop.Execute(() =>
      {
        Assert.AreEqual(loop.GetThreadId(), Thread.CurrentThread.ManagedThreadId);
      });
      Thread.Sleep(1000);
      loop.Stop();
    }

    [Test]
    public void AddScheduleTaskTest()
    {
      TurtleRock.Loop loop = new TurtleRock.Loop();
      loop.Start();
      loop.Schedule(new TestScheduleTask
      {
        Deadline = HighResolutionTimeSpan.DeadLine(new TimeSpan(0,0,0,10))
      });
      Thread.Sleep(11000);
    }
  }

  public class TestScheduleTask : IScheduledRunnable
  {
    public void Run()
    {
      var currentTs = HighResolutionTimeSpan.FromStartTimeSpan;
      var gap = currentTs - Deadline;
      var ts = HighResolutionTimeSpan.ToTimeSpan(gap);

      Assert.IsTrue(ts.Milliseconds < 10);
    }

    public HighResolutionTimeSpan Deadline { get; set; }
    public void Cancel()
    {
      throw new System.NotImplementedException();
    }
  }
}