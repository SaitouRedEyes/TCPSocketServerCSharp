using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace ServerSample
{
    public partial class Server : Form
    {
        delegate void SetTextCallback(string text);

        private Thread thread;
        private Socket connection;
        private NetworkStream socketStream;
        private BinaryWriter write;
        private BinaryReader read;
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
                while (true)
                {
                    this.SetText("Aguardando conexões...");
                    connection = listening.AcceptSocket();

                    connectionsNumber++;

                    socketStream = new NetworkStream(connection);

                    write = new BinaryWriter(socketStream);
                    read = new BinaryReader(socketStream);

                    this.SetText(connectionsNumber + " Conexões Recebidas!");
                    write.Write("Conexão Efetuada!");

                    send.ReadOnly = false;

                    string resp = "";

                    do
                    {
                        try
                        {
                            resp = read.ReadString();
                            this.SetText(resp);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    } while (connection.Connected);

                    this.SetText("Conexão Finalizada! \r\n");

                    CloseConnection();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

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

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (connection != null)
            {
                CloseConnection();
            }

            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void send_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && connection != null)
                {
                    write.Write(send.Text);

                    if (send.Text.Equals("FIM")) CloseConnection();

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
            write.Close();
            read.Close();
            socketStream.Close();
            connection.Close();
        }
    }
}