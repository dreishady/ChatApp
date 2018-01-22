//Author: Andrei Rico
//Purpose of code: This is the the user 1 to connect to the user 2.
//Date: 31/08/2017
//Known bugs: none

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;

namespace ChatServer
{
    public partial class Server : Form
    {
        //pipe server
        NamedPipeServerStream serverPipe = new NamedPipeServerStream("firstPipe");
        //pipe client
        NamedPipeClientStream clientPipe = new NamedPipeClientStream("secondPipe");

        public Server()
        {
            InitializeComponent();
            backgroundWorker1.WorkerSupportsCancellation = false;
            backgroundWorker2.WorkerSupportsCancellation = false;

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //enable backgroundworker 1
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerAsync();
            txtChat.Focus();
            //enable backgroundworker 2
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.RunWorkerAsync();
        }

        //send the message from the pipe
        private void btnSend_Click(object sender, EventArgs e)
        {

            String msg = " Server: " + txtChat.Text;
            lbxChat.Items.Add(msg);
            Byte[] msgByte = System.Text.Encoding.GetEncoding("windows-1256").GetBytes(msg);
            serverPipe.Write(msgByte, 0, msgByte.Length);
            StreamWriter sw = new StreamWriter(@"../../../dateTime.txt", true);
            sw.WriteLine(DateTime.Now + msg);
            sw.Close();

            if (btnSend.Enabled)
            {
                txtChat.Focus();
                txtChat.Clear();

            }
        }
        //run background worker as a server
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try

            {
                this.Text = "Waiting for Connection";
                serverPipe.WaitForConnection();

                if (serverPipe.IsConnected)
                {
                    txtChat.Focus();
                    btnStart.Enabled = false;
                    btnSend.Enabled = true;
                    this.Text = "Connected";

                }
            }
            //tching error
            catch (Exception ex)
            {
                Catch(ex);
            }

        }

        //run background worker as a client
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            //conneccting to server
            clientPipe.Connect();

            byte[] clientByte;

            while (clientPipe.IsConnected)
            {
                clientByte = new Byte[1000];
                for (int i = 0; i < clientByte.Length; i++)
                {
                    clientByte[i] = 0x20;

                }
                clientPipe.Read(clientByte, 0, clientByte.Length);
                string msgStr = System.Text.Encoding.GetEncoding("windows-1256").GetString(clientByte);
                lbxChat.Items.Add(msgStr.Trim());

            }//end of while loop

            clientPipe.Close();
            serverPipe.Close();
            this.Text = "Disconnected";
        }

        //error writer to error.log
        private void Catch(Exception ex)
        {
            string exMs = DateTime.Now + ex.Message;
            MessageBox.Show(exMs, "Exception");
            StreamWriter sw = new StreamWriter(@"../../../error.log", true);
            sw.WriteLine(DateTime.Now + exMs);
            sw.Close();
        }

        //exit button
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            clientPipe.Close();
            serverPipe.Close();
            Application.Exit();
        }

        //display previous conversation
        private void previousConversationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            StreamReader read = new StreamReader(@"../../../dateTime.txt", true);
            conversation(list, read);
       }

        //display errors from error.log
        private void viewErrorLogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<String> list = new List<string>();
            StreamReader read = new StreamReader(@"../../../error.log", true);
            errorList(list, read);         
        }

        //method for errorList
        private void errorList(List<string> list, StreamReader read)
        {
            string str;
            while ((str = read.ReadLine()) != null)
            {
                list.Add(str);
                lbxChat.Items.Add(DateTime.Now + " " + str);
            }
         }

        // method for previous conversation
        private void conversation(List<string> list, StreamReader read)
        {
            string str;
            while ((str = read.ReadLine()) != null)
            {
                list.Add(str);
                lbxChat.Items.Add(str);
            }
          
        }
    } 
   
}

