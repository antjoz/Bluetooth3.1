using System;

using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;

namespace Bluetooth3._1
{
    public partial class Form1 : Form
    {

        private BluetoothClient bluetoothClient;
        private BluetoothDeviceInfo[] devices;

        public Form1()
        {
            InitializeComponent();

            bluetoothClient = new BluetoothClient();
            lblStatus.Text = "Status: Bluetooth uruchomiony";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            listDevices.Items.Clear();
            lblStatus.Text = "Status: wyszukiwanie urządzeń...";

            try
            {
                devices = bluetoothClient.DiscoverDevices();

                foreach (BluetoothDeviceInfo device in devices)
                {
                    listDevices.Items.Add(device.DeviceName);
                }

                lblStatus.Text = "Status: znaleziono " + devices.Length + " urządzeń";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd wyszukiwania: " + ex.Message);
                lblStatus.Text = "Status: błąd";
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (listDevices.SelectedIndex == -1)
            {
                MessageBox.Show("Wybierz urządzenie z listy!");
                return;
            }

            BluetoothDeviceInfo device = devices[listDevices.SelectedIndex];

            try
            {
                bool paired = BluetoothSecurity.PairRequest(
                    device.DeviceAddress,
                    null // brak PIN – system zapyta automatycznie
                );

                if (paired)
                    lblStatus.Text = "Status: połączono z " + device.DeviceName;
                else
                    lblStatus.Text = "Status: nie udało się połączyć";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia: " + ex.Message);
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (listDevices.SelectedIndex == -1)
            {
                MessageBox.Show("Wybierz urządzenie!");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Wybierz plik do wysłania";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            BluetoothDeviceInfo device = devices[listDevices.SelectedIndex];
            Guid serviceClass = BluetoothService.ObexObjectPush;

            try
            {
                using (BluetoothClient client = new BluetoothClient())
                {
                    client.Connect(device.DeviceAddress, serviceClass);

                    using (Stream stream = client.GetStream())
                    using (FileStream fs = File.OpenRead(ofd.FileName))
                    {
                        fs.CopyTo(stream);
                    }
                }

                lblStatus.Text = "Status: plik wysłany poprawnie";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd wysyłania pliku: " + ex.Message);
                lblStatus.Text = "Status: błąd wysyłania";
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bluetoothClient?.Dispose();
        }
    }
}
