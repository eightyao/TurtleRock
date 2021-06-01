using System;

namespace TurtleRock
{
  public interface IExceptionCaught
  {
    void OnExceptionCaught(Exception e);
  }
}