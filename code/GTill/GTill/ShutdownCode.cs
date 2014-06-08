using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace GTill
{
    // Code taken from http://bytes.com/topic/c-sharp/answers/251367-shutdown-my-computer-using-c (User: Mark Rae)

    class ShutdownCode
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
        phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name,
        ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        /// <summary>
        /// Calls a shutdown, reboot, logoff etc
        /// </summary>
        /// <param name="flg">The action to take</param>
        private static void DoExitWin(int flg)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero,
            IntPtr.Zero);
            ok = ExitWindowsEx(flg, 0);

        }

        /// <summary>
        /// Shuts down the computer
        /// </summary>
        public static void ShutdownComputer()
        {
            DoExitWin(EWX_SHUTDOWN);
        }

        /// <summary>
        /// Reboots the computer
        /// </summary>
        public static void RebootComputer()
        {
            DoExitWin(EWX_REBOOT);
        }
    }
}

namespace EasterEggs
{

using System.Windows.Forms;
using System.Drawing;

    class SnakeGame : Form
    {
        enum Direction { Up, Right, Down, Left };
        Point[] pTurnPoints;
        Direction[] dTurnedFromDirs;
        Timer tmrUpdater;
        Direction dMoveDir = Direction.Down;
        Point pSnakeHead = new Point(50, 80);
        Point[] pDrawnOn;
        Direction dNextMoveDir;
        int nSnakeLength = 10;
        public int nScore = 0;
        Point pNextFood;

        public SnakeGame(Size sSize, Point pStartLoc)
        {
            this.Size = sSize;
            this.Width -= (this.Width % 10);
            this.Height -= (this.Height % 10);
            this.Location = pStartLoc;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.StartPosition = FormStartPosition.Manual;
            this.KeyDown += new KeyEventHandler(SnakeGame_KeyDown);
            InitialiseVariables();
            this.Paint += new PaintEventHandler(SnakeGame_Paint);
        }

