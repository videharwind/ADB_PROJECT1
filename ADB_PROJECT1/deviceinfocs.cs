using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADB_PROJECT1
{
    public partial class deviceinfocs : Form
    {
        public deviceinfocs()
        {
            InitializeComponent();
        }
        public void SaveDetailedCPUInfoToDB(string cpuRawText)
        {
            // Split each core's block
            var coreBlocks = Regex.Split(cpuRawText.Trim(), @"\n\s*\n");

            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                conn.Open();

                // Optional: Clear old records
                SqlCommand deleteCmd = new SqlCommand("DELETE FROM dbo.deviceinfo", conn);
                deleteCmd.ExecuteNonQuery();

                foreach (var block in coreBlocks)
                {
                    Match processor = Regex.Match(block, @"processor\s*:\s*(\d+)");
                    Match bogoMIPS = Regex.Match(block, @"BogoMIPS\s*:\s*(.+)");
                    Match features = Regex.Match(block, @"Features\s*:\s*(.+)");
                    Match implementer = Regex.Match(block, @"CPU implementer\s*:\s*(.+)");
                    Match architecture = Regex.Match(block, @"CPU architecture\s*:\s*(.+)");
                    Match variant = Regex.Match(block, @"CPU variant\s*:\s*(.+)");
                    Match part = Regex.Match(block, @"CPU part\s*:\s*(.+)");
                    Match revision = Regex.Match(block, @"CPU revision\s*:\s*(.+)");

                    if (processor.Success)
                    {
                        SqlCommand insertCmd = new SqlCommand(@"
                    INSERT INTO dbo.deviceinfo 
                    (processor, BogoMIPS, Features, CPUimplementer, CPUarchitecture, CPUvariant, CPUpart, CPUrevision) 
                    VALUES 
                    (@processor, @bogo, @features, @impl, @arch, @variant, @part, @rev)", conn);

                        insertCmd.Parameters.AddWithValue("@processor", int.Parse(processor.Groups[1].Value));
                        insertCmd.Parameters.AddWithValue("@bogo", bogoMIPS.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@features", features.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@impl", implementer.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@arch", architecture.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@variant", variant.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@part", part.Groups[1].Value.Trim());
                        insertCmd.Parameters.AddWithValue("@rev", revision.Groups[1].Value.Trim());

                        insertCmd.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }

            MessageBox.Show("Detailed CPU info saved for all cores.");
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

        private void LoadDeviceInfoToGrid()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                string query = "SELECT * FROM dbo.deviceinfo";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }

        private void deviceinfocs_Load(object sender, EventArgs e)
        {
            string cputext = ADB_RUN("shell cat /proc/cpuinfo");
            MessageBox.Show(cputext.Length.ToString());
            SaveDetailedCPUInfoToDB(cputext);
            LoadDeviceInfoToGrid();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
