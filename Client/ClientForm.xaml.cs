using Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientNamespace
{
    /// <summary>
    /// Interaction logic for ClientForm.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
        Client _client;
        bool _messageIsPrivate = false;

        public ClientForm(Client client)
        {
            InitializeComponent();

            _client = client;
        }

        public void UpdateChatWindow(string message)
        {
            MessageWindow.Dispatcher.Invoke(() =>
            {
                MessageWindow.Text += message + Environment.NewLine;
                MessageWindow.ScrollToEnd();
            });
        }

        private void SendTypedMessage()
        {
            if (InputField.Text != "")
            {
                if (!_messageIsPrivate)
                    _client.SendEncrypted(new ChatMessagePacket(InputField.Text));
                else
                    _client.SendEncrypted(new PrivateMessagePacket(ClientList.SelectedItem as string, InputField.Text));
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
                DisconnectButton.IsEnabled = true;
                InputField.IsEnabled = true;
                SubmitButton.IsEnabled = true;
                MessageWindow.IsEnabled = true;
                Emotes.IsEnabled = true;

                _client.SendMessage(new ConnectionPacket(NameBox.Text, _client.PublicKey));
                _client.Run();
            }
            else
                Console.WriteLine("Unable to connect to server");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _client.Close();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionButton.IsEnabled = true;
            NameBox.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            InputField.IsEnabled = false;
            SubmitButton.IsEnabled = false;
            MessageWindow.IsEnabled = false;
            Emotes.IsEnabled = false;

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
    }
}
