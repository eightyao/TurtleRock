using System.Text;
using NUnit.Framework;
using TurtleRock.Handler;

namespace TurtleRock.Test.Handler
{
  public class HeadLengthDecoderTest
  {
    [Test]
    public void DecoderTest()
    {
      string hello = "hello world";
      string sam = "sam";
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteInt32Be(hello.Length);
      buf.WriteString(hello, Encoding.UTF8);
      buf.WriteInt32Be(sam.Length);
      buf.WriteString(sam, Encoding.UTF8);
      
      HeadLengthDecoder decoder = new HeadLengthDecoder();
      MockChannelContext context = new MockChannelContext();
      decoder.OnChannelReceived(context, buf);
      Assert.IsTrue(context.NextBuffers.Count == 2);

      var first = context.NextBuffers[0].ReadString(hello.Length, Encoding.UTF8);
      var second = context.NextBuffers[1].ReadString(sam.Length, Encoding.UTF8);
      
      Assert.AreEqual(hello, first);
      Assert.AreEqual(sam, second);
    }
  }
}