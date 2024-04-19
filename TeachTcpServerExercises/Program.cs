using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeachTcpServerExercises
{
    class Program
    {
        static Socket? socket;
        static List<Socket> clientSockets = new List<Socket>();

        static bool isClose = false;
        static void Main(string[] args)
        {
            //1.建立Socket绑定 监听
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                socket.Bind(ipPoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ErrorCode);
                return;
            }
            socket.Listen(1024);
            //2.等待客户端连接（需要特别处理的地方）
            Thread acceptThread = new Thread(AcceptClientConnect);
            acceptThread.Start();
            //3.收发消息（需要特别处理的地方）
            //发送消息
            
            //接受消息
            Thread receiveThread = new Thread(ReceiveMsg);
            receiveThread.Start();
            //4.关闭相关
            while (true)
            {
                string input = Console.ReadLine();
                //关闭服务端 断开所有链接
                if (input == "Quit")
                {
                    isClose = true;
                    for(int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].Shutdown(SocketShutdown.Both);
                        clientSockets[i].Close();
                    }
                    clientSockets.Clear();
                    break;
                }
                //定义一个规则 广播消息 让所有客户端收到服务端发送消息
                else if (input.Substring(0, 2) == "B:")
                {
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].Send(Encoding.UTF8.GetBytes(input.Substring(2)));                  
                    }
                }
            }
        }
        //多线程接受多个客户端的连接请求
        static void AcceptClientConnect()
        {
            while (!isClose)
            {
                Socket clientSocket = socket.Accept();
                clientSockets.Add(clientSocket);
                clientSocket.Send(Encoding.UTF8.GetBytes("欢迎你连入服务端"));
            }
        }
        //多线程不断循环接受客户端传来的消息
        static void ReceiveMsg()
        {
            Socket clientSocket;
            byte[] result = new byte[1024 * 1024];
            int receiveNum;
            int i;
            while (!isClose)
            {
                for (i = 0; i < clientSockets.Count; i++)
                {
                    clientSocket = clientSockets[i];
                    //判断该socket是否有可以接受的消息 返回值为字节数
                    if (clientSocket.Available > 0)
                    {
                        receiveNum = clientSocket.Receive(result);
                        //如果直接收到消息 就处理 可能造成问题
                        //不能够及时的处理别人的消息
                        //为了不影响别人消息的处理 使用线程 为了节约线程开销 使用线程池处理
                        ThreadPool.QueueUserWorkItem(HandleMsg,(clientSocket,Encoding.UTF8.GetString(result,0,receiveNum)));

                    }
                }
            }
        }
        //线程池 处理接收到的消息
        static void HandleMsg(object obj)
        {
            (Socket s, string str) info = ((Socket s, string str))obj;
            Console.WriteLine("收到客户端{0}发来的消息 :{1}", info.s.RemoteEndPoint,info.str);
        }
    }
}
