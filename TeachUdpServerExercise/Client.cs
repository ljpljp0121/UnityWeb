using System.Net;

namespace TeachUdpServerExercise
{
    //用于记录和服务器通信过的客户端的IP和端口
    internal class Client
    {
        public IPEndPoint clientIPAndPort;
        public string clientStrID;
        //上一次接受的时间
        public long frontTime = -1;

        public Client(string ip, int port)
        {
            clientStrID = ip + port;
            clientIPAndPort = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void ReceiveMsg(byte[] bytes)
        {
            //可能还没处理完就被别的线程覆盖,所以先把信息拷贝一份
            byte[] cacheBytes = new byte[512];
            bytes.CopyTo(cacheBytes, 0);
            //记录收到消息的 系统时间
            frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            ThreadPool.QueueUserWorkItem(ReceiveHandle, cacheBytes);
        }

        //多线程处理消息
        private void ReceiveHandle(object obj)
        {
            //很不安全 所以要判断一下防止恶意信息发送
            try
            {
                byte[] bytes = obj as byte[];

                int nowIndex = 0;
                //先处理ID
                int msgID = BitConverter.ToInt32(bytes, nowIndex);
                nowIndex += 4;
                //再处理长度
                int msgLength = BitConverter.ToInt32(bytes, nowIndex);
                nowIndex += 4;
                //再解析消息体
                switch (msgID)
                {
                    case 1001:
                        PlayerInfo playerInfo = new PlayerInfo();
                        playerInfo.ReadBytes(bytes, nowIndex);
                        Console.WriteLine(playerInfo.playerID);
                        Console.WriteLine(playerInfo.playerData.name);
                        Console.WriteLine(playerInfo.playerData.atk);
                        Console.WriteLine(playerInfo.playerData.lev);
                        break;
                    case 1003:
                        QuitMsg quitMsg = new QuitMsg();
                        //没有消息体 不用反序列化
                        //TODO:处理退出
                        Program.serverSocket.RemoveClient(clientStrID);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("处理消息是出错" + ex.Message);
                Program.serverSocket.RemoveClient(clientStrID);
            }
        }
    }
}
