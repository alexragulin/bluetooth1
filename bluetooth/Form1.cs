using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace bluetooth
{
    public partial class Form : System.Windows.Forms.Form
    {
        private List<byte> hexm;
        private SerialPort port;
        static public String[] portlist;
        static public String portaction;

        public Form() {
            InitializeComponent();
            port = new SerialPort();
            portlist = SerialPort.GetPortNames();
            portaction = portlist[portlist.Count()-1];
            foreach (string port in portlist) comboPortList.Items.Add(port);
            comboPortList.SelectedIndex=comboPortList.Items.Count-1;
        }

//АНАЛИЗ ФАЙЛА
        private void send_bineep()
        {
            try
            {
                string url = "https://bineep.ru/analis/api";
                using (var webClient = new WebClient())
                {
                    var pars = new NameValueCollection();
                    pars.Add("type", "analis"); // Добавляем необходимые параметры в виде пар ключ, значение
                    pars.Add("hexm", BitConverter.ToString(hexm.GetRange(0, hexm.Count()).ToArray()).Replace("-", ""));
                    webClient.Encoding = Encoding.UTF8;                     //Кодировка
                    var response = webClient.UploadValues(url, pars);
                    Char delimiter = '&';
                    String[] substrings = Encoding.UTF8.GetString(response).Split(delimiter);
                    foreach (var substring in substrings)
                    {
                        if (substring.Count() > 1)
                        {
                            delimiter = '-';
                            String[] post_result = substring.Split(delimiter);
                            data_table.Rows.Add(post_result[0], post_result[1]);
                        }
                    }
                }
            }
            catch { MessageBox.Show("Нет интернет соединения");}
        }

//ЧИТАТЬ ПРОШИВКУ
        private void button_Write_Click_1(object sender, EventArgs e)
        {
            
            textBox.Clear();                                 //Очистка полей
            data_table.Rows.Clear();                         //Очистить таблицу
            fileName.Text = "Чтение файла с модуля.....";
            data_table.Rows.Add("Загрузка данных", ".....");
            try
            {
                port.PortName = portaction;
                port.BaudRate = 38400;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                port.Handshake = Handshake.None;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.RtsEnable = false;
                port.DtrEnable = true;
                port.Open();
                //Thread.Sleep(200);
                //byte[] dataToWrite = new byte[] { 165, 238, 2, 147, 32, 24, 150, 11, 12, 116, 101, 115, 116, 0, 207, 112, 108, 97, 116, 102, 111, 114, 109, 16, 147, 5, 228, 0, 1, 135, 21};
                //ReadBlock(dataToWrite);
                // Thread.Sleep(200);
                //dataToWrite = new byte[] { 165, 238, 2, 147, 32, 24, 150, 11, 12, 116, 101, 115, 116, 0, 10, 112, 108, 97, 116, 102, 111, 114, 109, 16, 147, 0, 0, 0, 1, 135, 103};
                // ReadBlock(dataToWrite);
                byte[] dataToWrite = new byte[] { 165, 238, 2, 149, 63, 5, 144, 0, 16, 0, 16, 30 };
                try
                {             
                    ReadBlock(dataToWrite);
                    tsslInfo.Text = "Платформа Arcadia";
                    Thread.Sleep(200);
                    int request = 24; // Число запросов к процессору
                    //int responseSize = 264; //Размер буфера для ответа
                    List<byte> startToWrite = new List<byte> { 165, 238, 2, 149, 63, 5, 144, 0};
                    byte[] dataTW = new byte[12];
                    string bit = "";
                    List<byte> buf_list = new List<byte>(); 
                    for (int i=0; i<request; i++)
                    {
                         dataTW[0] = startToWrite[0];
                         dataTW[1] = startToWrite[1];
                         dataTW[2] = startToWrite[2];
                         dataTW[3] = startToWrite[3];
                         dataTW[4] = startToWrite[4];
                         dataTW[5] = startToWrite[5];
                         dataTW[6] = startToWrite[6];
                         dataTW[7] = startToWrite[7];
                         dataTW[8] = (byte) (26 +i);
                         dataTW[9] = 255;
                         dataTW[10] = (byte) (26+i);
                         dataTW[11] = (byte) (dataTW[0] + dataTW[1] + dataTW[2] + dataTW[3] + dataTW[4] + dataTW[5] + dataTW[6] + dataTW[7] + dataTW[8] + dataTW[9] + dataTW[10]);
                         port.DiscardInBuffer(); //Очистка буфера
                         port.Write(dataTW, 0, dataTW.Length); //Отправка данных
                         Thread.Sleep(200); // Пайза 200 мс
                         byte[] buffer = new byte[264];  //Создание буфера
                         int bytesRead = port.Read(buffer, 0, buffer.Length); //Получение данных
                         bit += BitConverter.ToString(buffer).Replace("-", "").Substring(14, 512); //Обработка массива
                         buf_list=buffer.Skip(7).Take(256).ToList();
                        int er = hexm.Count;
                         for(int y=0; y < 256; y++, er++){
                                hexm[er] = buf_list[y];
                            }
                        // hexm = hexm.Concat(array).ToList();
                        //else hexm.AddRange(buffer.Skip(7).Take(256).ToList());
                        //Buffer.BlockCopy(hexm.ToArray(), 7, hexm.ToArray(), 0, 256);
                    }
                    //hexm = bit.ToList(); 
                    //tsslInfo.Text += " " + hexm.Count().ToString();

                    string h = "";
                    int z = 0;
                    do
                    {
                        h += bit.Substring(z, 32) + "\n";
                        z += 16;

                    } while ((bit.Length - z) > 32);
                    textBox.Text = h;
                }
                catch {
                    try
                    {
                        dataToWrite = new byte[] { 165, 238, 2, 147, 16, 5, 144, 16, 240, 241, 0, 190 };
                        ReadBlock(dataToWrite);
                        tsslInfo.Text = "Платформа EVO2";
                    }
                    catch
                    {
                        MessageBox.Show("Платформа неизвестна");
                        tsslInfo.Text = "Платформа неизвестна";
                    }   
                }
                port.Close();
            }
            catch { MessageBox.Show("ERROR: невозможно открыть порт:" + portaction); }
        }

//ЗАПИСИ И ЧТЕНИЕ В БУФЕР
        private bool ReadBlock(byte[] dataToWrite)
        {
            port.DiscardInBuffer();
            port.Write(dataToWrite, 0, dataToWrite.Length);
            Thread.Sleep(300);
            byte[] buffer = new byte[port.ReadBufferSize];
            int bytesRead = port.Read(buffer, 0, buffer.Length);
            if (buffer[0].ToString() == "90") return true;
            return false; 
        }


//СОХРАНИТЬ ФАЙЛ
        private void save()
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Firmware (*.eep)|*.eep|All files (*.*)|*.*";
            sf.FilterIndex = 1;
            sf.RestoreDirectory = true;
            sf.FileName = fileName.Text;
            if (sf.ShowDialog() == DialogResult.OK)
            {
                byte[] file = new byte[hexm.Count];
                for (int i = 0; i < (hexm.Count); i++)
                {
                    file[i] = hexm[i];
                }
                System.IO.File.WriteAllBytes(sf.FileName, file);
                MessageBox.Show("Файл сохранен успешно");
            }
        }

//ОТКРЫТЬ ФАЙЛ
        private void open()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Firmware (*.eep; *.bin;)| *.eep; *.bin;";
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
                            FileInfo file = new FileInfo(openFileDialog1.FileName);
                            hexm = File.ReadAllBytes(openFileDialog1.FileName).ToList<byte>();
                            if (file.Extension == ".eep" || file.Extension == ".bin" || file.Extension == ".EEP" || file.Extension == ".BIN" || file.Extension == ".s19" || file.Extension == ".S19")
                            {
                                tsslInfo.Text = "Открытие файла";
                                textBox.Clear();
                                string h = "";
                                int z = 0;
                                do
                                {
                                    h += BitConverter.ToString(hexm.GetRange(z, 16).ToArray()).Replace("-", "") + "\n";
                                    z += 16;
                                } while ((hexm.Count - z) > 16);
                                textBox.Text = h;
                                fileName.Text = openFileDialog1.SafeFileName;
                                send_bineep();
                            }
                            else { MessageBox.Show("Ошибка формата прошивки, необходимо .bin или .eep"); }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message); }
            }
        }

//ЗАПИСАТЬ ПРОШИВКУ
        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                port.PortName = portaction;
                port.BaudRate = 38400;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                port.Handshake = Handshake.None;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.RtsEnable = false;
                port.DtrEnable = true;
                port.Open();
                Thread.Sleep(200);
                byte[] dataToWrite = new byte[] { 165, 238, 2, 149, 63, 5, 144, 0, 26, 255, 26, 49};
                ReadBlock(dataToWrite);
                Thread.Sleep(200);
                port.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: Проблема с интерфейсом обмена данных. Original error: " + ex.Message); }
        }

        private void открытьФайлToolStripMenuItem_Click(object sender, EventArgs e) { open(); }  //Открыть
        private void btnOpen_Click(object sender, EventArgs e) { open(); }   //Открыть
        private void btnSave_Click(object sender, EventArgs e) { save(); }   //Сохранить
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { portaction = comboPortList.SelectedItem.ToString(); }  //Изменение порта
    }
}
