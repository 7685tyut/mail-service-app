using System;
using System.Windows;
using MailClientApp.ViewModels;

namespace MailClientApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set data context
            DataContext = new MainViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: Load data on startup
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Optional: Save settings on close
        }
    }
}