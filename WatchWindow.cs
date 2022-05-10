using System;
using System.Drawing;
using System.Windows.Forms;

namespace VideoEditor
{
    class WatchWindow : Form
    {
        private AxWMPLib.AxWindowsMediaPlayer wmp;

        public const int SCREEN_X = 900;
        public const int SCREEN_Y = 600;

        public WatchWindow()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            wmp = new AxWMPLib.AxWindowsMediaPlayer();
            SuspendLayout();

            // AxWindowsMediaPlayer
            wmp.Enabled = true;
            wmp.Location = new Point(0, 0);
            wmp.Name = "wmp";
            wmp.Size = new Size(884, 500);
            wmp.TabIndex = 0;

            // WatchWindow
            Controls.Add(wmp);
            Name = "WatchWindow";

            // ボタン
            Button openButton = new Button()
            {
                Text = "Open",
                Location = new Point(370, 500),
                Size = new Size(160, 60),
            };

            openButton.Click += new EventHandler(openButtonClicked);
            openButton.Font = new Font(openButton.Font.OriginalFontName, 16);
            Controls.Add(openButton);

            // ウィンドウが閉じられたときの処理
            FormClosing += new FormClosingEventHandler(windowClosing);

            Text = "Watch - Video Editor";
            Size = new Size(SCREEN_X, SCREEN_Y);

            ResumeLayout(false);
        }


        private void windowClosing(object sender, FormClosingEventArgs e)
        {
            wmp.close();
        }

        private void openButtonClicked(object sender, EventArgs e)
        {

            SuspendLayout();
            wmp.Ctlcontrols.pause();
            ResumeLayout(false);

            FileChooser chooser = new FileChooser(this);
            string filename = chooser.openVideoFile();

            if (filename != "") // 成功
            {
                SuspendLayout();
                wmp.URL = filename;
                Text = filename + " - Watch - Video Editor";
                ResumeLayout(false);
            }
        }
    }
}
