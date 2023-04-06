using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Reflection;
using System.Drawing;
using System.Windows.Media;

namespace Client1;


public partial class MainWindow : Window
{
    private enum Player { X, O };
    private Player currentPlayer;
    private char[] gameBoard = new char[9];

    private TcpClient client;
    private NetworkStream stream;
    private readonly Button[] buttons;


    public MainWindow()
    {
        InitializeComponent();
        ClientConnect();
        buttons = new[] { btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9 };
    }

    private void ClientConnect()
    {
        currentPlayer = Player.X;

        client = new TcpClient();
        client.Connect(IPAddress.Parse("127.0.0.1"), 12345);

        stream = client.GetStream();

        var reader = new Thread(Read);
        reader.Start();
    }

    private void Read()
    {
        byte[] data = new byte[256];
        string message;

        while (true)
        {
            try
            {
                int bytes = stream.Read(data, 0, data.Length);
                message = Encoding.UTF8.GetString(data, 0, bytes);

                if (message.StartsWith("move"))
                {
                    int index = int.Parse(message.Split()[1]);

                    Dispatcher.Invoke(() =>
                    {
                        if (currentPlayer == Player.X)
                        {
                            buttons[index].Content = "O";
                            gameBoard[index] = 'O';
                        }
                        else
                        {
                            buttons[index].Content = "X";
                            gameBoard[index] = 'X';
                        }
                    });

                    CheckGameOver();
                }
                else if (message.StartsWith("winner"))
                {
                    string winner = message.Split()[1];
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"winner is player {winner} ");
                        ResetGame();
                    });
                }
                else if (message.StartsWith("tie"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("tie");
                        ResetGame();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                break;
            }
        }
        stream.Close();
        client.Close();
    }

    private void CheckGameOver()
    {
        char[] playerSymbols = { 'X', 'O' };
        foreach (char symbol in playerSymbols)
        {
            if ((gameBoard[0] == symbol && gameBoard[1] == symbol && gameBoard[2] == symbol) ||
                (gameBoard[3] == symbol && gameBoard[4] == symbol && gameBoard[5] == symbol) ||
                (gameBoard[6] == symbol && gameBoard[7] == symbol && gameBoard[8] == symbol) ||
                (gameBoard[0] == symbol && gameBoard[3] == symbol && gameBoard[6] == symbol) ||
                (gameBoard[1] == symbol && gameBoard[4] == symbol && gameBoard[7] == symbol) ||
                (gameBoard[2] == symbol && gameBoard[5] == symbol && gameBoard[8] == symbol) ||
                (gameBoard[0] == symbol && gameBoard[4] == symbol && gameBoard[8] == symbol) ||
                (gameBoard[2] == symbol && gameBoard[4] == symbol && gameBoard[6] == symbol))
            {
                string winner = (symbol == 'X') ? "X" : "O";
                MessageBox.Show($"winner {winner}");
                SendMessage($"winner {winner}");
                ResetGame();
                return;
            }
        }

        bool isTie = true;
        foreach (char c in gameBoard)
        {
            if (c == '\0')
            {
                isTie = false;
                break;
            }
        }

        if (isTie)
        {
            SendMessage("tie ");
            MessageBox.Show("tie");
            ResetGame();
            return;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            int index = int.TryParse(button?.Tag?.ToString(), out var result) ? result : 0;

            if (gameBoard[index] != '\0')
                return;

            if (currentPlayer == Player.X)
            {
                if(sender is Button button1)
                {
                    button1.Content = "X";
                    button1.Foreground = Brushes.Red;
                    gameBoard[index] = 'X';
                }
            }
            else
            {
                if (sender is Button button1)
                {
                    button1.Content = "O";
                    button1.Foreground = Brushes.Blue;
                    gameBoard[index] = 'O';
                }
            }

            CheckGameOver();

            SendMessage($"move {index}");

            currentPlayer = (currentPlayer == Player.X) ? Player.O : Player.X;

        }
            
    }

    private void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetGame();
    }

    private void ResetGame()
    {
        for (int i = 0; i < 9; i++)
        {
            buttons[i].Content = "";
            gameBoard[i] = '\0';
        }

        currentPlayer = Player.X;
    }
}

