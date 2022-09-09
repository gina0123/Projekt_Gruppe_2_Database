using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Projekt_Gruppe_2
{
    public class Message 
    {        
        //public int MessageID { get; set; }
        public long TimestampUnix { get; set; }
        public Byte[] Payload { get; set; }
        public string IPSender { get; set; }
        public string AliasSender { get; set; }
        public string DataFormat { get; set; }
        public string IPEmpfaenger { get; set; }
        public int Port { get; set; }
        public override string ToString()
        {
            return "Message: " + TimestampUnix + " Payload: " + Payload + " IPSender: " + IPSender + " AliasSender: " + AliasSender + " DataFormat: " + DataFormat + " IPEmpfaenger: " + IPEmpfaenger + " Port: " + Port;
        }

    }
}
