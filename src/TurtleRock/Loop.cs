using System;
using System.Collections.Concurrent;
using System.Threading;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class Loop
  {
    private const int LOOP_QUEUE_TIMEOUT = 100;
    private const int DEFAULT_RUNNING_BREAK = 100;

    [ThreadStatic]
    private static Thread _currentThread;
    private static Thread CurrentThread => _currentThread ??= Thread.CurrentThread;

    private readonly Thread _loopThread;
    private readonly CancellationTokenSource _cts;
    private readonly BlockingCollection<ILoopRunnable> _loopQueue;
    private readonly ScheduleTaskQueue _scheduledQueue;
    private readonly CancellationToken _stopToken;

    public Action<Exception> OnExceptionCaught;
    
    public bool InEventLoop => _loopThread == CurrentThread;

    public Loop()
    {
      _loopThread = new Thread(Run);
      _loopQueue = new BlockingCollection<ILoopRunnable>(new ConcurrentQueue<ILoopRunnable>());
      _scheduledQueue = new ScheduleTaskQueue();
      _cts = new CancellationTokenSource();
      _stopToken = _cts.Token;
    }

    public void Start()
    {
      _loopThread.Start();
    }

    public void Stop()
    {
      _cts.Cancel();
    }

    public int GetThreadId()
    {
      return _loopThread.ManagedThreadId;
    }

    public void Assert()
    {
      if (!InEventLoop)
      {
        throw new LoopException("Operation is not executed in given thread loop");
      }
    }

    public void Execute(ILoopRunnable runnable)
    {
      if (InEventLoop)
      {
        SafeRun(runnable);
        return;
      }

      if (!_loopQueue.TryAdd(runnable, LOOP_QUEUE_TIMEOUT))
      {
        throw new LoopException("add runnable item to loop queue failed.");
      }
    }

    public void Execute(Action runnable)
    {
      Execute(new ActionRunnable(runnable));
    }

    public void Schedule(IScheduledRunnable scheduledTask)
    {
      Execute(() => { _scheduledQueue.Enqueue(scheduledTask); });
    }

    private void SafeRun(ILoopRunnable runnable)
    {
      try
      {
        runnable?.Run();
      }
      catch (Exception e)
      {
        if (runnable is IExceptionCaught exceptionCatcher)
        {
          exceptionCatcher.OnExceptionCaught(e);
          return;
        }
        
        OnExceptionCaught?.Invoke(e);
      }
    }

    private void Run()
    {
      while (!_stopToken.IsCancellationRequested)
      {
        RunScheduledTasks();
        RunInstanceTasks();
      }
    }

    private int GetNextTimeout(HighResolutionTimeSpan now)
    {
      var scheduledRunnable = _scheduledQueue.Peek();
      if (scheduledRunnable == null)
      {
        return LOOP_QUEUE_TIMEOUT;
      }
      
      if (scheduledRunnable.Deadline <= now)
      {
        return 0;
      }

      return HighResolutionTimeSpan.ToTimeSpan(scheduledRunnable.Deadline - now).Milliseconds;
    }

    private ILoopRunnable TryTakeLoopRunnable(int timeout)
    {
      return !_loopQueue.TryTake(out var runnable, timeout) ? null : runnable;
    }

    private void RunInstanceTasks()
    {
      var now = HighResolutionTimeSpan.FromStartTimeSpan;
      var taskKeepRunningTimeOut = HighResolutionTimeSpan.DeadLine(
        TimeSpan.FromMilliseconds(DEFAULT_RUNNING_BREAK));

      while (true)
      {
        var runnable = TryTakeLoopRunnable(GetNextTimeout(now));
        if (runnable == null)
        {
          return;
        }
        
        SafeRun(runnable);

        now = HighResolutionTimeSpan.FromStartTimeSpan;
        if (now >= taskKeepRunningTimeOut)
        {
          break;
        }
      }
    }

    private void RunScheduledTasks()
    {
      var now = HighResolutionTimeSpan.FromStartTimeSpan;
      var item = _scheduledQueue.Dequeue(now);
      while (item != null)
      {
        item.Run();
        item = _scheduledQueue.Dequeue(now);
      }
    }
    
    private void MoveScheduledTask()
    {
      var now = HighResolutionTimeSpan.FromStartTimeSpan;
      var item = _scheduledQueue.Dequeue(now);
      while (item != null)
      {
        if (!_loopQueue.TryAdd(item))
        {
          _scheduledQueue.Enqueue(item);
        }
        item = _scheduledQueue.Dequeue(now);
      }
    }
  }
}
