using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace ADB_PROJECT1
{
    public partial class Form1 : Form
    {
        string username;
        string password;
        public Form1()
        {
            InitializeComponent();

        }
        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            username = textBox1.Text;

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            password = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (username == "videh" && password == "arwind")
            {
                menu me = new menu();
                adb_commands adb = new adb_commands();
                me.Show();
            }
            else
            {
                MessageBox.Show("Incorrect username or password");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
    }
}
