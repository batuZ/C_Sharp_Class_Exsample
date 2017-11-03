﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.ServiceProcess;

namespace AM_Node_Controller
{
    class Program
    {
        private static int port = 22215;
        private const int bufferSize = 4096;
        private static string serviceName = "am_node_server";
        private static List<string> cmdList;
        static void Main(string[] args)
        {
            ConsoleWin32Helper.init();

            //检测服务
            if (!IsServiceExisted())
            {
                string fff = "E:\test\aa.bat";
                //安装务服
                cmdList = new List<string>();
                //cmdList.Add($"@sc create {serviceName} binPath= {Application.ExecutablePath}");
                cmdList.Add($"SET appPath= {fff}");
                cmdList.Add($"sc create {serviceName} binPath= %appPath%");
                cmdList.Add($"sc config {serviceName} start= AUTO"); //start = DEMAND(手动);start= DISABLED(禁用)
                cmdList.Add($"net start {serviceName}");           //net stop {serviceName}
                cmd(cmdList);
            }
            /*  安装删除服务，参考：http://www.cnblogs.com/pingming/p/5108947.html
               %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe "E:\wwwroot\kjsmtt\wwwroot\KJLMManagerShareOutBonus\KJLMManagerShareOutBonus.exe"
               %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe /u "E:\wwwroot\kjsmtt\wwwroot\KJLMManagerShareOutBonus\KJLMManagerShareOutBonus.exe"
               */


            //删掉服务
            cmdList = new List<string>();
            cmdList.Add($"@sc stop {serviceName}");
            cmdList.Add($"@sc delete {serviceName}");
            cmd(cmdList);


            //启动
            Thread threadMonitorInput = new Thread(new ThreadStart(startServer));
            threadMonitorInput.Start();

            //保持
            while (true)
            {
                Application.DoEvents();
                Thread.Sleep(1000);
            }
        }

        static void startServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server running...");

            while (true)
            {
                //如果没有用户连接，则在这一步挂起，进入等待状态！
                TcpClient client = listener.AcceptTcpClient();
                string clientIP = client.Client.RemoteEndPoint.ToString();

                //如果有连接请求则进入这步，开始工作！
                NetworkStream clientStream = client.GetStream();

                byte[] buffer = new byte[bufferSize];
                int readBytes = clientStream.Read(buffer, 0, bufferSize);
                string request = Encoding.UTF8.GetString(buffer).Substring(0, readBytes);

                //执行操作
                Console.WriteLine(request);

                //回传消息
                //byte[] backData = Encoding.ASCII.GetBytes(request.ToUpper());
                //clientStream.Write(backData, 0, backData.Length);

                clientStream.Close();
            }
        }

