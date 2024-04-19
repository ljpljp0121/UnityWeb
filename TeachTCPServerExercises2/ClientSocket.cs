using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TeachTCPServerExercises2
{
    //服务端连接客户端新产生Socket
    class ClientSocket
    {
        private static int CLIENT_BEGIN_ID = 1;
        public int clientID;
        public Socket socket;
        public ClientSocket(Socket socket)
        {
            this.socket = socket;
            clientID = CLIENT_BEGIN_ID;
            ++CLIENT_BEGIN_ID;
        }
        //是否是连接状态
        public bool IsConnected => this.socket.Connected;

        //关闭连接
        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        //发送给客户端信息
        public void Send(BaseInfo info)
        {
            if (socket != null)
            {
                try
                {
                    socket.Send(info.WriteBytes());
                }
                catch (Exception e)
                {
                    Console.WriteLine("发送消息报错 " + e.Message);
                    Close();
                }
            }
        }
        //接收客户端信息
        public void Receive()
        {
            if (socket == null)
                return;
            try
            {
                if (socket.Available > 0)
                {
                    byte[] result = new byte[1024 * 4];
                    int receiveNum = socket.Receive(result);
                    //先读出四个字节 转为ID 才知道用哪一个类型去处理反序列化
                    int infoID = BitConverter.ToInt32(result, 0);
                    BaseInfo info = null;
                    switch (infoID)
                    {
                        case 1001:
                            info = new PlayerInfo();
                            info.ReadBytes(result,4);
                            break;
                    }
                    if (info == null)
                        return;
                    ThreadPool.QueueUserWorkItem(HandleMsg, info);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("接受消息报错 " + e.Message);
                Close();
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
        }

    }
}