        void SnakeGame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && dMoveDir != Direction.Left)
                dNextMoveDir = Direction.Right;
            else if (e.KeyCode == Keys.Left && dMoveDir != Direction.Right)
                dNextMoveDir = Direction.Left;
            else if (e.KeyCode == Keys.Down && dMoveDir != Direction.Up)
                dNextMoveDir = Direction.Down;
            else if (e.KeyCode == Keys.Up && dMoveDir != Direction.Down)
                dNextMoveDir = Direction.Up;
            else if (e.KeyCode != Keys.Right && e.KeyCode != Keys.Left && e.KeyCode != Keys.Down && e.KeyCode != Keys.Up)
                this.Close();
        }

        void InitialiseVariables()
        {
            dTurnedFromDirs = new Direction[0];

            pTurnPoints = new Point[0];

            tmrUpdater = new Timer();
            tmrUpdater.Interval = 25;
            tmrUpdater.Enabled = true;
            tmrUpdater.Tick += new EventHandler(tmrUpdater_Tick);

            dNextMoveDir = dMoveDir;

            Random r = new Random();
            pNextFood = new Point(r.Next(20, this.Width - 20), r.Next(20, this.Height - 20));
                //new Point(r.Next(-pSnakeHead.X, this.Width - pSnakeHead.X), r.Next(-pSnakeHead.Y, this.Height - pSnakeHead.X));
        }

        void tmrUpdater_Tick(object sender, EventArgs e)
        {
            if (dNextMoveDir != dMoveDir)
            {
                AddTurnPoint();
                dMoveDir = dNextMoveDir;
            }
            switch (dNextMoveDir)
            {
                case Direction.Up:
                    pSnakeHead.Y -= 10;
                    break;
                case Direction.Down:
                    pSnakeHead.Y += 10;
                    break;
                case Direction.Left:
                    pSnakeHead.X -= 10;
                    break;
                case Direction.Right:
                    pSnakeHead.X += 10;
                    break;
            }
            if (pSnakeHead.X >= this.Width)
                pSnakeHead.X = 0;
            else if (pSnakeHead.X <= 0)
                pSnakeHead.X += this.Width;
            if (pSnakeHead.Y >= this.Height)
                pSnakeHead.Y = 0;
            else if (pSnakeHead.Y < 0)
                pSnakeHead.Y += this.Height;

            this.Invalidate();
            if (Collision())
            {
                tmrUpdater.Enabled = false;
                RecordHighScore();
                this.Close();
            }
        }

        void SnakeGame_Paint(object sender, PaintEventArgs e)
        {
            DrawScore(e.Graphics);
            DrawSnake(e.Graphics);
            DrawFoodPiece(e.Graphics);
        }

        void AddTurnPoint()
        {
            Point pToAdd = pSnakeHead;
            Direction dMovingAtTime = dMoveDir;

            Direction[] dPrevArray = dTurnedFromDirs;
            dTurnedFromDirs = new Direction[dPrevArray.Length + 1];
            for (int i = 1; i < dTurnedFromDirs.Length; i++)
            {
                dTurnedFromDirs[i] = dPrevArray[i - 1];
            }
            dTurnedFromDirs[0] = dMovingAtTime;

            Point[] pPrevArray = pTurnPoints;
            pTurnPoints = new Point[pPrevArray.Length + 1];
            for (int i = 1; i < pTurnPoints.Length; i++)
            {
                pTurnPoints[i] = pPrevArray[i - 1];
            }
            pTurnPoints[0] = pToAdd;
        }

        void RemoveLastTurnPoint()
        {
            Point[] pTemp = pTurnPoints;
            pTurnPoints = new Point[pTemp.Length - 1];
            for (int i = 0; i < pTurnPoints.Length; i++)
            {
                pTurnPoints[i] = pTemp[i];
            }

            Direction[] dTemp = dTurnedFromDirs;
            dTurnedFromDirs = new Direction[dTemp.Length - 1];
            for (int i = 0; i < dTurnedFromDirs.Length; i++)
            {
                dTurnedFromDirs[i] = dTemp[i];
            }
        }

        bool Collision()
        {
            for (int i = 0; i < pDrawnOn.Length; i++)
            {
                if (pDrawnOn[i] == pSnakeHead)
                    return true;
            }
            return false;
        }

        public void DrawSnake(Graphics g)
        {
            int nCurrentDrawX = pSnakeHead.X;
            int nCurrentDrawY = pSnakeHead.Y;
            int nCurrentTurnPoint = 0;
            Direction dCurrentDir = dMoveDir;
            Brush b = new SolidBrush(Color.Green);
            pDrawnOn = new Point[nSnakeLength];
            bool bChangedDir = false;
            for (int i = 0; i < nSnakeLength; i++)
            {
                g.FillRectangle(b, nCurrentDrawX - 2, nCurrentDrawY - 2, 9, 9);
                pDrawnOn[i] = new Point(nCurrentDrawX, nCurrentDrawY);

                // Now calculate the next draw position!!
                if (dTurnedFromDirs.Length == 0)
                {
                    switch (dCurrentDir)
                    {
                        case Direction.Down:
                            nCurrentDrawY -= 10;
                            break;
                        case Direction.Up:
                            nCurrentDrawY += 10;
                            break;
                        case Direction.Left:
                            nCurrentDrawX += 10;
                            break;
                        case Direction.Right:
                            nCurrentDrawX -= 10;
                            break;
                    }
                }
                else if (new Point(nCurrentDrawX, nCurrentDrawY) != pTurnPoints[nCurrentTurnPoint])
                {
                    switch (dCurrentDir)
                    {
                        case Direction.Down:
                            nCurrentDrawY -= 10;
                            break;
                        case Direction.Up:
                            nCurrentDrawY += 10;
                            break;
                        case Direction.Left:
                            nCurrentDrawX += 10;
                            break;
                        case Direction.Right:
                            nCurrentDrawX -= 10;
                            break;
                    }
                }
                else
                {
                    bChangedDir = true;
                    switch (dTurnedFromDirs[nCurrentTurnPoint])
                    {
                        case Direction.Right:
                            dCurrentDir = Direction.Right;
                            nCurrentDrawX -= 10;
                            break;
                        case Direction.Down:
                            dCurrentDir = Direction.Down;
                            nCurrentDrawY -= 10;
                            break;
                        case Direction.Up:
                            dCurrentDir = Direction.Up;
                            nCurrentDrawY += 10;
                            break;
                        case Direction.Left:
                            dCurrentDir = Direction.Left;
                            nCurrentDrawX += 10;
                            break;
                    }
                    if (dTurnedFromDirs.Length != nCurrentTurnPoint + 1)
                        nCurrentTurnPoint++;
                }
                if (nCurrentDrawX >= this.Width)
                    nCurrentDrawX -= this.Width;
                else if (nCurrentDrawX <= 0)
                    nCurrentDrawX += this.Width;

                if (nCurrentDrawY >= this.Height)
                    nCurrentDrawY -= this.Height;
                else if (nCurrentDrawY < 0)
                    nCurrentDrawY += this.Height;
            }
            if (nCurrentTurnPoint+1 < dTurnedFromDirs.Length || (dTurnedFromDirs.Length == 1 && bChangedDir == false))
            {
                RemoveLastTurnPoint();
            }
        }

        public void DrawFoodPiece(Graphics g)
        {
            Rectangle rSnakeHead = new Rectangle(pSnakeHead, new Size(10,10));
            Rectangle rFoodPiece = new Rectangle(pNextFood, new Size(10,10));
            if (!rSnakeHead.IntersectsWith(rFoodPiece))
            {
                g.FillEllipse(new SolidBrush(Color.White), new Rectangle(pNextFood, new Size(10, 10)));
            }
            else
            {
                Random r = new Random();
                pNextFood = new Point(r.Next(20, this.Width - 20), r.Next(20, this.Height - 20));
                g.FillEllipse(new SolidBrush(Color.White), new Rectangle(pNextFood, new Size(10, 10)));
                nScore++;
                nSnakeLength += 3;
            }
        }

        public void DrawScore(Graphics g)
        {
            g.DrawString("Score: " + nScore.ToString(), this.Font, new SolidBrush(this.ForeColor), new PointF(0, 0));
        }

        private void RecordHighScore()
        {
            if (this.Visible)
            {
                GTill.frmSingleInputBox fsi = new GTill.frmSingleInputBox("Enter your name to have your score recorded:");
                fsi.ShowDialog();
                if (fsi.Response != "$NONE")
                {
                    TextWriter tWriter = new StreamWriter("slog.txt", true);
                    tWriter.WriteLine(nScore.ToString() + "," + fsi.Response);
                    tWriter.Close();
                }
                MessageBox.Show("Current Top Scores: \n" + GetFiveTopScores());
            }
        }

        private string GetFiveTopScores()
        {
            TextReader tReader = new StreamReader("slog.txt");
            string[] sScores = tReader.ReadToEnd().Split('\n');
            tReader.Close();

            SnakeScore[] Scores = new SnakeScore[sScores.Length - 1];
            for (int i = 0; i < Scores.Length; i++)
            {
                sScores[i] = sScores[i].Replace('\r',' ');
                Scores[i] = new SnakeScore(Convert.ToInt32(sScores[i].Split(',')[0]), sScores[i].Split(',')[1]);
            }
            Array.Sort<SnakeScore>(Scores);
            string sToReturn = "";
            for (int i = 0; i < 5 && i < Scores.Length; i++)
            {
                sToReturn += Scores[i].Name + " - " + Scores[i].Score.ToString() + "\n";
            }

            return sToReturn;

        }

        private class SnakeScore : IComparable
        {
            public string Name;
            public int Score;

            #region IComparable Members

            public int CompareTo(object obj)
            {
                if (Score > ((SnakeScore)obj).Score)
                    return -1;
                else if (Score == ((SnakeScore)obj).Score)
                    return 0;
                else
                    return 1;
            }

            public SnakeScore(int Score, string Name)
            {
                this.Name = Name;
                this.Score = Score;
            }

            #endregion
        }


            
            
    }
}

