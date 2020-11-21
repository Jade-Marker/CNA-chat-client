using Packets;
using System;
using System.Collections.Generic;
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
                client.SendMessage(new ChatMessagePacket(InputField.Text));
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
    }
}
