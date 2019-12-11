// components required to run the code
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace UR2Lab10
{
    public partial class Form1 : Form
    {
        // create threads and class level variables
        VideoCapture _capture; // part of camera function
        Thread _captureThread;
        SerialPort arduinoSerial = new SerialPort(); // set up for serial communication
        bool enableCoordinateSending = true;
        Thread serialMonitoringThread;
        int rStart;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // create the capture object and processing thread
            _capture = new VideoCapture(1);
            _captureThread = new Thread(ProcessImage);
            _captureThread.Start();
            
            try
            { // sets up and checks serial communication with error protection
                arduinoSerial.PortName = "COM4";
                arduinoSerial.BaudRate = 115200;
                arduinoSerial.Open();
                serialMonitoringThread = new Thread(MonitorSerialData);
                serialMonitoringThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Initializing COM port");
                Close();
            }
        }

        private void MonitorSerialData()
        {
            while (true)
            {
                // block until \n character is received, extract command data
                string msg = arduinoSerial.ReadLine();
                // confirm the string has both < and > characters
                if (msg.IndexOf("<") == -1 || msg.IndexOf(">") == -1)
                {
                    continue;
                }
                // remove everything before the < character
                msg = msg.Substring(msg.IndexOf("<") + 1);
                // remove everything after the > character
                msg = msg.Remove(msg.IndexOf(">"));
                // if the resulting string is empty, disregard and move on
                if (msg.Length == 0)
                {
                    continue;
                }
                // parse the command
                if (msg.Substring(0, 1) == "S")
                {
                    // command is to suspend, toggle states accordingly:
                    ToggleFieldAvailability(msg.Substring(1, 1) == "1");
                }
            }
        }

        private void ToggleFieldAvailability(bool suspend)
        {
            Invoke(new Action(() =>
            {
                enableCoordinateSending = !suspend; // suspend serial communication while Arduino is busy
            }));
        }

        private void ProcessImage()
        {
            while (_capture.IsOpened)
            {
                // frame maintenance
                Mat sourceFrame = _capture.QueryFrame();
                // resize to PictureBox aspect ratio
                int newHeight = (sourceFrame.Size.Height * sourcePictureBox.Size.Width) / sourceFrame.Size.Width;
                Size newSize = new Size(sourcePictureBox.Size.Width, newHeight);
                CvInvoke.Resize(sourceFrame, sourceFrame, newSize);
                // display the image in the source PictureBox
                sourcePictureBox.Image = sourceFrame.Bitmap;

                Mat sourceFrameWithArt = sourceFrame.Clone(); // Primary image used in shape detection

                // special image variables for creating the different filtered and decorated images
                var binaryImage = sourceFrame.ToImage<Gray, byte>().ThresholdBinary(new Gray(125), new Gray(255)).Mat;
                var blurredImage = new Mat();
                var cannyImage = new Mat();
                var decoratedImage = new Mat();

                CvInvoke.GaussianBlur(sourceFrame, blurredImage, new Size(9, 9), 0);
                // convert to B/W
                CvInvoke.CvtColor(blurredImage, blurredImage, typeof(Bgr), typeof(Gray));
                // apply canny:
                // NOTE: Canny function can frequently create duplicate lines on the same shape
                // depending on blur amount and threshold values, some tweaking might be needed.
                // You might also find that not using Canny and instead using FindContours on
                // a binary-threshold image is more accurate.
                CvInvoke.Canny(blurredImage, cannyImage, 150, 255);
                // make a copy of the canny image, convert it to color for decorating:

                CvInvoke.CvtColor(cannyImage, decoratedImage, typeof(Gray), typeof(Bgr));

                // find contours:
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                using (VectorOfPoint approxContour = new VectorOfPoint())
                {
                    // Build list of contours
                    CvInvoke.FindContours(cannyImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                    // Build list of contours
                    CvInvoke.FindContours(binaryImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                    // Selecting largest contour
                    if (contours.Size > 0)
                    {
                        double maxArea = 0;
                        int chosen = 0;
                        int shapes = 0;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            // creates contours and points so a bounding box to enclose shapes can be created
                            VectorOfPoint contour = contours[i];
                            double area = CvInvoke.ContourArea(contour);
                            Rectangle boundingBox = CvInvoke.BoundingRectangle(contours[i]);

                            // Draw on the display frame
                            MarkDetectedObject(sourceFrameWithArt, contours[i], boundingBox, area);
                            // Creates a special blacked out frame with only the desired conturs shown
                            ContourImage(decoratedImage, contours[i], boundingBox, area);

                            // Part of the contour creation to identify shapes
                            if (area > maxArea)
                            {
                                maxArea = area;
                                chosen = i;
                            }
                            shapes = i;
                        }

                        // Applies the finished images to the output frames
                        contourBox.Image = decoratedImage.Bitmap;
                        roiPictureBox.Image = sourceFrameWithArt.Bitmap;
                    }
                }

                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    // A function that uses the contours to count the number of triangles and squares based on counting filtered contours
                    CvInvoke.FindContours(binaryImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    int sCount = 0;
                    int tCount = 0;
                    bool isRectangle = true;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        using (VectorOfPoint approxContour = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.10, true);
                            if (CvInvoke.ContourArea(approxContour, false) > 200 && CvInvoke.ContourArea(approxContour, false) < 4500) //only consider contours with area greater than 250
                            {
                                if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                                {
                                    tCount++;
                                }
                                if (approxContour.Size == 4) //The contour has 4 vertices, it is a square
                                {
                                    if (isRectangle == true)
                                        sCount++;
                                }

                            // Output for the labels for the triangle and square count
                            }
                            Invoke(new Action(() =>
                            {
                                squareCount.Text = ($"{sCount} squares detected");
                            }));
                            Invoke(new Action(() =>
                            {
                                triangleCount.Text = ($"{tCount} triangles detected");
                            }));
                        }
                    }
                }
            }
        }
        private void MarkDetectedObject(Mat frame, VectorOfPoint contour, Rectangle boundingBox, double area)
        {
            // This does most of the work of the program. It tracks the contours and their centerpoints and sends that info to the Arduino
            using (VectorOfPoint approxContour = new VectorOfPoint())
            {
                // variables for creating, filtering, and detecting shapes
                bool isRectangle = true;
                Point[] pts = approxContour.ToArray();
                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.10, true);
                // get an array of points in the contour

                if (contour.Size > 20 && area < 5000) // filters out shapes that are too small or too big from the image
                {
                    if (rStart == 1) // tracks if the start button is pushed
                    {
                        Invoke(new Action(() =>
                        {
                            onOffLabel.Text = ($"On");
                        }));
                    }
                    if (rStart == 0)
                    {
                        Invoke(new Action(() =>
                        {
                            onOffLabel.Text = ($"Off");
                        }));
                    }
                    // Drawing contour and box around it
                    if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                    {
                        CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Green).MCvScalar); // color of shape
                        CvInvoke.Rectangle(frame, boundingBox, new Bgr(Color.Cyan).MCvScalar); // color of bounding box
                    }
                    else if (approxContour.Size == 4) //The contour has 4 vertices, it is a square
                    {
                        for (int j = 0; j < edges.Length; j++)
                        {
                            double angle = Math.Abs(
                               edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                            if (angle < 80 || angle > 100) // verifies the angle on squares
                            {
                                isRectangle = false;
                            }
                        }

                        if (isRectangle == true)
                        {
                            CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Red).MCvScalar); // color of shape
                            CvInvoke.Rectangle(frame, boundingBox, new Bgr(Color.Cyan).MCvScalar); // color of bounding box
                        }
                    }
                    else
                    {

                    }

                    // Write information next to marked object
                    Point center = new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2); // finds center point of bounding box
                    var info = new string[] // creates the output for center point info on image
                    {
                        $"Area: {area}",
                        $"X Pos: {center.X / 3} of {frame.Width / 3}",
                        $"Y Pos: {center.Y / 3} of {frame.Height / 3}"
                    };

                    WriteMultilineText(frame, info, new Point(center.X, boundingBox.Bottom - 12)); // function that actually adds the text to the image for center point info

                    CvInvoke.Circle(frame, center, 2, new Bgr(Color.Orange).MCvScalar, 3); // the orange dot on the center of shapes

                    if (rStart == 1 && enableCoordinateSending) // VERY IMPORTANT, serial communication of location of shapes to Arduino
                    {
                        if (approxContour.Size == 3) // center of triangles
                        {
                            int xPos = center.X / 3;
                            int yPos = center.Y / 3;
                            int ts = 1;

                            SendPos(xPos, yPos, ts);
                        }
                        if (approxContour.Size == 4) // center of squares
                        {
                            int xPos = center.X / 3;
                            int yPos = center.Y / 3;
                            int ts = 2;

                            SendPos(xPos, yPos, ts);
                        }
                    }
                }
            }
        }

        private void SendPos(int xPos, int yPos, int ts) // Function that sends the serial communication to the Arduino
        {
            if (!enableCoordinateSending) // Stops data being sent while Arduino is working
            {
                return;
            }

            if (xPos > -1 && xPos < 58 && yPos > -1 && yPos < 44) // prevents bad positions from being sent
            {
                byte[] buffer = new byte[5] {
                Encoding.ASCII.GetBytes("<")[0],
                Convert.ToByte(xPos),
                Convert.ToByte(yPos),
                Convert.ToByte(ts),
                Encoding.ASCII.GetBytes(">")[0]
};
                arduinoSerial.Write(buffer, 0, 5);
            }
            else // shows if bad data is sent
            {
                Invoke(new Action(() =>
                {
                    onOffLabel.Text = ($"Error");
                }));
            }
        }

        private void ContourImage(Mat frame, VectorOfPoint contour, Rectangle boundingBox, double area)
        {
            // contour collection for the contour image output frame
            using (VectorOfPoint approxContour = new VectorOfPoint())
            {
                bool isRectangle = true;
                Point[] pts = approxContour.ToArray();
                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.10, true);
                // get an array of points in the contour

                if (contour.Size > 20 && area < 5000) // filters size of shapes shown
                {
                    // Drawing contour and box around it
                    if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                    {
                        CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Green).MCvScalar); // adds colors to shapes
                    }
                    else if (approxContour.Size == 4) //The contour has 4 vertices, it is a square
                    {
                        for (int j = 0; j < edges.Length; j++)
                        {
                            double angle = Math.Abs(
                               edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                            if (angle < 80 || angle > 100) // verifies square edge angles
                            {
                                isRectangle = false;
                            }
                        }

                        if (isRectangle == true)
                        {
                            CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Red).MCvScalar); // adds colors to shapes
                        }
                    }
                    else
                    {

                    }

                    // Write information next to marked object
                    Point center = new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2); // center of bounding box

                    CvInvoke.Circle(frame, center, 2, new Bgr(Color.Orange).MCvScalar, 3); // orange dot in center of bounding box
                }
            }
        }
        private void WriteMultilineText(Mat frame, string[] lines, Point origin) // writes text on the output frame
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * 10 + origin.Y; // Moving down on each line
                CvInvoke.PutText(frame, lines[i], new Point(origin.X, y),
                FontFace.HersheyPlain, 0.8, new Bgr(Color.Fuchsia).MCvScalar); //part of the code that puts shape info on the output image
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // terminate the image processing thread to avoid orphaned processes
            _captureThread.Abort();
            serialMonitoringThread.Abort();
        }

        private void sButton_Click(object sender, EventArgs e)
        {
            rStart = 1; // A start button so I can start C# without immediately sending data on serial communication
        }
    }
}
