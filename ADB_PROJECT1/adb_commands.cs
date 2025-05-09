using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ADB_PROJECT1
{
    public partial class adb_commands : Form
    {
        public adb_commands()
        {
            InitializeComponent();
        }

        public class CallLog
        {
            public int Id;
            public string PhoneNumber;
            public string CallType;
            public string Duration;
        }
        public void SaveCallLogsToDB(string adbOutput)
        {
            List<CallLog> logs = new List<CallLog>();
            var entries = Regex.Split(adbOutput, @"(?=Row \d+:)");

            int maxLogs = 5;

            foreach (string entry in entries)
            {
                if (logs.Count >= maxLogs)
                    break;

                Match idMatch = Regex.Match(entry, @"_id=(\d+)");
                Match numberMatch = Regex.Match(entry, @"number=([^\r\n,]+)");
                Match typeMatch = Regex.Match(entry, @"type=(\d+)");
                Match durationMatch = Regex.Match(entry, @"duration=(\d+)");

                if (idMatch.Success && numberMatch.Success && typeMatch.Success && durationMatch.Success)
                {
                    CallLog log = new CallLog
                    {
                        Id = int.Parse(idMatch.Groups[1].Value),
                        PhoneNumber = numberMatch.Groups[1].Value.Trim(),
                        CallType = typeMatch.Groups[1].Value switch
                        {
                            "1" => "Incoming",
                            "2" => "Outgoing",
                            "3" => "Missed",
                            _ => "Unknown"
                        },
                        Duration = durationMatch.Groups[1].Value
                    };
                    logs.Add(log);
                }
            }

            MessageBox.Show("Logs parsed: " + logs.Count);

            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                conn.Open();
                foreach (var c in logs)
                {
                    string checkQuery = "SELECT COUNT(*) FROM dbo.CallLogs WHERE Id = @Id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@Id", c.Id);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists == 0)
                    {
                        string insertQuery = "INSERT INTO dbo.CallLogs (Id, PhoneNumber, CallType, Duration) VALUES (@Id, @PhoneNumber, @CallType, @Duration)";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                        insertCmd.Parameters.AddWithValue("@Id", c.Id);
                        insertCmd.Parameters.AddWithValue("@PhoneNumber", c.PhoneNumber);
                        insertCmd.Parameters.AddWithValue("@CallType", c.CallType);
                        insertCmd.Parameters.AddWithValue("@Duration", c.Duration);
                        insertCmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        
        public static string ADB_RUN(string argument)
        {
            string adbpath = @"C:\Users\videh\Downloads\platform-tools-latest-windows\platform-tools\adb.exe";
            ProcessStartInfo startinfo = new ProcessStartInfo()
            {
                FileName = adbpath,
                Arguments = argument,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startinfo))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }

        private void LoadCallLogsToGrid()
        {
        
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                string query = "SELECT Id, PhoneNumber, CallType, Duration FROM dbo.CallLogs";
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                
                dataGridView1.DataSource = dataTable;
            }
        }

        private void adb_commands_Load(object sender, EventArgs e)
        {
            string callLogs = ADB_RUN("shell content query --uri content://call_log/calls/ --projection _id:number:type:date:duration");
            MessageBox.Show("Characters fetched: " + callLogs.Length);
            SaveCallLogsToDB(callLogs);
            MessageBox.Show("Call logs saved to database.");
            LoadCallLogsToGrid();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }
    }
}
