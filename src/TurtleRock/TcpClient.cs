using System;
using System.Net;

namespace TurtleRock
{
  public class TcpClient : IDisposable
  {
    private Action<AbstractTcpChannel> _initAction;
    private readonly LoopGroup _loopGroup;
    private readonly ChannelOptionApplier _channelOptionApplier;

    public TcpClient() : this(Environment.ProcessorCount)
    {
    }

    public TcpClient(int loopCount)
    {
      _channelOptionApplier = new ChannelOptionApplier();

      _loopGroup = new LoopGroup();
      _loopGroup.StartLoops(loopCount);
    }

    public TcpChannel ConnectAsync(string address, int port)
    {
      var endPoint = new IPEndPoint(IPAddress.Parse(address), port);

      TcpChannel channel = new TcpChannel(TcpChannel.ChannelMode.Client)
      {
        EventLoop = _loopGroup.Next(),
        InitAction = _initAction
      };

      _channelOptionApplier.Apply(channel);
      channel.ConnectAsync(endPoint);

      return channel;
    }

    public TcpClient StreamChainInitializer(Action<AbstractTcpChannel> handlerInitAction)
    {
      _initAction = handlerInitAction;
      return this;
    }

    public TcpClient Option(ChannelOption optionName, object value)
    {
      optionName.Check(value);

      _channelOptionApplier.Set(optionName, value);
      return this;
    }

    public void Dispose()
    {
      _loopGroup.StopLoops();
    }
  }
}
