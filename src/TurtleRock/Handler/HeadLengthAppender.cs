using TurtleRock.Exceptions;

namespace TurtleRock.Handler
{
  public class HeadLengthAppender : AbstractChainHandler
  {
    private readonly int _headSize;
    private readonly bool _isBigEndian;
    public HeadLengthAppender(int headSize = 4, bool isBigEndian = true)
    {
      _headSize = headSize;
      _isBigEndian = isBigEndian;
    }
    public override void Write(IChannelContext ctx, IBuffer buf)
    {
      var headBuf = new PooledHeapBuffer(_headSize);
      switch (_headSize)
      {
        case 2 when buf.WriterIndex > short.MaxValue:
          throw new ChainHandlerException("content length exceeds short.MaxValue");
        case 2 when _isBigEndian:
          headBuf.WriteInt16Be((short) buf.WriterIndex);
          break;
        case 2:
          headBuf.WriteInt16Le((short) buf.WriterIndex);
          break;
        case 4 when _isBigEndian:
          headBuf.WriteInt32Be((short) buf.WriterIndex);
          break;
        case 4:
          headBuf.WriteInt32Le((short) buf.WriterIndex);
          break;
      }

      ctx.FireWrite(headBuf);
      ctx.FireWrite(buf);
      ctx.FlushAsync();
    }
  }
}