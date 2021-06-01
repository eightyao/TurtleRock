using System;
using System.Diagnostics;
using System.Net;
using TurtleRock.Handler;

namespace TurtleRock.Example
{
  class Program
  {
    static void Main(string[] args)
    {
      //TestServer();
      TestClient();
    }

    static void TestClient()
    {
      TcpClient c = new TcpClient();
      c.Option(ChannelOption.NoDelay, true)
        .StreamChainInitializer(newChannel =>
        {
          newChannel.Chain.Append("echo", new EchoHandler());
        });

      var channel = c.ConnectAsync("127.0.0.1", 8003);
      Console.Read();
      c.Dispose();
    }

    static void TestServer()
    {
      Console.WriteLine($"Process Id {Process.GetCurrentProcess().Id}");
      TcpServer s = new TcpServer(Environment.ProcessorCount * 2);
      s.Option(ChannelOption.NoDelay, true)
         //.Option(ChannelOption.RecevieBufferCapacity, 8 * 1024)
        .Option(ChannelOption.HighWriteWaterMark, 128 * 1024)
        .Option(ChannelOption.LowWriteWaterMark, 64 * 1024)
        .ServerOption(ChannelOption.ReuseAddress, true)
        .ServerOption(ChannelOption.Backlog, 8192)
        .StreamChainInitializer(newChannel =>
        {
          //newChannel.Chain.Append("HeadLengthDecoder", new HeadLengthDecoder(4, 4092 * 40));
          //newChannel.Chain.Append("headLengthAppender", new HeadLengthAppender());
          // newChannel.Chain.Append("toLower", new ToLowerHandler());
          newChannel.Chain.Append("idle", new IdleHandler(5000, 0));
          newChannel.Chain.Append("echo", new EchoHandler());
        })
        .Start(new IPEndPoint(IPAddress.Any, 8888));

      Console.WriteLine("tcp server started, press any key to quit...");
      Console.Read();
      s.Shutdown();
      Console.WriteLine("tcp server shut down");
    }
  }
}
