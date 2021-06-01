using TurtleRock.Handler;

namespace TurtleRock.Example
{
  public class PingPongBenchmarkHandler : AbstractChainHandler
  {
    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      ctx.WriteAsync(buf);
      ctx.FlushAsync();
    }
  }
}