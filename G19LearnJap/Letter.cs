using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace G19LearnJap
{
    public class Choice
    {
        public string Romanji { get; private set; }
        public Image Hiragana { get; private set; }
        public bool IsCorrect { get; private set; }
        public bool IsRandom { get; private set; }
        public bool HasBeenChosen { get; set; }

        public Choice(string romanji, Image hiragana, bool isCorrect, bool isRandom)
        {
            Romanji = romanji;
            Hiragana = hiragana;
            IsCorrect = isCorrect;
            IsRandom = isRandom && !isCorrect; // make sure it is never random when this is the correct choice
            
            HasBeenChosen = false;
        }

        public Choice(Letter fromThisLetter, bool isRandom)
        {
            Romanji = fromThisLetter.Romanji;
            Hiragana = fromThisLetter.Hiragana;
            IsCorrect = false;
            IsRandom = isRandom;

            HasBeenChosen = false;
        }
    }

    public class Letter
    {
        #region Members

        public string Romanji { get; private set; }
        public Image Hiragana { get; private set; }
        public List<Choice> Choices = new List<Choice>(4);

        public static Dictionary<string, List<string>> StaticChoices = new Dictionary<string, List<string>>();
        #endregion

        public Letter(string romanji)
        {
            Romanji = romanji;
            Hiragana = Image.FromFile(string.Format("{0}{1}.png", LetterMgr.Instance.ActiveDir, romanji));
        }

        /// <summary>
        /// Fills als null-choices with random letters
        /// </summary>
        public void SetChoices()
        {
            Choices.Clear();
            Choices.Add(new Choice(Romanji, Hiragana, true, false));
            List<Letter> availableList = new List<Letter>(LetterMgr.Instance.AllLetters);
            availableList.Remove(this);

            #region static choices
            int nrOfStaticChoices = 0;
            if (SettingsMgr.Instance.UseStaticChoices)
            {
                if (StaticChoices.ContainsKey(Romanji))
                {
                    foreach (string s in StaticChoices[Romanji])
                    {
                        Letter staticLetter = LetterMgr.Instance.AllLetters.Find(l => l.Romanji == s);
                        if (staticLetter == null)
                        {
                            MessageBox.Show(
                                string.Format(
                                    "The static letter \"{0}\" was found in StaticChoices.xml but was not found as an image. Please check your StaticChoices.xml and the chapter in your resource folder.\nThis static choice will not be included until you fix it and restart me.\nThank you.",
                                    s), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        Choices.Add(new Choice(staticLetter, false));
                        availableList.Remove(staticLetter);
                        nrOfStaticChoices++;
                        if (nrOfStaticChoices == 3)
                            break; // Because we only have 4 options.
                    }
                }
            }

            #endregion

            #region random choices
            for (int i = 0; i < 3 - nrOfStaticChoices; i++)
                Choices.Add(GetRandomChoice(ref availableList));
            #endregion

            Choices.Shuffle();
        }

        private Choice GetRandomChoice(ref List<Letter> availableList)
        {
            Letter l;
            do
            {
                l = LetterMgr.Instance.AllLetters.GetRandom();
            } while (l == this || !availableList.Contains(l));

            availableList.Remove(l);
            
            return new Choice(l.Romanji, l.Hiragana, false, true);
        }

        public void HideWrongChoices()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!Choices[i].IsCorrect)
                    Choices[i].HasBeenChosen = true;
            }
        }

        public void ResetChoices()
        {
            Choices.ForEach(c=>c.HasBeenChosen = false);
        }

        /// <summary>
        /// Render itself and the choices. Don't render choices that have already been wrongly chosen.
        /// </summary>
        public void Render(Graphics g)
        {
            if (LetterMgr.Instance.LanguageDirection == LetterMgr.eLanguageDirection.J2E)
            {
                // render center
                g.DrawImage(Hiragana, 110, 70);

                #region render texts
                SizeF textSize;
               
                // Top
                if (!Choices[0].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[0].Romanji, LetterMgr.Font);
                    g.DrawString(Choices[0].Romanji, LetterMgr.Font, Brushes.White, 160 - textSize.Width / 2, 0);
                }

                // Right
                if (!Choices[1].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[1].Romanji, LetterMgr.Font);
                    g.DrawString(Choices[1].Romanji, LetterMgr.Font, Brushes.White, 320 - textSize.Width,
                                 120 - textSize.Height / 2);
                }

                // Bot
                if (!Choices[2].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[2].Romanji, LetterMgr.Font);
                    g.DrawString(Choices[2].Romanji, LetterMgr.Font, Brushes.White, 160 - textSize.Width / 2,
                                 240 - textSize.Height);
                }

                // Left
                if (!Choices[3].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[3].Romanji, LetterMgr.Font);
                    g.DrawString(Choices[3].Romanji, LetterMgr.Font, Brushes.White, 0, 120 - textSize.Height / 2);
                }
                #endregion
            }
            else
            {
                // render center
                SizeF textSize = g.MeasureString(Romanji, LetterMgr.Font);
                g.DrawString(Romanji, LetterMgr.Font, Brushes.White, 160 - textSize.Width / 2, 120 - textSize.Height / 2);
                             
                // Top
                if (!Choices[0].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[0].Romanji, LetterMgr.Font);
                    g.DrawImage(Choices[0].Hiragana, new Rectangle(160 - 45, 0, 90, 90));
                }

                // Right
                if (!Choices[1].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[1].Romanji, LetterMgr.Font);
                    g.DrawImage(Choices[1].Hiragana, new Rectangle(320-90,120-45,90,90));
                }

                // Bot
                if (!Choices[2].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[2].Romanji, LetterMgr.Font);
                    g.DrawImage(Choices[2].Hiragana, new Rectangle(160 - 45, 240-90,90,90));
                }

                // Left
                if (!Choices[3].HasBeenChosen)
                {
                    textSize = g.MeasureString(Choices[3].Romanji, LetterMgr.Font);
                    g.DrawImage(Choices[3].Hiragana, new Rectangle(0,120-45,90,90));
                }
            }
        }
    }
}
