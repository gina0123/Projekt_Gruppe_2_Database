using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Projekt_Gruppe_2_test;
using System.Threading;
using System.Media;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

namespace Projekt_Gruppe_2
{

    /// <summary>
    /// Interaktionslogik für ChatScreen.xaml
    /// </summary>
    public partial class ChatScreen : Window
    {
        Message message = new Message()
        {
            Port = 13000,
            IPEmpfaenger = Globals.IPEmpfaenger,
            AliasSender = Globals.AliasSender
        };

        AliasReceiver aliasReceiver = new AliasReceiver();
        TCPSender sender1 = new TCPSender();
        
        public ChatScreen()
        {            
            Thread thread1 = new Thread(threadDoWork);
            aliasReceiver.aliasReceiver = Globals.empfName;
            InitializeComponent();
            lblNameReceiver.Content = "Chat mit " + aliasReceiver.aliasReceiver;
            thread1.Start();
            ImportFromDatabase();

        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {          

            if (string.IsNullOrEmpty(message.DataFormat))
            {
                message.DataFormat = "textnachricht";

                //die Nachricht die übermittelt werden soll wird in einem Bytearray geschrieben
                Byte[] payload = Encoding.ASCII.GetBytes(textboxMessage.Text);
                //Byte[] payload = Encoding.ASCII.GetBytes(encryption(textboxMessage.Text));

                //setzte vom Objekt den Payload
                message.Payload = payload;
            }

            //setzte vom Objekt die aktuelle Zeit
            DateTime localDate = DateTime.Now;
            long unixTime = ((DateTimeOffset)localDate).ToUnixTimeSeconds();
            message.TimestampUnix = unixTime;
            
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.ConnectAsync("8.8.8.8", 65530);
                //socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                //versuch meine ip-Adresse zu bekommen, da wir mehrere IP-Adressen haben, muss die richtige gefunden werden
                //wir verbinden uns hierfür mit einem UDP-Socket und lesen dann dessen lokalen Endpunkt aus
                string localIP = endPoint.Address.ToString();

                //setzte vom Objekt die IP-Empfänger auf die gefundene Ip-Adresse
                message.IPSender = localIP;
            }
            
            //erstelle aus dem Objekt einen String und verschlüssele es
            string stringjson = JsonConvert.SerializeObject(message);
            
            //encryption(stringjson);
                        
            //starte die Methode senden mit der IP-Empfänger, dem stringjson und dem port
            sender1.send(Globals.IPEmpfaenger, stringjson, message.Port);

            Globals.messageList.Insert(Globals.count, message);
            Globals.count++;

            //setzte DataFormat wieder auf null
            message.DataFormat = string.Empty;

            if (textboxMessage.Text != string.Empty)
            {                
                DateTime datetime = UnixTimeStampToDateTime(message.TimestampUnix);
                string date = datetime.ToString("yyyy-MM-dd");
                if (date == Globals.date)
                {
                    string time = datetime.ToString("HH:mm:ss");
                    listChat.Items.Add(time + " " + Globals.AliasSender + ": " + textboxMessage.Text);
                }
                else
                {
                    listChat.Items.Add(datetime + " " + Globals.AliasSender + ": " + textboxMessage.Text);                                
                    Globals.date = date;
                }
            }   
            else if (textboxMessage.Text == string.Empty)
            {
                MessageBox.Show("Bitte gib eine Nachricht ein.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            textboxMessage.Clear();

            //zum Ende scrollen
            listChat.TabIndex = listChat.Items.Count - 1;
        }


        public void threadDoWork()
        {            
            TCPReceiver tcpReceiver = new TCPReceiver();
            int port = 13000;
            tcpReceiver.receive(port, this);                
        }
                        

        private void textboxMessage_GotMouseCapture(object sender, MouseEventArgs e)
        {
            textboxMessage.Clear();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {            
            var newWindow = new Connection();            
            this.Close();
            newWindow.Show();

            GetUserID();
        }

        private void btnData_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog openFileDlg = new OpenFileDialog();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();

            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            { 
                byte[] data = StreamFile(openFileDlg.FileName);
                string path = openFileDlg.FileName;
                string extension = Path.GetExtension(path);

                //SaveByteArrayToFileWithFileStream(data, extension);
                textboxMessage.Text = openFileDlg.FileName;
                message.Payload = data;
                message.DataFormat = extension;
            }
        }

        private void textboxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btnSend_Click(sender, e);                
            }
        }

        

