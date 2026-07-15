using System;
using System.Windows;
using MailClientApp.ViewModels;

namespace MailClientApp.Views
{
    public partial class AccountDialog : Window
    {
        public AccountDialog()
        {
            InitializeComponent();
        }

        public AccountDialog(AccountDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountDialogViewModel viewModel)
            {
                // Focus first field
            }
        }
    }
}