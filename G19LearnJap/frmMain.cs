using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace G19LearnJap
{
    public partial class frmMain : Form
    {
        #region Members
        #region LCD
        public int connection = DMcLgLCD.LGLCD_INVALID_CONNECTION;
        public int device = DMcLgLCD.LGLCD_INVALID_DEVICE;
        public int deviceType = DMcLgLCD.LGLCD_INVALID_DEVICE;
        private Bitmap LCD;
        #endregion

        #region Buttons

        private uint buttons = 0;
        private int config = 0;

        #endregion

        #region Tray

        private NotifyIcon TrayIcon = null;
        private ContextMenu TrayMenu;

        #endregion
        
        private string LCDShotFolder;
        private static readonly Image ErrorImg = Image.FromFile(string.Format("{0}/Resources/error.png", Application.StartupPath));
        private static readonly Image CorrectImg = Image.FromFile(string.Format("{0}/Resources/correct.png", Application.StartupPath));
        private static Image LoadingImg;
        private frmConfig ConfigForm;
        public static readonly Rectangle ScreenRect = new Rectangle(0, 0, 320, 240);
        public static frmMain Instance;
        Font GFont;

        #endregion

        public frmMain()
        {
            LoadingImg = Image.FromFile(string.Format("{0}/Resources/loading.png", Application.StartupPath));
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (DMcLgLCD.ERROR_SUCCESS == DMcLgLCD.LcdInit())
            {
                Instance = this;
                LetterMgr.Instance.Init();
                
                SettingsMgr.Instance.Load();
                if (SettingsMgr.Instance.IsFirstRun)
                {
                    SettingsMgr.Instance.IsFirstRun = false;
                    SettingsMgr.Instance.Save();
                    try
                    {
                        AutoStart.SetAutoStart();
                    }
                    catch
                    {}
                }

                LCDShotFolder = string.Format("{0}/LCD Shots/", Application.StartupPath);
                if (!Directory.Exists(LCDShotFolder))
                    Directory.CreateDirectory(LCDShotFolder);

                Visible = false;
                ShowInTaskbar = false;

                #region Tray
                if (SettingsMgr.Instance.UseTrayIcon)
                {
                    TrayMenu = new ContextMenu();
                    TrayMenu.MenuItems.Add("Take LCD &Shot", OnLCDShot);
                    TrayMenu.MenuItems.Add("&Config", OnConfig);
                    TrayMenu.MenuItems.Add("-");
                    TrayMenu.MenuItems.Add("&Exit", OnExit);
                    TrayIcon = new NotifyIcon();
                    TrayIcon.Text = "Learn Japanese";

                    TrayIcon.Icon = new Icon(Application.StartupPath + "/Icon.ico", 32, 32);
                    
                    // Add menu to tray icon and show it.
                    TrayIcon.ContextMenu = TrayMenu;
                    TrayIcon.Visible = true;
                }
                #endregion

                #region Font

                if (DMcLgLCD.LGLCD_DEVICE_BW == deviceType)
                    GFont = new Font("Arial", 5, FontStyle.Regular);
                else
                    GFont = new Font("Arial", 12, FontStyle.Regular);
                #endregion

                connection = DMcLgLCD.LcdConnectEx("Learn Japanese", 0, 0);

                if (DMcLgLCD.LGLCD_INVALID_CONNECTION != connection)
                {
                    device = DMcLgLCD.LcdOpenByType(connection, DMcLgLCD.LGLCD_DEVICE_QVGA);

                    if (DMcLgLCD.LGLCD_INVALID_DEVICE == device)
                    {
                        device = DMcLgLCD.LcdOpenByType(connection, DMcLgLCD.LGLCD_DEVICE_BW);
                        if (DMcLgLCD.LGLCD_INVALID_DEVICE != device)
                        {
                            deviceType = DMcLgLCD.LGLCD_DEVICE_BW;
                        }
                    }
                    else
                    {
                        deviceType = DMcLgLCD.LGLCD_DEVICE_QVGA;
                    }

                    if (DMcLgLCD.LGLCD_DEVICE_BW == deviceType)
                    {
                        LCD = new Bitmap(160, 43);
                        Graphics g = Graphics.FromImage(LCD);
                        g.Clear(Color.White);
                        g.Dispose();

                        DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_BW);
                        DMcLgLCD.LcdSetAsLCDForegroundApp(device, DMcLgLCD.LGLCD_FORE_YES);
                    }
                    else
                    {
                        LCD = new Bitmap(320, 240);
                        Graphics g = Graphics.FromImage(LCD);
                        g.Clear(Color.White);
                        g.Dispose();

                        DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_QVGA);
                        DMcLgLCD.LcdSetAsLCDForegroundApp(device, DMcLgLCD.LGLCD_FORE_YES);
                    }

                    if (deviceType > 0)
                    {
                        //commented out in favor of polling routine.
                        //DMcLgLCD.LcdSetButtonCallback(btnCallback);
                        DMcLgLCD.LcdSetConfigCallback(cfgCallback);
                        //The fastest you should send updates to the LCD is around 30fps or 34ms.  100ms is probably a good typical update speed.
                        timInput.Enabled = true;
                    }
                    timWorkaround.Enabled = true; // sometimes the LCD just shows a white screen for some reason on startup. this 100ms delay fixes that.
                }
            }
            else
            {
                MessageBox.Show("Error: The G-keyboard was not detected. The application will not close.",
                                "Hardware not found.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        #region Tray menu
        private void OnLCDShot(object sender, EventArgs e)
        {
            if (LCD != null)
                LCD.Save(string.Format("{0}LCD Shot-{1:yyyy-MM-dd_hh-mm-ss-tt}.png", LCDShotFolder, DateTime.Now), ImageFormat.Png);
        }
        private void OnConfig(object sender, EventArgs e)
        {
           ConfigForm = new frmConfig();
            ConfigForm.Show();
        }
        private void OnExit(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Render
        public void RenderLoadingScreen(string loadText)
        {
            Graphics g = Graphics.FromImage(LCD);
            g.DrawImage(LoadingImg,ScreenRect);
            SizeF measure = g.MeasureString(loadText, GFont);
            g.DrawString(loadText, GFont, Brushes.White, 160 - measure.Width/2, 185);
            g.Dispose();

            #region Send to LCD

            if (DMcLgLCD.LGLCD_DEVICE_BW == deviceType)
            {
                DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_BW);
            }
            else
            {
                DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_QVGA);
            }

            #endregion
        }

        public void Render()
        {
            #region Clear and draw background

            Graphics g = Graphics.FromImage(LCD);
            g.Clear(Color.Black);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            #endregion

            #region Draw to the screen
            LetterMgr.Instance.Render(g);
            g.DrawString(string.Format("{0}/{1}/{2}", LetterMgr.Instance.CorrectAnswersGiven, LetterMgr.Instance.LettersDone, LetterMgr.Instance.TotalLetters), GFont, Brushes.White, 0, 0);

            if (LetterMgr.Instance.State == eState.WrongChoice)
                g.DrawImage(ErrorImg, ScreenRect);
            else if (LetterMgr.Instance.State == eState.GoodChoice)
                g.DrawImage(CorrectImg, new Rectangle(40,0,240,240));
            #endregion

            #region Send to LCD

            if (DMcLgLCD.LGLCD_DEVICE_BW == deviceType)
            {
                DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_BW);
            }
            else
            {
                DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_QVGA);
            }

            #endregion

            #region Clean up

            g.Dispose();

            #endregion
        }

        #endregion

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (LCD != null)
            {
                GFont.Dispose();                
                LoadingImg.Dispose();
                
                LCD.Dispose();
                DMcLgLCD.LcdClose(device);
                DMcLgLCD.LcdDisconnect(connection);
                DMcLgLCD.LcdDeInit();
            }

            // Release the icon resource.
            if (SettingsMgr.Instance.UseTrayIcon)
            {
                if (TrayIcon != null)
                    TrayIcon.Dispose();
            }
        }

        #region Callbacks
        public void btnCallback(int deviceType, int dwButtons)
        {
            buttons = (uint)dwButtons;
        }

        public void cfgCallback(int cfgConnection)
        {
            config = cfgConnection;
        }
        #endregion

        #region Input
        private void timInput_Tick(object sender, EventArgs e)
        {
            if (ConfigForm == null || !ConfigForm.Visible)
            {
                buttons = DMcLgLCD.LcdReadSoftButtons(device);

                if (LetterMgr.Instance.State == eState.Normal)
                {
                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_LEFT) == DMcLgLCD.LGLCD_BUTTON_LEFT)
                    {
                        LetterMgr.Instance.Chose(LetterMgr.eChoice4.Left);
                    }
                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_RIGHT) == DMcLgLCD.LGLCD_BUTTON_RIGHT)
                    {
                        LetterMgr.Instance.Chose(LetterMgr.eChoice4.Right);
                    }
                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_UP) == DMcLgLCD.LGLCD_BUTTON_UP)
                    {
                        LetterMgr.Instance.Chose(LetterMgr.eChoice4.Up);
                    }
                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_DOWN) == DMcLgLCD.LGLCD_BUTTON_DOWN)
                    {
                        LetterMgr.Instance.Chose(LetterMgr.eChoice4.Down);
                    }
                    
                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_CANCEL) == DMcLgLCD.LGLCD_BUTTON_CANCEL)
                    {
                        LetterMgr.Instance.DirIDx++;
                        Render();
                    }

                    if ((buttons & DMcLgLCD.LGLCD_BUTTON_OK) == DMcLgLCD.LGLCD_BUTTON_OK)
                    {
                        if (LetterMgr.Instance.LanguageDirection == LetterMgr.eLanguageDirection.J2E)
                            LetterMgr.Instance.LanguageDirection = LetterMgr.eLanguageDirection.E2J;
                        else
                            LetterMgr.Instance.LanguageDirection = LetterMgr.eLanguageDirection.J2E;

                        Render();
                    }
                }
            }
        }
        #endregion

        private void timWorkaround_Tick(object sender, EventArgs e)
        {
            Render();
            timWorkaround.Dispose();
        }
    }
}
