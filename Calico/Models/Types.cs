using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.Models
{

    public class ImageConfig
    {
        public string Path { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class RepeatImageConfig
    {
        public string Path { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Repeat { get; set; }
        public int GapWidth { get; set; }
    }

    public class LineConfig
    {
        public int X0 { get; set; }
        public int Y0 { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int Size { get; set; }
    }
    public class RectConfig
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Color { get; set; }
        public int BorderSize { get; set; }
    }

    public class TextConfig
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public string Text { get; set; }
        public string Font { get; set; }
        public int Textsize { get; set; }
        public bool Bold { get; set; }
        public string Align { get; set; }
    }

    public class PicConfig
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string BackColor { get; set; }
        public int BorderSize { get; set; }
        public List<ImageConfig> Images { get; set; }
        public List<RepeatImageConfig> RepeatImages { get; set; }
        public List<LineConfig> Lines { get; set; }
        public List<TextConfig> Texts { get; set; }
        public List<RectConfig> Rects { get; set; }
    }

    public class DataConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BgColor { get; set; }

        public Dictionary<string, string> Dts { get; set; }

    }
}
