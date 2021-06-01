namespace TurtleRock.Handler
{
  public class HeadHandler : AbstractChainHandler
  {
    public override void Write(IChannelContext ctx, IBuffer buf)
    {
      ctx.ClientChannel.AppendWriteBuffer(buf);
    }

    public override void Flush(IChannelContext ctx)
    {
      ctx.ClientChannel.Flush();
    }
  }
}
