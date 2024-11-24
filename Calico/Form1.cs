using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Calico.Models;
using System.Drawing.Imaging;
using static System.Windows.Forms.AxHost;

namespace Calico
{
    public partial class Form1 : Form
    {
        private PicConfig config;
        private List<DataConfig> datas;

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            textBoxScript.Text = File.ReadAllText("samp.yaml");

            // 设置 Deserializer
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // 解析 YAML 字符串为 Config 对象
            config = deserializer.Deserialize<Calico.Models.PicConfig>(textBoxScript.Text);
            datas = deserializer.Deserialize<List<DataConfig>>(File.ReadAllText("data.yaml"));

            foreach(var data in datas)
            {
                listViewItems.Items.Add(string.Format("{0}.{1}", data.Id, data.Name));
            }
            listViewItems.SelectedIndices.Clear();
            if (listViewItems.Items.Count > 0)
                listViewItems.SelectedIndices.Add(0);
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Paint(object sender, PaintEventArgs e)
        {
            if (config == null || listViewItems.SelectedItems.Count < 1)
                return;

            var listviewItem = listViewItems.SelectedItems[0];
            var idStr = listviewItem.Text.Split('.')[0];
            DataConfig dataConfig = datas.Find(a => a.Id == int.Parse(idStr));
            Print(e.Graphics, dataConfig, 50, 50);
        }

        private void Print(Graphics g, DataConfig dataConfig, int xoff, int yoff)
        {
            var bgColorStr = config.BackColor;
            if (bgColorStr.Contains("$bgcolor"))
                bgColorStr = bgColorStr.Replace("$bgcolor", dataConfig.BgColor);
            var bgColor = FormatColor(bgColorStr);
            using (var b = new SolidBrush(bgColor))
                g.FillRectangle(b, xoff, yoff, config.Width, config.Height);

            // 绘制图像
            if (config.Images != null)
            {
                foreach (var imageConfig in config.Images)
                {
                    var path = imageConfig.Path;
                    if (path.Contains("$picpath"))
                        path = path.Replace("$picpath", dataConfig.Dts["picpath"]);

                    var imgWidth = imageConfig.Width;
                    var imgHeight = imageConfig.Height;
                    if (imgWidth > config.Width)
                        imgWidth = config.Width;
                    if (imgHeight > config.Height)
                        imgHeight = config.Height;

                    // 假设你有一个方法可以根据路径加载图像
                    var image = LoadImage("images/" + path);
                    var originalWidth = image.Width;
                    var originalHeight = image.Height;

                    var whRateImg = (double)originalWidth / originalHeight;
                    var whRateBG = (double)imgWidth / imgHeight;

                    // 计算缩放比例，以保持图像的宽高比
                    double scaleX = (double)imgWidth / originalWidth;
                    double scaleY = (double)imgHeight / originalHeight;
                    double scale = whRateImg > whRateBG ? scaleY : scaleX; // 选择较小的比例以适应目标区域

                    // 计算裁剪区域（如果需要的话）
                    int startX = 0, startY = 0;
                    int cropWidth = originalWidth, cropHeight = originalHeight;

                    if (whRateImg > whRateBG)
                    {
                        // 图像更宽，需要裁剪宽度
                        cropWidth = (int)(cropHeight * whRateBG);
                        startX = (originalWidth - cropWidth) / 2; // 水平居中裁剪区域
                    }
                    else
                    {
                        // 图像更高或等比，需要裁剪高度（或实际上可能不需要裁剪，但这里保持一致性）
                        cropHeight = (int)(cropWidth / whRateBG);
                        startY = (originalHeight - cropHeight) / 2; // 垂直居中裁剪区域
                    }


                    g.DrawImage(image, new Rectangle(xoff + imageConfig.X, yoff + imageConfig.Y, imgWidth, imgHeight), new Rectangle(startX, startY, cropWidth, cropHeight), GraphicsUnit.Pixel);



                }
            }

            // 绘制图像
            if (config.RepeatImages != null)
            {
                foreach (var imageConfig in config.RepeatImages)
                {
                    var path = imageConfig.Path;
                    var imgWidth = imageConfig.Width;
                    var imgHeight = imageConfig.Height;
                    var repeat = imageConfig.Repeat;
                    if (repeat.StartsWith("$"))
                        repeat = dataConfig.Dts[repeat.Substring(1)];
                    var iRepeat = int.Parse(repeat);

                    var image = LoadImage("images/" + path);
                    for (var i = 0; i < iRepeat; i++)
                    {
                        if (image != null)
                        {
                            g.DrawImage(image, new Rectangle(imageConfig.X + xoff + i * (imageConfig.GapWidth + imageConfig.Width), imageConfig.Y + yoff, imgWidth, imgHeight));
                        }
                    }
                }
            }

            if (config.Rects != null)
            {
                foreach (var rectConfig in config.Rects)
                {
                    var color = FormatColor(rectConfig.Color);
                    using (var pen = new SolidBrush(color))
                    {
                        g.FillRectangle(pen, rectConfig.X + xoff, rectConfig.Y + yoff, rectConfig.Width, rectConfig.Height);
                    }
                    if (rectConfig.BorderSize > 0)
                        using (var pen = new Pen(Color.Black, rectConfig.BorderSize))
                            g.DrawRectangle(pen, rectConfig.X + xoff, rectConfig.Y + yoff, rectConfig.Width, rectConfig.Height);
                }
            }

            using (Pen pen = new Pen(Color.Black, config.BorderSize))
                g.DrawRectangle(pen, xoff, yoff, config.Width, config.Height);
            // 绘制线条
            if (config.Lines != null)
            {
                foreach (var lineConfig in config.Lines)
                {
                    using (Pen pen = new Pen(Color.Black, lineConfig.Size))
                    {
                        var startX = lineConfig.X0;
                        var startY = lineConfig.Y0;
                        if (startX > config.Width)
                            startX = config.Width;
                        if (startY > config.Height)
                            startY = config.Height;
                        var endX = lineConfig.X1;
                        var endY = lineConfig.Y1;
                        if (endX > config.Width)
                            endX = config.Width;
                        if (endY > config.Height)
                            endY = config.Height;
                        g.DrawLine(pen, startX + xoff, startY + yoff, endX + xoff, endY + yoff);
                    }
                }
            }

            // 绘制文本
            if (config.Texts != null)
            {
                foreach (var textConfig in config.Texts)
                {
                    using (Font font = new Font(textConfig.Font, textConfig.Textsize, textConfig.Bold ? FontStyle.Bold : FontStyle.Regular))
                    {
                        // 根据对齐方式调整文本绘制的位置
                        StringFormat stringFormat = new StringFormat();
                        switch (textConfig.Align.ToLower())
                        {
                            case "left":
                                stringFormat.Alignment = StringAlignment.Near;
                                break;
                            case "center":
                                stringFormat.Alignment = StringAlignment.Center;
                                break;
                            case "right":
                                stringFormat.Alignment = StringAlignment.Far;
                                break;
                        }

                        var text = textConfig.Text;
                        if (text.Contains("$name"))
                            text = text.Replace("$name", dataConfig.Name);
                        if (text.Contains("$id"))
                            text = text.Replace("$id", dataConfig.Id.ToString("00"));
                        if (text.StartsWith("$"))
                            text = dataConfig.Dts[text.Substring(1)];

                        // 绘制文本
                        g.DrawString(text, font, Brushes.Black, new RectangleF(textConfig.X + xoff, textConfig.Y + yoff, textConfig.Width, font.Height), stringFormat);
                    }
                }
            }
        }

