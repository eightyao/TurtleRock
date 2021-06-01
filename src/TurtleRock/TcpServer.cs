using System;
using System.Net;

namespace TurtleRock
{
  public class TcpServer
  {
    private readonly LoopGroup _loopGroup;
    private readonly TcpServerChannel _serverChannel;
    private readonly ChannelOptionApplier _serverChannelOptionApplier;
    private readonly ChannelOptionApplier _channelOptionApplier;

    public TcpServer() : this(Environment.ProcessorCount)
    {
    }

    public TcpServer(int loopCount)
    {
      _serverChannelOptionApplier = new ChannelOptionApplier();
      _channelOptionApplier = new ChannelOptionApplier();

      _loopGroup = new LoopGroup();
      _serverChannel = new TcpServerChannel(
        _loopGroup, 
        _serverChannelOptionApplier, 
        _channelOptionApplier);

      _loopGroup.StartLoops(loopCount);
    }

    public void Start(IPEndPoint endPoint)
    {
      _serverChannel.StartListen(endPoint);
      _serverChannel.RegisterAccept();
    }

    public TcpServer Option(ChannelOption optionName, object value)
    {
      optionName.Check(value);

      _channelOptionApplier.Set(optionName, value);
      return this;
    }

    public TcpServer ServerOption(ChannelOption optionName, object value)
    {
      optionName.Check(value);

      _serverChannelOptionApplier.Set(optionName, value);
      return this;
    }

    public TcpServer StreamChainInitializer(Action<AbstractTcpChannel> handlerInitAction)
    {
      _serverChannel.InitAction = handlerInitAction;
      return this;
    }

    public void Shutdown()
    {
      _loopGroup.StopLoops();
    }
  }
}
