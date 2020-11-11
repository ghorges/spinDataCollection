using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.OleDb;
using System.Threading;
using System.IO.Ports;
namespace DataCollection
{
    // 还是用环形数组吧
    class Buffer
    {
        public void run()
        {
            stopFlag = false;
            bufLog.changeStart();
        }
        public void stop()
        {
            if (stopFlag != true)
            {
                stopFlag = true;
                bufLog.changeStop();
            }
        }
        public Buffer(System.IO.Ports.SerialPort com1, string strCom, string strLog)
        {
            right = 0;
            left = 0;
            // 初始化 com
            this.com = com1;
            com.ReadBufferSize = 16384;
            com.BaudRate = 115200;
            com.StopBits = StopBits.One;
            com.DataBits = 8;
            com.Parity = Parity.Even;
            this.strCom = strCom;
            com.PortName = strCom;

            this.strLog = strLog;
            bufLog = new log(strLog);

            com.Open();
            buffer = new byte[MAXSIZE];
            // 此处得开启两个线程
            Thread rec = new Thread(new ThreadStart(RecieveComData));
            rec.Start();
            Thread any = new Thread(new ThreadStart(anyComData));
            any.Start();
        }
        private void anyComData()
        {
            byte[] swapBuffer = new byte[3 * 1024 * 1024];
            while (true)
            {
                // 因为 right 可能是变化的
                int r = right;
                Thread.Sleep(1000);
                if (left != r)
                {
                    if (r > left)
                    {
                        for (int i = left; i < r; i++)
                        {
                            swapBuffer[i - left] = buffer[i];
                        }
                        bufLog.set(swapBuffer, r - left);
                        left = r;
                    }
                    else
                    {
                        // 从头开始
                        for (int i = left; i < maxSize; i++)
                        {
                            swapBuffer[i - left] = buffer[i];
                        }
                        bufLog.set(swapBuffer, maxSize - left);
                        left = 0;
                    }
                }
            }
        }
        // 开启一个线程去收
        private void RecieveComData()
        {
            byte[] swapBuffer = new byte[1024 * 1024];
            while (true)
            {
                if (stopFlag)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (right < left && left - right < 9200)
                {
                    Console.WriteLine("error right will find left");
                    continue;
                }
                Thread.Sleep(25);
                int size = com.Read(swapBuffer, 0, com.BytesToRead);
                Console.WriteLine(size.ToString());
                if (size > 0)
                {
                    for (int i = 0; i < size; i++)
                        Console.WriteLine(swapBuffer[i]);
                    if (size + right < maxSize)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            buffer[right + i] = swapBuffer[i];
                        }
                        right += size;
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                        {
                            if (right >= maxSize)
                            {
                                right %= maxSize;
                            }
                            buffer[right++] = swapBuffer[i];
                        }
                    }
                }
            }
        }
        bool stopFlag = true;           // 运行的标志
        private System.IO.Ports.SerialPort com;
        private string strCom;
        static int MAXSIZE = 30 * 1024 * 1024;
        private int right;
        private int left;
        private int maxSize = 1024 * 1024;
        private string strLog;
        private log bufLog;
        private byte[] buffer;
    }
}
