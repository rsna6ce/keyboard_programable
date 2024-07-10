using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

namespace keyboard_programable
{
    public partial class Form1 : Form
    {

        private UdpClient udpClient = null;
        private const int port = 59630;
        private string[] key_letters_cache = { "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_" };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
            udpClient = new UdpClient(localEP);
            udpClient.BeginReceive(ReceiveCallback, udpClient);
        }

        private void SetMessage(string msg)
        {
            labelMessage.Text = msg;
        }

        private void PerseEvent(string msg)
        {
            string[] key_letters = { "U", "D", "L", "R", "A", "B", "C", "D", "L", "l", "R", "r", "E", "T", "1", "2" };
            for (int i = 0; i<Math.Min(key_letters.Length,msg.Length); i++)
            {
                string key_letter = msg.Substring(i, 1);
                if (key_letter != key_letters_cache[i])
                {
                    // press or release
                    key_letters_cache[i] = key_letter;
                    if (key_letters[i] == key_letter)
                    {
                        // press event

                    }
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient udp = (UdpClient)ar.AsyncState;

            System.Net.IPEndPoint remoteEP = null;
            byte[] rcvBytes;
            try
            {
                rcvBytes = udp.EndReceive(ar, ref remoteEP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine("ERROR: udp receive({0}/{1})", ex.Message, ex.ErrorCode);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("ERROR: udp socket closed.");
                return;
            }

            string rcvMsg = System.Text.Encoding.UTF8.GetString(rcvBytes);
            this.Invoke(new Action<string>(this.SetMessage), rcvMsg);

            if (rcvMsg.EndsWith("H"))
            {
                string sendMsg = "H";
                byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendMsg);
                udp.Send(sendBytes, sendBytes.Length, remoteEP.Address.ToString(), remoteEP.Port);
            }
            else
            {
                PerseEvent(rcvMsg);
            }
            udp.BeginReceive(ReceiveCallback, udp);
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (udpClient != null)
            {
                udpClient.Close();
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
