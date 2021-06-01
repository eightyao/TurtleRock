using System;

namespace TurtleRock.Handler
{
  public interface IChannelContext
  {
    StreamChain Chain { get; }
    IChainHandler Handler { get; }
    string HandlerName { get; }
    ChannelContext Next { get; }
    ChannelContext Prev { get; }
    TcpChannel ClientChannel { get; }
    Loop EventLoop { get; }
    bool Connected { get; }
  
    void DisconnectAsync();
    void FireChannelConnected();
    void FireChannelReceived(IBuffer buf);
    void FireChannelDisconnected();
    void FireChannelDisconnecting();
    void FireChannelEvent(object channelEvent);
    void FireChannelException(Exception e);
    void FireWrite(IBuffer buf);
    void FireFlush();
    void WriteAsync(IBuffer buf);
    void FlushAsync();
  }
}