using System.Collections.Generic;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class LoopGroup
  {
    private int _dispatchIndex = 0;
    private readonly List<Loop> _loops = new List<Loop>();

    public Loop Next()
    {
      if (_loops.Count <= 0)
      {
        throw new LoopException("empty loop group");
      }

      if (_dispatchIndex >= _loops.Count)
      {
        _dispatchIndex = 0;
      }

      return _loops[_dispatchIndex++];
    }

    public void StartLoops(int loopCount)
    {
      for (int i = 0; i < loopCount; i++)
      {
        var loop = new Loop();
        _loops.Add(loop);
        loop.Start();
      }
    }

    public void StopLoops()
    {
      foreach (var loop in _loops)
      {
        loop.Stop();
      }
    }
  }
}
