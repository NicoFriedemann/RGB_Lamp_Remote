using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace ModernUINavigationApp.Pages.Settings
{
    class NetworkViewModel:INotifyPropertyChanged
    {
        public NetworkViewModel()
        {
            partnerPort = 11001;
            partnerIP = IPAddress.Parse("192.168.178.30");
            remoteListeningPort = 11000;
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
        private int remoteListeningPort;

        public int RemoteListeningPort
        {
            get => remoteListeningPort; set
            {
                NotifyPropertyChanged("RemoteListeningPort");
                if (RemoteListeningPort > 0 && RemoteListeningPort < 50000)
                {
                    remoteListeningPort = value;
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
