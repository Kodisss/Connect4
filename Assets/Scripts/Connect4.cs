using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class Connect4 : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject yellowCoin;
    [SerializeField] private GameObject redCoin;

    [Header("UI")]
    [SerializeField] private TMP_Text playerDisplay;
    [SerializeField] private GameObject playingButtons;

    private string currentPlayer;
    private string winner;
    private string gameMode;

    private bool alreadyPlayed;
    private int IADifficulty;

    private const int ROWS = 6;
    private const int COLUMNS = 7;

    private Cell[,] board = new Cell[ROWS, COLUMNS];

    [SerializeField] private bool debug;

    // Start is called before the first frame update
    private void Start()
    {
        currentPlayer = "Yellow";
        alreadyPlayed = false;
        IADifficulty = (PlayerPrefs.GetInt("Difficulty") + 1) * 2;
        //Debug.Log("Difficulty = " + IADifficulty);
        gameMode = PlayerPrefs.GetString("GameMode");
        DisplayPlayer();
        InitializeBoard();
    }

    // Update is called once per frame
    private void Update()
    {
        if(gameMode == "IA")
        {
            if (currentPlayer == "Red" && !alreadyPlayed)
            {
                playingButtons.SetActive(false);
                Invoke(nameof(IAPlay), 1.5f); // if the gamemode is set to IA and if it's red's turn wait a sec and play
                alreadyPlayed = true;
            }
        }
    }

    private void InitializeBoard()
    {
        // Initialize the board with empty cells
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                board[row, col] = new Cell();
            }
        }
    }

    private void IAPlay()
    {
        int column = FindBestMove();
        PlaceCoin(column);

        playingButtons.SetActive(true);
        alreadyPlayed = false;
    }

    private int FindBestMove()
    {
        int bestScore = int.MinValue;
        int bestColumn = 0;
        int currentMove = 0;

        for (int col = 0; col < COLUMNS; col++)
        {
            currentMove = FirstValidRowInCol(col);
            if(debug) Debug.Log(currentMove);
            if (currentMove != -1)
            {
                // Simulate placing a coin in the current column
                board[currentMove, col].PlaceColor(currentPlayer);
                int score = Minimax(IADifficulty, false); // Adjust the depth value as desired

                if(debug) Debug.Log("We check col " + col + " score is " +  score);

                // Undo the move
                board[currentMove, col].Clear();

                // Update the best move if the current score is higher
                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = col;
                }
            }
        }

        return bestColumn;
    }

    private int FirstValidRowInCol(int col)
    {
        for (int row = 0; row < ROWS; row++)
        {
            if (board[row, col].IsValid()) return row;
        }
        return -1;
    }

    private int Minimax(int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || CheckWin("Yellow") || CheckWin("Red"))
        {
            // Evaluate the board state and return the score
            // Positive score favors the maximizing player (AI), negative score favors the minimizing player (player)
            return EvaluateBoard();
        }

        if (isMaximizingPlayer)
        {
            int bestScore = int.MinValue;

            for (int col = 0; col < COLUMNS; col++)
            {
                if (board[ROWS - 1, col].IsValid())
                {
                    // Simulate placing a coin in the current column
                    int row = GetNextAvailableRow(col);
                    board[row, col].PlaceColor(currentPlayer);
                    int score = Minimax(depth - 1, false);

                    // Undo the move
                    board[row, col].Clear();

                    // Update the best score if the current score is higher
                    bestScore = Mathf.Max(bestScore, score);
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int col = 0; col < COLUMNS; col++)
            {
                if (board[ROWS - 1, col].IsValid())
                {
                    // Simulate placing a coin in the current column
                    int row = GetNextAvailableRow(col);
                    board[row, col].PlaceColor(GetOpponentPlayer());
                    int score = Minimax(depth - 1, true);

                    // Undo the move
                    board[row, col].Clear();

                    // Update the best score if the current score is lower
                    bestScore = Mathf.Min(bestScore, score);
                }
            }

            return bestScore;
        }
    }

    private int GetNextAvailableRow(int col)
    {
        for (int row = 0; row < ROWS; row++)
        {
            if (board[row, col].IsValid())
            {
                return row;
            }
        }

        return -1; // Column is full
    }


    private int EvaluateBoard()
    {
        // Add your own evaluation logic here to assign scores to different board states
        // For simplicity, you can start with a basic scoring system based on the number of connected coins for each player

        int score = 0;

        // Evaluate horizontal connections
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                score += EvaluateLine(row, col, 0, 1); // Evaluate right
            }
        }

        // Evaluate vertical connections
        for (int row = 0; row < ROWS - 3; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                score += EvaluateLine(row, col, 1, 0); // Evaluate down
            }
        }

        // Evaluate diagonal (ascending) connections
        for (int row = 3; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                score += EvaluateLine(row, col, -1, 1); // Evaluate ascending diagonal
            }
        }

        // Evaluate diagonal (descending) connections
        for (int row = 0; row < ROWS - 3; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                score += EvaluateLine(row, col, 1, 1); // Evaluate descending diagonal
            }
        }

        return score;
    }

    private int EvaluateLine(int startRow, int startCol, int rowDirection, int colDirection)
    {
        int score = 0;
        int playerCount = 0;
        int opponentCount = 0;

        for (int step = 0; step < 4; step++)
        {
            int row = startRow + step * rowDirection;
            int col = startCol + step * colDirection;

            string color = board[row, col].GetColor();

            if (color == currentPlayer)
            {
                playerCount++;
            }
            else if (color == GetOpponentPlayer())
            {
                opponentCount++;
            }
        }

        // Assign scores based on player and opponent counts
        if (playerCount == 4)
        {
            score += 100; // AI wins
        }
        else if (playerCount == 3 && opponentCount == 0)
        {
            score += 5; // AI has three connected coins
        }
        else if (playerCount == 2 && opponentCount == 0)
        {
            score += 2; // AI has two connected coins
        }
        else if (opponentCount == 4)
        {
            score -= 100; // Player wins
        }
        else if (opponentCount == 3 && playerCount == 0)
        {
            score -= 4; // Player has three connected coins, block it
        }
        else if (opponentCount == 2 && playerCount == 0)
        {
            score -= 2; // Player has two connected coins, block it
        }

        return score;
    }

    private string GetOpponentPlayer()
    {
        return currentPlayer == "Yellow" ? "Red" : "Yellow";
    }



    private Vector3 PositionIntToVector(int position)
    {
        Vector3 result = new Vector3(-5.7f,6f,0f);
        float buffer = -5.7f;

        if (position == 0)
        {
            return result;
        }

        for (int col = 0; col < position; col++)
        {
            buffer += 1.9f;
        }

        result.x = buffer;
        return result;
    }

    // changes the current player and displays it on the UI
    private void SwapPlayer()
    {
        if (currentPlayer == "Yellow")
        {
            currentPlayer = "Red";
        }
        else
        {
            currentPlayer = "Yellow";
        }
        DisplayPlayer();
    }

    private void DisplayPlayer()
    {
        playerDisplay.text = currentPlayer + " is playing";
    }

    // spawn the token
    private GameObject SetCoinObject(string color, Vector3 position)
    {
        if (color == "Yellow")
        {
            return Instantiate(yellowCoin, position, Quaternion.identity);
        }
        else
        {
            return Instantiate(redCoin, position, Quaternion.identity);
        }
    }

    public void ResetBoard()
    {
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                board[row, col].Clear();
                if (board[row, col].coin != null) Destroy(board[row, col].coin);
            }
        }
        
    }

    // Player input management

    /* this method takes the column in wich the player want to place a coin and places it.
    additionnaly it looks for the first valid row in the column to edit the board so the manager knows what's up*/
    public void PlaceCoin(int col)
    {
        int row = 0;

        if (!board[ROWS-1, col].IsValid()) return;

        for (int i = 0; i < ROWS; i++)
        {
            if(board[i, col].IsValid())
            {
                row = i; // we store the Y parameter that is the first valid in the column to implement in the board
                break;
            }
        }

        // then we set the board accordingly and we spawn the object on the map
        board[row, col].PlaceColor(currentPlayer);
        board[row, col].coin = SetCoinObject(currentPlayer, PositionIntToVector(col));

        //if (debug) DisplayBoard();

        if (CheckWin(currentPlayer))
        {
            winner = currentPlayer;
            Invoke(nameof(WinGestion), 1f);
        }

        SwapPlayer();
    }

    private void WinGestion()
    {
        PlayerPrefs.SetString("Winner", winner);
        SceneManager.LoadScene("EndScreen");
    }

    private bool CheckWin(string player)
    {
        // Check horizontal
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                if (board[row, col].GetColor() == player &&
                    board[row, col + 1].GetColor() == player &&
                    board[row, col + 2].GetColor() == player &&
                    board[row, col + 3].GetColor() == player)
                {
                    return true;
                }
            }
        }

        // Check vertical
        for (int row = 0; row < ROWS - 3; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                if (board[row, col].GetColor() == player &&
                    board[row + 1, col].GetColor() == player &&
                    board[row + 2, col].GetColor() == player &&
                    board[row + 3, col].GetColor() == player)
                {
                    return true;
                }
            }
        }

        // Check diagonal (ascending)
        for (int row = 3; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                if (board[row, col].GetColor() == player &&
                    board[row - 1, col + 1].GetColor() == player &&
                    board[row - 2, col + 2].GetColor() == player &&
                    board[row - 3, col + 3].GetColor() == player)
                {
                    return true;
                }
            }
        }

        // Check diagonal (descending)
        for (int row = 0; row < ROWS - 3; row++)
        {
            for (int col = 0; col < COLUMNS - 3; col++)
            {
                if (board[row, col].GetColor() == player &&
                    board[row + 1, col + 1].GetColor() == player &&
                    board[row + 2, col + 2].GetColor() == player &&
                    board[row + 3, col + 3].GetColor() == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // debug fonction used to send the board as logs in the console
    private void DisplayBoard()
    {
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                Debug.Log("Board[" + row + " ; " + col + "] = " + board[row, col].GetColor());
            }
        }
    }
}
