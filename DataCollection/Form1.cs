using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DataCollection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitBuffer();
        }
        public void InitBuffer()
        {
            COMs = new System.IO.Ports.SerialPort[32];
            COMs[0] = COM1; COMs[1] = COM2; COMs[2] = COM3; COMs[3] = COM4;
            COMs[4] = COM5; COMs[5] = COM6; COMs[6] = COM7; COMs[7] = COM8;
            COMs[8] = COM9; COMs[9] = COM10; COMs[10] = COM11; COMs[11] = COM12;
            COMs[12] = COM13; COMs[13] = COM14; COMs[14] = COM15; COMs[15] = COM16;
            COMs[16] = COM7; COMs[17] = COM18; COMs[18] = COM19; COMs[19] = COM20;
            COMs[20] = COM21; COMs[21] = COM22; COMs[22] = COM23; COMs[23] = COM24;
            COMs[24] = COM25; COMs[25] = COM26; COMs[26] = COM27; COMs[27] = COM28;
            COMs[28] = COM29; COMs[29] = COM30; COMs[30] = COM31; COMs[31] = COM32;

            buffers = new Buffer(COMs[0], "COM1", "logc1");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            buffers.run();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buffers.stop();
        }
        System.IO.Ports.SerialPort[] COMs;
        Buffer buffers;
    }
}
