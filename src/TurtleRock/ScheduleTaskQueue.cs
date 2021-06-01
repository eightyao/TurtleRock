using System.Collections.Generic;

namespace TurtleRock
{
  public class ScheduleTaskQueue : LinkedList<IScheduledRunnable>
  {
    public IScheduledRunnable Peek()
    {
      if (Count <= 0)
      {
        return null;
      }

      return Last.Value;
    }
    public void Enqueue(IScheduledRunnable runnable)
    {
      var node = First;

      while (node != null && runnable.Deadline < node.Value.Deadline)
      {
        node = node.Next;
      }

      if (node == null)
      {
        AddLast(runnable);
      }
      else
      {
        AddBefore(node, runnable);
      }
    }

    public IScheduledRunnable Dequeue(HighResolutionTimeSpan ts)
    {
      if (Count <= 0)
      {
        return null;
      }

      var item = Last.Value;

      if (item.Deadline > ts)
        return null;

      RemoveLast();
      return item;
    }
  }
}
