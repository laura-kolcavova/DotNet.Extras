using System.Drawing;

namespace DotNet.Tools.Graphics
{
    public static class ColorHelper
    {
        /// <summary>
        /// Compares two given <see cref="Color"/>s using Pythagorean distance between the Red, Green and Blue components.
        /// </summary>
        /// <param name="c1">Color 1.</param>
        /// <param name="c2">Color 2.</param>
        /// <returns>Value of how different two given <see cref="Color"/>s are.</returns>
        public static int CompareColours(Color c1, Color c2)
        {
            return (int)(Math.Pow((int)c1.R - c2.R, 2) + Math.Pow((int)c1.B - c2.B, 2) + Math.Pow((int)c1.G - c2.G, 2));
        }

    }
}
