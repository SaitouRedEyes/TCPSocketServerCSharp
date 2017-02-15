using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Globalization;

namespace ServerSample
{
    public partial class Server : Form
    {
        delegate void SetTextCallback(string text);

        private Thread thread;
        private Socket socket;
        private TcpListener listening;
        private int connectionsNumber;

        public Server()
        {
            InitializeComponent();

            connectionsNumber = 0;
            WaitConnections();
        }

        private void WaitConnections()
        {
            listening = new TcpListener(IPAddress.Any, 9000);
            listening.Start();

            thread = new Thread(new ThreadStart(RunServer));
            thread.Start();
        }

        private void RunServer()
        {
            try
            {
                while(true)
                {
                    this.SetText("Waiting connections...");
                    socket = listening.AcceptSocket();
                    this.SetText("Connection accepted from " + socket.RemoteEndPoint);

                    byte[] b = new byte[100];
                    int k = socket.Receive(b);

                    do
                    {
                        string[] request = ReadToString(k, b);
                        this.SetText("Web server ID = " + request[0]);

                        SendAnswer(request, socket);

                        this.SetText("Closing Connection:");
                        Console.WriteLine("Closing Connection:");

                        CloseConnection();
                    }
                    while (socket.Connected);

                    this.SetText("Connection End");
                    Console.WriteLine("Connection End");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        //Read the client data and convert it to an object that is easier to manipulate.
        private String[] ReadToString(int msg, byte[] b)
        {
            char cc = ' ';
            string text = null;
            
            for (int i = 0; i < msg - 1; i++)
            {
                cc = Convert.ToChar(b[i]);
                text += cc.ToString();
            }

            string[] textSplited = text.Split('&');

            return textSplited;
        }

        //Send to client the answer of a service.
        private void SendAnswer(string[] request, Socket s)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            
            switch ((int)float.Parse(request[0], CultureInfo.InvariantCulture.NumberFormat))
            {
                case 1: s.Send(asen.GetBytes(Sum(request[1], request[2]))); break;

                default: s.Send(asen.GetBytes("Web Service Not Found")); break;
            }

            this.SetText("Send Answer to: " + s.RemoteEndPoint);
            Console.WriteLine("Send Answer to: " + s.RemoteEndPoint);
        }

        //Sum of the 2 values.
        private string Sum(string request1, string request2)
        {
            float n1 = float.Parse(request1, CultureInfo.InvariantCulture.NumberFormat);
            float n2 = float.Parse(request2, CultureInfo.InvariantCulture.NumberFormat);

            return (n1 + n2).ToString();
        }

        //Set text on the windows form service interface.
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.show.InvokeRequired)
            {
                //Chamando o componente de interface ajustando o problema das thread diferentes  
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                //Quando estão ns mesma thread, o acesso é permitido.
                this.show.Text += text + "\r\n";
            }
        }

        //if the windows form service interface close, close the connection with the client.
        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (socket != null)
            {
                connectionsNumber--;
                CloseConnection();
            }

            System.Environment.Exit(System.Environment.ExitCode);
        }

        //Write "FIM" on the send TextField to close connection with the client.
        private void send_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && socket != null)
                {
                    ASCIIEncoding asen = new ASCIIEncoding();

                    socket.Send(asen.GetBytes(send.Text));

                    if (send.Text.Equals("FIM"))
                    {
                        connectionsNumber--;
                        CloseConnection();
                    }

                    send.Clear();
                }
            }
            catch (SocketException)
            {
                this.SetText("Atenção! Erro...");
            }
        }

        private void CloseConnection()
        {
            socket.Close();
        }
    }
}