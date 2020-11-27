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
        delegate void UpdateChatWindowDelegate(string message);

        Client client;
        bool messageIsPrivate = false;

        public ClientForm(Client client)
        {
            InitializeComponent();

            this.client = client;
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
                if (!messageIsPrivate)
                    client.SendMessage(new ChatMessagePacket(InputField.Text));
                else
                    client.SendMessage(new PrivateMessagePacket(ClientList.SelectedItem as string, InputField.Text));
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
            else if (e.Key == Key.Escape)
                SetMessageState(false);
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (client.Connect("127.0.0.1", 4444))
            {
                ConnectionButton.IsEnabled = false;
                NameBox.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                InputField.IsEnabled = true;
                SubmitButton.IsEnabled = true;
                MessageWindow.IsEnabled = true;

                client.SendMessage(new ConnectionPacket(NameBox.Text));
                client.Run();
            }
            else
                Console.WriteLine("Unable to connect to server");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Close();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionButton.IsEnabled = true;
            NameBox.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            InputField.IsEnabled = false;
            SubmitButton.IsEnabled = false;
            MessageWindow.IsEnabled = false;

            client.Close();
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
                messageIsPrivate = true;
            else
                messageIsPrivate = false;
            TargetText.Text = target;
        }
    }
}
