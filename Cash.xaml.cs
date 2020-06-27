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
using System.Windows.Shapes;
using System.IO;
using UserGroup;
using Client;
using CryptoSystem;
using System.Net.Sockets;

namespace MashCash
{
    /// <summary>
    /// Cash.xaml 的交互逻辑
    /// </summary>
    public partial class Cash : Window
    {

        private User current_user;

        public Cash(User cu)
        {
            current_user = cu;
            InitializeComponent();
            WindowState = WindowState.Normal;
            Visibility = Visibility.Visible;
            debug.Items.Add("======== 登陆成功 ========");
            debug.Items.Add("当前地址公钥为：");
            debug.Items.Add(cu.GetUserKeys()[1]);
            debug.Items.Add(" ");
            flush();
        }

        private void flush()
        {
            string url = Path.path + current_user.GetUserKeys()[1].ToString();
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
            int blc = 0;
            foreach (FileInfo fi in files)
            {
                if (fi.Extension.ToLower() == ".cash")
                {
                    check.Items.Add(fi.Name.Split('.')[0]);
                    using (StreamReader sr = new StreamReader(url + "\\" + fi.Name, new UTF8Encoding(false)))
                    {
                        sr.ReadLine();
                        sr.ReadLine();
                        blc += int.Parse(sr.ReadLine());
                    }
                }
            }
            balance.Content = blc.ToString();
            debug.Items.Add("==== 余额已刷新 ====");
            debug.Items.Add(" ");
        }

        private void ChangeMode(object sender, RoutedEventArgs e)
        {
            if(mode.Content.ToString().CompareTo("购币") == 0)
            {
                mode.Content = "付款";
                fun.Content = "付款";
                debug.Items.Add("==== 已切换到付款模式 ====");
                debug.Items.Add(" ");
                user.Visibility = Visibility.Visible;
            }
            else if (mode.Content.ToString().CompareTo("付款") == 0)
            {
                mode.Content = "收款";
                fun.Content = "收款";
                debug.Items.Add("==== 已切换到收款模式 ====");
                debug.Items.Add(" ");
                price.Visibility = Visibility.Collapsed;
                user.Visibility = Visibility.Collapsed;
            }
            else if (mode.Content.ToString().CompareTo("收款") == 0)
            {
                mode.Content = "购币";
                fun.Content = "购币";
                debug.Items.Add("==== 已切换到购币模式 ====");
                debug.Items.Add(" ");
                price.Visibility = Visibility.Visible;
            }
        }

        private void DebugClear(object sender, RoutedEventArgs e)
        {
            debug.Items.Clear();
        }

