using System;
using System.Net;
using System.Net.Sockets;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class AbstractTcpChannel 
  {
    protected SocketAsyncEventArgs ReadAsyncContext { get; set; }
    protected SocketAsyncEventArgs WriteAsyncContext { get; set; }
    
    protected readonly WriteFlushCache FlushCache = new WriteFlushCache();

    internal Socket ChannelSocket { get; set; }
    public Loop EventLoop { get; set; }
    public StreamChain Chain { get; protected set; }
    public TcpChannelStatus Status { get; protected set; }
    internal Action<AbstractTcpChannel> InitAction { get; set; }

    internal bool ReuseAddress
    {
      get
      {
        try
        {
          return (int)ChannelSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) != 0;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }
    internal int RcvBufSize
    {
      get
      {
        try
        {
          return ChannelSocket.ReceiveBufferSize;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.ReceiveBufferSize = value;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }
    internal bool NoDelay
    {
      get
      {
        try
        {
          return ChannelSocket.NoDelay;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.NoDelay = value;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }
    internal int SndBufSize
    {
      get
      {
        try
        {
          return ChannelSocket.SendBufferSize;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.SendBufferSize = value;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }
    internal bool KeepAlive
    {
      get
      {
        try
        {
          return (int)ChannelSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) != 0;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value ? 1 : 0);
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }
    internal int Linger
    {
      get
      {
        try
        {
          var lingerState = ChannelSocket.LingerState;
          return lingerState.Enabled ? lingerState.LingerTime : -1;
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
      set
      {
        try
        {
          ChannelSocket.LingerState = value < 0
            ? new LingerOption(false, 0)
            : new LingerOption(true, value);
        }
        catch (Exception e)
        {
          throw new ChannelException(e.Message, e);
        }
      }
    }

    internal int ReceiveBufferCapacity { get; set; }
    public IPEndPoint RemoteEndPoint { get; set; }

    public bool Closed
    {
      get
      {
        if (ChannelSocket == null)
        {
          return true;
        }

        return !ChannelSocket.Connected;
      }
    }

    protected AbstractTcpChannel()
    {
      ReadAsyncContext = new SocketAsyncEventArgs();
      WriteAsyncContext = new SocketAsyncEventArgs();
      Status = TcpChannelStatus.Closed;
    }

    internal void Initialize()
    {
      Chain = new StreamChain(this);
      InitAction?.Invoke(this);
      EventLoop.OnExceptionCaught += e => Chain.FireChannelException(e);
      
      if (ReadAsyncContext.SocketError != SocketError.Success)
      {
        Status = TcpChannelStatus.Closed;
        throw new ChannelException(ReadAsyncContext.SocketError.ToString());
      }

      Status = TcpChannelStatus.Open;
      
      Chain.FireChannelConnected();
    }

    public void Disconnect()
    {
      if (EventLoop.InEventLoop)
      {
        Close();
        return;
      }
      
      EventLoop.Execute(Close);
    }

    private void Close()
    {
      if (Status != TcpChannelStatus.Open)
      {
        return;
      }

      Status = TcpChannelStatus.Closing;
      
      try
      {
        ChannelSocket?.Shutdown(SocketShutdown.Both);
      }
      finally
      {
        ChannelSocket?.Dispose();
      }
      
      try
      {
        WriteAsyncContext?.Dispose();
        ReadAsyncContext?.Dispose();
        WriteAsyncContext = null;
        ReadAsyncContext = null;
      }
      catch (Exception e)
      {
        // ignored
      }

      FlushCache.ClearAll();

      Status = TcpChannelStatus.Closed;
      Chain.FireChannelDisconnected();
    }
  }

  public enum TcpChannelStatus
  {
    Open,
    Closing,
    Closed
  }
}