        // 假设的 LoadImage 方法，你需要根据实际情况实现它
        private System.Drawing.Image LoadImage(string path)
        {
            // 这里应该是加载图像的逻辑，比如从文件、资源等
            // 返回加载的图像对象，如果加载失败则返回 null
            return System.Drawing.Image.FromFile(path); // 仅为示例，实际使用时需要处理异常和错误情况
        }

        private Color FormatColor(string rgbString)
        {
            // 分割字符串并转换为整数数组
            string[] rgbComponents = rgbString.Split(',');
            int r = int.Parse(rgbComponents[0]);
            int g = int.Parse(rgbComponents[1]);
            int b = int.Parse(rgbComponents[2]);

            // 使用FromArgb方法创建Color对象
            return Color.FromArgb(r, g, b);
        }

        private void listViewItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabPage2.Invalidate();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            var bitmap = new Bitmap(config.Width*4, config.Height*3);
            var g = Graphics.FromImage(bitmap);
            int id = 0;
            int pngId = 101;
            foreach (var cfg in datas)
            {
                Print(g, cfg, (id%4)*config.Width, (id/4)*config.Height);
                id++;
                if (id == 12)
                {
                    id = 0;
                    pngId++;
                    bitmap.Save("output/" + pngId + ".png", ImageFormat.Png);
                    g.Clear(Color.White);
                }
            }
            if(id > 0)
            {
                pngId++;
                bitmap.Save("output/" + pngId + ".png", ImageFormat.Png);
            }

            g.Dispose();
            bitmap.Dispose();
        }
    }
}
