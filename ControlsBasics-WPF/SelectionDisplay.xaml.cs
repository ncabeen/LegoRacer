namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic
    /// </summary>
    public partial class SelectionDisplay : UserControl
    {
       /// <summary>
        /// Initializes a new instance of the <see cref="SelectionDisplay"/> class. 
        /// </summary>
        /// <param name="itemId">ID of the item that was selected</para>m
        public SelectionDisplay(int itemId)
        {
            this.InitializeComponent();
            const int NumCarComponents =4;
            Uri[] imageURIs = {new Uri("../Images/Chassis.png",UriKind.Relative), new Uri("../Images/Wheel.png",UriKind.Relative), 
                                                          new Uri("../Images/Body.png",UriKind.Relative), new Uri("../Images/Engine.png",UriKind.Relative)};

            this.messageTextBlock.Text = itemId.ToString();

            //int item_val = Int32.Parse(itemId);

            //this.messageTextBlock.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SelectedMessage, itemId);

        //Display image using the URI array.     Later change to ppublic array in MainWindow
            var part_image = new Image();
            part_image.Source = new BitmapImage(imageURIs[itemId]);
            this.messageImage.Source = part_image.Source;
            

        }

        /// <summary>
        /// Called when the OnLoaded storyboard completes.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnLoadedStoryboardCompleted(object sender, System.EventArgs e)
        {
            var parent = (Panel)this.Parent;
            parent.Children.Remove(this);
        }
    }
}
