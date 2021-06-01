using System;
using TurtleRock.Exceptions;

namespace TurtleRock.Handler
{
  public class ChannelContext : IChannelContext
  {
    public StreamChain Chain { get; }
    private AbstractTcpChannel Channel { get; }
    public IChainHandler Handler { get; }
    public string HandlerName { get; }
    
    public ChannelContext Next { get; set; }
    public ChannelContext Prev { get; set; }

    public TcpChannel ClientChannel => (TcpChannel)Channel;
    public Loop EventLoop => Channel.EventLoop;
    public bool Connected => ClientChannel.ChannelSocket.Connected;

    public ChannelContext(string name, IChainHandler handler, StreamChain chain, AbstractTcpChannel channel)
    {
      HandlerName = name;
      Handler = handler;
      Chain = chain;
      Channel = channel;
    }

    public void DisconnectAsync()
    {
      EventLoop.Assert();
      Next?.InvokeChannelDisconnecting();
    }

    public void FireChannelConnected()
    {
      EventLoop.Assert();
      Next?.InvokeChannelConnected();
    }

    public void FireChannelReceived(IBuffer buf)
    {
      EventLoop.Assert();
      Next?.InvokeChannelReceived(buf);
    }

    public void FireChannelDisconnected()
    {
      EventLoop.Assert();
      Next?.InvokeChannelDisconnected();
    }

    public void FireChannelDisconnecting()
    {
      EventLoop.Assert();
      Next?.InvokeChannelDisconnecting();
    }

    public void FireChannelEvent(object channelEvent)
    {
      EventLoop.Assert();
      Next?.InvokeChannelEvent(channelEvent);
    }

    public void FireChannelException(Exception e)
    {
      EventLoop.Assert();
      Next?.InvokeChannelException(e);
    }

    public void FireWrite(IBuffer buf)
    {
      EventLoop.Assert();
      Prev?.InvokeWrite(buf);
    }

    public void FireFlush()
    {
      EventLoop.Assert();
      Prev?.InvokeFlush();
    }

    public void WriteAsync(IBuffer buf)
    {
      if (EventLoop.InEventLoop)
      {
        WriteInternal(buf);
        return;
      }
      
      EventLoop.Execute(()=>{WriteInternal(buf);});
    }

    public void FlushAsync()
    {
      if (EventLoop.InEventLoop)
      {
        FlushInternal();
        return;
      }
      
      EventLoop.Execute(FlushInternal);
    }

    private void WriteInternal(IBuffer buf)
    {
      if (ClientChannel.Closed)
      {
        return;
      }
      
      InvokeWrite(buf);
    }

    private void FlushInternal()
    {
      if (ClientChannel.Closed)
      {
        return;
      }
      
      InvokeFlush();
    }

    internal void InvokeChannelConnected()
    {
      Handler.OnChannelConnected(this);
    }

    internal void InvokeChannelReceived(IBuffer buf)
    {
      Handler.OnChannelReceived(this, buf);
    }

    internal void InvokeChannelDisconnected()
    {
      Handler.OnChannelDisconnected(this);
    }
    
    internal void InvokeChannelDisconnecting()
    {
      Handler.OnChannelDisconnecting(this);
    }

    internal void InvokeChannelEvent(object channelEvent)
    {
      Handler.OnChannelEvent(this, channelEvent);
    }

    private void InvokeWrite(IBuffer buf)
    {
      Handler.Write(this, buf);
    }

    private void InvokeFlush()
    {
      Handler.Flush(this);
    }

    internal void InvokeChannelException(Exception e)
    {
      Handler.OnChannelException(this, e);
    }
  }
}
