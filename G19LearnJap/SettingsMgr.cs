using System.IO;
using System.Xml.Linq;
using System.Drawing;

namespace G19LearnJap
{
    public class SettingsMgr
    {
        public static SettingsMgr Instance = new SettingsMgr();
        const string Path = "Settings.xml";
#if XBOX
        IsolatedStorageFile FileStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif

        #region Settings

        public int TimerIntervalInMS = 1250;
        public bool UseTrayIcon = true;
        public bool IsFirstRun = true;
        public bool UseStaticChoices = true;
        public eModus Modus = eModus.OneChance;

        #endregion

        public SettingsMgr()
        {
            if (File.Exists(Path))
                Load();
            else
                Save();
        }
        
        public void Save()
        {
            try
            {
                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("root"));

                XElement settingsNode = new XElement("Settings");
                doc.Root.Add(settingsNode);

                settingsNode.Add(
                    new XElement("TimerIntervalInMS", TimerIntervalInMS),
                    new XElement("UseTrayIcon", UseTrayIcon.ToString()),
                    new XElement("IsFirstRun", IsFirstRun.ToString()),
                    new XElement("UseStaticChoices", UseStaticChoices.ToString()),
                    new XElement("Modus", (int)Modus)
                    );


                // Save
                doc.Save(Path, SaveOptions.None);
            }
            catch { }
        }

        public void Load()
        {
            try
            {
                XDocument doc = XDocument.Load(Path);

                XElement settingsNode = doc.Root.Element("Settings");
                TimerIntervalInMS = int.Parse(settingsNode.Element("TimerIntervalInMS").Value);
                UseTrayIcon = bool.Parse(settingsNode.Element("UseTrayIcon").Value);
                IsFirstRun = bool.Parse(settingsNode.Element("IsFirstRun").Value);
                UseStaticChoices = bool.Parse(settingsNode.Element("UseStaticChoices").Value);
                Modus = (eModus) (int.Parse(settingsNode.Element("Modus").Value));
            }
            catch { }
        }
    }
}
