namespace TurtleRock.Handler
{
  public class TailHandler : AbstractChainHandler
  {
    public override void OnChannelDisconnecting(IChannelContext ctx)
    {
      ctx.ClientChannel.Disconnect();
    }
  }
}
