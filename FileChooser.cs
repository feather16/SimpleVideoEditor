using System;
using System.Windows.Forms;

namespace VideoEditor
{
    public class FileChooser
    {

        Form owner;

        public FileChooser(Form owner_)
        {
            owner = owner_;
        }

        public string openVideoFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "mp4 file(*.mp4)|*.mp4|all file(*.*)|*.*";
            ofd.Title = "Open";
            if (ofd.ShowDialog() == DialogResult.OK) // 成功
            {
                Console.WriteLine("File selected " + ofd.FileName);
                return ofd.FileName;
            }
            else // 失敗
            {
                Console.WriteLine("Failed to open a file");
                /*MessageBox.Show(
                    "Failed to open a file.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);*/
                return "";
            }
        }
    }
}
