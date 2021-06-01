using System;
using System.Collections.Generic;

namespace TurtleRock
{
  public class ChannelOption
  {
    public static ChannelOption Backlog = new ChannelOption(typeof(int));
    public static ChannelOption NoDelay = new ChannelOption(typeof(bool));
    public static ChannelOption ReuseAddress = new ChannelOption(typeof(bool));
    public static ChannelOption RcvBufSize = new ChannelOption(typeof(int));
    public static ChannelOption SndBufSize = new ChannelOption(typeof(int));
    public static ChannelOption KeepAlive = new ChannelOption(typeof(bool));
    public static ChannelOption Linger = new ChannelOption(typeof(int));
    public static ChannelOption RecevieBufferCapacity = new ChannelOption(typeof(int));
    public static ChannelOption HighWriteWaterMark = new ChannelOption(typeof(int));
    public static ChannelOption LowWriteWaterMark = new ChannelOption(typeof(int));

    public Type ValueType { get; set; }

    public ChannelOption(Type type)
    {
      ValueType = type;
    }

    public void Check(object value)
    {
      if (value.GetType() != ValueType)
      {
        throw new ArgumentException("invalid option value");
      }
    }
  }

  public class ChannelOptionApplier
  {
    private readonly Dictionary<ChannelOption, object> _options 
      = new Dictionary<ChannelOption, object>();

    public void Set(ChannelOption option, object value)
    {
      _options.Add(option, value);
    }

    public void Apply(AbstractTcpChannel channel)
    {
      foreach (var option in _options)
      {
        if (option.Key == ChannelOption.Backlog 
            && channel is TcpServerChannel serverChannel)
        {
          serverChannel.Backlog = (int)option.Value;
        }

        if (option.Key == ChannelOption.SndBufSize)
        {
          channel.SndBufSize = (int)option.Value;
        }

        if (option.Key == ChannelOption.ReuseAddress)
        {
          channel.ReuseAddress = (bool)option.Value;
        }

        if (option.Key == ChannelOption.RcvBufSize)
        {
          channel.RcvBufSize = (int)option.Value;
        }

        if (option.Key == ChannelOption.NoDelay)
        {
          channel.NoDelay = (bool)option.Value;
        }

        if (option.Key == ChannelOption.KeepAlive)
        {
          channel.KeepAlive = (bool)option.Value;
        }

        if (option.Key == ChannelOption.Linger)
        {
          channel.Linger = (int)option.Value;
        }

        if (option.Key == ChannelOption.RecevieBufferCapacity)
        {
          channel.ReceiveBufferCapacity = (int) option.Value;
        }

        if (channel is TcpChannel tcpChannel)
        {
          if (option.Key == ChannelOption.HighWriteWaterMark)
          {
            tcpChannel.HighWriteWaterMark = (int) option.Value;
          }
        
          if (option.Key == ChannelOption.LowWriteWaterMark)
          {
            tcpChannel.LowWriteWaterMark = (int) option.Value;
          }
        }
      }
    }
  }
}
