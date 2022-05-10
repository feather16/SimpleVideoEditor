using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;

namespace VideoEditor
{
    class VideoFile
    {
        public double fps;
        public int frames;
        public double seconds;
        public Image thumbnail;

        public string filename;

        public VideoFile(string filename_)
        {
            filename = filename_;

            VideoCapture cap = new VideoCapture(filename);
            fps = cap.Fps;
            frames = cap.FrameCount;
            seconds = frames / fps;

            var mat = new Mat();
            cap.Read(mat);
            thumbnail = BitmapConverter.ToBitmap(mat);
        }
    }
}
