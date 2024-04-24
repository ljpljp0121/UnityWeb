using System.Net.Sockets;
using System.Net;
using System.Text;

namespace TeachUdpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //创建套接字
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //绑定本机地址
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
            socket.Bind(ipPoint);
            Console.WriteLine("服务器开启");
            //接受消息
            byte[] bytes = new byte[512];
            //初始化不重要 因为之后使用里面的值会被覆盖
            EndPoint remoteIpPoint2 = new IPEndPoint(IPAddress.Any, 0);
            int length = socket.ReceiveFrom(bytes, ref remoteIpPoint2);
            Console.WriteLine((remoteIpPoint2 as IPEndPoint).Address.ToString() + "发来了" +
                Encoding.UTF8.GetString(bytes, 0, length));

            //发送到指定目标
            //IPEndPoint remoteIpPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            //指定要发送的字节数 和远程计算机的IP 和 端口
            //收到消息后 已经知道谁发消息来 所以可以直接发挥
            socket.SendTo(Encoding.UTF8.GetBytes("欢迎发送给服务器消息"), remoteIpPoint2);
            //释放关闭
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
