﻿#region

using System.IO;
using System.Windows;
using LoLAccountChecker.Data;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

#endregion

namespace LoLAccountChecker.Views
{
    public partial class AccountsWindow
    {
        public AccountsWindow()
        {
            InitializeComponent();
            WindowManager.Accounts = this;

            _accountsGrid.ItemsSource = Checker.Accounts;
            _showPasswords.IsChecked = Settings.Config.ShowPasswords;
        }

        private async void BtnAddAccountClick(object sender, RoutedEventArgs e)
        {
            var settings = new LoginDialogSettings
            {
                AffirmativeButtonText = "Add",
                NegativeButtonVisibility = Visibility.Visible,
                NegativeButtonText = "Cancel"
            };

            var input = await this.ShowLoginAsync("New account", "Insert your new Account", settings);

            if (input == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(input.Username) || string.IsNullOrEmpty(input.Password))
            {
                return;
            }

            if (Checker.Accounts.Exists(a => a.Username == input.Username))
            {
                await this.ShowMessageAsync("New account", "Error: Account already exists.");
                return;
            }

            var account = new Account
            {
                Username = input.Username,
                Password = input.Password,
                State = Account.Result.Unchecked
            };

            Checker.Accounts.Add(account);

            WindowManager.Main.UpdateControlls();
            RefreshAccounts();
        }

        private void BtnImportClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt";

            var result = ofd.ShowDialog();

            if (result == true)
            {
                if (!File.Exists(ofd.FileName))
                {
                    return;
                }

                var accounts = Utils.GetLogins(ofd.FileName);
                var num = 0;

                foreach (var account in accounts)
                {
                    if (!Checker.Accounts.Exists(a => a.Username == account.Username))
                    {
                        Checker.Accounts.Add(account);
                        num++;
                    }
                }

                if (num > 0)
                {
                    this.ShowMessageAsync("Import", string.Format("Imported {0} accounts.", num));
                }
                else
                {
                    this.ShowMessageAsync("Import", "No new accounts found.");
                }

                RefreshAccounts();
                WindowManager.Main.UpdateControlls();
            }
        }

        private void ShowPasswordsClick(object sender, RoutedEventArgs e)
        {
            Settings.Config.ShowPasswords = _showPasswords.IsChecked == true;
            RefreshAccounts();
        }

        public void RefreshAccounts()
        {
            _accountsGrid.Items.Refresh();
        }
    }
}