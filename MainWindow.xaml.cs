using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Projekt_Gruppe_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class Globals
    {
        public static string empfName;
        public static string IPEmpfaenger;
        public static string AliasSender;
        public static string Payload;
        public static string showPayload;
        public static string date;
        public static List<Message> messageList = new List<Message>();
        public static string key;
        public static int messageCounter;
        public static int port;
        public static string IPSender;
        public static int User_ID;
        public static int count;
        public static string uebergabe;
    }

    public partial class MainWindow : Window
    {
        
        string name = "";
        string pw = "";

        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;

        public MainWindow()
        {            
            InitializeComponent();                
            pwdbox.Clear();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();

        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void txtboxUsername_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtboxUsername.Clear();
            txtboxUsername.FontStyle = FontStyles.Normal;
        }

        private void btnAnmelden_Click(object sender, RoutedEventArgs e)
        {
            String pawdSafe = pwdbox.Password.ToString(); //Eingegebenes Passwort wird in Variabel pawsafe gespeichert und als Text ausgegeben
            Globals.uebergabe = MD5Hash(pawdSafe);// vielleicht versuchen eine variable zu machen, sowie hier nicht wie globals.übergabe=strbuilder.Tostring();

            validierungDB();
        }

        private void pwdbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                String pawdSafe = pwdbox.Password.ToString(); //Eingegebenes Passwort wird in Variabel pawsafe gespeichert und als Text ausgegeben
                Globals.uebergabe = MD5Hash(pawdSafe);// vielleicht versuchen eine variable zu machen, sowie hier nicht wie globals.übergabe=strbuilder.Tostring();

                validierungDB();
            }
        }

        private void falscheEingabe()
        {
            MessageBox.Show("Username oder Passwort ist falsch.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            txtboxUsername.Text = "Username";
            pwdbox.Clear();
        }


        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();


        }


        private void validierungDB()
        {
            bool valid = false;
            con.Open();
            com.Connection = con;
            com.CommandText = "select Status from LoginData where Benutzername='" + txtboxUsername.Text + "' and Passwort='" + Globals.uebergabe + "'";
            dr = com.ExecuteReader();
            if (dr.Read())
            {
                if (Convert.ToBoolean(dr["Status"]) == true)
                {
                    con.Close();
                    logintrue();
                }
                else if (txtboxUsername.Text == string.Empty || txtboxUsername.Text == "Username")
                {
                    con.Close();
                    MessageBox.Show("Bitte einen Usernamen eingeben", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    con.Close();
                    falscheEingabe();
                }
            }
            else
            {
                con.Close();
                falscheEingabe();
            }
        }

        private void logintrue()
        {
            Globals.AliasSender = txtboxUsername.Text;
            var newWindow = new Connection();
            this.Close();
            newWindow.Show();
        }


        private void dbcreate_Click(object sender, RoutedEventArgs e)
        {
            string mainconn = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection sqlconn0 = new SqlConnection(mainconn);
            string sqlquery0 = "DROP DATABASE  IF EXISTS Messanger;";
            SqlCommand sqlcomm0 = new SqlCommand(sqlquery0, sqlconn0);
            sqlconn0.Open();
            sqlcomm0.ExecuteNonQuery();
            sqlconn0.Close();
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "create database Messanger; ";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
            sqlconn.Open();
            sqlcomm.ExecuteNonQuery();
            sqlconn.Close();
            SqlConnection sqlconn2 = new SqlConnection(mainconn);
            string sqlquery2 = "CREATE TABLE LoginData([Benutzer_ID][int] IDENTITY(1, 1) PRIMARY KEY NOT NULL, [Benutzername] [nvarchar](255) NOT NULL, [Passwort] [varchar](255) NOT NULL, [Status] [int] NULL,);";
            SqlCommand sqlcomm2 = new SqlCommand(sqlquery2, sqlconn2);
            sqlconn2.Open();
            sqlcomm2.ExecuteNonQuery();
            sqlconn2.Close();
            SqlConnection sqlconn3 = new SqlConnection(mainconn);
            string sqlquery3 = "Insert into LoginData values ('test','81dc9bdb52d04dc20036dbd8313ed055','1');";
            SqlCommand sqlcomm3 = new SqlCommand(sqlquery3, sqlconn3);
            sqlconn3.Open();
            sqlcomm3.ExecuteNonQuery();
            sqlconn3.Close();
            SqlConnection sqlconn4 = new SqlConnection(mainconn);
            string sqlquery4 = "create table Users (User_ID int IDENTITY(1,1) PRIMARY KEY NOT NULL, IPSender nvarchar(256), Port int, AliasSender nvarchar(256), Status int NOT NULL);";
            SqlCommand sqlcomm4 = new SqlCommand(sqlquery4, sqlconn4);
            sqlconn4.Open();
            sqlcomm4.ExecuteNonQuery();
            sqlconn4.Close();
            SqlConnection sqlconn5 = new SqlConnection(mainconn);
            string sqlquery5 = "create table Messages(Message_ID int IDENTITY(1,1) PRIMARY KEY NOT NULL, User_ID int Foreign Key References Users(User_ID), TimestampUnix float, DataFormat nvarchar(256), Payload varbinary(max), IPEmpfaenger nvarchar(256));";
            SqlCommand sqlcomm5 = new SqlCommand(sqlquery5, sqlconn5);
            sqlconn5.Open();
            sqlcomm5.ExecuteNonQuery();
            sqlconn5.Close();

            MessageBox.Show("DataBase is Created Successfully");

            
            /*
            DataSet database = new DataSet("Messanger");
            DataTable LoginData = database.Tables.Add("LoginData");


            LoginData.Columns.Add("Benutzer_ID", typeof(Int32));
            LoginData.Columns["Benutzer_ID"].AutoIncrement = true;
            LoginData.Columns["Benutzer_ID"].AutoIncrementSeed = 1;
            LoginData.Columns["Benutzer_ID"].AutoIncrementStep = 1;
            LoginData.Columns["Benutzer_ID"].Unique = true;
            LoginData.PrimaryKey = new DataColumn[] { LoginData.Columns["Benutzer_ID"] };
            LoginData.Columns.Add("Benutzername", typeof(string));
            LoginData.Columns["Benutzername"].AllowDBNull = false;
            LoginData.Columns.Add("Passwort", typeof(string));
            LoginData.Columns["Passwort"].AllowDBNull = false;
            LoginData.Columns.Add("Status", typeof(Int32));
            LoginData.Columns["Status"].AllowDBNull = false;

            LoginData.Rows.Add( 1,"test", "81dc9bdb52d04dc20036dbd8313ed055", 1);


            DataTable Users = database.Tables.Add("Users");
            Users.Columns.Add("User_ID", typeof(Int32));
            Users.Columns["User_ID"].AutoIncrement = true;
            Users.Columns["User_ID"].AutoIncrementSeed = 1;
            Users.Columns["User_ID"].AutoIncrementStep = 1;
            Users.Columns["User_ID"].Unique = true;
            Users.PrimaryKey = new DataColumn[] { LoginData.Columns["User_ID"] };
            Users.Columns.Add("IPSender", typeof(string));
            Users.Columns.Add("Port", typeof(Int32));
            Users.Columns.Add("AliasSender", typeof(string));
            Users.Columns.Add("Status", typeof(Int32));
            Users.Columns["Status"].AllowDBNull = false;


            DataTable Messages = database.Tables.Add("Messages");
            Messages.Columns.Add("Message_ID", typeof(Int32));
            Messages.Columns["Message_ID"].AutoIncrement = true;
            Messages.Columns["Message_ID"].AutoIncrementSeed = 1;
            Messages.Columns["Message_ID"].AutoIncrementStep = 1;
            Messages.Columns["Message_ID"].Unique = true;
            Messages.PrimaryKey = new DataColumn[] { LoginData.Columns["Message_ID"] };
            Messages.Columns.Add("User_ID", typeof(Int32));
            ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("KeyUser_ID", Users.Columns["User_ID"], Messages.Columns["User_ID"]);
            Messages.Columns.Add("TimestampUnix", typeof(float));
            Messages.Columns.Add("DataFormat", typeof(string));
            Messages.Columns.Add("Payload", typeof(SqlBinary));
            Messages.Columns.Add("IPEmpfaenger", typeof(string));

            */
            MessageBox.Show("Datensatz erfolgreich eingefügt. \n username: test \n passwort: 1234");
        }
    }
   }

