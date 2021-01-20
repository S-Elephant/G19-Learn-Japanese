using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace G19LearnJap
{
    public partial class frmConfig : Form
    {
        private SettingsProp SP = new SettingsProp();
        public frmConfig()
        {
            InitializeComponent();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Text = string.Format("G19 - Learn Japanese - Version: {0}", version);

            pgSettings1.SelectedObject = SP;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSaveClose_Click(object sender, EventArgs e)
        {
            SettingsMgr.Instance.TimerIntervalInMS = SP.TimerIntervalInMS;
            SettingsMgr.Instance.UseTrayIcon = SP.UseTrayIcon;
            SettingsMgr.Instance.UseStaticChoices = SP.UseStaticChoices;
            SettingsMgr.Instance.Modus = LetterMgr.Instance.Modus = SP.Modus;
            SettingsMgr.Instance.Save();
            LetterMgr.Instance.ApplyConfig();
            Close();
        }
    }
}
