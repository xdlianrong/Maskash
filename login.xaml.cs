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

namespace MashCash
{
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
            flush();
        }

        private void flush()
        {
            DirectoryInfo di = new DirectoryInfo(Path.path);
            FileInfo[] files = di.GetFiles();
            users.Items.Clear();
            foreach (FileInfo fi in files)
            {
                if (fi.Extension.ToLower() == ".key")
                {
                    users.Items.Add(fi.Name.Split('.')[0]);
                }
            }
            if(users.Items.Count == 0)
            {
                SignInButton.IsEnabled = false;
            }
            else
            {
                SignInButton.IsEnabled = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SignUp(object sender, RoutedEventArgs e)
        {
            //注册实例化
            SignUp signUp = new SignUp();
            signUp.ShowDialog();
            flush();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            //登陆实例化
            FileInfo file = new FileInfo(Path.path + users.Text + ".key");
            if (file.Exists)
            {
                using (StreamReader sr = new StreamReader(Path.path + users.Text + ".key", new UTF8Encoding(false)))
                {
                    string uname = sr.ReadLine();
                    string privateKey = sr.ReadLine();
                    string publicKey = sr.ReadLine();


                    //远程登陆请求
                    string str = "<SignIn>" + publicKey;
                    string recv = SocketClass.Send(str);
                    if (recv.CompareTo("sign in success") == 0)
                    {
                        ShowInTaskbar = false;
                        WindowState = WindowState.Minimized;
                        Visibility = Visibility.Hidden;
                        // 进入登陆界面
                        Console.WriteLine("进入登陆界面");
                        Cash cash = new Cash(new User(uname, BigInt.HexCode(privateKey), BigInt.HexCode(publicKey)));
                        cash.Show();
                    }
                    else if(recv.CompareTo("admin sign in success") == 0)
                    {
                        ShowInTaskbar = false;
                        WindowState = WindowState.Minimized;
                        Visibility = Visibility.Hidden;
                        // 进入登陆界面
                        Console.WriteLine("进入监管者登陆界面");
                        Regulator regulator = new Regulator();
                        regulator.Show();
                    }
                    else
                    {
                        users.Text = recv;
                    }
                }
            }
            
        }
    }
}
