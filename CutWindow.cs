using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VideoEditor
{
    class CutWindow : Form
    {
        private AxWMPLib.AxWindowsMediaPlayer wmp;

        public const int SCREEN_X = 900;
        public const int SCREEN_Y = 700;
		
		// 0 : ファイル選択
		// 1 : 切り取り始め選択
		// 2 : 切り取り終わり選択
		// 3 : 切り取り
		private int mode = 0;
        private VideoFile video;
        private double cutBeginPos = 0;
        private double cutEndPos = 0;

        public const int PROGRESS_BAR_MAX = 1000;
        Process process;
        ProgressBar progressBar = new ProgressBar()
        {
            Minimum = 0,
            Maximum = PROGRESS_BAR_MAX,
            Value = 0,
            Location = new Point((int)(SCREEN_X * 0.1), (int)(SCREEN_Y * 0.4)),
            Size = new Size((int)(SCREEN_X * 0.8), (int)(SCREEN_Y * 0.1)),
            Visible = false,
        };

        Label label = new Label()
        {
            Location = new Point((int)(SCREEN_X * 0.38), (int)(SCREEN_Y * 0.58)),
            Size = new Size((int)(SCREEN_X * 0.24), (int)(SCREEN_Y * 0.08)),
            Visible = false,
        };

        // ボタン
        Button openButton = new Button()
        {
            Text = "Open",
            Location = new Point(270, 600),
            Size = new Size(160, 60),
        };
		Button nextButton = new Button()
        {
            Text = "Next",
            Location = new Point(470, 600),
            Size = new Size(160, 60),
			Enabled = false,
        };
		Button backButton = new Button()
        {
            Text = "Back",
            Location = new Point(270, 600),
            Size = new Size(160, 60),
			Visible = false,
        };

        public CutWindow()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            wmp = new AxWMPLib.AxWindowsMediaPlayer();
            SuspendLayout();

            // ダブルバッファリング設定
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // AxWindowsMediaPlayer
            wmp.Enabled = true;
            wmp.Location = new Point(0, 0);
            wmp.Name = "wmp";
            wmp.Size = new Size(884, 500);
            wmp.TabIndex = 0;


            // WatchWindow
            Controls.Add(wmp);
            Name = "CutWindow";

            // ボタン
            openButton.Click += new EventHandler(openButtonClicked);
            openButton.Font = new Font(openButton.Font.OriginalFontName, 16);
            Controls.Add(openButton);
			nextButton.Click += new EventHandler(nextButtonClicked);
            nextButton.Font = new Font(nextButton.Font.OriginalFontName, 16);
            Controls.Add(nextButton);
			backButton.Click += new EventHandler(backButtonClicked);
            backButton.Font = new Font(backButton.Font.OriginalFontName, 16);
            Controls.Add(backButton);

            // 進捗バー
            Controls.Add(progressBar);

            // ラベル
            label.Text = "Cutting...      ";
            label.Font = new Font(label.Font.OriginalFontName, 16);
            Controls.Add(label);

            // タイマー
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(tick);
            timer.Start();

            // ウィンドウが閉じられたときの処理
            FormClosing += new FormClosingEventHandler(windowClosing);

            Text = "Cut - Video Editor";
            Size = new Size(SCREEN_X, SCREEN_Y);

            ResumeLayout(false);
        }

        private void tick(object sender, EventArgs e)
        {
            openButton.Visible = mode == 0 || mode == 1;
            nextButton.Enabled = mode == 1 || mode == 2 && cutBeginPos < getCurrentPos();
            backButton.Visible = mode == 2 || mode == 3;
            backButton.Enabled = mode == 2;
            label.Visible = mode == 3;
            progressBar.Visible = mode == 3;
            wmp.Visible = mode != 3;

            // 進捗バーの更新
            if (mode == 3)
            {
                string logFilePath = @"sub/log.txt";
                try
                {
                    int frames = (int)((cutEndPos - cutBeginPos) * video.seconds * 30);

                    FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    StreamReader reader = new StreamReader(fs);

                    int maxFrame = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // frame=が記載されている
                        if (line.Length >= 11 && line.Substring(0, 6) == "frame=")
                        {
                            int frameLength;
                            if (line.IndexOf("fps") == -1) frameLength = 5;
                            else frameLength = line.IndexOf("fps") - 7;

                            int frame = int.Parse(line.Substring(6, frameLength));
                            maxFrame = Math.Max(frame, maxFrame);
                        }
                    }
                    Console.WriteLine("maxFrame = " + maxFrame);
                    double progressRate = Math.Min((double)maxFrame / frames, 1);
                    progressBar.Value = (int)(progressRate * PROGRESS_BAR_MAX);
                    string percentage = ((int)(progressRate * 1000)).ToString("0000");
                    label.Text =
                        "Cutting..." +
                        (percentage[0] == '0' ? ' ' : percentage[0]) +
                        (percentage[0] == '0' && percentage[1] == '0' ? ' ' : percentage[1]) +
                        percentage[2] +
                        '.' +
                        percentage[3] +
                        '%';
                    Console.WriteLine("progressRate = " + maxFrame + " / " + frames + " = " + progressRate);
                    Console.WriteLine("percentage = " + percentage);
                    Console.WriteLine("label.Text = " + label.Text);
                }
                catch (IOException excp)
                {
                    Console.WriteLine(excp.Message);
                }
            }
            else
            {
                progressBar.Value = 0;
                label.Text = "Cutting...  0.0%";
            }

            Refresh();
        }

        const int ARROW_X_MIN = 29;
        const int ARROW_X_MAX = 839;
        const int ARROW_Y = 498;
        const string DEFAULT_FONT = "Arial";
        const int ARROW_FONT_SIZE = 14;
        const string ARROW = "↑";

        protected override void OnPaint(PaintEventArgs e)
        {	
            if(mode == 1)
            {
                drawBeginArrow(e.Graphics, getCurrentPos());
                drawMessage(e.Graphics, "Select the begin point of cutting");
            }
            else if(mode == 2)
            {
                drawBeginArrow(e.Graphics, cutBeginPos);
                drawEndArrow(e.Graphics, getCurrentPos());
                drawMessage(e.Graphics, "Select the end point of cutting");
            }
        }

        private void drawBeginArrow(Graphics g, double pos) // 0 ≤ pos ≤ 1
        {
            int x = (int)(ARROW_X_MAX * pos + ARROW_X_MIN * (1 - pos));
            g.DrawString(
                ARROW,
                new Font(DEFAULT_FONT, ARROW_FONT_SIZE),
                new SolidBrush(Color.Red),
                x,
                ARROW_Y
                );
            g.DrawString(
                "begin",
                new Font(DEFAULT_FONT, ARROW_FONT_SIZE),
                new SolidBrush(Color.Red),
                x - ARROW_FONT_SIZE * 5 / 4,
                ARROW_Y + ARROW_FONT_SIZE * 3 / 2
                ) ;
        }

        private void drawEndArrow(Graphics g, double pos) // 0 ≤ pos ≤ 1
        {
            int x = (int)(ARROW_X_MAX * pos + ARROW_X_MIN * (1 - pos));
            g.DrawString(
                ARROW,
                new Font(DEFAULT_FONT, ARROW_FONT_SIZE),
                new SolidBrush(Color.Blue),
                x,
                ARROW_Y
                );
            g.DrawString(
                "end",
                new Font(DEFAULT_FONT, ARROW_FONT_SIZE),
                new SolidBrush(Color.Blue),
                x - ARROW_FONT_SIZE * 3 / 4,
                ARROW_Y + ARROW_FONT_SIZE * 3 / 2
                );
        }

        private void drawMessage(Graphics g, string message)
        {
            g.DrawString(
                message,
                new Font(DEFAULT_FONT, 16),
                new SolidBrush(Color.Black),
                270,
                550
                );
        }

        private void windowClosing(object sender, FormClosingEventArgs e)
        {
            wmp.close();

            try
            {
                Console.WriteLine("try to kill : ProcessName = " + process.ProcessName);
                process.Kill();
                Console.WriteLine("killed : ProcessName = " + process.ProcessName);
            }
            catch (Exception excp)
            {
                Console.WriteLine("falied to kill : " + excp.Message);
            }
        }

        private void openButtonClicked(object sender, EventArgs e)
        {

            Console.WriteLine("wmp.Ctlcontrols.currentPosition = " + wmp.Ctlcontrols.currentPosition);

            SuspendLayout();
            wmp.Ctlcontrols.pause();
            ResumeLayout(false);

            FileChooser chooser = new FileChooser(this);
            string filename = chooser.openVideoFile();

            if (filename != "") // 成功
            {
                SuspendLayout();
                wmp.URL = filename;
                Text = filename + " - Cut - Video Editor";
                ResumeLayout(false);

                mode = 1;
                video = new VideoFile(filename);
            }
        }
		
		private void nextButtonClicked(object sender, EventArgs e)
        {
            double currentPos = getCurrentPos();

            if(mode == 1)
            {
                cutBeginPos = currentPos;
                mode++;
            }
            else if(mode == 2 && cutBeginPos < currentPos)
            {

                // 動画の再生を一時停止
                SuspendLayout();
                wmp.Ctlcontrols.pause();
                ResumeLayout(false);

                var fileNamingDialog = new FileNamingDialog();
                fileNamingDialog.ShowDialog();
                string outputFileName = fileNamingDialog.getText();
                if (outputFileName != "")
                {
                    cutEndPos = currentPos;
                    cut(outputFileName);
                }
            }
        }
		
		private void backButtonClicked(object sender, EventArgs e)
        {
			if(mode == 2)
            {
                //
            }
            else if(mode == 3)
            {
                //
            }
            mode--;
        }

        private void cut(string outputFileName)
        {
            progressBar.Value = 0;

            // logの初期化
            try
            {
                using (var sw = new StreamWriter(@"sub/log.txt")) { }
            }
            catch (IOException) { }

            mode = 3;

            // ffmpeg
            if (!Directory.Exists("out"))
            {
                Directory.CreateDirectory("out");
            }

            double startSecond = cutBeginPos * video.seconds;
            double seconds = (cutEndPos - cutBeginPos) * video.seconds;

            // ffmpeg -ss [開始地点(秒)] -i [入力する動画パス] -t [切り出す秒数] [出力する動画パス]
            Console.WriteLine("sub\\ffmpeg -ss " + startSecond + " -i " + wmp.URL + " -t " + seconds + " out\\" + outputFileName + " 2> sub\\log.txt");
            var p = new ProcessStartInfo();
            p.FileName = "sub\\execute.bat";
            p.Arguments = "sub\\ffmpeg -ss " + startSecond + " -i \"" + wmp.URL + "\" -t " + seconds + " out\\" + outputFileName + " 2> sub\\log.txt";

            p.CreateNoWindow = true;
            p.UseShellExecute = false;

            process = Process.Start(p);
            Console.WriteLine("ProcessName = " + process.ProcessName);
            process.SynchronizingObject = this;
            process.Exited += new EventHandler(processExited);
            process.EnableRaisingEvents = true;
        }

        private double getCurrentPos() // [0, 1]
        {
            return wmp.Ctlcontrols.currentPosition / video.seconds;
        }

        private void processExited(object sender, EventArgs e)
        {
            MessageBox.Show(
                this,
                "Done Cutting.",
                "Message",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            mode = 2;
        }

    }
}
