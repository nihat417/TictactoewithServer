using Server.Repository;
using System.Net;
using System.Net.Sockets;


namespace TicTacToeServer
{
    class Program
    {
        static void Main()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 12345);
            serverSocket.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client connected");

                Game game = new Game(clientSocket);
                Thread thread = new Thread(new ThreadStart(game.Start));
                thread.Start();
            }
        }
    }   
}
