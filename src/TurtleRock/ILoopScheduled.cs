using System;
using System.Collections.Generic;
using System.Text;

namespace TurtleRock
{
  public interface ILoopScheduled
  {
    HighResolutionTimeSpan Deadline { get; set; }
    void Cancel();
  }
}
