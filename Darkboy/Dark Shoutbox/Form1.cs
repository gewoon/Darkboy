using System;
using System.Drawing;
using System.Windows.Forms;
using DBLib;
using Dark_Shoutbox.Properties;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dark_Shoutbox
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]                   //Needed to check is window is activated
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        int on;
        public DBSession sess { get; set; }
        public Form1()
        {
            InitializeComponent();
            if (Settings.Default.remember)
            {
                Settings.Default.Upgrade();
                textBox3.Text = Settings.Default.username;
                textBox2.Text = Settings.Default.password;
                checkBox3.Checked = true;
            }
            comboBox1.SelectedIndex = 0;
            if (comboBox1.SelectedIndex == 1)
                on = 1;
            else
                on = 2;
        }
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;
            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);
            return activeProcId == procId;
        }
        public void getShouts(ShoutReceivedArgs e)                                 //Receive shouts from Darkbox
        {
            foreach (DBShout s in e.Shouts)
            {
                string time;
                if (checkBox4.Checked)                                             //Show the time messages were sent
                   time = $"[{s.Date}] ";
                else
                   time = "";
                Invoke(new MethodInvoker(() => richTextBox1.Text += time + $"{s.Username}: {s.Message}\n"));
                if (!ApplicationIsActivated())                                     //Shows notification if a message is sent
                {
                    if (on == 2)
                        notifyIcon1.ShowBalloonTip(3000);
                }
            }
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;                  //Keep scrolling to incoming messages
            richTextBox1.ScrollToCaret();
        }
        private async void button1_Click(object sender, EventArgs e)                 //Login button
        {
            if (checkBox3.Checked)                                                   //Saves username & password to local variables
            {
                Settings.Default.remember = true;
                Settings.Default.username = textBox3.Text;
                Settings.Default.password = textBox2.Text;
                Settings.Default.Save();
                Settings.Default.Reload();
            }
                sess = new DBSession(textBox3.Text, textBox2.Text);                  //Login with credentials 
                await sess.LoginAsync();
            if (sess.Status == DBStatus.LoggedIn)
            {
                sess.ReceiveShouts = true;
                sess.OnShoutReceived += getShouts;
                MessageBox.Show($"Logged in as: " + textBox3.Text, "Succes");
                pictureBox1.Visible = false;
                groupBox1.Visible = true;
                groupBox2.Visible = false;
                groupBox4.Visible = true;
                groupBox5.Visible = true;
                this.Text = "Darkboy - " + textBox3.Text;
                label7.Visible = false;
                label8.Visible = false;
                timer1.Stop();
            }
            else
                MessageBox.Show("Something went wrong, try again!", "Oops!");                   //Error when logging in
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (sess.Status == DBStatus.LoggedIn)                                               //Send message
            {
                await sess.SendShoutAsync(textBox1.Text);
                textBox1.ResetText();
            }
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)                             
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(this, new EventArgs());
        }
        private void balloonClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
            this.TopMost = false;
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void sKoreButton1_Click(object sender, EventArgs e)                       //Dismiss
        {
            Size = new Size(313, 393);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)                 //HACKER MODE FTW
        {
            if (checkBox1.Checked)
                richTextBox1.ForeColor = Color.Green;   
            else
                richTextBox1.ForeColor = Color.White;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                TopMost = true;
            else
                TopMost = false;
        }
        private void sKoreButton2_Click(object sender, EventArgs e)                       
        {
            groupBox6.Visible = true;
            sKoreButton2.SendToBack();
        }
        private void sKoreButton3_Click(object sender, EventArgs e)                       
        {
            groupBox6.Visible = false;
            sKoreButton3.SendToBack();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)                    //Show send button
        {
            if (checkBox5.Checked)
            {
                textBox1.Size = new Size(156, 20);
                button2.Visible = true;
            }
            else
            {
                textBox1.Size = new Size(228, 20);
                button2.Visible = false;
            }
        }
    }
}