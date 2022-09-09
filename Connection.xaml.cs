using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using Projekt_Gruppe_2_test;

namespace Projekt_Gruppe_2
{
    /// <summary>
    /// Interaktionslogik für Verbindung.xaml
    /// </summary>
    /// 
    
    public partial class Connection : Window
    { 
        Message message = new Message()
        {
            Port = 13000
        };
        
        public Connection()
        {
            InitializeComponent();

            txtNameSender.Text = Globals.AliasSender;
            message.AliasSender = txtNameSender.Text;
            txtPort.Text = Convert.ToString(message.Port);
            txtIPSender.Text = GetLocalIP();
            Globals.IPSender = GetLocalIP();
        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Globals.port = Convert.ToInt32(txtPort.Text);

            if (txtNameReceiver.Text == string.Empty)
            {
                MessageBox.Show("Bitte einen Empfänger eingeben.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            } else if (txtNameReceiver.Text != string.Empty)
            {              
                Globals.empfName = txtNameReceiver.Text;
            }
            
            TCPSender sender1 = new TCPSender();
            string ipadresse = txtIpEmpf.Text;

            bool IsIP(string ipadresse)
            {
                return System.Text.RegularExpressions.Regex.IsMatch(ipadresse, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
            }

            if (!IsIP(ipadresse))
            {
                MessageBox.Show("Bitte eine IP-Adresse eingeben.","Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                txtIpEmpf.Clear();
                return;
            }else
                if (IsIP(ipadresse))
            {
                //setzte vom Objekt die Ip-Adresse
                message.IPEmpfaenger = ipadresse;
                Globals.IPEmpfaenger = ipadresse;

                CheckInDatabase();
                /*
                var newWindow = new ChatScreen();
                this.Close();
                newWindow.Show();
                */
            }
        }

        private void txtNameReceiver_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btnConnect_Click(sender, e);
            }
        }


        //Datenbankanbindung
        //
        //

        private void CheckInDatabase()
        {
            SqlConnection con = new SqlConnection();
            SqlCommand com = new SqlCommand();
            SqlDataReader dr;
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
            con.Open();
            com.Connection = con;
            com.CommandText = "SELECT * FROM Users WHERE IPSender ='" + Globals.IPEmpfaenger + "'";
            dr = com.ExecuteReader();
            if (dr.Read())
            {
                if (Convert.ToBoolean(dr["Status"]) == true)
                {
                    con.Close();
                    ExistingInDatabase();
                }
                else
                {
                    con.Close();
                    SaveInDatabase();
                }
            }
            else
            {
                con.Close();

                //MessageBox.Show("Ein Fehler ist aufgetreten", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                //return;

                SaveInDatabase();

            }
        }

        private void SaveInDatabase()
        {

            AliasReceiver empf = new AliasReceiver();
            empf.aliasReceiver = txtNameReceiver.Text;

            SqlConnection con = new SqlConnection();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "INSERT INTO [Users] (IPSender, Port, AliasSender, Status) Values(@ip, '13000', @al, '1')";
            cmd.Parameters.AddWithValue("@ip", Globals.IPEmpfaenger);
            cmd.Parameters.AddWithValue("@al", empf.aliasReceiver);
            cmd.Connection = con;
            int a = cmd.ExecuteNonQuery();
            if (a == 1)
            {
                MessageBox.Show("User erfolgreich hinzugefügt");
            }
            con.Close();

            var newWindow = new ChatScreen();
            this.Close();
            newWindow.Show();

        }


        private void ExistingInDatabase()
        {
            MessageBox.Show("Diese IP-Adresse ist bereits bekannt", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);


            var newWindow = new ChatScreen();
            this.Close();
            newWindow.Show();

        }

    }
}
