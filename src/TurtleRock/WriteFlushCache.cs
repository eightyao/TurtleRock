using System;
using System.Collections.Generic;

namespace TurtleRock
{
  internal class FlushNode
  {
    public IBuffer Buf { get; }

    public FlushNode(IBuffer buf)
    {
      Buf = buf;
    }
  }
  public class WriteFlushCache
  {
    private readonly LinkedList<FlushNode> _bufferList = new LinkedList<FlushNode>();
    private LinkedListNode<FlushNode> _unFlushedPointer;
    private volatile int _isWritable = 1;
    private long _totalPendingSize = 0;

    internal int HighWriteWaterMark { get; set; } = 64 * 1024;
    internal int LowWriteWaterMark { get; set; } = 8 * 1024;
    
    internal WriteFlushCache()
    {
      _unFlushedPointer = null;
    }

    internal bool IsWritable()
    {
      return _isWritable > 0;
    }
    
    private void CheckWaterMark()
    {
      if (HighWriteWaterMark >= _totalPendingSize)
      {
        SetWritable(false);
      }

      if (LowWriteWaterMark <= _totalPendingSize)
      {
        SetWritable(true);
      }
    }

    private void SetWritable(bool writable)
    {
      _isWritable = writable ? 1 : 0;
    }

    private void AddUpPendingSize(int bytes)
    {
      _totalPendingSize += bytes;
      CheckWaterMark();
    }

    private void MinusPendingSize(int bytes)
    {
      _totalPendingSize -= bytes;
      CheckWaterMark();
    }
    
    internal void AppendBuffer(IBuffer buf)
    {
      LinkedListNode<FlushNode> node = new LinkedListNode<FlushNode>(new FlushNode(buf));
      _bufferList.AddLast(node);
      AddUpPendingSize(buf.ReadableBytes);
      
      if (_bufferList.Count == 1)
      {
        _unFlushedPointer = node;
      }
    }

    internal List<ArraySegment<byte>> GetUnFlushedBytes()
    {
      List<ArraySegment<byte>> unFlushedBytes = new List<ArraySegment<byte>>();
      var next = _unFlushedPointer;
      if (next == null)
      {
        return unFlushedBytes;
      }
      
      while (true)
      {
        var node = next?.Value;
        if (node == null)
        {
          break;
        }

        var buf = node.Buf;
        if (buf == null)
        {
          continue;
        }
        
        unFlushedBytes.Add(new ArraySegment<byte>(buf.Array, buf.ReaderIndex, buf.ReadableBytes));
        AddUpPendingSize(buf.ReadableBytes);
        next = next.Next;
      }

      return unFlushedBytes;
    }

    internal void Maintain(int writtenBytesCount)
    {
      var next = _unFlushedPointer;
      var writtenBytes = writtenBytesCount;
      while (true)
      {
        var node = next?.Value;
        if (node == null)
        {
          break;
        }
        
        var buf = node.Buf;
        if (buf == null)
        {
          next = next.Next;
          if (next == null)
          {
            break;
          }
          
          continue;
        }

        if (writtenBytes >= buf.ReadableBytes)
        {
          var consumedBytes = buf.ReadableBytes;
          writtenBytes -= consumedBytes;
          MinusPendingSize(consumedBytes);

          next = next.Next;
          if (next == null)
          {
            break;
          }
        }
        else
        {
          buf.ReaderIndex += writtenBytes;
          MinusPendingSize(writtenBytes);
          break;
        }
      }

      _unFlushedPointer = next;
    }

    internal void ClearAll()
    {
      foreach (var flushNode in _bufferList)
      {
        flushNode.Buf.Release();
      }
      _bufferList.Clear();
      _unFlushedPointer = null;
    }
  }
}