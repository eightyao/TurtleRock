using System;
using TurtleRock.Exceptions;
using TurtleRock.Handler;

namespace TurtleRock
{
  public class StreamChain
  {
    private readonly AbstractTcpChannel _channel;

    public ChannelContext Head { get; set; }
    public ChannelContext Tail { get; set; }

    public StreamChain(AbstractTcpChannel channel)
    {
      _channel = channel;

      Head = new ChannelContext("Head", new HeadHandler(), this, _channel);
      Tail = new ChannelContext("Tail", new TailHandler(), this, _channel);
      Head.Next = Tail;
      Tail.Prev = Head;
    }

    public void InsertAfter(string name, IChainHandler handler, string existingHandlerName)
    {
      if (string.IsNullOrEmpty(existingHandlerName))
      {
        throw new ArgumentNullException(nameof(existingHandlerName));
      }
      
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      var target = FindContextByName(existingHandlerName);

      ChannelContext newContext = new ChannelContext(name, handler, this, _channel);

      newContext.Next = target.Next;
      target.Next = newContext;
      newContext.Next.Prev = newContext;
      newContext.Prev = target;

      handler.OnAddToChain(newContext);
    }

    public void Append(string name, IChainHandler handler)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }
      
      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }
      
      var target = Tail.Prev;
      InsertAfter(name, handler, target.HandlerName);
    }

    public void Remove(string name)
    {
      var target = FindContextByName(name);
      var prev = target.Prev;
      var next = target.Next;
      prev.Next = next;
      next.Prev = prev;
      target.Handler.OnRemoveFromChain(target);
    }

    public void FireChannelConnected()
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelConnected();
        return;
      }
      
      _channel.EventLoop.Execute(()=> { Head.InvokeChannelConnected(); });
    }

    public void FireChannelDisconnected()
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelDisconnected();
        return;
      }

      _channel.EventLoop.Execute(() => { Head.InvokeChannelDisconnected(); });
    }
    
    public void FireChannelDisconnecting()
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelDisconnecting();
        return;
      }

      _channel.EventLoop.Execute(() => { Head.InvokeChannelDisconnecting(); });
    }

    public void FireChannelReceived(IBuffer buf)
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelReceived(buf);
        return;
      }
      
      _channel.EventLoop.Execute(() => { Head.InvokeChannelReceived(buf); });
    }

    public void FireChannelEvent(object channelEvent)
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelEvent(channelEvent);
        return;
      }
      
      _channel.EventLoop.Execute(() => { Head.InvokeChannelEvent(channelEvent); });
    }

    public void FireChannelException(Exception e)
    {
      if (_channel.EventLoop.InEventLoop)
      {
        Head.InvokeChannelException(e);
        return;
      }
      
      _channel.EventLoop.Execute(() => { Head.InvokeChannelException(e); });
    }

    public ChannelContext FindContextByName(string name)
    {
      ChannelContext target = Head;
      while (target != null)
      {
        if (target.HandlerName == name)
        {
          return target;
        }

        target = target.Next;
      }

      throw new ChainHandlerException("could not find context");
    }
  }
}
