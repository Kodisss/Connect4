using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Connect4 : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject yellowCoin;
    [SerializeField] private GameObject redCoin;

    private string currentPlayer;

    private const int ROWS = 7;
    private const int COLUMNS = 7;

    private Cell[,] board = new Cell[ROWS, COLUMNS];

    // Start is called before the first frame update
    private void Start()
    {
        currentPlayer = "Yellow";
        InitializeBoard();
        PlaceCoin(3);
        PlaceCoin(3);
        PlaceCoin(4);
        PlaceCoin(0);
        DisplayBoard();
        Invoke(nameof(ResetBoard), 5f);
    }

    // Update is called once per frame
    private void Update()
    {
        
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
    }

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

    private void ResetBoard()
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

        SwapPlayer();
    }

    // debug fonction used to send the board as logs in the console
    private void DisplayBoard()
    {
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                Debug.Log(board[row, col].GetColor());
            }
        }
    }
}
