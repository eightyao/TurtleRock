using System;

namespace TurtleRock.Exceptions
{
  public class ChainHandlerException : Exception
  {
    public ChainHandlerException()
    {
    }

    public ChainHandlerException(string msg)
      : base(msg)
    {
    }

    public ChainHandlerException(string msg, Exception innerException)
      : base(msg, innerException)
    {

    }
  }
}