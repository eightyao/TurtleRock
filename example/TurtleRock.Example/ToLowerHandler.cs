using System.Text;
using TurtleRock.Handler;

namespace TurtleRock.Example
{
  public class ToLowerHandler : AbstractChainHandler
  {
    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      string replyStr = buf.GetString(buf.ReadableBytes, Encoding.UTF8);
      buf.Release();
      
      var lower = replyStr.ToLower();
      IBuffer replyBuf = new PooledHeapBuffer();
      replyBuf.WriteString(lower, Encoding.UTF8);
      ctx.FireChannelReceived(replyBuf);
    }
  }
}