        private void MultiFun(object sender, RoutedEventArgs e)
        {
            if(mode.Content.ToString().CompareTo("购币") == 0)
            {
                int prices;
                try
                {
                    prices = int.Parse(price.Text);
                }
                catch(Exception ex)
                {
                    debug.Items.Add("<错误> 请输入合法的交易金额！");
                    debug.Items.Add(" ");
                    return;
                }
                if (prices <= 0)
                {
                    debug.Items.Add("<错误> 请输入合法的交易金额！");
                    debug.Items.Add(" ");
                    return;
                }
                string anwser = SocketClass.Send("<BuyCon>" + current_user.GetUserKeys()[1] + "|" + price.Text);
                if(anwser.CompareTo("Buy Coin Failed") == 0)
                {
                    debug.Items.Add("<错误> 购币失败！");
                    debug.Items.Add(" ");
                    return;
                }
                string filename = new BigInt((ulong)Convert.ToUInt64(anwser.Split('|')[0])).ToString();
                string money = anwser.Split('|')[1];
                string r = anwser.Split('|')[2];
                string sn = new BigInt(Crypto.Hash(anwser.Split('|')[2] + current_user.GetUserKeys()[0])).ToString();
                anwser.Replace("|", "\n");
                debug.Items.Add("购币成功！本次购币承诺为" + filename);
                using (StreamWriter sw = new StreamWriter(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + filename + ".cash", false, new UTF8Encoding(false)))
                {
                    sw.WriteLine(filename);
                    sw.WriteLine(sn);
                    sw.WriteLine(money);
                    sw.WriteLine(r);
                }
                flush();
            }
            else if (mode.Content.ToString().CompareTo("付款") == 0)
            {
                BigInt CM_o;
                BigInt SN_o;
                uint v_o;
                ulong r_o;
                using (StreamReader sr = new StreamReader(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + check.Text + ".cash", new UTF8Encoding(false)))
                {
                    CM_o = BigInt.HexCode(sr.ReadLine());
                    SN_o = BigInt.HexCode(sr.ReadLine());
                    v_o = Convert.ToUInt32(sr.ReadLine());
                    r_o = Convert.ToUInt64(sr.ReadLine());
                }
                uint v_r;
                try
                {
                    v_r = uint.Parse(price.Text);
                }
                catch (Exception ex)
                {
                    debug.Items.Add("<错误> 请输入合法的交易金额！");
                    debug.Items.Add(" ");
                    return;
                }
                if (v_r <= 0 || v_r > v_o)
                {
                    debug.Items.Add("<错误> 请输入合法的交易金额！");
                    debug.Items.Add(" ");
                    return;
                }
                string CM_spk = current_user.GetUserKeys()[1].ToString();
                string CM_rpk = user.Text;
                uint v_s = v_o - v_r;
                if (v_s > 0)
                {
                    BigInt r_s1 = BigInt.Random();
                    BigInt CM_s = 7 ^ new BigInt(v_s) + BigInt.HexCode(current_user.GetUserKeys()[1].ToString()) ^ r_s1;
                    BigInt r_r1 = BigInt.Random();
                    BigInt CM_r = 7 ^ new BigInt(v_r) + BigInt.HexCode(current_user.GetUserKeys()[1].ToString()) ^ r_r1;
                    if (CM_rpk.Split('x')[0] != "0")
                    {
                        debug.Items.Add("<错误> 请输入合法的收款地址！");
                        debug.Items.Add(" ");
                        return;
                    }
                    string[] Ev_r = new Crypto(BigInt.HexCode(CM_rpk)).EncodeData(new BigInt(v_r).ToString());
                    string[] Ev_r_ = new Crypto(BigInt.HexCode("0xC593A92EC43E6A90")).EncodeData(new BigInt(v_r).ToString());
                    string s = "<Paymen>" + SN_o.ToString() + "|" + r_r1.ToString() + "|" + CM_spk + "|" + CM_rpk + "|" + CM_o.ToString() + "|" + CM_r.ToString() + "|" + CM_s.ToString() + "|" + Ev_r[0] + "|" + Ev_r[1] + "|" + Ev_r_[0] + "|" + Ev_r_[1];
                    string anwser = SocketClass.Send(s);
                    Console.WriteLine(anwser);
                    if(anwser.CompareTo("pay success") == 0)
                    {
                        using (StreamWriter sw = new StreamWriter(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + CM_r.ToString() + ".cash", false, new UTF8Encoding(false)))
                        {
                            sw.WriteLine(CM_s.ToString());
                            sw.WriteLine(new BigInt(Crypto.Hash(r_s1.ToString() + current_user.GetUserKeys()[0])).ToString());
                            sw.WriteLine(v_s);
                            sw.WriteLine(Convert.ToUInt64(r_s1.ToString(), 16).ToString());
                            File.Delete(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + CM_o.ToString() + ".cash");
                            debug.Items.Add("==== 发布交易 ====");
                            debug.Items.Add("SN_o: " + SN_o.ToString());
                            debug.Items.Add("r_r1: " + r_r1.ToString());
                            debug.Items.Add("CM_spk: " + CM_spk);
                            debug.Items.Add("CM_rpk: " + CM_rpk);
                            debug.Items.Add("CM_o: " + CM_o.ToString());
                            debug.Items.Add("CM_r: " + CM_r.ToString());
                            debug.Items.Add("CM_s: " + CM_s.ToString());
                            debug.Items.Add("E(v_r): " + Ev_r[0] + " | " + Ev_r[1]);
                            debug.Items.Add("E'(v_r): " + Ev_r_[0] + " | " + Ev_r_[1]);
                            debug.Items.Add(" ");
                        }
                        flush();
                    }
                    else
                    {
                        debug.Items.Add(anwser);
                    }
                }
                else
                {
                    string[] Ev_r = new Crypto(BigInt.HexCode(CM_rpk)).EncodeData(new BigInt(v_r).ToString());
                    string[] Ev_r_ = new Crypto(BigInt.HexCode("0xC593A92EC43E6A90")).EncodeData(new BigInt(v_r).ToString());
                    string s = "<Paymen>" + SN_o.ToString() + "|" + new BigInt(r_o).ToString() + "|" + CM_spk + "|" + CM_rpk + "|" + CM_o.ToString() + "|" + "0x0000000000000000" + "|" + CM_o.ToString() + "|" + Ev_r[0] + "|" + Ev_r[1] + "|" + Ev_r_[0] + "|" + Ev_r_[1];
                    string anwser = SocketClass.Send(s);
                    Console.WriteLine(anwser);
                    if (anwser.CompareTo("pay success") == 0)
                    {
                        File.Delete(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + CM_o.ToString() + ".cash");
                        debug.Items.Add("==== 发布交易 ====");
                        debug.Items.Add("SN_o: " + SN_o.ToString());
                        debug.Items.Add("r_r1: " + r_o.ToString());
                        debug.Items.Add("CM_spk: " + CM_spk);
                        debug.Items.Add("CM_rpk: " + CM_rpk);
                        debug.Items.Add("CM_o: " + CM_o.ToString());
                        debug.Items.Add("CM_r: " + CM_o.ToString());
                        debug.Items.Add("CM_s: " + "无");
                        debug.Items.Add("E(v_r): " + Ev_r[0] + " | " + Ev_r[1]);
                        debug.Items.Add("E'(v_r): " + Ev_r_[0] + " | " + Ev_r_[1]);
                        debug.Items.Add(" ");
                        flush();
                    }
                    else
                    {
                        debug.Items.Add(anwser);
                    }
                }
            }
            else if (mode.Content.ToString().CompareTo("收款") == 0)
            {
                string anwser = SocketClass.Send("<GetBil>" + current_user.GetUserKeys()[1]);
                string[] bill_list = anwser.Split('|');
                if (bill_list.Length == 1)
                {
                    debug.Items.Add("当前无可用收款");
                }
                else
                {
                    for(int i = 0; i < bill_list.Length / 4; i++)
                    {
                        debug.Items.Add("转账：" + bill_list[4 * i + 1]);
                        string[] s = { bill_list[4 * i + 2], bill_list[4 * i + 3] };
                        string v = current_user.Decrypt(s);
                        ulong money;
                        try
                        {
                            money = Convert.ToUInt64(v.Split('x')[1], 16);
                            debug.Items.Add("金额：" + money.ToString());
                            string sn = new BigInt(Crypto.Hash(bill_list[4 * i] + current_user.GetUserKeys()[0])).ToString();
                            debug.Items.Add("交易成功！本次交易承诺为" + bill_list[4 * i + 1]);
                            using (StreamWriter sw = new StreamWriter(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + bill_list[4 * i + 1] + ".cash", false, new UTF8Encoding(false)))
                            {
                                sw.WriteLine(bill_list[4 * i + 1]);
                                sw.WriteLine(sn);
                                sw.WriteLine(money.ToString());
                                sw.WriteLine(Convert.ToUInt64(bill_list[4 * i]).ToString());
                            }
                            flush();
                        }
                        catch(Exception ex)
                        {
                            debug.Items.Add("无效交易");
                        }
                    }
                }
            }
        }

        private void ShowInfo(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Path.path + current_user.GetUserKeys()[1].ToString() + "\\" + check.Text + ".cash", new UTF8Encoding(false)))
                {
                    string show = "";
                    show += "<代币承诺><CM>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<序列号><SN>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<金额><v>\n";
                    show += sr.ReadLine() + "\n\n";
                    show += "<随机数><r>\n";
                    show += sr.ReadLine();
                    info.Text = show;
                }
            }
            catch(Exception ex)
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
