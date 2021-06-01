using System;
using System.Text;
using TurtleRock.Handler;

namespace TurtleRock.Example
{
  public class EchoHandler : AbstractChainHandler
  {
    public int _readIdleCount;
    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      //_readIdleCount = 0;
      // ctx.WriteAsync(buf);
      // ctx.FlushAsync();
      // var buf1 = new PooledHeapBuffer(1024);
      // var buf2 = new PooledHeapBuffer(1024);
      //
      // buf1.WriteString("Your message is ", Encoding.UTF8);
      // buf2.WriteBytes(buf.Array, buf.ReaderIndex, buf.ReadableBytes);
      // buf.Release();
      //
      // ctx.Write(buf1);
      // ctx.Write(buf2);
      // ctx.Flush();
    }

    public override void OnChannelDisconnected(IChannelContext ctx)
    {
      Console.WriteLine($"endpoint: {ctx.ClientChannel.RemoteEndPoint} disconnected.");
    }

    public override void OnChannelDisconnecting(IChannelContext ctx)
    {
      Console.WriteLine($"endpoint: {ctx.ClientChannel.RemoteEndPoint} disconnecting.");
      ctx.FireChannelDisconnecting();
    }

    public override void OnChannelConnected(IChannelContext ctx)
    {
      // var buf = new PooledHeapBuffer();
      // buf.WriteString("Welcome to echo server", Encoding.UTF8);
      // ctx.WriteAsync(buf);
      // ctx.FlushAsync();
      //
      Console.WriteLine($"endpoint: {ctx.ClientChannel.RemoteEndPoint} connected.");
    }

    public override void OnChannelException(IChannelContext ctx, Exception e)
    {
      Console.WriteLine(e);
      ctx.FireChannelException(e);
    }

    public override void OnChannelEvent(IChannelContext ctx, object channelEvent)
    {
      if (channelEvent is IdleEvent idleEvent)
      {
        if (idleEvent.Status == IdleEventStatus.ReadIdle)
        {
          Console.WriteLine($"{idleEvent.Status} {_readIdleCount}");
          _readIdleCount++;
          if (_readIdleCount >= 3)
          {
            ctx.DisconnectAsync();
          }
        }
      }

      base.OnChannelEvent(ctx, channelEvent);
    }
  }
}
