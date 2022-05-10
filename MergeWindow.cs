using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VideoEditor
{
    class MergeWindow : Form 
    {
        public const int SCREEN_X = 900;
        public const int SCREEN_Y = 600;
        public const int BUTTON_SX = (int)(0.2 * SCREEN_X);
        public const int BUTTON_SY = SCREEN_Y / 10;
        public const int LABEL_SX = (int)(0.24 * SCREEN_X);
        public const int LABEL_SY = SCREEN_Y / 14;
        public const int PROGRESS_BAR_SX = (int)(0.8 * SCREEN_X);
        public const int PROGRESS_BAR_SY = SCREEN_Y / 10;

        public const int PROGRESS_BAR_MAX = 1000;

        public const int TRACK_BAR_MAX = 100;

        List<VideoPanel> mergedVideos = new List<VideoPanel>();
        bool isProcessing = false;

        int sumOfFrames = 0;

        Process process;

        bool suppressMovingThumbnail = false;
        int trackBarValueBefore;

        Button addButton = new Button()
        {
            Text = "Add",
            Location = new Point((int)(SCREEN_X * 0.45 - BUTTON_SX), (int)(SCREEN_Y * 0.8)),
            Size = new Size(BUTTON_SX, BUTTON_SY),
        };
        Button mergeButton = new Button()
        {
            Text = "Merge",
            Location = new Point((int)(SCREEN_X * 0.55), (int)(SCREEN_Y * 0.8)),
            Size = new Size(BUTTON_SX, BUTTON_SY),
        };

        Label label = new Label()
        {
            Location = new Point((int)(SCREEN_X * 0.38), (int)(SCREEN_Y * 0.58)),
            Size = new Size(LABEL_SX, LABEL_SY),
            Visible = false,
        };

        TrackBar trackBar = new TrackBar()
        {
            Location = new Point((int)(SCREEN_X * 0.1), (int)(SCREEN_Y * 0.44)),
            Size = new Size((int)(SCREEN_X * 0.8), (int)(SCREEN_Y * 0.18)),
            Maximum = TRACK_BAR_MAX,
            TickFrequency = 5,
            LargeChange = 10,
            SmallChange = 5,
        };

        ProgressBar progressBar = new ProgressBar()
        {
            Minimum = 0,
            Maximum = PROGRESS_BAR_MAX,
            Value = 0,
            Location = new Point((int)(SCREEN_X * 0.1), (int)(SCREEN_Y * 0.66)),
            Size = new Size(PROGRESS_BAR_SX, PROGRESS_BAR_SY),
            Visible = false,
        };

        public MergeWindow()
        {
            SuspendLayout();

            // ダブルバッファリング設定
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;//

            Text = "Merge - Video Editor";
            Size = new Size(SCREEN_X, SCREEN_Y);

            // ボタン
            addButton.Click += new EventHandler(addButtonClicked);
            mergeButton.Click += new EventHandler(mergeButtonClicked);
            addButton.Font = new Font(addButton.Font.OriginalFontName, 16);
            mergeButton.Font = new Font(mergeButton.Font.OriginalFontName, 16);
            Controls.Add(addButton);
            Controls.Add(mergeButton);

            // タイマー
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(tick);
            timer.Start();

            // トラックバー
            trackBar.Scroll += new EventHandler(trackBarScrolled);
            Controls.Add(trackBar);

            // ラベル
            label.Text = "Merging...      ";
            label.Font = new Font(label.Font.OriginalFontName, 16);
            Controls.Add(label);

            // 進捗バー
            Controls.Add(progressBar);

            // ウィンドウが閉じられたときの処理
            FormClosing += new FormClosingEventHandler(windowClosing);

            ResumeLayout(false);
        }

        private void addButtonClicked(object sender, EventArgs e)
        {
            var chooser = new FileChooser(this);
            var filename = chooser.openVideoFile();

            if (filename != "") // 成功
            {
                mergedVideos.Add(new VideoPanel(this, filename));
            }
        }

        private void mergeButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Merge Button Clicked : mergedVideos.Count = " + mergedVideos.Count);
            if(mergedVideos.Count == 0) // 動画が選択されていない場合
            {
                MessageBox.Show(
                    this,
                    "No videos selected.", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
            else
            {
                var fileNamingDialog = new FileNamingDialog();
                fileNamingDialog.ShowDialog();
                string outputFileName = fileNamingDialog.getText();
                if (outputFileName != "")
                {
                    merge(outputFileName);
                }
            }
        }

        private void merge(string outputFileName)
        {
            progressBar.Value = 0;

            // logの初期化
            try
            {
                using (var sw = new StreamWriter(@"sub/log.txt")) {}
            }
            catch(IOException) {}

            isProcessing = true;

            // フレーム数の総和
            sumOfFrames = 0;
            foreach (var video in mergedVideos)
            {
                sumOfFrames += video.Frames;
            }

            // filelist
            using (var sw = new StreamWriter(@"sub/filelist.txt"))
            {
                foreach (var video in mergedVideos)
                {
                    sw.WriteLine("file '" + video.Filename + "'");
                }
            }

            // ffmpeg
            if (!Directory.Exists("out"))
            {
                Directory.CreateDirectory("out");
            }

            Console.WriteLine("> sub\\execute.bat " + "sub\\ffmpeg -y -f concat -safe 0 -i sub\\filelist.txt -c copy out\\" + outputFileName + " 2> sub\\log.txt");
            var p = new ProcessStartInfo();
            p.FileName = "sub\\execute.bat";
            p.Arguments = "sub\\ffmpeg -y -f concat -safe 0 -i sub\\filelist.txt -c copy out\\" + outputFileName + " 2> sub\\log.txt";

            p.CreateNoWindow = true;
            p.UseShellExecute = false;

            process = Process.Start(p);
            Console.WriteLine("ProcessName = " + process.ProcessName);
            process.SynchronizingObject = this;
            process.Exited += new EventHandler(processExited);
            process.EnableRaisingEvents = true;
        }
		
        private void tick(object sender, EventArgs e)
        {
            addButton.Enabled = !isProcessing;
            mergeButton.Enabled = !isProcessing;
            label.Visible = isProcessing;
            progressBar.Visible = isProcessing;

            if(mergedVideos.Count <= 3)
            {
                trackBar.Enabled = false;
                trackBar.Value = 0;
            }
            else
            {
                trackBar.Enabled = true;
            }

            // 進捗バーの更新
            if (isProcessing)
            {
                string logFilePath = @"sub/log.txt";
                try
                {
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
                    double progressRate = Math.Min((double)maxFrame / sumOfFrames, 1);
                    progressBar.Value = (int)(progressRate * PROGRESS_BAR_MAX);
                    string percentage = ((int)(progressRate * 1000)).ToString("0000");
                    label.Text =
                        "Merging..." +
                        (percentage[0] == '0' ? ' ' : percentage[0]) +
                        (percentage[0] == '0' && percentage[1] == '0' ? ' ' : percentage[1]) +
                        percentage[2] +
                        '.' +
                        percentage[3] +
                        '%';
                    Console.WriteLine("progressRate = " + maxFrame + " / " + sumOfFrames + " = " + progressRate);
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
                label.Text = "Merging...  0.0%";
            }


            for (int i = 0; i < mergedVideos.Count; i++)
            {
                mergedVideos[i].button.Enabled = !isProcessing;
                if (!mergedVideos[i].enabled)
                {
                    mergedVideos.RemoveAt(i);
                    i--;
                }
            }

            Refresh();
        }

        private void trackBarScrolled(object sender, EventArgs e)
        {
            suppressMovingThumbnail = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            int trackBarValue;
            if (suppressMovingThumbnail)
            {
                trackBarValue = trackBarValueBefore;
                suppressMovingThumbnail = false;
            }
            else
            {
                trackBarValueBefore = trackBar.Value;
                trackBarValue = trackBar.Value;
            }

            for (var i = 0; i < mergedVideos.Count; i++)
            {
                mergedVideos[i].drawThumbnail(g, i, (double)trackBarValue / TRACK_BAR_MAX * (mergedVideos.Count - 3));
            }
        }

        private void processExited(object sender, EventArgs e)
        {
            MessageBox.Show(
                this,
                "Done Merging.",
                "Message",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            isProcessing = false;
        }

        private void windowClosing(object sender, FormClosingEventArgs e)
        {
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

    }
}
