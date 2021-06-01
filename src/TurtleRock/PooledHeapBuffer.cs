using System;
using System.Buffers;
using System.Text;
using System.Threading;
using TurtleRock.Common;
using TurtleRock.Exceptions;

namespace TurtleRock
{
  public class PooledHeapBuffer : IBuffer
  {
    public const int DEFAULT_BUFFER_BLOCK_SIZE = 1024;
    
    private IMemoryOwner<byte> _lastMemoryOwner;
    private int _refCnt = 1;

    private byte[] InternalBuffer => _lastMemoryOwner.Memory.GetArray().Array;
    public int ReaderIndex { get; set; } = 0;
    public int WriterIndex { get; set; } = 0;

    public int ReadableBytes => WriterIndex - ReaderIndex;
    public int Length => WriterIndex;
    public byte[] Array => InternalBuffer;
    public int Capacity { get; private set; }

    public PooledHeapBuffer():this(DEFAULT_BUFFER_BLOCK_SIZE){}
    public PooledHeapBuffer(int capacity)
    {
      Capacity = capacity;
      _lastMemoryOwner = MemoryPool<byte>.Shared.Rent(capacity);
    }

    public IBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
    {
      Buffer.BlockCopy(InternalBuffer, index, dst, dstIndex, length);
      return this;
    }

    public byte GetByte(int dstIndex)
    {
      return InternalBuffer[dstIndex];
    }

    public IBuffer ReadBytes(int length)
    {
      var newBuf = new PooledHeapBuffer();
      newBuf.WriteBytes(Array, ReaderIndex, length);
      ReaderIndex += length;
      return newBuf;
    }

    public byte ReadByte()
    {
      var b = GetByte(ReaderIndex);
      ReaderIndex += 1;
      return b;
    }

    public int ReadInt32Le()
    {
      var value = GetInt32Le();
      ReaderIndex += 4;
      return value;
    }

    public int ReadInt32Be()
    {
      var value = GetInt32Be();
      ReaderIndex += 4;
      return value;
    }

    public short ReadInt16Le()
    {
      var value = GetInt16Le();
      ReaderIndex += 2;
      return value;
    }

    public short ReadInt16Be()
    {
      var value = GetInt16Be();
      ReaderIndex += 2;
      return value;
    }

    public int GetInt32Le()
    {
      return unchecked(
        Array[ReaderIndex] |
        Array[ReaderIndex + 1] << 8 |
        Array[ReaderIndex + 2] << 16 |
        Array[ReaderIndex + 3] << 24);
    }

    public int GetInt32Be()
    {
      return unchecked(
        Array[ReaderIndex] << 24 |
        Array[ReaderIndex + 1] << 16 |
        Array[ReaderIndex + 2] << 8 |
        Array[ReaderIndex + 3]);
    }

    public short GetInt16Le()
    {
      return unchecked((short)(Array[ReaderIndex] | Array[ReaderIndex + 1] << 8));
    }

    public short GetInt16Be()
    {
      return unchecked((short)(Array[ReaderIndex] << 8 | Array[ReaderIndex + 1]));
    }

    public string ReadString(int length, Encoding encoding)
    {
      var str = GetString(length, encoding);
      ReaderIndex += length;
      return str;
    }

    public string GetString(int index, int length, Encoding encoding)
    {
      if (length == 0)
      {
        return string.Empty;
      }

      return encoding.GetString(InternalBuffer, index, length);
    }

    public string GetString(int length, Encoding encoding)
    {
      return GetString(ReaderIndex, length, encoding);
    }

    public IBuffer WriteBytes(byte[] writeBuf, int srcIndex, int length)
    {
      CheckGrow(length);

      SetBytes(WriterIndex, writeBuf, srcIndex, length);
      WriterIndex += length;
      return this;
    }

    public IBuffer WriteByte(byte writeByte)
    {
      CheckGrow(1);

      SetByte(WriterIndex, writeByte);
      WriterIndex += 1;
      return this;
    }

    public IBuffer WriteString(char[] value, int stringIndex, int count, Encoding encoding)
    {
      CheckGrow(count);
      
      SetString(WriterIndex, value, stringIndex, count, encoding);
      WriterIndex += count;
      return this;
    }

    public IBuffer WriteString(string value, Encoding encoding)
    {
      CheckGrow(value.Length);
      
      SetString(WriterIndex, value, encoding);
      WriterIndex += value.Length;
      return this;
    }

    public IBuffer WriteInt32Le(int value)
    {
      SetInt32Le(WriterIndex, value);
      WriterIndex += 4;
      return this;
    }

