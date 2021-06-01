using System;

namespace TurtleRock.Handler
{
  public class AbstractChainHandler : IChainHandler
  {
    public virtual void OnAddToChain(IChannelContext ctx)
    {
    }

    public virtual void OnRemoveFromChain(IChannelContext ctx)
    {
    }

    public virtual void OnChannelConnected(IChannelContext ctx)
    {
      ctx.FireChannelConnected();
    }

    public virtual void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      ctx.FireChannelReceived(buf);
    }

    public virtual void OnChannelDisconnected(IChannelContext ctx)
    {
      ctx.FireChannelDisconnected();
    }
    
    public virtual void OnChannelDisconnecting(IChannelContext ctx)
    {
      ctx.FireChannelDisconnecting();
    }

    public virtual void OnChannelEvent(IChannelContext ctx, object channelEvent)
    {
      ctx.FireChannelEvent(channelEvent);
    }

    public virtual void Write(IChannelContext ctx, IBuffer buf)
    {
      ctx.FireWrite(buf);
    }

    public virtual void Flush(IChannelContext ctx)
    {
      ctx.FireFlush();
    }

    public virtual void OnChannelException(IChannelContext ctx, Exception e)
    {
      ctx.FireChannelException(e);
    }
  }
}
