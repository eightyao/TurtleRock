using System;
using System.Text;
using NUnit.Framework;

namespace TurtleRock.Test.Buffer
{
  [TestFixture]
  public class PooledHeapBufferTest
  {
    [Test]
    public void ReadAndWriteStringTest()
    {
      string str1 = "Hello";
      string str2 = "world";
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteString(str1, Encoding.UTF8);
      buf.WriteString(str2, Encoding.UTF8);

      string result = buf.ReadString(str1.Length + str2.Length, Encoding.UTF8);
      Assert.IsTrue(result.Equals(str1 + str2));
    }
    
    [Test]
    public void ReadAndWriteBytes()
    {
      string hello = "hello";
      byte[] world = Encoding.UTF8.GetBytes(hello);
      
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteBytes(world, 0, world.Length);

      IBuffer resultBytes = buf.ReadBytes(buf.ReadableBytes);
      string resultStr = resultBytes.ReadString(hello.Length, Encoding.UTF8);
      Assert.IsTrue(resultStr.Equals(hello));
    }

    [Test]
    public void ReadAndWriteInt32Be()
    {
      int number = 1024;
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteInt32Be(number);
      int result = buf.ReadInt32Be();
      Assert.AreEqual(number, result);
    }

    [Test]
    public void ReadAndWriteInt32Le()
    {
      int number = 1024;
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteInt32Le(number);
      int result = buf.ReadInt32Le();
      Assert.AreEqual(number, result);
    }
    
    [Test]
    public void ReadAndWriteInt16Be()
    {
      short number = 1024;
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteInt16Be(number);
      int result = buf.ReadInt16Be();
      Assert.AreEqual(number, result);
    }
    
    [Test]
    public void ReadAndWriteInt16Le()
    {
      short number = 1024;
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteInt16Le(number);
      int result = buf.ReadInt16Le();
      Assert.AreEqual(number, result);
    }

    [Test]
    public void RetainAndReleaseTest()
    {
      string str = "123";
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteString("123", Encoding.UTF8);
      Assert.IsTrue(str.Equals(buf.GetString(str.Length, Encoding.UTF8)));
      
      buf.Retain();
      buf.Release();
      
      Assert.IsTrue(str.Equals(buf.GetString(str.Length, Encoding.UTF8)));

      buf.Release();

      try
      {
        string result = buf.GetString(str.Length, Encoding.UTF8);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOf<ObjectDisposedException>(e);
        return;
      }
      
      throw new Exception("buf should not available now");
    }

    [Test]
    public void SlimAndGrowTest()
    {
      string str = "hello";
      IBuffer buf = new PooledHeapBuffer();
      for (int i = 0; i < 10000; i++)
      {
        buf.WriteString(str, Encoding.UTF8);
      }

      string result = buf.ReadString(str.Length * 6000, Encoding.UTF8);
      string expected = string.Empty;
      for (int i = 0; i < 6000; i++)
      {
        expected += str;
      }
      
      Assert.IsTrue(expected.Equals(result));

      int beforeCapacity = buf.Capacity;
      buf.Slim();
      int afterCapacity = buf.Capacity;
      
      Assert.IsTrue(beforeCapacity > afterCapacity);
    }
  }
}