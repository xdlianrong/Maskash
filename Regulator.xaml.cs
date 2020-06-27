using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Client;
using CryptoSystem;

namespace MashCash
{
    /// <summary>
    /// Regulator.xaml 的交互逻辑
    /// </summary>
    public partial class Regulator : Window
    {
        public Regulator()
        {
            InitializeComponent();
            flush();
        }

        private void flush()
        {
            string url = Path.path + "0xC593A92EC43E6A90";
            try
            {
                if (!Directory.Exists(url)) Directory.CreateDirectory(url);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            DirectoryInfo di = new DirectoryInfo(url + "\\");
            FileInfo[] files = di.GetFiles();
            check.Items.Clear();
            foreach (FileInfo fi in files)
            {
                if (fi.Extension.ToLower() == ".cash")
                {
                    check.Items.Add(fi.Name.Split('.')[0]);
                }
            }
            debug.Items.Add("==== 交易已刷新 ====");
            debug.Items.Add(" ");
        }

        private void flush(object sender, RoutedEventArgs e)
        {
            string anwser = SocketClass.Send("<GetBil>" + "0xC593A92EC43E6A90");
            Crypto crypto = new Crypto(new BigInt(Convert.ToUInt64("0xC593A92EC43E6A90", 16)), new BigInt(Convert.ToUInt64("0x9560FBB1826FE800", 16)));
            string[] bill_list = anwser.Split('|');
            if (bill_list.Length == 1)
            {
                debug.Items.Add("无新交易");
            }
            else
            {
                for (int i = 0; i < bill_list.Length / 11; i++)
                {
                    debug.Items.Add("转账：" + bill_list[11 * i + 1]);
                    string[] s = { bill_list[11 * i + 9], bill_list[11 * i + 10] };
                    string v = crypto.DecodeData(s);
                    ulong money;
                    try
                    {
                        money = Convert.ToUInt64(v.Split('x')[1], 16);
                        debug.Items.Add("金额：" + money.ToString());
                        debug.Items.Add("交易签名为" + bill_list[11 * i]);
                        using (StreamWriter sw = new StreamWriter(Path.path + "0xC593A92EC43E6A90" + "\\" + bill_list[11 * i] + ".cash", false, new UTF8Encoding(false)))
                        {
                            for (int w = 0; w < 11; w++)
                            {
                                sw.WriteLine(bill_list[11 * i + w]);
                            }
                            sw.WriteLine(v);
                        }
                        flush();
                    }
                    catch (Exception ex)
                    {
                        debug.Items.Add("!!!! 该交易信息存在问题，请注意查证 !!!!");
                        debug.Items.Add(ex.Message);
                    }
                }
            }
            flush();
        }

        private void ShowInfo(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Path.path + "0xC593A92EC43E6A90" + "\\" + check.Text + ".cash", new UTF8Encoding(false)))
                {
                    string show = "";
                    show += "<序列号><SN_o>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<付款随机数><r_r1>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<发送方><S>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<接收方><R>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<付款承诺><CM_o>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<找零承诺><CM_s>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<收款承诺><CM_r>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<收款方公钥转账金额密文><E(v_r)>\n";
                    show += sr.ReadLine() + " | " + sr.ReadLine() + "\n\n";
                    show += "<监管者公钥转账金额密文><E'(v_r)>\n";
                    show += sr.ReadLine() + " | " + sr.ReadLine() + "\n\n";
                    show += "<转账金额><v_r>\n";
                    show += Convert.ToInt32(sr.ReadLine(), 16).ToString();
                    info.Text = show;
                }
            }
            catch (Exception ex)
            {
                debug.Items.Add(ex.Message);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
