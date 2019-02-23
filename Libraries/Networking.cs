using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModernUINavigationApp.Libraries
{
    public class UDPSender
    {
        private static UdpClient sender;
        private IPAddress partnerIP;
        private int partnerPort;

        public UDPSender(IPAddress PartnerIP, int PartnerPort)
        {
            sender = new UdpClient();
            partnerIP = PartnerIP;
            this.PartnerPort = PartnerPort;
        }

        public IPAddress PartnerIP { get => partnerIP; set => partnerIP = value; }
        public int PartnerPort { get => partnerPort; set => partnerPort = value; }

        public void SendString(String msg)
        {
            sender.Connect(new IPEndPoint(partnerIP, PartnerPort));
            byte[] text = Encoding.ASCII.GetBytes(msg.Replace(',', '.'));
            sender.Send(text, text.Length);
        }
    }

    public class UDPReceiver :INotifyPropertyChanged  
    {
        private static UdpClient receiver;
        private int receivePort;
        private bool loggingActive;
        private const int C_MAXLOGCOUNT = 1000;
        
        private ObservableCollection<DebugLogElement> receivedMessageLog;
        public ObservableCollection<DebugLogElement> ReceivedMessageLog { get => receivedMessageLog; set => receivedMessageLog = value; }

        public bool LoggingActive { get => loggingActive; set => loggingActive = value; }
        public int ReceivePort { get => receivePort; set => receivePort = value; }

        public event NetworkEventHandler OnError = delegate { };
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void NetworkEventHandler(string errorMessage);

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public UDPReceiver(int ReceivePort)
        {
            receiver = new UdpClient();
            this.ReceivePort = ReceivePort;
            loggingActive = false;
            receivedMessageLog = new ObservableCollection<DebugLogElement>();
            StartListening();
        }

        private void StartListening()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (receiver != null)
                    {
                        receiver.Dispose();
                    }
                    receiver = new UdpClient(ReceivePort);
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, ReceivePort);
                    try
                    {
                        byte[] bytes = receiver.Receive(ref groupEP);
                        if (loggingActive)
                        {
                            Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                            {
                                String message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                                RGB_Lamp.e_udpmsg_type msgType = GetMessageType(message);
                                DebugLogElement dle = new DebugLogElement();
                                switch (msgType)
                                {
                                    //implement format
                                    case RGB_Lamp.e_udpmsg_type.undefined_udpmsg_type:
                                        break;
                                    case RGB_Lamp.e_udpmsg_type.json:
                                        break;
                                    case RGB_Lamp.e_udpmsg_type.pos_based_format:
                                        try
                                        {
                                            dle.Topic = message.Substring(0, message.IndexOf("->"));
                                            message = message.Replace(dle.Topic + "->", "");
                                            dle.Message = message;
                                        }
                                        catch (Exception)
                                        {
                                            dle.Topic = "ERROR";
                                            dle.Message = "cannot interprete debug message";
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (dle != null)
                                {
                                    receivedMessageLog.Add(dle);
                                    if (receivedMessageLog.Count > C_MAXLOGCOUNT)
                                    {
                                        receivedMessageLog.RemoveAt(1);
                                    }
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

        private RGB_Lamp.e_udpmsg_type GetMessageType(String msg)
        {
            //implement better format detection
            if (msg.Contains("{") && msg.Contains("}"))
            {
                return RGB_Lamp.e_udpmsg_type.json;
            }
            else
            {
                return RGB_Lamp.e_udpmsg_type.pos_based_format;
            }
        }

    }

    public class DebugLogElement
    {
        private DateTime eventTime;
        private String topic;
        private String message;

        public DateTime EventTime { get => eventTime; }
        public string Topic { get => topic; set => topic = value; }
        public string Message { get => message; set => message = value; }

        public DebugLogElement(String Topic, String Message)
        {
            eventTime = DateTime.Now;
            topic = Topic;
            message = Message;
        }

        public DebugLogElement()
        {
            eventTime = DateTime.Now;
            topic = String.Empty;
            message = String.Empty;
        }

    }
}
