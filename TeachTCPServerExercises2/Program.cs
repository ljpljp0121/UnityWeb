using System.Net.Sockets;
using System.Text;

namespace TeachTCPServerExercises2
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket socket = new ServerSocket();
            socket.Start("127.0.0.1", 8080, 1024);
            Console.WriteLine("服务器开启成功");
            while (true)
            {
                string input = Console.ReadLine();
                //关闭服务端 断开所有链接
                if (input == "Quit")
                {
                    socket.Close();
                }
                //定义一个规则 广播消息 让所有客户端收到服务端发送消息
                else if (input.Substring(0, 2) == "B:")
                {
                    if (input.Substring(2) == "1001")
                    {
                        PlayerInfo player = new PlayerInfo();
                        player.playerID = 0101;
                        player.playerData = new PlayerData();
                        player.playerData.name = "服务器传来的消息";
                        player.playerData.lev = 10;
                        player.playerData.atk = 1111;
                        socket.Broadcast(player);
                    }
                }
            }
        }
    }
}
