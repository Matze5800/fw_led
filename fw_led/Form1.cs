using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace fw_led
{
    public partial class fw_led : Form
    {
        private bool disabled_by_sleep = false;

        public PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

        decimal diskAct;

        [DllImport("fw_dll", EntryPoint = "cmd_led")]
        static extern void cmd_led(int state);

        public fw_led()
        {
            InitializeComponent();
            Shown += Form_Shown;
            diskCounter.BeginInit();
            loadSettings();
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    if (checkBox4.Checked)
                    {
                        disabled_by_sleep = true;
                        disable();
                    }
                    break;
                case SessionSwitchReason.SessionUnlock:
                    if(disabled_by_sleep)
                    {
                        enable();
                        disabled_by_sleep = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.min_startup)
            {
                this.WindowState = FormWindowState.Minimized;
                if (Settings.Default.min_to_tray)
                {
                    this.Hide();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            diskAct = (decimal)diskCounter.NextValue();
            toolStripStatusLabel1.Text = "Storage Activity: " + diskAct.ToString("000.00");
            if (diskAct < numericUpDown3.Value)
            {
                cmd_led(comboBox1.SelectedIndex);
            }
            if (numericUpDown3.Value <= diskAct && diskAct <= numericUpDown1.Value)
            {
                cmd_led(comboBox2.SelectedIndex);
            }
            if (numericUpDown1.Value <= diskAct && diskAct <= numericUpDown2.Value)
            {
                cmd_led(comboBox3.SelectedIndex);
            }
            if (diskAct > numericUpDown2.Value)
            {
                cmd_led(comboBox4.SelectedIndex);
            }

        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            label4.Text = "Color between " + numericUpDown1.Value.ToString() + " and";
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            label5.Text = "Color above " + numericUpDown2.Value.ToString();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown4.Value;
        }

        private void loadSettings()
        {
            numericUpDown4.Value = Settings.Default.interval;
            numericUpDown3.Value = Settings.Default.idle_threshold;
            comboBox1.SelectedIndex = Settings.Default.idle_color;
            numericUpDown1.Value = Settings.Default.threshold1;
            comboBox2.SelectedIndex = Settings.Default.threshold1_color;
            numericUpDown2.Value = Settings.Default.threshold2;
            comboBox3.SelectedIndex = Settings.Default.threshold2_color;
            comboBox4.SelectedIndex = Settings.Default.max_color;
            checkBox1.Checked = Settings.Default.min_to_tray;
            checkBox3.Checked = Settings.Default.min_startup;
            checkBox4.Checked = Settings.Default.standby_disable;
            if (TaskService.Instance.FindTask($"fw_led") == null)
            {
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }
            toolStripStatusLabel2.Text = "Settings loaded.";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            numericUpDown4.Value = 100;
            numericUpDown3.Value = 0.01M;
            comboBox1.SelectedIndex = 0;
            numericUpDown1.Value = 100;
            comboBox2.SelectedIndex = 2;
            numericUpDown2.Value = 500;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 1;
            checkBox1.Checked = true;
            checkBox3.Checked = false;
            checkBox4.Checked = true;
            toolStripStatusLabel2.Text = "Default settings restored.";
            MessageBox.Show("The default settings have been restored.\n\nPlease click save if you also want to overwrite the settings file!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings.Default.Reload();
            loadSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Default.interval = numericUpDown4.Value;
            Settings.Default.idle_threshold = numericUpDown3.Value;
            Settings.Default.idle_color = comboBox1.SelectedIndex;
            Settings.Default.threshold1 = numericUpDown1.Value;
            Settings.Default.threshold1_color = comboBox2.SelectedIndex;
            Settings.Default.threshold2 = numericUpDown2.Value;
            Settings.Default.threshold2_color = comboBox3.SelectedIndex;
            Settings.Default.max_color = comboBox4.SelectedIndex;
            Settings.Default.min_to_tray = checkBox1.Checked;
            Settings.Default.min_startup = checkBox3.Checked;
            Settings.Default.standby_disable = checkBox4.Checked;
            Settings.Default.Save();
            toolStripStatusLabel2.Text = "Settings saved.";
        }

        private void fw_led_SizeChanged(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized && checkBox1.Checked)
            {
                this.Hide();
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                notifyIcon1.Visible = true;
            }
            else
            {
                notifyIcon1.Visible = false;
            }
        }

        public void DeleteTask(string name)
        {
            using (TaskService ts = new TaskService())
            {
                if (ts.GetTask(name) != null)
                {
                    ts.RootFolder.DeleteTask(name);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();

                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    td.RegistrationInfo.Description = $"Run fw_led on startup.";
                    td.Triggers.Add(new LogonTrigger());
                    td.Actions.Add(new ExecAction(Application.ExecutablePath, null));

                    ts.RootFolder.RegisterTaskDefinition($"fw_led", td);
                    toolStripStatusLabel2.Text = "Startup task created.";
                }
            }
            else
            {
                DeleteTask("fw_led");
                toolStripStatusLabel2.Text = "Startup task removed.";
            }
        }

        private void enable()
        {
            timer1.Enabled = true;
            button4.Enabled = false;
            button5.Enabled = true;
            toolStripStatusLabel2.Text = "Enabled.";
        }

        private void disable()
        {
            timer1.Enabled = false;
            button4.Enabled = true;
            button5.Enabled = false;
            cmd_led(4);
            toolStripStatusLabel2.Text = "Disabled. LED in auto-mode.";
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enable();
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disable();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            enable();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            disable();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}