# TurtleRock
a netty-like NIO TCP/IP communication framework for .net core, across platform supporting both IOCP and Epoll

非常喜欢Netty, 手痒自己在.net core上自己写一个，外部接口引用了Netty的设计哲学，底层是不一样的实现

Ping pong benchmark environment
  * hardware: i7-4770k 16G RAM
  * tool: Beetlex
  * live connection: 1000
  * single message size: around 700 bytes

Ping Pong benchmark result:
  * AVG. requests per second(Read): 123795
  * AVG. requests per second(write): 123795
  * AVG. IO per second (read): 91MB 
  * AVG. IO per second (write): 91MB 

  
  ![image](https://github.com/eightyao/TurtleRock/blob/main/benchmark/turtlerockbenchmark.png)
