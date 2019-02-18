using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace rgb_remote
{
    public class Networking :INotifyPropertyChanged  
    {
        private static UdpClient talker;
        private static UdpClient listener;
        private IPAddress partnerIP;
        private int partnerPort;
        private int remoteListeningPort;
        private const int C_MAXLOGCOUNT = 1000;
        
        private ObservableCollection<String> receivedMessageLog;
        public ObservableCollection<String> ReceivedMessageLog { get => receivedMessageLog; set => receivedMessageLog = value; }

        private bool loggingActive;
        public bool LoggingActive { get => loggingActive; set => loggingActive = value; }
        
        
        public event NetworkEventHandler OnError = delegate { };
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void NetworkEventHandler(string errorMessage);

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public Networking(IPAddress PartnerIP, int PartnerPort, int RemoteListeningPort)
        {
            talker = new UdpClient();
            partnerIP = PartnerIP;
            partnerPort = PartnerPort;
            remoteListeningPort = RemoteListeningPort;
            loggingActive = false;
            receivedMessageLog = new ObservableCollection<String>();
            StartListening();
        }

        public void SendString(String msg)
        {
            talker.Connect(new IPEndPoint(partnerIP, partnerPort));
            byte[] text = Encoding.ASCII.GetBytes(msg.Replace(',', '.'));
            talker.Send(text, text.Length);
        }

        private void StartListening()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (listener != null)
                    {
                        listener.Dispose();
                    }
                    listener = new UdpClient(remoteListeningPort);
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, remoteListeningPort);
                    try
                    {
                        byte[] bytes = listener.Receive(ref groupEP);
                        if (loggingActive)
                        {
                            Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                            {
                                receivedMessageLog.Add(DateTime.Now.ToString() + " " + Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                                if (receivedMessageLog.Count > C_MAXLOGCOUNT)
                                {
                                    receivedMessageLog.RemoveAt(1);
                                }
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e.ToString());
                    }
                }
            });
        }
    }
}
