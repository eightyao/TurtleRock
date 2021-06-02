# TurtleRock
a netty-like NIO TCP/IP communication framework for .net core, across platform supporting both IOCP and Epoll

非常喜欢Netty, 手痒自己在.net core上自己写一个，外部接口引用了Netty的设计哲学，底层是不一样的实现

## Benchmark

#### Ping pong benchmark environment
  * hardware: i7-4770k 16G RAM
  * tool: Beetlex
  * live connection: 1000
  * single message size: around 700 bytes

#### Ping Pong benchmark result:
  * AVG. requests per second(Read): 123795
  * AVG. requests per second(write): 123795
  * AVG. IO per second (read): 91MB 
  * AVG. IO per second (write): 91MB 
  
  ![image](https://github.com/eightyao/TurtleRock/blob/main/benchmark/turtlerockbenchmark.png)



## Samples

#### To create a simple server there below:
* HeadLengthDecoder is decoding the stream via the length in header
* headLengthAppender is appending length header to message before flush
* IdleHandler is commonly idle event which commonly used by heartbeat

```c#
    static TcpServer StartServer()
    {
      var server = new TcpServer(Environment.ProcessorCount * 2);
      server.Option(ChannelOption.NoDelay, true)
        .Option(ChannelOption.HighWriteWaterMark, 128 * 1024)
        .Option(ChannelOption.LowWriteWaterMark, 64 * 1024)
        .ServerOption(ChannelOption.ReuseAddress, true)
        .ServerOption(ChannelOption.Backlog, 8192)
        .StreamChainInitializer(newChannel =>
        {
          newChannel.Chain.Append("HeadLengthDecoder", new HeadLengthDecoder(4, 4092 * 40));
          newChannel.Chain.Append("headLengthAppender", new HeadLengthAppender());
          newChannel.Chain.Append("idle", new IdleHandler(3000, 0));
          newChannel.Chain.Append("echo", new EchoHandler());
        })
        .Start(new IPEndPoint(IPAddress.Any, 8888));

      return server;
    }
```

#### To create tcp client there below:

```c#
      var client = new TcpClient(1);
      client.Option(ChannelOption.NoDelay, true)
        .StreamChainInitializer(newChannel =>
        {
          newChannel.Chain.Append("HeadLengthDecoder", new HeadLengthDecoder(4, 4092 * 40));
          newChannel.Chain.Append("headLengthAppender", new HeadLengthAppender());
          newChannel.Chain.Append("echo", new ClientEchoHandler());
        });

      var channel = _client.ConnectAsync("127.0.0.1", 8888);
      
      IBuffer buf = new PooledHeapBuffer();
      buf.WriteString("hello, turtlerock", Encoding.UTF8);
      
      channel.WriteAsync(buf);
      channel.FlushAsync();
```
