using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRage.Utils;
using VRageMath;

namespace avaness.LcdImagePlugin
{
    [HarmonyPatch(typeof(MyTextPanel), "CreateTerminalControls")]
    public class Patch_CreateTerminalControls
    {
        private static bool controls = true;
        private static string filter;

        private static string GetFilter()
        {
            if (filter != null)
                return filter;

            StringBuilder sb = new StringBuilder();
            sb.Append("Image Files|");
            foreach(var codec in ImageCodecInfo.GetImageEncoders())
                sb.Append(codec.FilenameExtension).Append(';');
            filter = sb.ToString();
            return filter;
        }

        public static void Prefix()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyTextPanel>())
                controls = false;
        }

        public static void Postfix()
        {
            if(!controls)
            {
                MyTerminalControlSeparator<MyTextPanel> sep = new MyTerminalControlSeparator<MyTextPanel>();
                MyTerminalControlFactory.AddControl(sep);
                MyTerminalControlButton<MyTextPanel> panel = new MyTerminalControlButton<MyTextPanel>("OpenFile", MyStringId.GetOrCompute("Open File"), MyStringId.GetOrCompute("Open an image and convert it to characters."), ButtonAction);
                MyTerminalControlFactory.AddControl(panel);
                controls = true;
            }
        }

        private static void ButtonAction(MyTextPanel panel)
        {
            Form form = GetMainForm();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = GetFilter();
            if (dialog.ShowDialog(form) == DialogResult.OK && File.Exists(dialog.FileName))
            {
                try
                {
                    IMyTextSurface surface = panel;
                    surface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    surface.FontSize = panel.BlockDefinition.MinFontSize;
                    surface.Font = "Monospace";
                    surface.TextPadding = 0;
                    WriteImage(Resize(Image.FromFile(dialog.FileName), GetPanelSize(panel)), panel);
                }
                catch (Exception e)
                {
                    MessageBox.Show(GetMainForm(), "Error while processing image: " + e.ToString());
                }
            }
        }

        private static Size GetPanelSize(MyTextPanel panel)
        {
            switch (panel.BlockDefinition.Id.SubtypeName)
            {
                case "LargeLCDPanel5x3":
                case "LargeTextPanel":
                    return new Size(178, 107);
                case "SmallLCDPanelWide":
                case "LargeLCDPanelWide":
                    return new Size(356, 178);
                default:
                    return new Size(178, 178);
            }
        }

        private static Form GetMainForm()
        {
            if (Application.OpenForms.Count > 0)
                return Application.OpenForms[0];
            else
                return new Form { TopMost = true };
        }

        private static Bitmap Resize(Image img, Size size)
        {
            return new Bitmap(img, size);
        }

        private static void WriteImage(Bitmap img, MyTextPanel panel)
        {
            StringBuilder sb = new StringBuilder();

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, img.Width, img.Height);
            BitmapData data = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int size = data.Stride * img.Height;
            byte[] bytes = new byte[size];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            img.UnlockBits(data);

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    byte blue = bytes[(y * data.Stride) + x * 3]; //B
                    byte green = bytes[(y * data.Stride) + (x * 3) + 1]; //G
                    byte red = bytes[(y * data.Stride) + (x * 3) + 2]; //R
                    sb.Append(GetPixel(red, green, blue));
                }
                sb.AppendLine();
            }

            ((IMyTextSurface)panel).WriteText(sb);
        }

        private static char GetPixel(int r, int g, int b)
        {
            return (char)(0xe100 + ((r >> 5) << 6) + ((g >> 5) << 3) + (b >> 5));
        }
    }
}
