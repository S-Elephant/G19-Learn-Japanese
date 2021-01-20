using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace G19LearnJap
{
    public enum eState {Normal, WrongChoice, GoodChoice}

    public enum eModus
    {
        OneChance = 0,
        RepeatTillGood = 1
    }

    public class LetterMgr
    {
        #region Members

        public eState State { get; private set; }
        private Timer StateTimer = new Timer();
        public eModus Modus = SettingsMgr.Instance.Modus;

        public enum eLanguageDirection
        {
            J2E,
            E2J
        }

        public enum eChoice4
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        public eLanguageDirection LanguageDirection { get; set; }

        public static LetterMgr Instance = new LetterMgr();
        public static readonly string LetterDir = string.Format("{0}/Resources/Letters/", Application.StartupPath);
        public List<string> LetterDirs;

        public int TotalLetters { get; private set; }

        public List<Letter> AllLetters;
        public List<Letter> AvailableLetters;
        public List<Letter> ProcessedLetters;

        public Letter ActiveLetter
        {
            get
            {
                if (AvailableLetters.Count > 0)
                    return AvailableLetters[AvailableLetters.Count - 1];
                else
                    return null;
            }
        }

        public string ActiveDir { get { return LetterDirs[DirIDx]; } }
        private int m_DirIdx = 0;
        public int DirIDx
        {
            get { return m_DirIdx; }
            set
            {
                if (value < 0)
                    m_DirIdx = LetterDirs.Count - 1;
                else if (value == LetterDirs.Count)
                    m_DirIdx = 0;
                else
                    m_DirIdx = value;
                frmMain.Instance.RenderLoadingScreen(new DirectoryInfo(ActiveDir).Name);
                LoadDir();
            }
        }

        public int CorrectAnswersGiven { get; private set; }
        public int LettersDone { get; private set; }
        private bool ThisAnswerIsStillCorrect;
        public static Font Font;

        #endregion

        public LetterMgr()
        {
      
        }

        public void LoadDir()
        {
            CorrectAnswersGiven = 0;
            LettersDone = 0;

            #region Static Choices
            
            Letter.StaticChoices = new Dictionary<string, List<string>>();
            string staticChoicesFile = string.Format("{0}/StaticChoices.xml", ActiveDir);
            if (File.Exists(staticChoicesFile))
            {
                XDocument doc = XDocument.Load(staticChoicesFile);
                foreach (XElement key in doc.Root.Element("Keys").Elements())
                {
                    if (!Letter.StaticChoices.ContainsKey(key.Attribute("romaji").Value))
                    {
                        List<string> staticChoices = new List<string>();
                        foreach (XElement staticChoice in key.Elements())
                        {
                            staticChoices.Add(staticChoice.Value);
                        }
                        Letter.StaticChoices.Add(key.Attribute("romaji").Value, staticChoices);

                        XAttribute alsoReverseAttr = key.Attribute("alsoReverse");
                        if (alsoReverseAttr != null && bool.Parse(alsoReverseAttr.Value))
                        {
                            List<string> reverseChoices = new List<string>(staticChoices);
                            reverseChoices.Add(key.Attribute("romaji").Value);

                            foreach (string choice in staticChoices)
                            {
                                if (!Letter.StaticChoices.ContainsKey(choice))
                                {
                                    List<string> temp = new List<string>(reverseChoices);
                                    temp.Remove(choice);
                                    Letter.StaticChoices.Add(choice, temp);
                                }
                                else
                                {
                                    MessageBox.Show(
                                        string.Format("\"{0}\" already exists (reversing). Check your StaticChoices.xml!",
                                          key.Attribute("romaji").Value), "Warnning", MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            string.Format("\"{0}\" already exists. Check your StaticChoices.xml!",
                                          key.Attribute("romaji").Value), "Warnning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                doc = null;
            }

            #endregion

            string[] images = Directory.GetFiles(LetterDirs[DirIDx], "*.png", SearchOption.TopDirectoryOnly);
            TotalLetters = images.Count();
            AllLetters = new List<Letter>(TotalLetters);
            AvailableLetters = new List<Letter>(TotalLetters);
            ProcessedLetters = new List<Letter>(TotalLetters);

            foreach (string image in images)
                AllLetters.Add(new Letter(Path.GetFileNameWithoutExtension(image)));
            AvailableLetters = new List<Letter>(AllLetters);
            RandomizeLetters();
            SetChoices();
            LanguageDirection = eLanguageDirection.J2E;

            ThisAnswerIsStillCorrect = true;
        }

        public void Init()
        {
            State = eState.Normal;
            StateTimer.Interval = SettingsMgr.Instance.TimerIntervalInMS;
            StateTimer.Tick += new EventHandler(StateTimer_Tick);

            LetterDirs = Directory.GetDirectories(LetterDir).ToList(); // "*.png", SearchOption.AllDirectories);
            LetterDirs = new List<string>(LetterDirs);
            for (int i = 0; i < LetterDirs.Count; i++)
                LetterDirs[i] += "/";

            m_DirIdx = 1; // start with syllables. Use the private setter to prevent a second LoadDir() call
            LoadDir();

            #region Font
            if (DMcLgLCD.LGLCD_DEVICE_BW == frmMain.Instance.deviceType)
                Font = new Font("Arial", 12, FontStyle.Regular);
            else
                Font = new Font("Arial", 18, FontStyle.Regular);
            #endregion
        }

        public void ApplyConfig()
        {
            StateTimer.Interval = SettingsMgr.Instance.TimerIntervalInMS;
        }

        void StateTimer_Tick(object sender, EventArgs e)
        {
            switch (State)
            {
                case eState.Normal:
                    throw new Exception("Timer should not tick in state.normal");
                case eState.WrongChoice:
                    SwitchState(eState.Normal);
                    break;
                case eState.GoodChoice:
                    SwitchState(eState.Normal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            frmMain.Instance.Render();
        }

        public void Restart()
        {
            State = eState.Normal;
            StateTimer.Enabled = false;

            AvailableLetters.Clear();
            ProcessedLetters.Clear();

            foreach (Letter letter in AllLetters)
                AvailableLetters.Add(letter);
            RandomizeLetters();
            SetChoices();

            CorrectAnswersGiven = 0;
            LettersDone = 0;
            ThisAnswerIsStillCorrect = true;
        }

        private void SetChoices()
        {
            foreach (Letter letter in AllLetters)
                letter.SetChoices();
        }

        private  void RandomizeLetters()
        {
            AvailableLetters.Shuffle();
        }

        private void NextLetter()
        {
            if (ThisAnswerIsStillCorrect)
                CorrectAnswersGiven++;
            if (Modus == eModus.OneChance || ThisAnswerIsStillCorrect)
                LettersDone++;
        }

        private void ShowScores()
        {
            // TODO:

            Restart();
        }

        public void SwitchState(eState newState)
        {
            switch (newState)
            {
                case eState.Normal:
                    StateTimer.Enabled = false;
                    if(State == eState.GoodChoice)
                    {
                        if (ThisAnswerIsStillCorrect || Modus == eModus.OneChance)
                        {
                            AvailableLetters.RemoveAt(AvailableLetters.Count - 1);
                            if (AvailableLetters.Count == 0)
                                ShowScores();
                        }
                        else
                        {
                            ActiveLetter.ResetChoices();
                            AvailableLetters.Shuffle();
                        }
                        ThisAnswerIsStillCorrect = true;
                    }
                    break;
                case eState.WrongChoice:
                    StateTimer.Stop();
                    StateTimer.Start();
                    StateTimer.Enabled = true;
                    break;
                case eState.GoodChoice:
                    StateTimer.Stop();
                    StateTimer.Start();
                    StateTimer.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("newState");
            }
            State = newState;
        }

        public void Chose(eChoice4 choice)
        {
            Choice chosenChoice = ActiveLetter.Choices[(int) choice];
            if (!chosenChoice.HasBeenChosen)
            {
                if (chosenChoice.IsCorrect)
                {
                    ActiveLetter.HideWrongChoices();
                    NextLetter();
                    SwitchState(eState.GoodChoice);
                }
                else
                {
                    ThisAnswerIsStillCorrect = false;
                    chosenChoice.HasBeenChosen = true;
                    SwitchState(eState.WrongChoice);
                }

                frmMain.Instance.Render();
            }
        }

        public void Render(Graphics g)
        {
            ActiveLetter.Render(g);
        }
    }
}
