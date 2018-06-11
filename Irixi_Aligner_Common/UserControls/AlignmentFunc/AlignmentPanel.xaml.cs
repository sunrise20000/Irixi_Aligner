using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GalaSoft.MvvmLight.Command;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Alignment.Interfaces;
using Irixi_Aligner_Common.Alignment.SpiralScan;

namespace Irixi_Aligner_Common.UserControls.AlignmentFunc
{
    /// <summary>
    /// Interaction logic for AlignmentPanel.xaml
    /// </summary>
    public partial class AlignmentPanel : UserControl
    {
        public AlignmentPanel()
        {
            InitializeComponent();
        }

        #region Properties
        
        public Uri PropertiesTemplate
        {
            get { return (Uri)GetValue(PropertiesTemplateProperty); }
            set { SetValue(PropertiesTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertiesTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesTemplateProperty =
            DependencyProperty.Register("PropertiesTemplate", typeof(Uri), typeof(UserControl), 
                new PropertyMetadata(null, (s, e)=> 
                {
                    var owner = s as AlignmentPanel;
                    owner.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = (Uri)e.NewValue });
                }));


        public Transform3DGroup ContentTransform
        {
            get { return (Transform3DGroup)GetValue(ContentTransformProperty); }
            set { SetValue(ContentTransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for transform3DGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentTransformProperty =
            DependencyProperty.Register("ContentTransform", typeof(Transform3DGroup), typeof(UserControl), new PropertyMetadata(new Transform3DGroup()
            {
                Children = new Transform3DCollection()
                {
                    new RotateTransform3D(new AxisAngleRotation3D()
                {
                    Angle = -40,
                    Axis = new Vector3D(0, 1, 0)
                }),

                new RotateTransform3D(new AxisAngleRotation3D()
                {
                    Angle = 20,
                    Axis = new Vector3D(1, 0, 0)
                })
                }
            }));

        public ICommand StartCommand
        {
            get {
                return (ICommand)GetValue(StartCommandProperty);
            }
            set {
                SetValue(StartCommandProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for StartCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCommandProperty =
            DependencyProperty.Register("StartCommand", typeof(ICommand), typeof(UserControl), new PropertyMetadata(null));

        
        public object StartCommandParameter
        {
            get { return (object)GetValue(StartCommandParameterProperty); }
            set { SetValue(StartCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCommandParameterProperty =
            DependencyProperty.Register("StartCommandParameter", typeof(object), typeof(UserControl), new PropertyMetadata(null));

        public ICommand StopCommand
        {
            get { return (ICommand)GetValue(StopCommandProperty); }
            set { SetValue(StopCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StopCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopCommandProperty =
            DependencyProperty.Register("StopCommand", typeof(ICommand), typeof(UserControl), new PropertyMetadata(null));
        #endregion

        #region Methods

        private RelayCommand Reset3DChartView
        {
            get
            {
                return new RelayCommand(() =>
                {

                    Transform3DCollection collection = new Transform3DCollection
                    {
                        new RotateTransform3D(new AxisAngleRotation3D()
                        {
                            Angle = -40,
                            Axis = new Vector3D(0, 1, 0)
                        }),

                        new RotateTransform3D(new AxisAngleRotation3D()
                        {
                            Angle = 20,
                            Axis = new Vector3D(1, 0, 0)
                        })
                    };

                    Transform3DGroup group = new Transform3DGroup()
                    {
                        Children = collection,
                    };

                    this.ContentTransform = group;
                });
            }
        }


        /// <summary>
        /// Reset set the view of 3D chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmResetView_Click(object sender, RoutedEventArgs e)
        {
            Transform3DCollection collection = new Transform3DCollection
            {
                new RotateTransform3D(new AxisAngleRotation3D()
                {
                    Angle = -40,
                    Axis = new Vector3D(0, 1, 0)
                }),

                new RotateTransform3D(new AxisAngleRotation3D()
                {
                    Angle = 20,
                    Axis = new Vector3D(1, 0, 0)
                })
            };

            Transform3DGroup group = new Transform3DGroup()
            {
                Children = collection,
            };

            this.ContentTransform = group;
        }

        #endregion
    }
}
