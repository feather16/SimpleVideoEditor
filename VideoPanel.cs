using System;
using System.Drawing;
using System.Windows.Forms;

namespace VideoEditor
{
    class VideoPanel
    {
        public bool enabled = true;

        public Button button;
        public Label pathLabel = new Label();
        public Label timeLabel = new Label();

        public VideoFile video;

        public string Filename
        {
            get
            {
                return video.filename;
            }
        }

        public int Frames
        {
            get
            {
                return video.frames;
            }
        }

        private const int BUTTON_SX = 30;
        private const int BUTTON_SY = 30;

        private const int THUMBNAIL_SX = (int)(MergeWindow.SCREEN_X * 0.26);
        private const int THUMBNAIL_SY = (int)(MergeWindow.SCREEN_Y * 0.28);

        private const int PATH_LABEL_SX = (int)(THUMBNAIL_SX * 0.68);
        private const int PATH_LABEL_SY = THUMBNAIL_SX / 4;

        private const int TIME_LABEL_SX = (int)(THUMBNAIL_SX * 0.3);
        private const int TIME_LABEL_SY = THUMBNAIL_SX / 4;

        private const int PATH_MAXLEN = 18;

        public VideoPanel(Form owner, string filename_)
        {
            video = new VideoFile(filename_);

            // ボタン
            button = new Button()
            {
                Text = "X",
                Location = new System.Drawing.Point(-100, -100),
                Size = new System.Drawing.Size(BUTTON_SX, BUTTON_SY),
            };
            button.Click += delegate (object sender, EventArgs e)
            {
                enabled = false;
                button.Enabled = false;
                button.Visible = false;
                pathLabel.Enabled = false;
                pathLabel.Visible = false;
                timeLabel.Enabled = false;
                timeLabel.Visible = false;
            };
            owner.Controls.Add(button);

            // ラベル
            pathLabel.Text = (video.filename.Length > PATH_MAXLEN) ?
                "..." + video.filename.Substring(video.filename.Length + 3 - PATH_MAXLEN, PATH_MAXLEN - 3) :
                video.filename;
            pathLabel.Font = new Font(pathLabel.Font.OriginalFontName, 10);
            pathLabel.Size = new System.Drawing.Size(PATH_LABEL_SX, PATH_LABEL_SY);
            timeLabel.Text = toTimeString();
            timeLabel.Font = new Font(timeLabel.Font.OriginalFontName, 12);
            timeLabel.Size = new System.Drawing.Size(TIME_LABEL_SX, TIME_LABEL_SY);
            owner.Controls.Add(pathLabel);
            owner.Controls.Add(timeLabel);
        }
        
        public void drawThumbnail(Graphics g, int index, double scrolled)
        {
            const int scrx = MergeWindow.SCREEN_X;
            const int scry = MergeWindow.SCREEN_Y;
            double i = index - scrolled;
            int imlx = (int)((0.06 + 0.295 * i) * scrx);
            const int imly = (int)(0.09 * scry);
            const int imsx = THUMBNAIL_SX;
            const int imsy = THUMBNAIL_SY;
            const int bsx = BUTTON_SX;
            const int bsy = BUTTON_SY;
            int pllx = imlx;
            const int plly = imly + imsy;
            int tllx = imlx + imsx - TIME_LABEL_SX;
            const int tlly = imly + imsy;
            
            button.Location = new System.Drawing.Point(imlx - bsx / 2, imly - bsy / 2);
            g.DrawImage(video.thumbnail, imlx, imly, imsx, imsy);
            pathLabel.Location = new System.Drawing.Point(pllx, plly);
            timeLabel.Location = new System.Drawing.Point(tllx, tlly);
        }

        private string toTimeString() // 初期化時に一度だけ呼び出す
        {
            int iSeconds = (int)video.seconds;
            int min = iSeconds / 60;
            int sec = iSeconds % 60;
            if (min >= 60)
            {
                int h = min / 60;
                min = min % 60;
                return string.Format("{0:0}:{1:00}:{2:00}", h, min, sec); // length is 7
            }
            else
            {
                return string.Format("  {0:00}:{1:00}", min, sec); // length is 7
            }
        }
    }
}
