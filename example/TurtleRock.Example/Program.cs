using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using TurtleRock.Handler;

namespace TurtleRock.Example
{
  class Program
  {
    private static TcpServer _server;
    private static TcpClient _client;
    static void Main(string[] args)
    {
      StartServer();
      var channel = StartClient();
      
      Console.Read();
      
      channel.Disconnect();
      _client.Dispose();
      _server.Shutdown();
    }

    static TcpChannel StartClient()
    {
      _client = new TcpClient(1);
      _client.Option(ChannelOption.NoDelay, true)
        .StreamChainInitializer(newChannel =>
        {
          newChannel.Chain.Append("HeadLengthDecoder", new HeadLengthDecoder(4, 4092 * 40));
          newChannel.Chain.Append("headLengthAppender", new HeadLengthAppender());
          newChannel.Chain.Append("echo", new ClientEchoHandler());
        });

      var channel = _client.ConnectAsync("127.0.0.1", 8888);
      return channel;
    }

    static TcpServer StartServer()
    {
      _server = new TcpServer(Environment.ProcessorCount * 2);
      _server.Option(ChannelOption.NoDelay, true)
        .Option(ChannelOption.HighWriteWaterMark, 128 * 1024)
        .Option(ChannelOption.LowWriteWaterMark, 64 * 1024)
        .ServerOption(ChannelOption.ReuseAddress, true)
        .ServerOption(ChannelOption.Backlog, 8192)
        .StreamChainInitializer(newChannel =>
        {
          newChannel.Chain.Append("HeadLengthDecoder", new HeadLengthDecoder(4, 4092 * 40));
          newChannel.Chain.Append("headLengthAppender", new HeadLengthAppender());
          newChannel.Chain.Append("idle", new IdleHandler(3000, 0));
          newChannel.Chain.Append("echo", new EchoHandler());
        })
        .Start(new IPEndPoint(IPAddress.Any, 8888));

      return _server;
    }
  }
}
