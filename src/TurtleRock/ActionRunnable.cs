using System;

namespace TurtleRock
{
  public class ActionRunnable : ILoopRunnable
  {
    private readonly Action _action;

    public ActionRunnable(Action action)
    {
      _action = action;
    }

    public void Run()
    {
      _action?.Invoke();
    }
  }
}