        /// <summary>
        /// 判断服务是否存在
        /// </summary>
        /// <returns></returns>
        static bool IsServiceExisted()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower() == serviceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 调用CMD
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static bool cmd(List<string> str)
        {
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "cmd.exe";

            CmdProcess.StartInfo.CreateNoWindow = true;
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;
            CmdProcess.StartInfo.RedirectStandardOutput = true;
            CmdProcess.StartInfo.RedirectStandardError = true;
            
            CmdProcess.Start();
            foreach (string cmd in str)
            {
                CmdProcess.StandardInput.WriteLine(cmd);
            }
            CmdProcess.StandardInput.WriteLine("exit");
            CmdProcess.WaitForExit();
            return false;

            /** ex
            //创建执行CMD
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "cmd.exe";

            //配置开发方式输入输出错误
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            CmdProcess.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出   

            //执行cmd1
            CmdProcess.StartInfo.Arguments = "/c " + "=====cmd命令======";//“/C”表示执行完命令后马上退出  
            CmdProcess.Start();//执行  
            CmdProcess.StandardOutput.ReadToEnd();//输出  
            CmdProcess.WaitForExit();//等待程序执行完退出进程  
            CmdProcess.Close();//结束 
            
            //执行cmd2
            CmdProcess.StandardInput.WriteLine("notepad" + "&exit"); //向cmd窗口发送输入信息  
            CmdProcess.StandardInput.AutoFlush = true;  //提交  
            CmdProcess.Start();//执行  
            CmdProcess.StandardOutput.ReadToEnd();//输出  
            CmdProcess.WaitForExit();//等待程序执行完退出进程  
            CmdProcess.Close();//结束  
            */
        }
    }

    class ConsoleWin32Helper
    {
        static NotifyIcon _NotifyIcon = new NotifyIcon();
        static ConsoleWin32Helper()
        {
            //初始化托盘
            _NotifyIcon.Icon = new Icon("103.ico");
            _NotifyIcon.Visible = false;
            _NotifyIcon.Text = "AM_Node_Controller";

            //右键
            ContextMenu menu = new ContextMenu();
            _NotifyIcon.ContextMenu = menu;

            //右键成员
            //MenuItem item = new MenuItem();
            //item.Text = "退出";
            //item.Index = 0;
            //menu.MenuItems.Add(item);
            //item.Click += Item_Click;

            //事件
            _NotifyIcon.MouseDoubleClick += new MouseEventHandler(_NotifyIcon_MouseDoubleClick);

        }

        public static void init()
        {
            Console.Title = "AM_Node_Controller : " + getLoaclIP();
            ShowNotifyIcon();
            Hidden();
        }

        /// <summary>
        /// 右键事件：退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Item_Click(object sender, EventArgs e)
        {
            //  exit("AM_Node_Controller : IP");
        }

        /// <summary>
        /// 托盘显示气泡
        /// </summary>
        static void ShowNotifyIcon()
        {
            _NotifyIcon.Visible = true;
            _NotifyIcon.ShowBalloonTip(3000, "", "Automesh 节点管理器已启动。", ToolTipIcon.None);
        }

        /// <summary>
        /// 托盘双击事件：显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void _NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }
        /// <summary>
        /// 获取当前IP
        /// </summary>
        /// <returns></returns>
        static string getLoaclIP()
        {
            //当前IP
            IPAddress[] IP = Dns.GetHostAddresses(Dns.GetHostName());
            string temp = "";
            for (int i = 0; i < IP.Length; i++)
            {
                if (IP[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    temp = IP[i].ToString();
                    break;
                }
            }
            return temp;
        }


        #region 禁用关闭按钮

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("User32.dll", EntryPoint = "ShowWindow")]
        static extern bool ShowWindow(IntPtr hwind, int cmdShow);

        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        static extern bool SetForegroundWindow(IntPtr hwind);


        ///<summary>
        /// 禁用关闭按钮
        ///</summary>
        ///<param name="consoleName">控制台名字</param>
        static void DisableCloseButton(string title)//线程睡眠，确保closebtn中能够正常FindWindow，否则有时会Find失败。。 
        {
            Thread.Sleep(100);
            IntPtr windowHandle = FindWindow(null, title);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }

        /// <summary>
        /// 窗体是否存在
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        static bool IsExistsConsole(string title)
        {
            IntPtr windowHandle = FindWindow(null, title);
            if (windowHandle.Equals(IntPtr.Zero))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="title"></param>
        static void Hidden()
        {
            IntPtr ParenthWnd = new IntPtr(0);
            ParenthWnd = FindWindow(null, Console.Title);
            int normalState = 0;//窗口状态(隐藏)
            ShowWindow(ParenthWnd, normalState);
        }

        /// <summary>
        /// 恢复
        /// </summary>
        /// <param name="title"></param>
        static void Show()
        {
            IntPtr ParenthWnd = new IntPtr(0);
            ParenthWnd = FindWindow(null, Console.Title);
            int normalState = 9;//窗口状态(隐藏)
            ShowWindow(ParenthWnd, normalState);
        }
        #endregion

    }
}
