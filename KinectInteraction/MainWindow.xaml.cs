using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Windows;
using System.Windows.Data;

namespace KinectInteraction {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private KinectSensorChooser sensorChooser;
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            sensorChooser = new KinectSensorChooser();
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooserUi.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();
            var regionSensorBinding = new Binding("Kinect") {
                Source = sensorChooser
            };
            BindingOperations.SetBinding(kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
        }
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args) {
            bool error = false;
            if (args.OldSensor != null) {
                try {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException) {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }
            if (args.NewSensor != null) {
                try {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); args.NewSensor.SkeletonStream.Enable();

                    try {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException) {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        error = true;
                    }
                }
                catch (InvalidOperationException) {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }
    }
}
