using System;

namespace TurtleRock.Handler
{
  public interface IChainHandler
  {
    void OnAddToChain(IChannelContext ctx);
    void OnRemoveFromChain(IChannelContext ctx);
    void OnChannelConnected(IChannelContext ctx);
    void OnChannelReceived(IChannelContext ctx, IBuffer buf);
    void OnChannelDisconnected(IChannelContext ctx);
    void OnChannelDisconnecting(IChannelContext ctx);
    void OnChannelEvent(IChannelContext ctx, object channelEvent);
    void Write(IChannelContext ctx, IBuffer buf);
    void Flush(IChannelContext ctx);
    void OnChannelException(IChannelContext ctx, Exception e);
  }
}
