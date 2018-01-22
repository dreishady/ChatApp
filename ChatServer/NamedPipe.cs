using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    class NamedPipe
    {
        #region Initialize

        List<int> Queue = new List<int>();
        public static int PipeInstance = 5;
        public string PipeName = "ServerPipe1";
        public NamedPipeServerStream[] ZPipe = new NamedPipeServerStream[PipeInstance];

        #endregion

        #region NamedPipe Singleton

        private static NamedPipe _instance;
        public static NamedPipe Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NamedPipe();
                }
                return _instance;
            }
        }

        #endregion

        /// <summary>
        /// Creates the server pipe.
        /// </summary>
        /// <param name="i">The i.</param>
        public void CreatePipe(int i)
        {
            ZPipe[i] = new NamedPipeServerStream(PipeName, PipeDirection.Out, PipeInstance, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            ZPipe[i].WaitForConnection();
            Queue.Add(i);
        }

        /// <summary>
        /// Sends the data to client.
        /// </summary>
        /// <param name="attachedData">The attached data.</param>
        public void SendData(string attachedData)
        {
            foreach (int queueOrder in Queue)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(ZPipe[queueOrder]))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine(attachedData);
                        ZPipe[queueOrder].WaitForPipeDrain();
                        sw.Close();
                    }
                    MessageBox.Show("Client" + queueOrder + " Disconnected");
                    Queue.Remove(queueOrder);
                }
                catch (IOException er)
                {
                    Queue.Remove(queueOrder);
                    MessageBox.Show("Client" + queueOrder + " Disconnected. Due to - " + er.Message.ToString());
                    SendData(attachedData);
                }
                break;
            }
        }
    }
}
