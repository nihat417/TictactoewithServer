using System.Net.Sockets;
using System.Text;

namespace Server.Repository
{
    class Game
    {
        private TcpClient clientSocket;
        private NetworkStream networkStream;
        private byte[] buffer;
        private char[,] board;
        private bool turn;

        public Game(TcpClient clientSocket)
        {
            this.clientSocket = clientSocket;
            networkStream = clientSocket.GetStream();
            buffer = new byte[clientSocket.ReceiveBufferSize];
            board = new char[3, 3];
            turn = true;
        }

        public void Start()
        {
            Send("Welcome Tic Tac Toe Game");

            while (true)
            {
                int bytes = networkStream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer, 0, bytes);
                data = data.Replace("\0", "");

                if (data.ToLower() == "exit")
                {
                    Send("Game ended");
                    break;
                }

                if (data.Length == 3)
                {
                    int row = int.Parse(data[0].ToString());
                    int col = int.Parse(data[2].ToString());

                    if (board[row, col] == '\0')
                    {
                        char mark = turn ? 'X' : 'O';
                        board[row, col] = mark;
                        turn = !turn;
                        SendBoard();
                        CheckWin(mark);
                    }
                    else
                    {
                        Send("Invalid move");
                    }
                }
                else
                {
                    Send("Invalid input");
                }
            }

            networkStream.Close();
            clientSocket.Close();
        }

        private void Send(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            networkStream.Write(data, 0, data.Length);
        }

        private void SendBoard()
        {
            string boardString = "";

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    boardString += board[i, j];
                    if (j < 2)
                    {
                        boardString += "|";
                    }
                }
                if (i < 2)
                {
                    boardString += "\n-+-+-\n";
                }
            }

            Send(boardString);
        }

        private void CheckWin(char mark)
        {
            if (board[0, 0] == mark && board[0, 1] == mark && board[0, 2] == mark ||
                board[1, 0] == mark && board[1, 1] == mark && board[1, 2] == mark ||
                board[2, 0] == mark && board[2, 1] == mark && board[2, 2] == mark ||
                board[0, 0] == mark && board[1, 0] == mark && board[2, 0] == mark ||
                board[0, 1] == mark && board[1, 1] == mark && board[2, 1] == mark ||
                board[0, 2] == mark && board[1, 2] == mark && board[2, 2] == mark ||
                board[0, 0] == mark && board[1, 1] == mark && board[2, 2] == mark ||
                board[0, 2] == mark && board[1, 1] == mark && board[2, 0] == mark)
            {
                Send(mark + " wins!");
                board = new char[3, 3];
                turn = true;
                SendBoard();
            }
            else if (board[0, 0] != '\0' && board[0, 1] != '\0' && board[0, 2] != '\0' &&
                     board[1, 0] != '\0' && board[1, 1] != '\0' && board[1, 2] != '\0' &&
                     board[2, 0] != '\0' && board[2, 1] != '\0' && board[2, 2] != '\0')
            {
                Send("tie");
                board = new char[3, 3];
                turn = true;
                SendBoard();
            }
        }
    }
}
