using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using TurtleRock.Handler;

namespace TurtleRock.Test.Handler
{
  public class HeadLengthAppenderTest
  {
    [Test]
    public void AppenderTest()
    {
      HeadLengthAppender appender = new HeadLengthAppender();
      MockChannelContext channelContext = new MockChannelContext();
      
      IBuffer buf = new PooledHeapBuffer();
      string str = "hello world";
      buf.WriteString(str, Encoding.UTF8);
      appender.Write(channelContext, buf);
      Assert.IsTrue(channelContext.AddedBuffers.Count == 2);
      var length = channelContext.AddedBuffers[0].ReadInt32Be();
      Assert.IsTrue(length == str.Length);
    }
  }
  
  public class MockChannelContext : IChannelContext
  {
    public StreamChain Chain { get; set; }
    public IChainHandler Handler { get; set; }
    public string HandlerName { get; set; }
    public ChannelContext Next { get; set; }
    public ChannelContext Prev { get; set; }
    public TcpChannel ClientChannel { get; } = null;
    public TurtleRock.Loop EventLoop { get; } = null;
    public bool Connected => true;
    public void DisconnectAsync() { }
    public void FireChannelConnected() { }
    
    public List<IBuffer> AddedBuffers { get; } = new List<IBuffer>();
    public List<IBuffer> NextBuffers { get; } = new List<IBuffer>();
    public void FireChannelReceived(IBuffer buf)
    {
      NextBuffers.Add(buf);
    }
    public void Write(IBuffer buf)
    {
      AddedBuffers.Add(buf);
    }
    
    public void FireChannelDisconnected() { }
    public void FireChannelDisconnecting()
    {
      
    }

    public void FireChannelEvent(object channelEvent) { }

    public void FireChannelException(Exception e) { }

    public void FireWrite(IBuffer buf) { }

    public void FireFlush() { }
    public void WriteAsync(IBuffer buf)
    {
    }

    public void FlushAsync()
    {
    }

    public void Flush() { }
  }
}