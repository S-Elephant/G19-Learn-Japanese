using System.ComponentModel;
using System.Drawing;

namespace G19LearnJap
{
    [DefaultPropertyAttribute("TimerIntervalInMS")]
    public class SettingsProp
    {
        private bool m_UseTrayIcon = SettingsMgr.Instance.UseTrayIcon;
        [CategoryAttribute("General"), DescriptionAttribute("Whether this program uses a tray icon.")]
        public bool UseTrayIcon { get { return m_UseTrayIcon; } set { m_UseTrayIcon = value; } }

        private int m_TimerIntervalInMS = SettingsMgr.Instance.TimerIntervalInMS;
        [CategoryAttribute("General"), DescriptionAttribute("The green check / red cross display time in milliseconds.")]
        public int TimerIntervalInMS { get { return m_TimerIntervalInMS; } set { m_TimerIntervalInMS = value; } }

        [CategoryAttribute("General"), DescriptionAttribute("Whether this application starts automatically with Windows.")]
        public bool AutoStartWithWindows { get { return AutoStart.IsAutoStartEnabled(); } set
        {
            if(value)
                AutoStart.SetAutoStart();
            else
                AutoStart.UnSetAutoStart();
        } }

        private bool m_UseStaticChoices = SettingsMgr.Instance.UseStaticChoices;
        [CategoryAttribute("General"), DescriptionAttribute("Whether you would like to use some predefined answers or just everything random.")]
        public bool UseStaticChoices { get { return m_UseStaticChoices; } set { m_UseStaticChoices = value; } }

        private eModus m_Modus = SettingsMgr.Instance.Modus;
        [CategoryAttribute("General"), DescriptionAttribute("OneChance: You will see every letter just once no matter what.\n RepeatTillGood: Any wrongly answered questions will be put back in the pool to answer again untill you do it right.")]
        public eModus Modus { get { return m_Modus; } set { m_Modus = value; } }
    }
}
