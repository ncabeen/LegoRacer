//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DependencyProperty PageUpEnabledProperty = DependencyProperty.Register(
            "PageUpEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageDownEnabledProperty = DependencyProperty.Register(
            "PageDownEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageLeftEnabledProperty = DependencyProperty.Register(
           "PageLeftEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageRightEnabledProperty = DependencyProperty.Register(
           "PageRightEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        private const double ScrollErrorMargin = 0.001;

        private const int PixelScrollByAmount = 20;

        public const int NumCarComponents = 4;
        public string[] labels = { "Chassis", "Wheel", "Body", "Engine" }; 

        private readonly KinectSensorChooser sensorChooser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // Clear out placeholder content
            this.wrapPanel.Children.Clear();

            scrollViewer.ScrollToTop();
            scrollViewer.ScrollToVerticalOffset(100);

            //Label array

            //uri array to images 
            Uri[] imageURIs = new Uri[NumCarComponents] {new Uri("../Images/Chassis.png",UriKind.Relative), new Uri("../Images/Wheel.png",UriKind.Relative), 
                                                      new Uri("../Images/Body.png",UriKind.Relative), new Uri("../Images/Engine.png",UriKind.Relative)};

            // Add in display content
            for (var index = 0; index < NumCarComponents; ++index)
            {
                var button = new KinectTileButton { Label = labels[index] };
                var brush = new ImageBrush();  //D:\Kinect\ControlsBasics-WPF\Images
                brush.ImageSource = new BitmapImage(imageURIs[index]);
                button.Background = brush;
                
                /*
                Image img = new Image();
                img.Source = new BitmapImage(new Uri("..Images/Doge.jpg", UriKind.Relative));
                button.Content = img;
                */
                this.wrapPanel.Children.Add(button);
            }

            // Bind listner to scrollviwer scroll position change, and check scroll viewer position
            this.UpdatePagingButtonState();
            scrollViewer.ScrollChanged += (o, e) => this.UpdatePagingButtonState();
        }


        /// <summary>
        /// CLR Property Wrappers for PageLeftEnabledProperty
        /// </summary>
        public bool PageLeftEnabled
        {
            get
            {
                return (bool)GetValue(PageLeftEnabledProperty);
            }

            set
            {
                this.SetValue(PageLeftEnabledProperty, value);
            }
        }

        /// <summary>
        /// CLR Property Wrappers for PageLeftEnabledProperty
        /// </summary>
        public bool PageRightEnabled
        {
            get
            {
                return (bool)GetValue(PageRightEnabledProperty);
            }

            set
            {
                this.SetValue(PageRightEnabledProperty, value);
            }
        }
       
        /// <summary>
        /// CLR Property Wrappers for PageLeftEnabledProperty
        /// </summary>
        public bool PageDownEnabled
        {
            get
            {
                return (bool)GetValue(PageDownEnabledProperty);
            }

            set
            {
                this.SetValue(PageDownEnabledProperty, value);
            }
        }

        /// <summary>
        /// CLR Property Wrappers for PageRightEnabledProperty
        /// </summary>
        public bool PageUpEnabled
        {
            get
            {
                return (bool)GetValue(PageUpEnabledProperty);
            }

            set
            {
                this.SetValue(PageUpEnabledProperty, value);
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void KinectTileButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (KinectTileButton)e.OriginalSource;
            var selectionDisplay = new SelectionDisplay(Array.IndexOf(labels, button.Label as String));
            this.kinectRegionGrid.Children.Add(selectionDisplay);
            e.Handled = true;
        }

        /// <summary>
        /// Handle paging right (next button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageRightButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + PixelScrollByAmount);
        }

        /// <summary>
        /// Handle paging left (previous button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageLeftButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - PixelScrollByAmount);
        }

        private void PageUpButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - PixelScrollByAmount);
        }

        private void PageDownButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + PixelScrollByAmount);
        }

        /// <summary>
        /// Change button state depending on scroll viewer position
        /// </summary>
        private void UpdatePagingButtonState()
        {
            this.PageLeftEnabled = scrollViewer.HorizontalOffset > ScrollErrorMargin;
            this.PageRightEnabled = scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth - ScrollErrorMargin;
            this.PageUpEnabled = scrollViewer.VerticalOffset > ScrollErrorMargin;
            this.PageDownEnabled = scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight - ScrollErrorMargin;
        }




    }
}
