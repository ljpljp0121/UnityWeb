using System.Net.Sockets;

namespace TeachTcpServerAsync
{
    internal class ClientSocket
    {
        public Socket socket;
        public int clientID;
        private static int CLIENT_BEGIN_ID = 1;
        //缓存的字节数组和长度
        private byte[] cacheBytes = new byte[1024];
        private int cacheNum = 0;

        //上一次收到消息的时间
        private long frontTime = -1;
        //超时时间
        private static int TIME_OUT_TIME = 10;


        public ClientSocket(Socket socket)
        {
            this.clientID = CLIENT_BEGIN_ID++;
            this.socket = socket;

            //接收消息
            this.socket.BeginReceive(cacheBytes, cacheNum, cacheBytes.Length, SocketFlags.None, ReceiveCallBack, null);
            ThreadPool.QueueUserWorkItem(CheckTimeOut);
        }

        //间隔一段时间检查超时 ， 如果超时就主动断开该客户端的连接
        private void CheckTimeOut(object obj)
        {
            while (this.socket != null && this.socket.Connected)
            {
                if (frontTime != -1 &&
                    DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
                {
                    Program.serverSocket.CloseClientSocket(this);
                    //break;
                }
                Thread.Sleep(5000);
            }
        }

        //接受回调函数
        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                if (this.socket != null && this.socket.Connected)
                {
                    //消息成功
                    int num = this.socket.EndReceive(result);
                    //处理分包粘包
                    HandleReceiveMsg(num);
                    this.socket.BeginReceive(cacheBytes, cacheNum, cacheBytes.Length, SocketFlags.None, ReceiveCallBack, this.socket);
                }
                else
                {
                    Console.WriteLine("没有连接,停止收消息");
                    Program.serverSocket.CloseClientSocket(this);
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("接受消息出错:" + e.SocketErrorCode + e.Message);
                Program.serverSocket.CloseClientSocket(this);
            }
        }


        //发送
        public void Send(BaseInfo msg)
        {
            if (socket != null && this.socket.Connected)
            {
                byte[] bytes = msg.WriteBytes();
                this.socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallBack, null);
            }
            else
            {
                Program.serverSocket.CloseClientSocket(this);
            }
        }
        //发送回调函数
        private void SendCallBack(IAsyncResult result)
        {
            try
            {
                if (socket != null && socket.Connected)
                    this.socket.EndSend(result);
                else
                    Program.serverSocket.CloseClientSocket(this);
            }
            catch (SocketException e)
            {
                Console.WriteLine("发送消息出错:" + e.SocketErrorCode + e.Message);
                Program.serverSocket.CloseClientSocket(this);
            }
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        private void HandleReceiveMsg(int receiveNum)
        {
            int msgID = 0;
            int msgLength = 0;
            int nowIndex = 0;
            //由于消息接收后是直接存储在 cacheBytes中的 所以不需要进行什么拷贝操作
            //收到消息的字节数量
            cacheNum += receiveNum;
            while (true)
            {
                //每次将长度设置为-1 是避免上一次解析的数据 影响这一次的判断
                msgLength = -1;
                if (cacheNum - nowIndex >= 8)
                {
                    //解析ID
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    //解析长度
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                }

                if (cacheNum - nowIndex >= msgLength && msgLength != -1)
                {
                    //解析消息体
                    BaseInfo baseMsg = null;
                    switch (msgID)
                    {
                        case 1001:
                            baseMsg = new PlayerInfo();
                            baseMsg.ReadBytes(cacheBytes, nowIndex);
                            break;
                        case 1003:
                            baseMsg = new QuitMsg();
                            //由于该消息没有消息体 所以都不用反序列化
                            break;
                        case 999:
                            baseMsg = new Heart();
                            //由于该消息没有消息体 所以都不用反序列化
                            break;
                    }
                    if (baseMsg != null)
                    {
                        ThreadPool.QueueUserWorkItem(HandleMsg, baseMsg);
                    }
                    nowIndex += msgLength;
                    if (nowIndex == cacheNum)
                    {
                        cacheNum = 0;
                        break;
                    }
                }
                else
                {
                    //如果不满足 证明有分包 
                    //那么我们需要把当前收到的内容 记录下来
                    //有待下次接受到消息后 再做处理
                    //receiveBytes.CopyTo(cacheBytes, 0);
                    //cacheNum = receiveNum;
                    //如果进行了 id和长度的解析 但是 没有成功解析消息体 那么我们需要减去nowIndex移动的位置
                    if (msgLength != -1)
                        nowIndex -= 8;
                    //就是把剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                    Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                    cacheNum = cacheNum - nowIndex;
                    break;
                }
            }
        }

        //线程池处理客户端信息
        private void HandleMsg(object obj)
        {
            BaseInfo info = obj as BaseInfo;
            if (info is PlayerInfo)
            {
                PlayerInfo playerInfo = (PlayerInfo)info;
                Console.WriteLine(playerInfo.playerID);
                Console.WriteLine(playerInfo.playerData.name);
                Console.WriteLine(playerInfo.playerData.lev);
                Console.WriteLine(playerInfo.playerData.atk);
            }
            else if (info is QuitMsg)
            {
                //收到断开连接消息 将自己添加到待删除列表
                Program.serverSocket.CloseClientSocket(this);
            }
            else if (info is Heart)
            {
                //收到心跳消息 记录收到心跳消息的时间
                frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                Console.WriteLine("收到心跳消息");
            }
        }
    }
}
