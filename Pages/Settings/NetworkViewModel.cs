using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace ModernUINavigationApp.Pages.Settings
{
    class NetworkViewModel:INotifyPropertyChanged
    {
        public NetworkViewModel()
        {
            partnerPort = 11001;
            partnerIP = IPAddress.Parse("192.168.178.30");
            receivePort = 11000;
            Application.Current.Properties.Add("PartnerPort", partnerPort);
            Application.Current.Properties.Add("PartnerIP", partnerIP);
            Application.Current.Properties.Add("ReceiverPort", receivePort);
        }

        private IPAddress partnerIP;
        public String PartnerIP
        {
            get => partnerIP.ToString();
            set
            {
                NotifyPropertyChanged("PartnerIP");
                partnerIP = IPAddress.Parse(value);
            }
        }

        private int partnerPort;
        public int PartnerPort
        {
            get => partnerPort; set
            {
                NotifyPropertyChanged("PartnerPort");
                if (PartnerPort > 0 && PartnerPort < 50000)
                {
                    partnerPort = value;
                }
            }
        }
        private int receivePort;

        public int ReceivePort
        {
            get => receivePort; set
            {
                NotifyPropertyChanged("RemoteListeningPort");
                if (ReceivePort > 0 && ReceivePort < 50000)
                {
                    receivePort = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
