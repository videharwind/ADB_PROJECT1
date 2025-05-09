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
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
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
           
            var coreBlocks = Regex.Split(cpuRawText.Trim(), @"\n\s*\n");

            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                conn.Open();

              
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
        private void ExportDeviceInfoToPDF()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Android;Integrated Security=True;"))
            {
                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.deviceinfo", conn);
                DataTable table = new DataTable("deviceinfo");
                adapter.Fill(table);

                Report report = new Report();

                report.RegisterData(table, "deviceinfo");
                report.GetDataSource("deviceinfo").Enabled = true;

                
                ReportPage page = new ReportPage();
                report.Pages.Add(page);

                
                DataBand dataBand = new DataBand
                {
                    Name = "DataBand1",
                    DataSource = report.GetDataSource("deviceinfo"),
                    Height = 30 
                };
                page.Bands.Add(dataBand);

                float x = 0;
                float colWidth = 100;

                foreach (DataColumn col in table.Columns)
                {
                    TextObject textObject = new TextObject();
                    textObject.Bounds = new RectangleF(x, 0, colWidth, 20);
                    textObject.Text = $"[deviceinfo.{col.ColumnName}]";
                    textObject.Border.Lines = BorderLines.All;
                    dataBand.Objects.Add(textObject);
                    x += colWidth;
                }

                report.Prepare();

                PDFSimpleExport pdfExport = new PDFSimpleExport();
                report.Export(pdfExport, "DeviceInfoReport33.pdf");

                MessageBox.Show("PDF Report exported successfully.");
                conn.Close();
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
            ExportDeviceInfoToPDF();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
