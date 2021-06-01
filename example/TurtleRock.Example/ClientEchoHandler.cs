using System;
using System.Text;
using TurtleRock.Handler;

namespace TurtleRock.Example
{
  public class ClientEchoHandler : AbstractChainHandler
  {
    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      string msg = buf.GetString(buf.ReadableBytes, Encoding.UTF8);
      Console.WriteLine($"client received:{msg}");
      ctx.WriteAsync(buf);
      ctx.FlushAsync();
    }
  }
}