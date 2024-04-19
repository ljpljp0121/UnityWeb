using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeachTCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建套接字Socket
            Socket socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //用Bind方法绑定套接字和地址
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                socketTCP.Bind(ipPoint);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
                return;
            }
            //用Listen方法监听
            socketTCP.Listen(1024);
            Console.WriteLine("服务端绑定监听结束，等待客户端连入");
            //用Accept方法等待客户端连接
            //会卡住等待连接，阻塞式
            //建立连接，accept返回一个新的套接字
            Socket socketClient =  socketTCP.Accept();
            Console.WriteLine("客户端连入");
            //用send和receive来手法数据
            //发送
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.playerID = 666;
            playerInfo.playerData = new PlayerData();
            playerInfo.playerData.name = "我是廖建鹏";
            playerInfo.playerData.lev = 20;
            playerInfo.playerData.atk = 88;

            socketClient.Send(playerInfo.WriteBytes());
            //接受
            byte[] result = new byte[1024];
            //返回值为接受到的字节数
            int receiveNum = socketClient.Receive(result);
            Console.WriteLine("接受了{0}发来的消息:{1}",
                socketClient.RemoteEndPoint.ToString(),
                Encoding.UTF8.GetString(result,0,receiveNum));
            //释放连接
            socketClient.Shutdown(SocketShutdown.Both);
            //关闭套接字
            socketClient.Close();

            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
