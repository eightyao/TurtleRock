using System;
using System.Collections.Generic;
using System.Text;

namespace TurtleRock.Exceptions
{
  public class BufferException : Exception
  {
    public BufferException()
    {
    }

    public BufferException(string msg)
      : base(msg)
    {
    }

    public BufferException(string msg, Exception innerException)
      : base(msg, innerException)
    {

    }
  }
}
