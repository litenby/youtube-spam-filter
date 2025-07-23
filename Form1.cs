using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtube_spam_filter
{
    public partial class Form1 : Form
    {

        private SpamFilter bot;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            bot = new SpamFilter();
            bot.Log = message => Invoke((Action)(() => logTextBox.AppendText(message + Environment.NewLine)));

            bot.Start();

            button3.Enabled = false;
            button5.Enabled = true;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            bot?.Disconnect();
            button3.Enabled = true;
            button5.Enabled = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
