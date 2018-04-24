using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace bluetooth
{
    public partial class Form1 : Form
    {
        private List<byte> hexm;

        public Form1()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                portListBox.Items.Add(port);
            }
            //portListBox.Select();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Firmware (*.eep; *.bin; *.s19)| *.eep; *.bin; *.s19";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            data_table.Rows.Clear();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            //Заблокирвоать кнопки
                            textBox1.Text += Environment.NewLine+"Открыть файл...";
                            save.Enabled = false;
                            cleaner.Enabled = false;
                            FileInfo file = new FileInfo(openFileDialog1.FileName);
                            hexm = File.ReadAllBytes(openFileDialog1.FileName).ToList<byte>();
                            if (file.Extension == ".eep" || file.Extension == ".bin" || file.Extension == ".EEP" || file.Extension == ".BIN" || file.Extension == ".s19" || file.Extension == ".S19")
                            {
                                progressBar1.Value = 10;
                                data_table.Rows.Add("Имя файла", openFileDialog1.SafeFileName);
                                data_table.Rows.Add("Размер файла", file.Length.ToString() + " байт");
                                data_table.Rows.Add("Директория", file.DirectoryName);
                                data_table.Rows.Add("Расширение", file.Extension);
                                data_table.Rows.Add("Дата", file.LastAccessTime.ToString());
                                progressBar1.Value = 20;
                                textBox1.Text += Environment.NewLine+"Анализ прошивки...";
                                try
                                {
                                    string url = "https://bineep.ru/analis/api";
                                    using (var webClient = new WebClient())
                                    using (var stream = webClient.OpenRead("http://www.google.com"))
                                    {
                                        progressBar1.Value = 30;
                                        // Создаём коллекцию параметров
                                        var pars = new NameValueCollection();
                                        // Добавляем необходимые параметры в виде пар ключ, значение
                                        pars.Add("type", "analis");
                                        pars.Add("hexm", BitConverter.ToString(hexm.GetRange(0, hexm.Count()).ToArray()).Replace("-", ""));
                                        //Кодировка
                                        webClient.Encoding = System.Text.Encoding.UTF8;
                                        // Посылаем параметры на сервер
                                        // Может быть ответ в виде массива байт
                                        var response = webClient.UploadValues(url, pars);
                                        Char delimiter = '&';
                                        String[] substrings = Encoding.UTF8.GetString(response).Split(delimiter);
                                        foreach (var substring in substrings)
                                        {
                                            if (substring.Count() > 1)
                                            {
                                                delimiter = '-';
                                                String[] post_result;
                                                post_result = substring.Split(delimiter);
                                                data_table.Rows.Add(post_result[0], post_result[1]);
                                            }
                                        }
                                        progressBar1.Value = 100;
                                        textBox1.Text += Environment.NewLine+"Файл обработан...";
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Нет интернет соединения");
                                }
                            }
                            else { MessageBox.Show("Ошибка формата прошивки, необходимо .bin или .eep"); }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message); }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://bineep.ru");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int TimeOut = 1500;
            SerialPort port = new SerialPort();
            port.PortName = "COM5";
            port.BaudRate = 115200;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataBits = 8;
            port.Handshake = Handshake.None;
            port.Open();

            byte[] send = new byte[] { 0xee, 0x02, 0x95, 0x49, 0x02, 0x90, 0x20, 0x25 };
            port.Write(send, 0, send.Length);
            Thread.Sleep(TimeOut);
            //вариант 1-ничего не выдает программа
             textBox1.Text = port.ReadExisting();

            //2 вариант - зависает на этом месте(наверное timeoutexeption. ждет каких-то данных..)
            //port.Read(StartByte, 0, StartByte.Length);
            //listBox1.Items.Add(StartByte);
           // port.Close();
        }
    }
}
