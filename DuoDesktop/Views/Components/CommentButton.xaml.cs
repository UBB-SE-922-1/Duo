using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DuoDesktop.Views.Components
{
    public sealed partial class CommentButton : UserControl
    {
        public event RoutedEventHandler Click;

        public CommentButton()
        {
            this.InitializeComponent();
        }

        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
} 