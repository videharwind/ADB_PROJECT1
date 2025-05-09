using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADB_PROJECT1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
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
        private void Form2_Load(object sender, EventArgs e)
        {
            string contactinfo = ADB_RUN("shell content query --uri content://contacts/phones/");
            MessageBox.Show(contactinfo.Length.ToString());
        }
    }
}
