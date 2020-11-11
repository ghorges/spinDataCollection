# spinDataCollection
同时收集多路（最多支持 32 路）陀螺仪的数据并进行分析及处理。

日志部分使用双缓冲区，buffer 部分使用环形 buffer，支持无锁编程。
