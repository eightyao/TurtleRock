using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class TcpServerChannel : AbstractTcpChannel
  {
    private readonly LoopGroup _loopGroup;
    private int _backlog = 8192;

    private readonly ChannelOptionApplier _serverChannelOptionApplier;
    private readonly ChannelOptionApplier _channelOptionApplier;

    public int Backlog
    {
      get => _backlog;
      set
      {
        if (value <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(Backlog));
        }

        _backlog = value;
      }
    }

    public TcpServerChannel(LoopGroup loopGroup,
      ChannelOptionApplier serverOptionApplier,
      ChannelOptionApplier optionApplier) : this(loopGroup)
    {
      _serverChannelOptionApplier = serverOptionApplier;
      _channelOptionApplier = optionApplier;
    }

    public TcpServerChannel(LoopGroup loopGroup)
    {
      _loopGroup = loopGroup;
      ReadAsyncContext.Completed += ListenAsyncArgsOnCompleted;
    }

    private void ListenAsyncArgsOnCompleted(object sender, SocketAsyncEventArgs e)
    {
      if (e.LastOperation == SocketAsyncOperation.Accept)
      {
        ProcessAccept();
      }
    }

    private void ProcessAccept()
    {
      if(_loopGroup == null) throw new ChannelException("null LoopGroup");

      var tcpChannel = new TcpChannel(TcpChannel.ChannelMode.Server, ReadAsyncContext)
      {
        EventLoop = _loopGroup.Next(),
        InitAction = InitAction
      };

      _channelOptionApplier.Apply(tcpChannel);

      tcpChannel.EventLoop.Execute(() =>
      {
        tcpChannel.Initialize();
        tcpChannel.RegisterReceive();
      });
      
      RegisterAccept();
    }

    public void StartListen(IPEndPoint endPoint)
    {
      ChannelSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      _serverChannelOptionApplier.Apply(this);

      ChannelSocket.Bind(endPoint);
      try
      {
        ChannelSocket.Listen(Backlog);
      }
      catch (SocketException)
      {
        ChannelSocket.Dispose();
        throw;
      }
    }

    public void RegisterAccept()
    {
      ReadAsyncContext.AcceptSocket = null;

      if (!ChannelSocket.AcceptAsync(ReadAsyncContext))
      {
        ProcessAccept();
      }
    }
  }
}
