using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    private const int ROWS = 7;
    private const int COLUMNS = 6;

    private Cell[,] board = new Cell[ROWS, COLUMNS];

    [SerializeField] private bool debug;

    // Start is called before the first frame update
    private void Start()
    {
        currentPlayer = "Yellow";
        alreadyPlayed = false;
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
        // code for the AI to play
        PlaceCoin(Random.Range(0, 7));

        playingButtons.SetActive(true);
        alreadyPlayed = false;
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
    additionnaly it looks for the first valid row in the column to edit the board so the manager knows what's up
    keep in mind that the column selected is stored in the "row" of the array because it's driven by the X parameter.
    so if the names don't match it's normal*/
    public void PlaceCoin(int row)
    {
        int col = 0;

        if (!board[row, COLUMNS-1].IsValid()) return;

        for (int i = 0; i < COLUMNS; i++)
        {
            if(board[row, i].IsValid())
            {
                col = i; // we store the Y parameter that is the first valid in the column to implement in the board
                break;
            }
        }

        // then we set the board accordingly and we spawn the object on the map
        board[row, col].PlaceColor(currentPlayer);
        board[row, col].coin = SetCoinObject(currentPlayer, PositionIntToVector(row));

        if (debug) DisplayBoard();

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
