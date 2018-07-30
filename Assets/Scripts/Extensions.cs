using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Extensions
{
    public static bool Is(this bool[,] array, Gameplay.Tile tile)
    {
        return array.Is(tile.position);
    }

    public static void Set(this bool[,] array, Gameplay.Tile tile, bool value)
    {
        array.Set(tile.position, value);
        tile.RefreshColor();
    }

    public static bool Is(this bool[,] array, Vector2Int position)
    {
        return array[position.x, position.y];
    }

    public static void Set(this bool[,] array, Vector2Int position, bool value)
    {
        array[position.x, position.y] = value;
    }

    public static string VisualizationString<T>(this T[,] array, System.Converter<T, string> converter)
    {
        string result = "";
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                result += converter(array[x, y]);
            }

            result += "\n";
        }

        return result;
    }
}
