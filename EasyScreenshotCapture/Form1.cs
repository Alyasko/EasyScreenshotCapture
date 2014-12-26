using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyScreenshotCapture
{
    public partial class Form1 : Form
    {
        Utilities.GlobalKeyboardHook hook = new Utilities.GlobalKeyboardHook();

        string save_path = "";
        bool hotkeys_reassigning = false;

        Queue<int> q_hotkeys = new Queue<int>();
        List<int> hotkeys_save_screenshot = new List<int>();
        System.Media.SoundPlayer sound_player = new System.Media.SoundPlayer();

        List<int> l_hotkeys_register = new List<int>();

        public Form1()
        {
            InitializeComponent();

            // Hotkeys to hook
            hotkeys_save_screenshot.Add((int)Keys.LControlKey);
            hotkeys_save_screenshot.Add((int)Keys.PrintScreen);
            // ---------------

            hook.KeyDown += hook_KeyDown;
            hook.KeyUp += hook_KeyUp;

            notifyIcon1.Icon = SystemIcons.WinLogo;
        }

        void hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (l_hotkeys_register.Contains(e.KeyValue))
            {
                l_hotkeys_register.Remove(e.KeyValue);
            }
            lvHistory.Items.Add("KEY_UP");
            lvHistory.Items[lvHistory.Items.Count - 1].SubItems.Add(e.KeyData.ToString());
            lvHistory.Items[lvHistory.Items.Count - 1].SubItems.Add(e.KeyValue.ToString());

            lvHistory.EnsureVisible(lvHistory.Items.Count - 1);

            lblStatusCounter.Text = String.Format("Status counter: " + l_hotkeys_register.Count.ToString());

            if (l_hotkeys_register.Count == 0)
            {
                ProcessHotkeys();
            }
        }

        private void ProcessHotkeys()
        {
            bool handled = true;
            if (hotkeys_reassigning == false)
            {
                if (q_hotkeys.Count == hotkeys_save_screenshot.Count)
                {
                    for (int i = 0; q_hotkeys.Count != 0; i++ )
                    {
                        if (q_hotkeys.Dequeue() != hotkeys_save_screenshot[i])
                        {
                            handled = false;
                            break;
                        }
                    }

                    if (handled)
                    {
                        sound_player.Play();
                        SaveScreenshot(save_path);

                        lvMessages.Items.Add("Screen captured!");
                        lvMessages.EnsureVisible(lvMessages.Items.Count - 1);
                        notifyIcon1.ShowBalloonTip(1000, "Information", "Screen captured!", ToolTipIcon.Info);
                    }
                }
            }
            else
            {
                hotkeys_save_screenshot.Clear();
                while (q_hotkeys.Count != 0)
                {
                    hotkeys_save_screenshot.Add(q_hotkeys.Dequeue());
                }
                hotkeys_reassigning = false;
                lblInfo.Visible = false;
                btnReassign.Enabled = true;
            }
            q_hotkeys.Clear();
        }

        void hook_KeyDown(object sender, KeyEventArgs e)
        {
            lvHistory.Items.Add("Key DOWN: " + e.KeyValue.ToString());
            lvHistory.Items[lvHistory.Items.Count - 1].SubItems.Add(e.KeyData.ToString());
            lvHistory.Items[lvHistory.Items.Count - 1].SubItems.Add(e.KeyValue.ToString());
            lvHistory.EnsureVisible(lvHistory.Items.Count - 1);

            if (!q_hotkeys.Contains((int)e.KeyCode))
            {
                if(!l_hotkeys_register.Contains(e.KeyValue))
                {
                    l_hotkeys_register.Add(e.KeyValue);
                }
                q_hotkeys.Enqueue((int)e.KeyCode);
                lvQueue.Items.Clear();
                foreach(int i in q_hotkeys)
                {
                    lvQueue.Items.Add(i.ToString());
                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    Show();
                    this.WindowState = FormWindowState.Normal;
                }
            }
        }

        public void SaveScreenshot(string path)
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            bmp.Save(path + String.Format("//SCREEN{0}{1}{2}{3}{4}{5}.png", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
        }

        private void btnHideToTray_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            save_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            tbPath.Text = save_path;

            sound_player.SoundLocation = "Data//screenshot.wav";
            sound_player.Load();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                save_path = folderBrowserDialog1.SelectedPath;
                tbPath.Text = save_path;
            }
        }

        private void btnReassign_Click(object sender, EventArgs e)
        {
            hotkeys_reassigning = true;
            lblInfo.Visible = true;
            btnReassign.Enabled = false;
        }
    }
}
