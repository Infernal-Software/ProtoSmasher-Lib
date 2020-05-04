using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ProtoSmasher_Lib.Events
{
    public class Output
    {
        public readonly Color Color;
        public readonly bool NewLine;
        public readonly string Message;

        private Color ConvertColor(uint color)
        {
            var R = color & 255;
            var G = (color >> 8) & 255;
            var B = (color >> 16) & 255;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }

        public Output(dynamic body)
        {
            var rawColor = ((uint?)body["Color"]).GetValueOrDefault(0xFFFFFFFF);

            Color = ConvertColor(rawColor);
            Message = (string)body["Message"];
            NewLine = (bool)body["NewLine"];
        }
    }
}
