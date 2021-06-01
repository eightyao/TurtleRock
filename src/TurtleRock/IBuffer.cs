using System;
using System.Text;

namespace TurtleRock
{
  public interface IBuffer : IDisposable
  {
    IBuffer WriteByte(byte writeByte);
    IBuffer WriteBytes(byte[] writeBuf, int srcIndex, int length);
    IBuffer WriteString(char[] value, int stringIndex, int count, Encoding encoding);
    IBuffer WriteString(string value, Encoding encoding);
    IBuffer WriteInt32Le(int value);
    IBuffer WriteInt32Be(int value);
    IBuffer WriteInt16Le(short value);
    IBuffer WriteInt16Be(short value);
    IBuffer SetBytes(int dstIndex, byte[] writeBuf, int srcIndex, int length);
    IBuffer SetByte(int dstIndex, byte writeByte);
    IBuffer SetString(int bufIndex, char[] value, int stringIndex, int count, Encoding encoding);
    IBuffer SetString(int bufIndex, string value, Encoding encoding);
    IBuffer SetInt32Le(int bufIndex, int value);
    IBuffer SetInt32Be(int bufIndex, int value);
    IBuffer SetInt16Le(int bufIndex, short value);
    IBuffer SetInt16Be(int bufIndex, short value);
    IBuffer ReadBytes(int length);
    string ReadString(int length, Encoding encoding);
    int ReadInt32Le();
    int ReadInt32Be();
    short ReadInt16Le();
    short ReadInt16Be();
    byte GetByte(int dstIndex);
    byte ReadByte();
    IBuffer GetBytes(int index, byte[] dst, int dstIndex, int length);
    string GetString(int index, int length, Encoding encoding);
    string GetString(int length, Encoding encoding);
    int GetInt32Le();
    int GetInt32Be();
    short GetInt16Le();
    short GetInt16Be();
    void Slim();
    int ReaderIndex { get; set; }
    int WriterIndex { get; set; }
    int ReadableBytes { get; }
    int Length { get; }
    byte[] Array { get; }
    int Capacity { get; }
    int Retain();
    int Release();
  }
}
