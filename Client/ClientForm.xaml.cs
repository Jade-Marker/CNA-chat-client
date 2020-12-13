using Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ClientNamespace
{
    /// <summary>
    /// Interaction logic for ClientForm.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
        private Client _client;
        private bool _messageIsPrivate = false;

        private List<BitmapImage> _profilePictures;
        private int _profileIndex = 0;

        private const int cIconHeightOffset = 10;
        private const int cChatHeight = 16;
        private const int cChatWithImageHeight = 32;

        public ClientForm(Client client)
        {
            InitializeComponent();
            _client = client;
            LoadProfilePictures();
        }

        public void UpdateChatWindow(string message, int profilePictureIndex)
        {
            MessageWindow.Dispatcher.Invoke(() =>
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                if (profilePictureIndex != -1)
                {
                    Image image = new Image();
                    image.Source = _profilePictures[profilePictureIndex];
                    image.Height = cChatWithImageHeight;
                    stackPanel.Children.Add(image);
                }


                TextBox chat = new TextBox();
                if (profilePictureIndex != -1)
                    chat.Height = cChatWithImageHeight;
                else
                    chat.Height = cChatHeight;
                chat.Text = message;
                chat.Style = FindResource("ChatStyle") as Style;
                stackPanel.Children.Add(chat);


                ListBoxItem item = new ListBoxItem();
                item.Content = stackPanel;
                MessageWindow.Items.Add(item);
                MessageWindow.ScrollIntoView(item);
            });
        }

        private void SendTypedMessage()
        {
            if (InputField.Text != "")
            {
                if (!_messageIsPrivate)
                    _client.SendEncrypted(new ChatMessagePacket(InputField.Text, _profileIndex));
                else
                    _client.SendEncrypted(new PrivateMessagePacket(ClientList.SelectedItem as string, InputField.Text, _profileIndex));
                InputField.Text = "";
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SendTypedMessage();
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SendTypedMessage();
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client.Connect("127.0.0.1", 4444))
            {
                ConnectionButton.IsEnabled = false;
                NameBox.IsEnabled = false;
                ProfileList.Visibility = Visibility.Hidden;
                ProfileText.Visibility = Visibility.Hidden;

                DisconnectButton.IsEnabled = true;
                InputField.IsEnabled = true;
                SubmitButton.IsEnabled = true;
                Emotes.IsEnabled = true;
                MessageWindow.Visibility = Visibility.Visible;

                _client.SendMessage(new ConnectionPacket(NameBox.Text, _client.PublicKey));
                _client.Run();
            }
            else
                Console.WriteLine("Unable to connect to server");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _client.Close();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionButton.IsEnabled = true;
            NameBox.IsEnabled = true;
            ProfileList.Visibility = Visibility.Visible;
            ProfileText.Visibility = Visibility.Visible;

            DisconnectButton.IsEnabled = false;
            InputField.IsEnabled = false;
            SubmitButton.IsEnabled = false;
            Emotes.IsEnabled = false;
            MessageWindow.Visibility = Visibility.Hidden;

            _client.Close();
        }

        public void UpdateClientList(string name)
        {
            ClientList.Dispatcher.Invoke(() =>
            {
                ClientList.Items.Add(name);
                ClientList.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            });
        }

        public void RemoveClient(string name)
        {
            ClientList.Dispatcher.Invoke(() =>
            {
                ClientList.Items.Remove(name);
            });
        }

        public void ClearClientList()
        {
            ClientList.Dispatcher.Invoke(() =>
            {
                ClientList.Items.Clear();
            });
        }

        private void ClientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = (sender as ListBox).SelectedItem as string;
            if (name != null)
                SetMessageState(true, name);
            else
                SetMessageState(false);
        }

        private void SetMessageState(bool isPrivate, string target = "All")
        {
            if (isPrivate)
                _messageIsPrivate = true;
            else
                _messageIsPrivate = false;
            TargetText.Text = target;
        }

        private void SwapMessageState(string target)
        {
            if (_messageIsPrivate)
                SetMessageState(false);
            else
                SetMessageState(true, target);
        }

        private void Viewbox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                SwapMessageState(ClientList.SelectedItem as string);
        }

        private void Emotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputField.Text += Emotes.SelectedItem;
            Emotes.SelectedItem = null;
        }

        private void ProfilePictures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _profileIndex = ProfileList.SelectedIndex;
        }

        private void LoadProfilePictures()
        {
            _profilePictures = new List<BitmapImage>();

            string[] iconFilePaths = Directory.GetFiles(Environment.CurrentDirectory + "\\Profiles\\");

            foreach (string file in iconFilePaths)
            {
                if (file.EndsWith(".png"))
                {
                    BitmapImage profile = new BitmapImage();

                    profile.BeginInit();
                    profile.UriSource = new Uri(file);
                    profile.EndInit();

                    _profilePictures.Add(profile);

                    Image image = new Image();
                    image.Source = profile;
                    image.Height = ProfileList.Height - cIconHeightOffset;
                    ProfileList.Items.Add(image);
                }
            }
        }
    }
}
