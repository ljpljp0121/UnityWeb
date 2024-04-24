using System.Net;
using System.Net.Sockets;

namespace TeachUdpServerExercise
{
    internal class ServerSocket
    {
        private bool isClose;
        private Socket socket;

        //通过记录谁给服务端发了消息 ，记录ip和端口号
        private Dictionary<string, Client> clientDic = new Dictionary<string, Client>();

        public void Start(string ip, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //声明一个用于UDP通信的Socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipPoint);
                isClose = false;
                //消息接受的处理线程
                ThreadPool.QueueUserWorkItem(ReceiveMsg);
                //定时检测超时线程
                ThreadPool.QueueUserWorkItem(CheckTimeOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CheckTimeOut(object? obj)
        {
            long nowTime = 0;
            List<string> delList = new List<string>();
            while (true)
            {
                //每20秒检测一次
                Thread.Sleep(20000);
                nowTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                foreach (Client client in clientDic.Values)
                {
                    //超过10秒没收到消息 将其加入待移除列表
                    if (nowTime - client.frontTime >= 10)
                    {
                        delList.Add(client.clientStrID);
                    }
                    //从待删除列表移除 超时客户端信息
                    for (int i = 0; i < delList.Count; i++)
                    {
                        RemoveClient(delList[i]);
                    }
                    delList.Clear();
                }
            }
        }

        //接收消息
        private void ReceiveMsg(object? obj)
        {
            //接受消息容器
            byte[] bytes = new byte[512];
            //记录谁发的
            EndPoint ipPoint = new IPEndPoint(IPAddress.Any, 0);
            //有IP和端口组成 作为Client唯一标识
            string strID = "";
            string ip;
            int port;
            //接受消息
            while (!isClose)
            {
                if (socket.Available > 0)
                {
                    lock (socket)
                        socket.ReceiveFrom(bytes, ref ipPoint);
                    //处理消息最好不要在这里直接处理 交给客户端对象处理
                    //收到消息时判断是不是记录了 客户端信息
                    ip = (ipPoint as IPEndPoint).Address.ToString();
                    port = (ipPoint as IPEndPoint).Port;
                    strID = ip + port;
                    //判断有没有记录客户端信息并处理
                    if (clientDic.ContainsKey(strID))
                    {
                        clientDic[strID].ReceiveMsg(bytes);
                    }
                    else
                    {
                        clientDic.Add(strID, new Client(ip, port));
                        clientDic[strID].ReceiveMsg(bytes);
                    }
                }
            }
        }

        /// <summary>
        /// 指定发送消息给某个目标
        /// </summary>
        /// <param name="info">消息</param>
        /// <param name="ipPoint">目标</param>
        public void SendTo(BaseInfo info, IPEndPoint ipPoint)
        {
            try
            {
                socket.SendTo(info.WriteBytes(), ipPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("发送消息报错 " + ex.ErrorCode + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送消息报错(可能是序列化问题) " + ex.Message);
            }
        }

        //广播消息
        public void Broadcast(BaseInfo info)
        {
            foreach (Client client in clientDic.Values)
            {
                SendTo(info, client.clientIPAndPort);
            }
        }

        //关闭Socket
        public void Close()
        {
            if (socket != null)
            {
                isClose = true;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        //移除客户端信息
        public void RemoveClient(string clientID)
        {
            if (clientDic.ContainsKey(clientID))
            {
                Console.WriteLine("客户端{0}被移除了" + clientDic[clientID].clientIPAndPort);
                clientDic.Remove(clientID);
            }
        }
    }
}
