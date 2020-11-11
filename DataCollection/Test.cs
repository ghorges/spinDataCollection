using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCollection
{
    class Test
    {
        Test() 
        {
            // 测试日志
            // Thread rec = new Thread(new ThreadStart(logTest));
            // rec.Start();
        }
        // 测试日志
        public void logTest()
        {
            log1 = new log("log1");
            byte[] test = new byte[1000];
            string str = "123456789";
            for (int i = 0; i < 8; i++)
            {
                test[i] = (byte)str[i];
            }
            int num = 0;
            while (true)
            {
                num += 8;
                log1.set(test, 8);
            }
        }
        log log1;
    }
}
