using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ModernUINavigationApp.Libraries
{
    class RGB_Lamp
    {
        private const char C_CMD_BEGIN_CHAR = '<';
        private const char C_CMD_END_CHAR = '>';
        private const int C_MAX_VAL = 4096;
        private const int C_MAX_RGB_COLORPICKER = 255;
        private IPAddress partnerIP;
        private int partnerPort;
        private e_udpmsg_type msgType;

        public e_udpmsg_type MsgType { get => msgType; set => msgType = value; }
        public IPAddress PartnerIP { get => partnerIP; set => partnerIP = value; }
        public int PartnerPort { get => partnerPort; set => partnerPort = value; }

        public enum e_udpmsg_type
        {
            undefined_udpmsg_type,
            json,
            pos_based_format,
            element_count_udpmsg_type,
        };

        public enum E_prog_nmbr
        {
            auto1,
            auto2,
            auto3,
            auto4,
            auto5,
            auto6,
            auto7,
            auto8,
            auto9,
            man_blynk,
            man_pc_rgb,
            man_pc_hsv,
        };

        public enum E_udpmsg_cmd
        {
            undefined_udpmsg_cmd,
            change_prog,
            change_color,
            add_msg_receiver,
            element_count_udpmsg_cmd,
        };

        public enum E_udpmsg_parname
        {
            undefined_udpmsg_parname,
            program_number,
            red,
            green,
            blue,
            hue,
            saturation,
            value,
            debug_level,
            port,
            element_count_udpmsg_parname,
        };

        public enum e_debug_level
        {
            dl_debug,
            dl_info,
            dl_error,
        };

        public RGB_Lamp(IPAddress PartnerIP, int PartnerPort, e_udpmsg_type MsgType)
        {
            partnerIP = PartnerIP;
            partnerPort = PartnerPort;
            msgType = MsgType;
        }

        public void SendCommand(String cmd)
        {
            Libraries.UDPSender sender = new Libraries.UDPSender(partnerIP, partnerPort);
            sender.SendString(cmd);
        }

        public void ChangeHSVColor(float hue, float saturation, float value)
        {
            String controlMessage = string.Empty;
            switch (msgType)
            {
                case e_udpmsg_type.json:
                    break;
                case e_udpmsg_type.pos_based_format:
                    controlMessage = C_CMD_BEGIN_CHAR + ((int)E_udpmsg_cmd.change_color).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.hue).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + hue.ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.saturation).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + saturation.ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.value).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + value.ToString() + C_CMD_END_CHAR;
                    break;
                default:
                    break;
            }
            SendCommand(controlMessage);
        }

        public void ChangeRGBColor(float red, float green, float blue)
        {
            int r = Convert.ToInt16(red * C_MAX_VAL);
            int g = Convert.ToInt16(green * C_MAX_VAL);
            int b = Convert.ToInt16(blue * C_MAX_VAL);
            String controlMessage = string.Empty;
            switch (msgType)
            {
                case e_udpmsg_type.json:
                    break;
                case e_udpmsg_type.pos_based_format:
                    controlMessage = C_CMD_BEGIN_CHAR + ((int)E_udpmsg_cmd.change_color).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.red).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + r.ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.green).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + g.ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.blue).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + b.ToString() + C_CMD_END_CHAR;
                    break;
                default:
                    break;
            }
            SendCommand(controlMessage);
        }

        public void SubscribeDebugMsg(e_debug_level dl, int receivePort)
        {
            String controlMessage = string.Empty;
            switch (msgType)
            {
                case e_udpmsg_type.json:
                    break;
                case e_udpmsg_type.pos_based_format:
                    controlMessage = C_CMD_BEGIN_CHAR + ((int)E_udpmsg_cmd.add_msg_receiver).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.debug_level).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)dl).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)E_udpmsg_parname.port).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + receivePort.ToString() + C_CMD_END_CHAR;
                    break;
                default:
                    break;
            }
            SendCommand(controlMessage);
        }

        public void ChangeProgramESP32(E_udpmsg_cmd cmd, E_udpmsg_parname par, float val)
        {
            string controlMessage = string.Empty;
            switch (msgType)
            {
                case e_udpmsg_type.json:
                    break;
                case e_udpmsg_type.pos_based_format:
                    controlMessage = C_CMD_BEGIN_CHAR + ((int)cmd).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + ((int)par).ToString() + C_CMD_END_CHAR +
                    C_CMD_BEGIN_CHAR + (val + 1).ToString() + C_CMD_END_CHAR;
                    break;
                default:
                    break;
            }
            SendCommand(controlMessage);
        }
    }
}
