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
using System.Text.Json;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;


namespace keyboard_programable
{
    public partial class Form1 : Form
    {

        private UdpClient _udpClient = null;
        private const int _port = 59630;
        private string[] _key_letters_cache = { "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_", "_" };
        private string[] _key_letters = { "U", "D", "L", "R", "A", "B", "C", "D", "L", "l", "R", "r", "E", "T", "1", "2" };
        private string[] _key_layouts = { "UP", "DOWN", "LEFT", "RIGHT", "A", "B", "C", "D", "L1", "L2", "R1", "R2", "SELECT", "START", "ONE", "TWO" };
        private string _filename = "";
        private Parameter _param;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string exe_dir = Path.GetDirectoryName(Application.ExecutablePath) + @"\";
            _filename = exe_dir + "parameter.json";

            read_json(_filename);
            //TODO load laytouts name
            comboBoxLayout.SelectedIndex = 0;

            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, _port);
            _udpClient = new UdpClient(localEP);
            _udpClient.BeginReceive(ReceiveCallback, _udpClient);
        }

        private void read_json(string filename)
        {
            using (StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("utf-8")))
            {
                string line = sr.ReadToEnd();
                _param = JsonSerializer.Deserialize<Parameter>(line);
            }
        }
        private void write_json(string filename)
        {
            string json_str = JsonSerializer.Serialize(_param, new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), WriteIndented = true });
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.GetEncoding("utf-8")))
            {
                writer.WriteLine(json_str);
            }
        }


        private void SetMessage(string msg)
        {
            labelMessage.Text = msg;
        }

        private void PerseEvent(string msg)
        {
            for (int i = 0; i<Math.Min(_key_letters.Length,msg.Length); i++)
            {
                string key_letter = msg.Substring(i, 1);
                if (key_letter != _key_letters_cache[i])
                {
                    // press or release
                    _key_letters_cache[i] = key_letter;
                    if (_key_letters[i] == key_letter)
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
                Console.WriteLine("ERROR: udp socket closed.({0})", ex.Message);
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
            if (_udpClient != null)
            {
                _udpClient.Close();
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //TODO: open parameter.json default editor
            //TODO: reload parameter.json
            //TODO: select default layout
        }

        private void comboBoxLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBoxLayout.SelectedIndex;
            // TODO: load layout to listview
        }
    }
    public class key_layout
    {
        public string UP { get; set; }
        public string DOWN { get; set; }
        public string LEFT { get; set; }
        public string RIGHT { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string R1 { get; set; }
        public string R2 { get; set; }
        public string SELECT { get; set; }
        public string START { get; set; }
        public string ONE { get; set; }
        public string TWO { get; set; }
    }
    public class Parameter
    {
        public IList<key_layout> key_layouts { get; set; }
    }

}
