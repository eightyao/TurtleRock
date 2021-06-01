using System;

namespace TurtleRock.Exceptions
{
  public class ChannelException : Exception
  {
    public ChannelException()
    {
    }

    public ChannelException(string msg)
      : base(msg)
    {
    }

    public ChannelException(string msg, Exception innerException)
      : base(msg, innerException)
    {

    }
  }
}
