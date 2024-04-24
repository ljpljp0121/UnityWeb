using System.Net.Sockets;

namespace TeachUdpServerExercise
{
    internal class Program
    {
        public static ServerSocket serverSocket;
        static void Main(string[] args)
        {
            serverSocket = new ServerSocket();
            serverSocket.Start("127.0.0.1", 8080);
            Console.WriteLine("服务器启动成功");
            while (true)
            {
                string input = Console.ReadLine();
                if (input.Substring(0, 2) == "B:")
                {
                    PlayerInfo playerInfo = new PlayerInfo();
                    playerInfo.playerData = new PlayerData();
                    playerInfo.playerID = 134;
                    playerInfo.playerData.name = "廖建鹏的服务器";
                    playerInfo.playerData.atk = 100;
                    playerInfo.playerData.lev = 99;
                    playerInfo.playerData.sex = true;
                    serverSocket.Broadcast(playerInfo);
                }
            }
        }
    }
}
