using System;

namespace TurtleRock.Handler
{
  public class HeadLengthDecoder : AbstractChainHandler
  {
    private readonly int _headLen;
    private readonly bool _isBigEndian;
    private readonly int _maxLength;

    private IBuffer _msgBufStack;
    private int _targetLen;

    public HeadLengthDecoder(int headLen = 4, int maxLength = 4092, bool isBigEndian = true)
    {
      _headLen = headLen;
      _isBigEndian = isBigEndian;
      _maxLength = maxLength;
    }

    public override void OnChannelDisconnected(IChannelContext ctx)
    {
      ReleaseMsgBufStack();
      ctx.FireChannelDisconnected();
    }

    public override void OnChannelReceived(IChannelContext ctx, IBuffer buf)
    {
      if (!ctx.Connected)
      {
        return;
      }

      _msgBufStack ??= new PooledHeapBuffer();
      _msgBufStack.Slim();
      _msgBufStack.WriteBytes(buf.Array, 0, buf.WriterIndex);

      while (true)
      {
        if (_msgBufStack.ReadableBytes < _headLen)
        {
          return;
        }

        if (_targetLen == 0)
        {
          _targetLen = _isBigEndian 
            ? _msgBufStack.ReadInt32Be()
            : _msgBufStack.ReadInt32Le();

          if (_targetLen < 0 || _targetLen > _maxLength)
          {
            ReleaseMsgBufStack();
            _targetLen = 0;
            Console.WriteLine("max length exceeded.");
            ctx.DisconnectAsync();
            return;
          }
        }

        if (_msgBufStack.ReadableBytes < _targetLen)
        {
          return;
        }

        if (_msgBufStack.ReadableBytes >= _targetLen)
        {
          var messageBuf =
            _msgBufStack.ReadBytes(_targetLen);
          
          _targetLen = 0;

          ctx.FireChannelReceived(messageBuf);
        }
      }
    }

    private void ReleaseMsgBufStack()
    {
      _msgBufStack?.Release();
      _msgBufStack = null;
    }
  }
}
