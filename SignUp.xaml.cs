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
using UserGroup;
using Client;

namespace MashCash
{
    /// <summary>
    /// SignUp.xaml 的交互逻辑
    /// </summary>
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void SignUpButton(object sender, RoutedEventArgs e)
        {
            User user = new User(username.Text);
            string path = Path.path + username.Text + ".key";
            UTF8Encoding utf8 = new UTF8Encoding(false);

            //本地保存公私钥
            using (StreamWriter sw = new StreamWriter(path, false, utf8))
            {
                sw.WriteLine("<" + username.Text + ">");
                sw.WriteLine(user.GetUserKeys()[0].ToString());
                sw.WriteLine(user.GetUserKeys()[1].ToString());
            }

            //远程注册请求
            string str = "<SignUp>" + user.GetUserKeys()[1].ToString();
            string recv = SocketClass.Send(str);
            if(recv.CompareTo("sign up success") == 0)
            {
                Close();
            }
            else
            {
                username.Text = recv;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
