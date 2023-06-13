using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject coin = null;
    private string color;
    private bool isOccupied;

    public Cell()
    {
        color = " ";
        isOccupied = false;
    }

    public void PlaceColor(string input)
    {
        color = input;
        isOccupied = true;
    }

    public void Clear()
    {
        color = " ";
        isOccupied = false;
    }

    public bool IsValid()
    {
        return !isOccupied;
    }

    public string GetColor()
    {
        return color;
    }
}