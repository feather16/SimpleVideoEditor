using System;
using System.Drawing;
using System.Windows.Forms;

namespace VideoEditor
{
	class FileNamingDialog : Form
    {
        private TextBox textBox;

		string defaultName;
		// mp4 avi wmv mov flv
		
		private const int SCREEN_X = 400;
		private const int SCREEN_Y = 350;

		private GroupBox groupBox = new GroupBox()
		{
			Text = "extension",
			Location = new Point((int)(SCREEN_X * 0.67), (int)(SCREEN_Y * 0.1)),
			Size = new Size((int)(SCREEN_X * 0.25), (int)(SCREEN_Y * 0.5)),
		};
		private RadioButton[] radioButtons = new RadioButton[5];
		private string[] extensions = { "mp4", "avi", "wmv", "mov", "flv" };

		bool canceled = true;

		public FileNamingDialog()
        {
            SuspendLayout();

			defaultName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

			FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
			
			Text = "Name the output file";
            Size = new System.Drawing.Size(SCREEN_X, SCREEN_Y);

			// ボタン
			Button okButton = new Button()
			{
				Text = "OK",
				Location = new Point((int)(SCREEN_X * 0.05), (int)(SCREEN_Y * 0.65)),
				Size = new Size((int)(SCREEN_X * 0.4), (int)(SCREEN_Y * 0.15)),
			};
            Button cancelButton = new Button()
			{
				Text = "Cancel",
				Location = new Point((int)(SCREEN_X * 0.5), (int)(SCREEN_Y * 0.65)),
				Size = new Size((int)(SCREEN_X * 0.4), (int)(SCREEN_Y * 0.15)),
			};
			okButton.Click += new EventHandler(okButtonClicked);
            cancelButton.Click += new EventHandler(cancelButtonClicked);
			okButton.Font = new Font(okButton.Font.OriginalFontName, 14);
            cancelButton.Font = new Font(cancelButton.Font.OriginalFontName, 14);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

			// ラジオボタン
			for(int i = 0; i < radioButtons.Length; i++)
			{
				radioButtons[i] = new RadioButton
				{
					AutoSize = true,
					Location = new Point((int)(SCREEN_X * 0.7), (int)(SCREEN_Y * (0.6 + 0.05 * i))),
					Size = new Size((int)(SCREEN_X * 0.25), (int)(SCREEN_Y * 0.06)),
					Text = "." + extensions[i],
					Font = new Font(Font.OriginalFontName, 11),
					Left = (int)(SCREEN_X * 0.05),
					Top = (int)(SCREEN_Y * (0.07 + 0.08 * i)),
				};
			}
			radioButtons[0].Checked = true;
			groupBox.Font = new Font(groupBox.Font.OriginalFontName, 12);
			groupBox.Controls.AddRange(radioButtons);
			Controls.Add(groupBox);


			// テキストボックス
			textBox = new TextBox()
			{
				Text = defaultName,
				Location = new Point((int)(SCREEN_X * 0.04), (int)(SCREEN_Y * 0.13)),
				Size = new Size((int)(SCREEN_X * 0.6), (int)(SCREEN_Y * 0.2)),
			};
			textBox.Font = new Font(textBox.Font.OriginalFontName, 12);
			Controls.Add(textBox);

			// ウィンドウが閉じられたときの処理
			FormClosing += new FormClosingEventHandler(windowClosing);

			ResumeLayout(false);
        }
		
		private void okButtonClicked(object sender, EventArgs e)
		{
			Close();
			canceled = false;
		}
		
		private void cancelButtonClicked(object sender, EventArgs e)
		{
			Close();
		}

		private void windowClosing(object sender, FormClosingEventArgs e)
		{
		}

		public string getText()
		{
			string extension = "";
			foreach(var b in radioButtons)
			{
				if (b.Checked)
				{
					extension = b.Text;
					break;
				}
			}
			return canceled ? "" : textBox.Text + extension;
		}

	}
}
