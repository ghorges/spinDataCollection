using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// 不能用 char，只能用 byte，因为 c# 的 char 可能不是一个字节
// C#中和Java一样是一种系统自动回收释放资源的语言，所以不用考虑 c# 的不回收问题
// 有一种特殊情况没处理到，如果 log1 写文件还没写完，但是 log2 内存写满了，应该怎么处理？
// （应该使用原子操作，必须等待上一个写完，才可以写下一个。）
namespace DataCollection
{
    class log
    {
        public log(string str)
        {
            FileName = "\\";
            FileName += str;
            FileName += ".txt";
            log1 = new byte[MAXSIZE];
            log2 = new byte[MAXSIZE];
        }
        public void changeStart()
        {
            stop = false;
        }
        public void changeStop()
        {
            if (stop != true)
            {
                stop = true;
                // 这里也需要加这个
                while (true)
                {
                    if (mutexCount == 0)
                    {
                        break;
                    }
                    Thread.Sleep(50);
                }
                int num = flag ? 1 : 2;
                thread(num);
                index1 = 0;
                index2 = 0;
                flag = true;
            }
        }
        // 如果新来的日志长度大于最大长度，最大长度扩容
        private void append(int size)
        {
            while (MAXSIZE < size)
            {
                MAXSIZE *= 2;
            }
            // 若是 log1 的回合，log2 不用拷贝
            if (flag)
            {
                log2 = log1;
                log1 = new byte[MAXSIZE];
                for (int i = 0; i < index1; i++)
                {
                    log1[i] = log2[i];
                }
                log2 = new byte[MAXSIZE];
            }
            else
            {
                log1 = log2;
                log2 = new byte[MAXSIZE];
                for (int i = 0; i < index2; i++)
                {
                    log2[i] = log1[i];
                }
                log1 = new byte[MAXSIZE];
            }
        }

        // 添加，只能单线程执行
        // 多线程执行会出问题
        public int set(byte[] data, int size)
        {
            // 判断是否运行
            if (stop)
            {
                return -1;
            }
            if (data.Length < size)
            {
                return -1;
            }
            if (MAXSIZE < size)
            {
                append(size);
            }
            if (flag == true)
            {
                if (index1 + size < MAXSIZE)
                {
                    for (int i = 0; i < size; i++)
                    {
                        log1[index1 + i] = data[i];
                    }
                    index1 += size;
                }
                else
                {
                    // 写入日志
                    /*
                    Thread t = new Thread(new ThreadStart(thread));
                    t.Start();
                     */
                    // thread();
                    index2 = 0;
                    flag = false;
                    Thread t = new Thread(new ParameterizedThreadStart(thread));
                    object obj = 1;
                    while (true)
                    {
                        if (mutexCount == 0)
                        {
                            break;
                        }
                        Thread.Sleep(50);
                    }
                    t.Start(obj);
                    for (int i = 0; i < size; i++)
                    {
                        log2[i] = data[i];
                    }
                    index2 = size;
                }
            }
            else
            {
                if (index2 + size < MAXSIZE)
                {
                    for (int i = 0; i < size; i++)
                    {
                        log2[index2 + i] = data[i];
                    }
                    index2 += size;
                }
                else
                {
                    index1 = 0;
                    flag = true;
                    Thread t = new Thread(new ParameterizedThreadStart(thread));
                    object obj = 2;
                    while (true)
                    {
                        if (mutexCount == 0)
                        {
                            break;
                        }
                        Thread.Sleep(50);
                    }
                    t.Start(obj);
                    for (int i = 0; i < size; i++)
                    {
                        log1[i] = data[i];
                    }
                    index1 = size;
                }
            }
            return 0;
        }

        // 通过加锁的方式保证是单个线程在用的
        // 因为若不是单线程 log1 log2 同时写文件
        // 此时新来的数据就会有问题
        private void thread(object number)
        {
            int i = (int)number;
            Interlocked.Add(ref mutexCount, 1);
            if (i == 2)
            {
                writeFile(log2, index2);
            }
            else if (i == 1)
            {
                writeFile(log1, index1);
            }
            Interlocked.Add(ref mutexCount, -1);
            return;
        }

        private void writeFile(byte[] data, int len)
        {
            string path = Directory.GetCurrentDirectory() + "\\LoginfoMsg";
            string filename = path + FileName;
            // string cont = "";
            // FileInfo fileInf = new FileInfo(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Console.WriteLine(filename);
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            fs.Seek(0, SeekOrigin.End);
            string str1 = "";
            for (int i = 0; i < len; i++)
            {
                str1 += data[i].ToString();
                // suiyi
                str1 += " ";
            }
            StreamWriter r = new StreamWriter(fs);
            r.Write(str1);
            r.Close();
            //fs.Write(data, 0, len);
            fs.Close();
            /*
            if (File.Exists(filename))//如何文件存在 则在文件后面累加
            {
                FileStream myFss = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamReader r = new StreamReader(myFss);
                cont = r.ReadToEnd();
                r.Close();
                myFss.Close();
            }
             */
            /*
            FileStream myFs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter n = new StreamWriter(myFs);
            n.WriteLine(cont);
            n.Write();
            n.Close();
            myFs.Close();
            */
            /* 放到一个文件就行
            if (fileInf.Length >= 1024 * 1024 * 200)
            {
                string NewName = path + "MsgLog" + DateTime.Now.ToShortDateString() + ".txt";
                File.Move(filename, NewName);
            }
             */
        }
        bool stop = true;           // 运行的标志
        private int mutexCount = 0;     // 对这个变量必须用原子操作
        // 大概 60-70s 写一次
        int MAXSIZE = 1024 * 1024;
        private byte[] log1;
        private byte[] log2;
        private int index1 = 0;
        private int index2 = 0;
        private bool flag = true;
        private string FileName = "";
    }
}
