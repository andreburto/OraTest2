using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oracle.DataAccess.Client;
using OraTest2;

namespace OraTest2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Creds cred = new Creds();
        private List<Letter> ltrs = new List<Letter>();
        public List<OraTest2.TablesChoices> tbls = new List<OraTest2.TablesChoices>();
        public string connStr;
        public delegate void UpdateCombo();
        private bool _chosen = false;

        // EVENTS
        public MainWindow()
        {
            InitializeComponent();

            if (cred.IsFile)
            {
                cred.Load();
                txtID.Text = cred.Id; txtServer.Text = cred.Server; txtDB.Text = cred.Database;
                if (cred.Pw != "nada") { txtPW.Text = cred.Pw; }
            }

            LoadLetters();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            cred.Save(txtID.Text, txtPW.Text, txtServer.Text, txtDB.Text);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            MakeConnection();
            cmbTables.IsEnabled = false;
            btnChoose.IsEnabled = false;
            cmbLetters.IsEnabled = true;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            OracleConnection connTest = new OracleConnection();
            bool yesno = true;
            try
            {
                // Reopen the connection
                connTest.ConnectionString = MakeConn();
                connTest.Open();
            }
            catch (Exception ex)
            {
                yesno = false;
                ErrMsg(this.connStr + "\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.Source);
            }
            finally
            {
                if (connTest.State != ConnectionState.Closed) { connTest.Close(); }
                connTest.Dispose();
            }
            if (yesno == true) { MessageBox.Show("Succesful connection", "Success"); }
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            if (this._chosen == false) { return; }
            OraTest2.TablesChoices table = (OraTest2.TablesChoices)cmbTables.SelectedValue;
            LoadTableData(table.Title);
        }

        private void cmbLetters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Letter temp = (Letter)cmbLetters.SelectedValue;
            if (temp.Chr.Length > 0)
            {
                lblOne.Content = "Letter: " + temp.Chr;
                LoadTableList(temp.Chr);
                this._chosen = false;
            }
            else
            {
                lblOne.Content = "Bad value";
            }
        }

        private void cmbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._chosen = true;
        }

        public void MakeConnection()
        {
            OracleConnection connOra = new OracleConnection();

            try
            {
                // Connect to the database
                connOra.ConnectionString = MakeConn();
                connOra.Open();
            }
            catch (Exception ex)
            {
                ErrMsg(ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.Source);
            }
            finally
            {
                // No matterv what, close and dispose
                connOra.Close();
                connOra.Dispose();
            }
        }

        public void LoadLetters()
        {
            for (int cnt = 65; cnt <= 90; cnt++)
            {
                char ltr = (char)cnt;
                ltrs.Add(new Letter(ltr.ToString()));
            }
            cmbLetters.DataContext = ltrs;
        }

        public void LoadTableList(string ltr)
        {
            OracleConnection connOra = new OracleConnection();
            OracleDataReader reader;
            OracleCommand cmd;
            string queryA = "select table_name FROM all_tables WHERE substr(table_name, 0, 1) = '"+ ltr+ "' order by table_name";

            try
            {
                // Clear tables
                cmbTables.IsEnabled = false;
                btnChoose.IsEnabled = false;
                this.tbls.Clear();

                // Reopen the connection
                connOra.ConnectionString = MakeConn();
                connOra.Open();

                // Create the command
                cmd = new OracleCommand(queryA);
                cmd.Connection = connOra;
                cmd.CommandType = CommandType.Text;

                // Execute
                reader = cmd.ExecuteReader();

                // Grab the data
                while (reader.Read())
                {
                    string title = reader.GetString(0).Substring(0);
                    tbls.Add(new OraTest2.TablesChoices(title));
                }

                // No need for these any more
                cmd.Dispose();
                reader.Dispose();
            }
            catch (Exception ex)
            {
                ErrMsg(this.connStr + "\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.Source);
            }
            finally
            {
                connOra.Close();
                connOra.Dispose();
            }

            this.cmbTables.DataContext = null;
            this.cmbTables.DataContext = this.tbls;
            cmbTables.IsEnabled = true;
            btnChoose.IsEnabled = true;
        }

        public void LoadTableData(string tblName)
        {
            OracleConnection connOra = new OracleConnection();
            string queryB = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE, DATA_DEFAULT, CHAR_LENGTH FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = '" + tblName + "'";

            try
            {
                // Reopen the connection
                connOra.ConnectionString = MakeConn();
                connOra.Open();

                // Create the command
                OracleCommand cmd = new OracleCommand(queryB);
                cmd.Connection = connOra;
                cmd.CommandType = CommandType.Text;

                // Execute
                DataSet ds = new DataSet();
                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                adapter.Fill(ds);

                dgTable1.ItemsSource = new DataView(ds.Tables[0]);
            }
            catch (Exception ex)
            {
                ErrMsg(this.connStr + "\n\n" + ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.Source);
            }
            finally
            {
                connOra.Close();
                connOra.Dispose();
            }
        }

        public string MakeConn()
        {
                // Setup connection string
                if (txtID.Text.Length == 0) { throw new Exception("No id"); }
                if (txtPW.Text.Length == 0) { throw new Exception("No password"); }
                if (txtServer.Text.Length == 0) { throw new Exception("No server"); }
                if (txtDB.Text.Length == 0) { throw new Exception("No database"); }
                return @"User ID=" + txtID.Text + @";Password=" + txtPW.Text + @";Data Source=" + txtServer.Text + @":1521/" + txtDB.Text;
        }

        public void ErrMsg(string msg)
        {
            try
            {
                if (msg.Length == 0) { throw new Exception("No mesage passed for error"); }
                MessageBox.Show(msg, "ERROR", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                ErrMsg(ex.Message);
            }
        }
    }
}
