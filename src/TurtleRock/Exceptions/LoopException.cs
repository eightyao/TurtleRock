using System;
using System.Collections.Generic;
using System.Text;

namespace TurtleRock.Exceptions
{
  public class LoopException : Exception
  {
    public LoopException()
    {
    }

    public LoopException(string msg)
      : base(msg)
    {
    }

    public LoopException(string msg, Exception innerException)
      : base(msg, innerException)
    {

    }
  }
}
