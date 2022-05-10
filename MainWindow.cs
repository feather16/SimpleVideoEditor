using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace VideoEditor
{
    partial class MainWindow : Form
    {

        Hashtable buttons = new Hashtable();

        public MainWindow()
        {

            /*FileStream fs = new FileStream(@"sub\log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(fs);
            Console.WriteLine("fs.Length = " + fs.Length);
            fs.Seek(2, System.IO.SeekOrigin.Begin);
            Console.WriteLine(reader.ReadLine());
            Console.WriteLine(reader.ReadLine());*/

            Console.WriteLine(Environment.CurrentDirectory);
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Text = "Video Editor";
            Size = new Size(900, 600);

            buttons["watch"] = new Button()
            {
                Text = "Watch",
                Location = new Point(0, 0),
                Size = new Size(100, 100),
            };
            buttons["merge"] = new Button()
            {
                Text = "Merge",
                Location = new Point(0, 100),
                Size = new Size(100, 100),
            };
            buttons["cut"] = new Button()
            {
                Text = "Cut",
                Location = new Point(0, 200),
                Size = new Size(100, 100),
            };

            ((Button)buttons["watch"]).Click += new EventHandler(watchButtonClicked);
            ((Button)buttons["merge"]).Click += new EventHandler(mergeButtonClicked);
            ((Button)buttons["cut"]).Click += new EventHandler(cutButtonClicked);

            foreach (Button b in buttons.Values)
            {
                b.Font = new Font(b.Font.OriginalFontName, 16);
                Controls.Add(b);
            }

            ResumeLayout(false);
        }

        void watchButtonClicked(object sender, EventArgs e)
        {
            var watchWindow = new WatchWindow();
            watchWindow.ShowDialog(this);
        }

        void mergeButtonClicked(object sender, EventArgs e)
        {
            var mergeWindow = new MergeWindow();
            mergeWindow.ShowDialog(this);
        }

        void cutButtonClicked(object sender, EventArgs e)
        {
            var cutWindow = new CutWindow();
            cutWindow.ShowDialog(this);
        }
    }
}