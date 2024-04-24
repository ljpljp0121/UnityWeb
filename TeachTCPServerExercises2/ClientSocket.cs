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
        //缓存的字节数组和长度 处理分包
        private byte[] cacheBytes = new byte[1024 * 1024];
        private int cacheNum = 0;

        private static int CLIENT_BEGIN_ID = 1;
        public int clientID;
        public Socket socket;


        //上一次收到消息的时间
        private long frontTime = -1;
        //超时时间
        private static int TIME_OUT_TIME = 10;



        public ClientSocket(Socket socket)
        {
            this.socket = socket;
            clientID = CLIENT_BEGIN_ID;
            ++CLIENT_BEGIN_ID;
            //方便理解所以开线程专门计时 但比较消耗性能 不建议
            //ThreadPool.QueueUserWorkItem(CheckTimeOut);
        }
        //间隔一段时间检查超时 ， 如果超时就主动断开该客户端的连接
        private void CheckTimeOut(/*object obj*/)
        {
            // while (IsConnected)
            //{
            if (frontTime != -1 &&
                DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
            {
                Program.socket.AddDelSocket(this);
                //break;
            }
            //Thread.Sleep(5000);
            //}
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
            if (IsConnected)
            {
                try
                {
                    socket.Send(info.WriteBytes());
                }
                catch (Exception e)
                {
                    Console.WriteLine("发送消息报错 " + e.Message);
                    Program.socket.AddDelSocket(this);
                }
            }
            else
            {
                Program.socket.AddDelSocket(this);
            }
        }
        //接收客户端信息
        public void Receive()
        {
            if (!IsConnected)
            {
                return;
                Program.socket.AddDelSocket(this);
            }
            try
            {
                if (socket.Available > 0)
                {
                    byte[] result = new byte[1024 * 5];
                    int receiveNum = socket.Receive(result);
                    HandleReceiveMsg(result, receiveNum);
                    ////先读出四个字节 转为ID 才知道用哪一个类型去处理反序列化
                    //int infoID = BitConverter.ToInt32(result, 0);
                    //BaseInfo info = null;
                    //switch (infoID)
                    //{
                    //    case 1001:
                    //        info = new PlayerInfo();
                    //        info.ReadBytes(result,4);
                    //        break;
                    //}
                    //if (info == null)
                    //    return;
                    //ThreadPool.QueueUserWorkItem(HandleMsg, info);
                }

                //检测是否超时
                CheckTimeOut();
            }
            catch (Exception e)
            {
                Console.WriteLine("接受消息报错 " + e.Message);
                //解析错误也认为要将其压入待删除socket
                Program.socket.AddDelSocket(this);
            }
        }
        //处理消息 解决分包 粘包
        private void HandleReceiveMsg(byte[] receiveBytes, int receiveNum)
        {
            int msgID = 0;
            int msgLength = 0;
            int nowIndex = 0;

            //收到消息是查看 缓存是否为空 有则拼接到后面
            receiveBytes.CopyTo(cacheBytes, cacheNum);
            cacheNum += receiveNum;

            while (true)
            {
                //每次将长度设置为-1 避免上一次解析数据影响这一次判断
                msgLength = -1;
                //处理解析一条消息
                if (cacheNum - nowIndex >= 8)
                {
                    //解析ID
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    //解析长度
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                }
                //长度大于Length才解析
                if (cacheNum - nowIndex >= msgLength && msgLength != -1)
                {
                    //解析消息体
                    BaseInfo baseInfo = null;
                    switch (msgID)
                    {
                        case 1001:
                            baseInfo = new PlayerInfo();
                            baseInfo.ReadBytes(cacheBytes, nowIndex);
                            break;
                        case 1003:
                            baseInfo = new QuitMsg();
                            //由于该消息没有消息体 所以不用序列化
                            break;
                        case 999:
                            baseInfo = new Heart();
                            break;
                    }
                    if (baseInfo != null)
                        ThreadPool.QueueUserWorkItem(HandleMsg, baseInfo);
                    nowIndex += msgLength;
                    if (nowIndex == cacheNum)
                    {
                        cacheNum = 0;
                        break;
                    }
                }
                else
                {
                    //还是不满足说明有分包，
                    //要把它保存下来下一次接收到消息将其拼接起来 再做处理
                    //receiveBytes.CopyTo(cacheBytes, 0);
                    //cacheNum = receiveNum;
                    //如果进行了id和长度解析 但是没有成功解析消息体 那么需要减去nowIndex移动位置
                    if (msgLength != -1)
                    {
                        nowIndex -= 8;
                    }
                    //将剩余没有解析的字节数组 留下 剔除已经解析的字节数组，留待下一条消息来拼接解析
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
                Program.socket.AddDelSocket(this);
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
