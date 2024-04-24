using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeachUdpAsyncServer
{
    internal class ServerSocket
    {
        private bool isClose;
        private Socket socket;

        //通过记录谁给服务端发了消息 ，记录ip和端口号
        private Dictionary<string, Client> clientDic = new Dictionary<string, Client>();
        //接收消息容器
        private byte[] cacheBytes = new byte[512];

        public void Start(string ip, int port)
        {
            //通过记录谁发了消息 把它的 ip和端口记下来
            EndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //声明一个用于UDP通信的Socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipPoint);
                isClose = false;
                //消息接受的处理线程
                socket.BeginReceiveFrom(cacheBytes, 0, cacheBytes.Length, SocketFlags.None, ref ipPoint, ReceiveMsg, ipPoint);
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
        private void ReceiveMsg(IAsyncResult result)
        {
            //接受消息容器
            //记录谁发的
            EndPoint ipPoint = result.AsyncState as IPEndPoint;

            string ip = (ipPoint as IPEndPoint).Address.ToString();
            int port = (ipPoint as IPEndPoint).Port;
            string strID = ip + port;//拼接成一个唯一ID
            try
            {
                socket.EndReceiveFrom(result, ref ipPoint);
                //判断有没有记录这个客户端信息 如果有 用它直接处理消息
                if (clientDic.ContainsKey(strID))
                {
                    clientDic[strID].ReceiveMsg(cacheBytes);
                }
                else
                {
                    clientDic.Add(strID, new Client(ip, port));
                    clientDic[strID].ReceiveMsg(cacheBytes);
                }
                //继续接受消息
                socket.BeginReceiveFrom(cacheBytes, 0, cacheBytes.Length, SocketFlags.None, ref ipPoint, ReceiveMsg, ipPoint);
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

        /// <summary>
        /// 指定发送消息给某个目标
        /// </summary>
        /// <param name="info">消息</param>
        /// <param name="ipPoint">目标</param>
        public void SendTo(BaseInfo info, IPEndPoint ipPoint)
        {
            try
            {
                byte[] bytes = info.WriteBytes();
                socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, ipPoint, (result) =>
                {
                    try
                    {
                        socket.EndSendTo(result);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("发送消息报错 " + ex.ErrorCode + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("发送消息报错(可能是序列化问题) " + ex.Message);
                    }
                }, null);
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
                socket = null;
            }
        }

        //移除客户端信息
        public void RemoveClient(string clientID)
        {
            if (clientDic.ContainsKey(clientID))
            {
                Console.WriteLine("客户端{0}被移除了", clientDic[clientID].clientIPAndPort);
                clientDic.Remove(clientID);
            }
        }
    }
}
