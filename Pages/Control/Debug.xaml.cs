using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.Net;

namespace ModernUINavigationApp.Pages.Control
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class Debug : UserControl
    {
        private Libraries.UDPReceiver receiver;
        private int partnerPort = 11001;
        private int receivePort = 11000;
        private IPAddress partnerIP = IPAddress.Parse("192.168.178.30");
            

        public Debug()
        {
            InitializeComponent();
            //receivePort =  int.Parse(Application.Current.Properties["ReceiverPort"].ToString());
            receiver = new Libraries.UDPReceiver(receivePort);
            
            this.dataGrid1.ItemsSource = receiver.ReceivedMessageLog;
        }


        private void startRemoteDebugging_Click(object sender, RoutedEventArgs e)
        {
            receiver.LoggingActive = true;
            Libraries.RGB_Lamp rgb = new Libraries.RGB_Lamp(partnerIP, partnerPort,Libraries.RGB_Lamp.e_udpmsg_type.pos_based_format);
            rgb.SubscribeDebugMsg(Libraries.RGB_Lamp.e_debug_level.dl_debug, receivePort);
        }

        private void stopRemoteDebugging_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DebugLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