        private void btnKonfetti_Click(object sender, RoutedEventArgs e)
        {
           // string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Sound\\samba.wav";
            //SoundPlayer player = new SoundPlayer(Properties.Ressources.samba);

            if (konfettiGif.Visibility == Visibility.Hidden)
            {
                konfettiGif.Visibility = Visibility.Visible;
                listChat.Visibility = Visibility.Hidden;
                textboxMessage.Visibility = Visibility.Hidden;
                //player.Load();
                //player.Play();
            }
            else if (konfettiGif.Visibility == Visibility.Visible)
            {
                konfettiGif.Visibility = Visibility.Hidden;
                listChat.Visibility = Visibility.Visible;
                textboxMessage.Visibility = Visibility.Visible;
                //player.Stop();
            }
        }

        public byte[] StreamFile(string filename)
        {
            byte[] fileData = null;

            using (FileStream fs = File.OpenRead(filename))
            {
                var binaryReader = new BinaryReader(fs);
                fileData = binaryReader.ReadBytes((int)fs.Length);
            }
            return fileData;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }                

        /*
        public static string GetRandomKey(int length)
        {
            byte[] rgb = new byte[length];
            RNGCryptoServiceProvider rngCrypt = new RNGCryptoServiceProvider();
            rngCrypt.GetBytes(rgb);
            return Convert.ToBase64String(rgb);
        }
        */

        private string encryption(string _text)
        {
            //Globals.key = GetRandomKey(32);            
            Globals.key = "Xn2r5u8x/A?D(G+KbPeShVmYq3s6v9y$";          
            var encryptedString = AesOperation.EncryptString( Globals.key, _text);
            
            return encryptedString;
        }


        //Database
        //Database


        public void ImportFromDatabase()
        {
            return;
            GetUserID();
            SqlConnection con = new SqlConnection();
            SqlCommand com = new SqlCommand();
            SqlDataReader dr;
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
            con.Open();
            com.Connection = con;
            com.CommandText = "SELECT TimestampUnix, CONVERT(VARCHAR(1000), Payload) from Messages"/* WHERE User_ID = '" + Globals.User_ID + "'"*/;

            //Result= User_ID;
            dr = com.ExecuteReader();
            /*
            while (dr.Read())
            {
                //Debug.WriteLine("{0}", dr.GetInt32(0));
                Globals.User_ID = dr.GetInt32(0);
            }
            */
            /*
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    Debug.WriteLine("{0}\t{1}\t{2}", dr.GetFloat(0),
                        /*dr.GetString(1), dr.GetString(1));
                    //listChat.Items.Add(DBTime + " " + Globals.EmpfName + ": " + DBPayload);
                }
            }
    */
            while (dr.Read())
            {
                Debug.WriteLine("{0}\t{1}", Convert.ToDouble(dr.GetFloat(0)),
                    /*dr.GetString(1), */Convert.ToDouble(dr.GetString(1)));
                //listChat.Items.Add(DBTime + " " + Globals.EmpfName + ": " + DBPayload);

            }

            dr.Close();
            con.Close();

            //listChat.Items.Add(time + " " + Globals.AliasSender + ": " + textboxNachricht.Text);
            listChat.Items.Add("Test654");

        }



        public void GetUserID()
        {


            SqlConnection con = new SqlConnection();
            SqlCommand com = new SqlCommand();
            SqlDataReader dr;



            for (var i = 1; i < Globals.messageList.Count + 1;)  //while
            {
                Debug.WriteLine("{0}", i);
                Debug.WriteLine("{0}", Globals.messageList.Count);



                con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
                con.Open();
                com.Connection = con;
                com.CommandText = "SELECT User_ID from Users WHERE IPSender = '" + Globals.IPEmpfaenger + "'";

                //Result= User_ID;
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    //Debug.WriteLine("{0}", dr.GetInt32(0));
                    Globals.User_ID = dr.GetInt32(0);
                }
                dr.Close();
                con.Close();

                WriteMessageInDatabase();

            }
            Globals.count = 0;
            return;

        }

        public void WriteMessageInDatabase()
        {

            SqlConnection con = new SqlConnection();
            SqlCommand com = new SqlCommand();
            SqlDataReader dr;

            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
            con.Open();
            com.Connection = con;

            //for 
            com.CommandText = "INSERT INTO Messages VALUES (@uid, @ts, @df, @pl, @ipe)";
            com.Parameters.AddWithValue("@uid", Globals.User_ID);
            com.Parameters.AddWithValue("@ts", Globals.messageList[0].TimestampUnix);
            com.Parameters.AddWithValue("@df", Globals.messageList[0].DataFormat);
            com.Parameters.AddWithValue("@pl", Globals.messageList[0].Payload);
            //Eigene IP
            com.Parameters.AddWithValue("@ipe", Globals.IPSender);

            int a = com.ExecuteNonQuery();
            if (a == 1)
            {
                Debug.WriteLine("send to database");
            }
            con.Close();

            Globals.messageList.RemoveAt(0);



            return;
        }
    }
}
