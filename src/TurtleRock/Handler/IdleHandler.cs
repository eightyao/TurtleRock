using System;

namespace TurtleRock.Handler
{
  public class IdleHandler : AbstractChainHandler
  {
    public TimeSpan ReadIdleTime { get; }
    public TimeSpan WriteIdleTime { get; }
    public TimeSpan LastReadTicks { get; private set; }
    public TimeSpan LastWriteTicks { get; private set; }

    public IScheduledRunnable LastScheduledReadIdleTask { get; set; }
    public IScheduledRunnable LastScheduledWriteIdleTask { get; set; }

    public IdleHandler(int readIdleTimeInMs, int writeIdleTimeInMs)
    {
      ReadIdleTime = TimeSpan.FromMilliseconds(readIdleTimeInMs);
      WriteIdleTime = TimeSpan.FromMilliseconds(writeIdleTimeInMs);
    }

    public override void OnChannelConnected(IChannelContext ctx)
    {
      LastWriteTicks = LastReadTicks = TimeSpan.FromMilliseconds(Environment.TickCount);

      if (ReadIdleTime != TimeSpan.Zero)
      {
        ctx.EventLoop.Schedule(new ReadIdleScheduledTask(ctx, ReadIdleTime));
      }

      if (WriteIdleTime != TimeSpan.Zero)
      {
        ctx.EventLoop.Schedule(new WriteIdleScheduledTask(ctx, WriteIdleTime));
      }
      
      ctx.FireChannelConnected();
    }

    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      LastReadTicks = TimeSpan.FromMilliseconds(Environment.TickCount);
      ctx.FireChannelReceived(buf);
    }

    public override void OnChannelDisconnected(IChannelContext ctx)
    {
      LastScheduledReadIdleTask?.Cancel();
      LastScheduledWriteIdleTask?.Cancel();

      ctx.FireChannelDisconnected();
    }

    public override void Write(IChannelContext ctx, IBuffer buf)
    {
      LastWriteTicks = TimeSpan.FromMilliseconds(Environment.TickCount);
      ctx.FireWrite(buf);
    }
  }

  public class ReadIdleScheduledTask : IScheduledRunnable
  {
    private readonly IChannelContext _ctx;
    private volatile bool _isCancel = false;

    public HighResolutionTimeSpan Deadline { get; set; }
    public void Cancel()
    {
      _isCancel = true;
    }

    public ReadIdleScheduledTask(IChannelContext ctx, TimeSpan delay)
    {
      _ctx = ctx;
      Deadline = HighResolutionTimeSpan.DeadLine(delay);
    }

    public ReadIdleScheduledTask(IChannelContext ctx, HighResolutionTimeSpan deadLine)
    {
      _ctx = ctx;
      Deadline = deadLine;
    }

    public void Run()
    {
      if (_ctx.ClientChannel.Closed)
        return;

      if (_isCancel)
        return;

      var now = TimeSpan.FromMilliseconds(Environment.TickCount);
      var idleHandler = (IdleHandler) _ctx.Handler;

      var timePassedFromLastRead = now - idleHandler.LastReadTicks;
      if (timePassedFromLastRead >= idleHandler.ReadIdleTime)
      {
        ReadIdleScheduledTask nextScheduledTask = new ReadIdleScheduledTask(
          _ctx, idleHandler.ReadIdleTime);
        
        _ctx.EventLoop.Schedule(nextScheduledTask);
        idleHandler.LastScheduledReadIdleTask = nextScheduledTask;
        _ctx.Chain.FireChannelEvent(new IdleEvent {Status = IdleEventStatus.ReadIdle});
      }
      else
      {
        ReadIdleScheduledTask reScheduledTask = new ReadIdleScheduledTask(
          _ctx, idleHandler.ReadIdleTime - timePassedFromLastRead);
        
        _ctx.EventLoop.Schedule(reScheduledTask);
        idleHandler.LastScheduledReadIdleTask = reScheduledTask;
      }
    }
  }
  
  public class WriteIdleScheduledTask : IScheduledRunnable
  {
    private readonly IChannelContext _ctx;
    private volatile bool _isCancel = false;

    public HighResolutionTimeSpan Deadline { get; set; }
    public void Cancel()
    {
      _isCancel = true;
    }

    public WriteIdleScheduledTask(IChannelContext ctx, TimeSpan delay)
    {
      _ctx = ctx;
      Deadline = HighResolutionTimeSpan.DeadLine(delay);
    }

    public WriteIdleScheduledTask(IChannelContext ctx, HighResolutionTimeSpan deadLine)
    {
      _ctx = ctx;
      Deadline = deadLine;
    }

    public void Run()
    {
      if (_ctx.ClientChannel.Closed)
        return;

      if (_isCancel)
        return;

      var now = TimeSpan.FromMilliseconds(Environment.TickCount);
      var idleHandler = (IdleHandler) _ctx.Handler;
      var timePassedFromLastWrite = now - idleHandler.LastWriteTicks;
      if (timePassedFromLastWrite >= idleHandler.WriteIdleTime)
      {
        WriteIdleScheduledTask nextScheduledTask = new WriteIdleScheduledTask(
          _ctx, idleHandler.WriteIdleTime);
        
        _ctx.EventLoop.Schedule(nextScheduledTask);
        idleHandler.LastScheduledWriteIdleTask = nextScheduledTask;
        _ctx.Chain.FireChannelEvent(new IdleEvent(){Status = IdleEventStatus.WriteIdle});
      }
      else
      {
        WriteIdleScheduledTask reScheduledTask = new WriteIdleScheduledTask(
          _ctx, idleHandler.WriteIdleTime - timePassedFromLastWrite);
        _ctx.EventLoop.Schedule(reScheduledTask);
        idleHandler.LastScheduledWriteIdleTask = reScheduledTask;
      }
      
    }
  }

  public class IdleEvent
  {
    public IdleEventStatus Status { get; set; }
  }

  public enum IdleEventStatus
  {
    ReadIdle,
    WriteIdle,
    ReadWriteIdle
  }
}
