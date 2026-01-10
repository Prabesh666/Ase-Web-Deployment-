using System;
using System.Diagnostics;
using System.Drawing;
using BOOSE;

namespace MyWebApp.Boose
{
    /// <summary>
    /// Canvas implementation used by BOOSE commands to draw shapes,
    /// lines, text, and manage pen state inside a bitmap.
    /// </summary>
    public class canvasApp : ICanvas
    {
        private int xPos, yPos;
        private int XCanvasSize, YCanvasSize;

        private Color penColour;
        private Pen Pen;
        private int penSize = 5;

        private const int XSIZE = 640;
        private const int YSIZE = 480;

        private Bitmap bm;
        private Graphics g;

        /// <summary>
        /// Creates a new canvas with default size and initializes the drawing surface.
        /// </summary>
        public canvasApp()
        {
            try
            {
                Debug.WriteLine(AboutBOOSE.about());

                bm = new Bitmap(XSIZE, YSIZE);
                g = Graphics.FromImage(bm);

                Set(XSIZE, YSIZE);

                penColour = Color.Red;
                Pen = new Pen(penColour, penSize);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing canvasApp: {ex.Message}");
                throw new InvalidOperationException("Failed to initialize canvasApp.", ex);
            }
        }

        /// <summary>
        /// Current X drawing coordinate.
        /// </summary>
        public int Xpos { get => xPos; set => xPos = value; }

        /// <summary>
        /// Current Y drawing coordinate.
        /// </summary>
        public int Ypos { get => yPos; set => yPos = value; }

        /// <summary>
        /// Current pen colour used for drawing.
        /// </summary>
        public object PenColour
        {
            get => penColour;
            set
            {
                penColour = (Color)value;
                Pen = new Pen(penColour, penSize);
            }
        }

        /// <summary>
        /// Draws a circle using the current pen settings.
        /// </summary>
        /// <param name="radius">Circle radius.</param>
        /// <param name="filled">Whether the circle is filled.</param>
        public void Circle(int radius, bool filled)
        {
            if (radius <= 0)
                throw new CanvasException("Radius must be greater than zero.");

            if (filled)
                g.FillEllipse(new SolidBrush(penColour), xPos - radius, yPos - radius, radius * 2, radius * 2);
            else
                g.DrawEllipse(Pen, xPos - radius, yPos - radius, radius * 2, radius * 2);
        }

        /// <summary>
        /// Clears the canvas to a light gray background.
        /// </summary>
        public void Clear()
        {
            g.Clear(Color.LightGray);
        }

        /// <summary>
        /// Draws a line from the current position to a target coordinate.
        /// </summary>
        /// <param name="toX">Target X coordinate.</param>
        /// <param name="toY">Target Y coordinate.</param>
        public void DrawTo(int toX, int toY)
        {
            if (toX < 0 || toX > XCanvasSize || toY < 0 || toY > YCanvasSize)
                throw new CanvasException($"Invalid screen position DrawTo: {toX}, {toY}");

            g.DrawLine(Pen, xPos, yPos, toX, toY);

            xPos = toX;
            yPos = toY;
        }

        /// <summary>
        /// Moves the cursor to a new position without drawing.
        /// </summary>
        /// <param name="x">New X coordinate.</param>
        /// <param name="y">New Y coordinate.</param>
        public void MoveTo(int x, int y)
        {
            if (x < 0 || x > XCanvasSize || y < 0 || y > YCanvasSize)
                throw new CanvasException($"Invalid screen position MoveTo: {x}, {y}");

            xPos = x;
            yPos = y;
        }

        /// <summary>
        /// Draws a rectangle from the current position.
        /// </summary>
        /// <param name="width">Rectangle width.</param>
        /// <param name="height">Rectangle height.</param>
        /// <param name="filled">Whether the rectangle is filled.</param>
        public void Rect(int width, int height, bool filled)
        {
            if (width < 0 || height < 0)
                throw new CanvasException($"Invalid rectangle dimensions: {width}, {height}");

            if (filled)
                g.FillRectangle(new SolidBrush(penColour), xPos, yPos, width, height);
            else
                g.DrawRectangle(Pen, xPos, yPos, width, height);
        }

        /// <summary>
        /// Resets the cursor position to origin (0,0).
        /// </summary>
        public void Reset()
        {
            xPos = 0;
            yPos = 0;
        }

        /// <summary>
        /// Sets canvas size and resets cursor position.
        /// </summary>
        /// <param name="xsize">Canvas width.</param>
        /// <param name="ysize">Canvas height.</param>
        public void Set(int xsize, int ysize)
        {
            XCanvasSize = xsize;
            YCanvasSize = ysize;

            xPos = 0;
            yPos = 0;
        }

        /// <summary>
        /// Changes the drawing pen colour.
        /// </summary>
        /// <param name="red">Red component (0-255).</param>
        /// <param name="green">Green component (0-255).</param>
        /// <param name="blue">Blue component (0-255).</param>
        public void SetColour(int red, int green, int blue)
        {
            if (red > 255 || green > 255 || blue > 255)
                throw new CanvasException("Invalid colour value!");

            penColour = Color.FromArgb(red, green, blue);
            Pen = new Pen(penColour, penSize);
        }

        /// <summary>
        /// Draws a triangle from the current position.
        /// </summary>
        /// <param name="width">Triangle base width.</param>
        /// <param name="height">Triangle height.</param>
        public void Tri(int width, int height)
        {
            if (width < 0 || height < 0)
                throw new CanvasException("Invalid triangle dimensions");

            Point[] points =
            {
                new Point(xPos, yPos),
                new Point(xPos - width / 2, yPos + height),
                new Point(xPos + width / 2, yPos + height)
            };

            g.DrawPolygon(Pen, points);
        }

        /// <summary>
        /// Renders a text string at the current canvas position.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        public void WriteText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                g.DrawString(text, new Font("Arial", 12), new SolidBrush(penColour), xPos, yPos);
        }

        /// <summary>
        /// Returns the internal bitmap used for drawing.
        /// </summary>
        /// <returns>Bitmap object.</returns>
        public object getBitmap()
        {
            if (bm == null) throw new CanvasException("Bitmap not initialized.");
            return bm;
        }
    }
}
