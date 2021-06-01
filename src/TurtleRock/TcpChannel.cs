using System;
using System.Net;
using System.Net.Sockets;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class TcpChannel : AbstractTcpChannel
  {
    public enum ChannelMode
    {
      Server,
      Client
    }

    private ChannelMode Mode { get; }
    private bool _flushing;
    
    public int HighWriteWaterMark
    {
      get => FlushCache.HighWriteWaterMark;
      set => FlushCache.HighWriteWaterMark = value;
    }

    public int LowWriteWaterMark
    {
      get => FlushCache.LowWriteWaterMark;
      set => FlushCache.LowWriteWaterMark = value;
    }

    public TcpChannel(ChannelMode mode) 
      : this(mode, new Socket(SocketType.Stream, ProtocolType.Tcp))
    {

    }
    public TcpChannel(ChannelMode mode, SocketAsyncEventArgs listenerAsyncContext)
      : this(mode, listenerAsyncContext.AcceptSocket)
    {
      if (mode != ChannelMode.Server)
      {
        throw new ChannelException("this constructor is only allowed in server mode");
      }

      RemoteEndPoint = (IPEndPoint)listenerAsyncContext.AcceptSocket.RemoteEndPoint;
    }

    private TcpChannel(ChannelMode mode, Socket socket)
    {
      Mode = mode;
      ChannelSocket = socket;

      ReadAsyncContext.Completed += AsyncContextOnCompleted;
      WriteAsyncContext.Completed += AsyncContextOnCompleted;
    }

    public void ConnectAsync(IPEndPoint endPoint)
    {
      if (Mode != ChannelMode.Client)
      {
        throw new ChannelException("could not call connect in server mode.");
      }

      ReadAsyncContext.RemoteEndPoint = endPoint;

      if (!ChannelSocket.ConnectAsync(ReadAsyncContext))
      {
        EventLoop.Execute(ProcessConnect);
      }
    }

    public void WriteAsync(IBuffer buf)
    {
      EventLoop.Execute(() =>
      {
        Chain.Tail.WriteAsync(buf);
      });
    }
    
    public void FlushAsync()
    {
      EventLoop.Execute(() =>
      {
        Chain.Tail.FlushAsync();
      });
    }

    public bool IsWritable()
    {
      return FlushCache.IsWritable();
    }

    internal void AppendWriteBuffer(IBuffer buf)
    {
      FlushCache.AppendBuffer(buf);
    }
    
    internal void RegisterReceive()
    {
      if (!DoRegisterRead())
      {
        ProcessReceive();
      }
    }
    
    internal void Flush()
    {
      if (!ChannelSocket.Connected)
      {
        return;
      }

      if (CanFlush())
      {
        StartFlush();
      }
    }
    private void AsyncContextOnCompleted(object sender, SocketAsyncEventArgs e)
    {
      switch (e.LastOperation)
      {
        case SocketAsyncOperation.Receive:
          if (EventLoop.InEventLoop)
          {
            ProcessReceive();
          }
          else
          {
            EventLoop.Execute(ProcessReceive);
          }
          break;
        case SocketAsyncOperation.Send:
          if (EventLoop.InEventLoop)
          {
            ProcessWrite();
          }
          else
          {
            EventLoop.Execute(ProcessWrite);
          }
          break;
        case SocketAsyncOperation.Connect:
          if (EventLoop.InEventLoop)
          {
            ProcessConnect();
          }
          else
          {
            EventLoop.Execute(ProcessConnect);
          }
          break;
      }
    }

    private void ProcessConnect()
    {
      Initialize();
      RegisterReceive();
    }

    private bool CanFlush()
    {
      return !_flushing;
    }

    private void StartFlush()
    {
      _flushing = true;
      DoFlush();
    }

    private void FinishFlush()
    {
      FlushCache.ClearAll();
      _flushing = false;
    }

    private void DoFlush()
    {
      var byteBufferList = FlushCache.GetUnFlushedBytes();
      if (byteBufferList.Count <= 0)
      {
        FinishFlush();
        return;
      }

      WriteAsyncContext.BufferList = byteBufferList;

      if (!ChannelSocket.SendAsync(WriteAsyncContext))
      {
        ProcessWrite();
      }
    }

    private void ProcessWrite()
    {
      if (SocketError.Success != WriteAsyncContext.SocketError)
      {
        return;
      }

      FlushCache.Maintain(WriteAsyncContext.BytesTransferred);
      DoFlush();
      
    }

    private bool DoRegisterRead()
    {
      IBuffer receiveBuf = ReceiveBufferCapacity == 0
        ? new PooledHeapBuffer()
        : new PooledHeapBuffer(ReceiveBufferCapacity);

      ReadAsyncContext.SetBuffer(receiveBuf.Array, 0, receiveBuf.Capacity);
      ReadAsyncContext.UserToken = receiveBuf;
      return ChannelSocket.ReceiveAsync(ReadAsyncContext);
    }

    private void ProcessReceive()
    {
      while (true)
      {
        if (ReadAsyncContext.BytesTransferred <= 0 || ReadAsyncContext.SocketError != SocketError.Success)
        {
          FireChainDisconnecting();
          return;
        }
        
        FireChainReceive();

        if (Status != TcpChannelStatus.Open)
        {
          return;
        }
        
        if (!DoRegisterRead())
        {
          continue;
        }

        break;
      }
    }

    private void FireChainDisconnecting()
    {
      Chain.FireChannelDisconnecting();
    }
    
    private void FireChainReceive()
    {
      IBuffer receiveBuf = (IBuffer) ReadAsyncContext.UserToken;
      receiveBuf.WriterIndex += ReadAsyncContext.BytesTransferred;

      Chain.FireChannelReceived(receiveBuf);
    }
  }
}