    public IBuffer WriteInt32Be(int value)
    {
      SetInt32Be(WriterIndex, value);
      WriterIndex += 4;
      return this;
    }

    public IBuffer WriteInt16Le(short value)
    {
      SetInt16Le(WriterIndex, value);
      WriterIndex += 2;
      return this;
    }

    public IBuffer WriteInt16Be(short value)
    {
      SetInt16Be(WriterIndex, value);
      WriterIndex += 2;
      return this;
    }

    public IBuffer SetBytes(int dstIndex, byte[] writeBuf, int srcIndex, int length)
    {
      Buffer.BlockCopy(writeBuf, srcIndex, InternalBuffer, dstIndex, length);
      return this;
    }

    public IBuffer SetByte(int dstIndex, byte writeByte)
    {
      InternalBuffer[dstIndex] = writeByte;
      return this;
    }

    public IBuffer SetString(int bufIndex, char[] value, int stringIndex, int count, Encoding encoding)
    {
      byte[] strBytes = encoding.GetBytes(value, stringIndex, count);
      return SetBytes(WriterIndex, strBytes, bufIndex, count);
    }

    public IBuffer SetString(int bufIndex, string value, Encoding encoding)
    {
      var bytes = encoding.GetBytes(value);
      return SetBytes(WriterIndex, bytes, 0, value.Length);
    }

    public IBuffer SetInt32Le(int bufIndex, int value)
    {
      unchecked
      {
        Array[bufIndex] = (byte)value;
        Array[bufIndex + 1] = (byte)(value >> 8);
        Array[bufIndex + 2] = (byte)(value >> 16);
        Array[bufIndex + 3] = (byte)(value >> 24);
      }

      return this;
    }

    public IBuffer SetInt32Be(int bufIndex, int value)
    {
      unchecked
      {
        Array[bufIndex] = (byte)(value >> 24);
        Array[bufIndex + 1] = (byte)(value >> 16);
        Array[bufIndex + 2] = (byte)(value >> 8);
        Array[bufIndex + 3] = (byte)value;
      }

      return this;
    }

    public IBuffer SetInt16Le(int bufIndex, short value)
    {
      unchecked
      {
        Array[bufIndex] = (byte)value;
        Array[bufIndex + 1] = (byte)(value >> 8);
      }

      return this;
    }

    public IBuffer SetInt16Be(int bufIndex, short value)
    {
      unchecked
      {
        Array[bufIndex] = (byte)(value >> 8);
        Array[bufIndex + 1] = (byte)value;
      }

      return this;
    }

    private void CheckGrow(int addLength)
    {
      while (Capacity - WriterIndex < addLength)
      {
        Grow();
      }
    }
    private void Grow()
    {
      var newMemory = MemoryPool<byte>.Shared.Rent(Capacity * 2);
      var newBuf = newMemory.Memory.GetArray().Array;
      if (newBuf == null)
      {
        throw new BufferException("allocate bytes from memory pool failed.");
      }

      Buffer.BlockCopy(InternalBuffer,0,newBuf,0, Capacity);

      _lastMemoryOwner.Dispose();
      _lastMemoryOwner = newMemory;
      Capacity *= 2;
    }

    public void Slim()
    {
      if (ReaderIndex <= DEFAULT_BUFFER_BLOCK_SIZE)
      {
        return;
      }
      
      var unreadDataLength = ReadableBytes;
      var newMemorySize = (ReadableBytes / DEFAULT_BUFFER_BLOCK_SIZE + 1) * DEFAULT_BUFFER_BLOCK_SIZE;
      var newMemory = MemoryPool<byte>.Shared.Rent(newMemorySize);
      var newBuf = newMemory.Memory.GetArray().Array;

      if (newBuf == null)
      {
        throw new BufferException("allocate bytes from memory pool failed.");
      }

      Buffer.BlockCopy(InternalBuffer, ReaderIndex, newBuf, 0, unreadDataLength);

      _lastMemoryOwner.Dispose();
      _lastMemoryOwner = newMemory;

      ReaderIndex = 0;
      WriterIndex = unreadDataLength;
      Capacity = newMemorySize;
    }

    public int Retain()
    {
      return Interlocked.Increment(ref _refCnt);
    }

    public int Release()
    {
      Interlocked.Decrement(ref _refCnt);
      if (_refCnt <= 0)
      {
        Dispose();
      }

      return _refCnt;
    }

    public void Dispose()
    {
      _lastMemoryOwner?.Dispose();
    }
  }
}
