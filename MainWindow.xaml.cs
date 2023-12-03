using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.Constants;
using static Windows.Win32.PInvoke;
using Microsoft.UI;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Shapes;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private Microsoft.UI.Input.GestureRecognizer _gestureRecognizer;
        private Visual _rectangleVisual;
        private Point _relativePoint;

        public MainWindow()
        {

            this.InitializeComponent();

            HWND hwnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);

            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Debug.WriteLine("HWND");
            //Textdata.Text = hwnd.ToString();
            _rectangleVisual = ElementCompositionPreview.GetElementVisual(GestureRectangle);
            _gestureRecognizer = new Microsoft.UI.Input.GestureRecognizer
            {
                GestureSettings =
                    GestureSettings.Tap |
                    GestureSettings.DoubleTap |
                    GestureSettings.RightTap |
                    GestureSettings.Drag |
                    GestureSettings.Hold |
                    GestureSettings.HoldWithMouse
            };
            _gestureRecognizer.Tapped += OnTapped;
            _gestureRecognizer.RightTapped += OnRightTapped;
            _gestureRecognizer.Dragging += OnDrag;
            _gestureRecognizer.Holding += OnHold;
        }

        private void SetWindowSize(HWND hwnd, int width, int height)
        {
            // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
            uint dpi = GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            //SetWindowPos(hwnd, default, 0, 0, width, height, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
        }

        private void PlacementCenterWindowInMonitorWin32(HWND hwnd)
        {
            RECT windowMonitorRectToAdjust;
            GetWindowRect(hwnd, out windowMonitorRectToAdjust);
            ClipOrCenterRectToMonitorWin32(ref windowMonitorRectToAdjust);
            SetWindowPos(hwnd, default, windowMonitorRectToAdjust.left,
                windowMonitorRectToAdjust.top, 0, 0,
                SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
        }

        private void ClipOrCenterRectToMonitorWin32(ref RECT adjustedWindowRect)
        {
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();
            GetMonitorInfo(MonitorFromRect(adjustedWindowRect, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST), ref mi);

            RECT rcWork = mi.rcWork;
            int w = adjustedWindowRect.right - adjustedWindowRect.left;
            int h = adjustedWindowRect.bottom - adjustedWindowRect.top;

            adjustedWindowRect.left = rcWork.left + (rcWork.right - rcWork.left - w) / 2;
            adjustedWindowRect.top = rcWork.top + (rcWork.bottom - rcWork.top - h) / 2;
            adjustedWindowRect.right = adjustedWindowRect.left + w;
            adjustedWindowRect.bottom = adjustedWindowRect.top + h;
        }

        //
        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            GestureRectangle.CapturePointer(args.Pointer);
            Gesture.Text = "pressed";
            PointerPoint pointerPoint = args.GetCurrentPoint(Grid);
          

            // Get coordinates relative to the Rectangle.
            GeneralTransform transform = Grid.TransformToVisual(GestureRectangle);
            // GeneralTransform transform = Grid.TransformToVisual(Grid);
            _relativePoint = transform.TransformPoint(new Point(pointerPoint.Position.X, pointerPoint.Position.Y));

            _gestureRecognizer.ProcessDownEvent(pointerPoint);
            //

            //Get the tapped location
            Point tapLocation = pointerPoint.Position;
            Gesture.Text = tapLocation.ToString();

            //create Ellipse
            Ellipse circle = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = new SolidColorBrush(Colors.AliceBlue),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1
            };

            circle.Margin = new Thickness(tapLocation.X - circle.Width / 2, tapLocation.Y - circle.Height / 2, 0, 0);

            Grid.Children.Add(circle);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            Gesture.Text = "moved";
            PointerPoint currentPoint = args.GetCurrentPoint(Grid);
            if (currentPoint.IsInContact)
            {
                IList<PointerPoint> points = args.GetIntermediatePoints(Grid);
                _gestureRecognizer.ProcessMoveEvents(points);
            }
            else
            {
                _gestureRecognizer.CompleteGesture();
            }
        }

        private void createCircle(object sender, PointerEventArgs args)
        {

            //Get the tapped location
            Point tapLocation = args.CurrentPoint.Position;
            Gesture.Text = tapLocation.ToString();

            //create Ellipse
            Ellipse circle = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = new SolidColorBrush(Colors.AliceBlue),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1
            };

            circle.Margin = new Thickness(tapLocation.X - circle.Width / 2, tapLocation.Y - circle.Height / 2, 0, 0);

            Grid.Children.Add(circle);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            Gesture.Text = "released";
            PointerPoint pointerPoint = args.GetCurrentPoint(Grid);
            _gestureRecognizer.ProcessUpEvent(pointerPoint);
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            Gesture.Text = "cancelled";
            _gestureRecognizer.CompleteGesture();
        }

        private void OnTapped(object sender, TappedEventArgs args)
        {
            Gesture.Text = "tapped";
            if (args.TapCount == 2)
            {
                GestureResultText.Text = "Double Tapped";
            }
            else
            {
                GestureResultText.Text = "Tapped";
            }

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
        }

        private void OnRightTapped(object sender, RightTappedEventArgs args)
        {
            GestureResultText.Text = "Right Tapped";

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
        }

        private void OnDrag(object sender, DraggingEventArgs args)
        {
            GestureResultText.Text = "Drag";

            var recOffset = _rectangleVisual.Offset;
            recOffset.X = (float)(args.Position.X - _relativePoint.X);
            recOffset.Y = (float)(args.Position.Y - _relativePoint.Y);
            _rectangleVisual.Offset = recOffset;
        }

        private void OnHold(object sender, HoldingEventArgs args)
        {
            GestureResultText.Text = "Holding";

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Red);
        }
    }
}
