using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using System.Configuration;
using WindowsFormsApplication1.Properties;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string ip = "192.168.201.132";
        string username = "key1234";
        string pwd = "1234";
        public Form1()
        {
            InitializeComponent();
            
            pwd = txtPwd.Text;
            ip = txtIP.Text.Trim();
            username = txtUsername.Text;

        }
        string ssh_sendcommand(string c) {
            string re = "";
            c = c + "&& echo 成功 || echo 失败";
            using (var sshclient = new SshClient(ip, username, pwd))
            {
                try
                {
                    sshclient.Connect();
                    re = sshclient.CreateCommand(c).Execute();
                    sshclient.Disconnect();
                }
                catch { MessageBox.Show("连接失败"); }

            }
            if (re=="")
                return ("opps");
            else
                return re;

        }
      
        private void sshconnect(string[] args)
        {

            // Setup Credentials and Server Information
            ConnectionInfo ConnNfo = new ConnectionInfo(ip, 22, username,
                new AuthenticationMethod[]{
                new PasswordAuthenticationMethod(username,pwd),
                }
            );

            // Execute a (SHELL) Command - prepare upload directory
            using (var sshclient = new SshClient(ConnNfo))
            {
                sshclient.Connect();
                using (var cmd = sshclient.CreateCommand("mkdir -p /tmp/uploadtest && chmod +rw /tmp/uploadtest"))
                {
                    cmd.Execute();
                    Console.WriteLine("Command>" + cmd.CommandText);
                    Console.WriteLine("Return Value = {0}", cmd.ExitStatus);
                }
                sshclient.Disconnect();
            }

            // Upload A File
            using (var sftp = new SftpClient(ConnNfo))
            {
                string uploadfn = "Renci.SshNet.dll";

                sftp.Connect();
                sftp.ChangeDirectory("/tmp/uploadtest");
                using (var uplfileStream = System.IO.File.OpenRead(uploadfn))
                {
                    sftp.UploadFile(uplfileStream, uploadfn, true);
                }
                sftp.Disconnect();
            }

            // Execute (SHELL) Commands
            using (var sshclient = new SshClient(ConnNfo))
            {
                sshclient.Connect();

                // quick way to use ist, but not best practice - SshCommand is not Disposed, ExitStatus not checked...
                Console.WriteLine(sshclient.CreateCommand("cd /tmp && ls -lah").Execute());
                Console.WriteLine(sshclient.CreateCommand("pwd").Execute());
                Console.WriteLine(sshclient.CreateCommand("cd /tmp/uploadtest && ls -lah").Execute());
                sshclient.Disconnect();
            }
            Console.ReadKey();
        }
        /// <summary>
        /// 实际上只是检测一下连接是否成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connect_Click(object sender, EventArgs e)
        {
            if (ip == "" || username == "" || pwd =="") { MessageBox.Show("请输入完整信息"); }
            else
            {
                string re = ssh_sendcommand("cd /");
                switch (re)
                    {
                    case "opps":
                        MessageBox.Show("连接失败");
                        break;
                    case "失败":
                        MessageBox.Show("连接失败");
                        break;
                    default:
                        Settings.Default["ip"] = ip;
                        Settings.Default["pwd"] = pwd;
                        Settings.Default["username"] = username;
                        Settings.Default.Save();
                        MessageBox.Show("连接成功");
                        re = ssh_sendcommand("tcpreplay");
                        if (re == "失败") { MessageBox.Show("服务器端软件安装失败"); }
                        break;

                }

      

            }
        
            
  
            
            
        }



        private void button1_Click(object sender, EventArgs e)
        {
          
            MessageBox.Show("saved");






        }

       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                Boxreturnresult.Text += "command:" + textBox1.Text+ "\r\n";
                string a = ssh_sendcommand(textBox1.Text);
                Boxreturnresult.Text += a+ "\r\n";
                if (Boxreturnresult.Text.Length > 10000) { Boxreturnresult.Text = ""; }
                textBox1.Text = "";
            }
        }

        private void txtIP_TextChanged(object sender, EventArgs e)
        {
            ip = txtIP.Text.Trim();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            username = txtUsername.Text;
        }

        private void txtPwd_TextChanged(object sender, EventArgs e)
        {
            pwd = txtPwd.Text;
        }

    

     

        private void Form1_Load(object sender, EventArgs e)
        {
            txtIP.Text = Settings.Default["ip"].ToString();
            txtPwd.Text = Settings.Default["pwd"].ToString();
            txtUsername.Text = Settings.Default["username"].ToString();
        }
    }
}
