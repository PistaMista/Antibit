using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Shape", menuName = "Shape", order = 1)]
    public class Shape : ScriptableObject
    {
        public enum TileRequirement : byte
        {
            FRIENDLY,
            ENEMY,
            BLANK,
            DOESNT_MATTER
        }
        public Vector2Int footprint;
        public TileRequirement[] baseSequence;
        TileRequirement[][,] variations;
        byte startingCompleteness;
        byte requiredCompleteness;

        public static void InitializeAll()
        {
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    shape.CalculateVariations();
                    shape.CalculateRequiredCompleteness();
                }
            }
        }
        void CalculateVariations()
        {
            //0: L => R, B => T
            //1: B => T, R => L
            //2: R => L, T => B
            //3: T => B, L => R
            TileRequirement[][,] shapes = new TileRequirement[4][,];
            for (int i = 0; i < 4; i++)
            {
                shapes[i] = i % 2 == 0 ? new TileRequirement[footprint.x, footprint.y] : new TileRequirement[footprint.y, footprint.x];
            }

            int written_position = 0;
            for (int y = 0; y < footprint.y; y++)
            {
                for (int x = 0; x < footprint.x; x++)
                {
                    TileRequirement req = baseSequence[written_position++];
                    shapes[0][x, y] = req;
                    shapes[1][footprint.y - 1 - y, x] = req;
                    shapes[2][footprint.x - 1 - x, footprint.y - 1 - y] = req;
                    shapes[3][y, footprint.x - 1 - x] = req;
                }
            }

            List<TileRequirement[,]> variations = new List<TileRequirement[,]>();
            for (int i = 0; i < 4; i++)
            {
                TileRequirement[,] shape = shapes[i];
                Debug.Log(shape.VisualizationString(x => " " + x.ToString().First() + " "));
                if (!variations.Any(x => x.GetLength(0) == shape.GetLength(0) && x.GetLength(1) == shape.GetLength(1) && x.Cast<TileRequirement>().SequenceEqual(shape.Cast<TileRequirement>())))
                    variations.Add(shape);
            }

            this.variations = variations.ToArray();
        }

        void CalculateRequiredCompleteness()
        {
            startingCompleteness = (byte)baseSequence.Count(x => x == TileRequirement.BLANK);
            requiredCompleteness = (byte)baseSequence.Count(x => x == TileRequirement.FRIENDLY || x == TileRequirement.ENEMY);
        }

        public static void InitializeGhosts()
        {
            InitializeGhostCompletenessArrays();
            InitializeGhostCompletenessChangingTileArrays();

            int[,] hook_count = new int[Board.board.size.x, Board.board.size.y];
            uint ghost_count = 0;
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    foreach (TileRequirement[,] variation in shape.variations)
                    {
                        Vector2Int bounds = new Vector2Int(Board.board.size.x - variation.GetLength(0), Board.board.size.y - variation.GetLength(1));
                        for (int board_x = 0; board_x <= bounds.x; board_x++)
                        {
                            for (int board_y = 0; board_y <= bounds.y; board_y++)
                            {
                                for (int structure_x = 0; structure_x < variation.GetLength(0); structure_x++)
                                {
                                    for (int structure_y = 0; structure_y < variation.GetLength(1); structure_y++)
                                    {
                                        Vector2Int pos = new Vector2Int(board_x + structure_x, board_y + structure_y);
                                        Board.board[pos.x, pos.y].ghostIDs[hook_count[pos.x, pos.y]] = ghost_count;
                                        Board.board[pos.x, pos.y].ghostRequirements[hook_count[pos.x, pos.y]++] = variation[structure_x, structure_y];
                                    }
                                }

                                ghost_count++;
                            }
                        }
                    }
                }
            }
        }

        static void InitializeGhostCompletenessArrays()
        {
            int ghostCount = 0;
            foreach (Structure structure in Board.board.structurePrefabs)
                foreach (Shape shape in structure.shapes)
                    foreach (TileRequirement[,] variation in shape.variations)
                        ghostCount += Mathf.Clamp((Board.board.size.x - variation.GetLength(0) + 1) * (Board.board.size.y - variation.GetLength(1) + 1), 0, int.MaxValue);

            Player.player_on_turn.ghosts = new byte[ghostCount];
            Player.player_on_turn.opponent.ghosts = new byte[ghostCount];
            Debug.Log("Ghost count: " + ghostCount);
        }

        static void InitializeGhostCompletenessChangingTileArrays()
        {
            Vector2Int[,] edgeDistances = new Vector2Int[Board.board.size.x, Board.board.size.y];
            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    edgeDistances[x, y] = new Vector2Int((Mathf.Abs(Mathf.Abs(x - Board.board.center.x) - Board.board.center.x) + 1), (Mathf.Abs(Mathf.Abs(y - Board.board.center.y) - Board.board.center.y) + 1));
                }
            }

            int[,] arraySizes = new int[Board.board.size.x, Board.board.size.y];
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    int variations = shape.variations.Length;

                    for (int x = 0; x < Board.board.size.x; x++)
                    {
                        for (int y = 0; y < Board.board.size.y; y++)
                        {
                            arraySizes[x, y] += (Mathf.Clamp(edgeDistances[x, y].x, 0, shape.footprint.x) * Mathf.Clamp(edgeDistances[x, y].y, 0, shape.footprint.y)) * variations;
                        }
                    }
                }
            }

            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    int size = arraySizes[x, y];
                    Tile tile = Board.board[x, y];

                    tile.ghostIDs = new uint[size];
                    tile.ghostRequirements = new TileRequirement[size];
                }
            }
        }
        enum TileChange
        {
            TAKE,
            OVERTAKE,
            UNTAKE
        }
        public static void UpdateGhostTile(Tile tile, Player old_owner, Player new_owner)
        {
            TileChange change = old_owner != null ? (new_owner != null ? TileChange.OVERTAKE : TileChange.UNTAKE) : TileChange.TAKE;

            for (int i = 0; i < tile.ghostIDs.Length; i++)
            {
                uint id = tile.ghostIDs[i];
                TileRequirement requirement = tile.ghostRequirements[i];

                switch (requirement)
                {
                    case TileRequirement.FRIENDLY:
                        switch (change)
                        {
                            case TileChange.TAKE:
                                ChangeGhost(new_owner, id, 1);
                                break;
                            case TileChange.UNTAKE:
                                ChangeGhost(old_owner, id, -1);
                                break;
                            case TileChange.OVERTAKE:
                                ChangeGhost(old_owner, id, -1);
                                ChangeGhost(new_owner, id, 1);
                                break;
                        }
                        break;
                    case TileRequirement.ENEMY:
                        switch (change)
                        {
                            case TileChange.TAKE:
                                ChangeGhost(new_owner.opponent, id, 1);
                                break;
                            case TileChange.UNTAKE:
                                ChangeGhost(old_owner.opponent, id, -1);
                                break;
                            case TileChange.OVERTAKE:
                                ChangeGhost(old_owner, id, 1);
                                ChangeGhost(new_owner, id, -1);
                                break;
                        }
                        break;
                    case TileRequirement.BLANK:
                        switch (change)
                        {
                            case TileChange.TAKE:
                                ChangeGhost(new_owner, id, -1);
                                ChangeGhost(new_owner.opponent, id, -1);
                                break;
                            case TileChange.UNTAKE:
                                ChangeGhost(old_owner, id, 1);
                                ChangeGhost(old_owner.opponent, id, 1);
                                break;
                        }
                        break;
                }
            }
        }

        static void ChangeGhost(Player player, uint id, int change)
        {
            int completeness = player.ghosts[id];
            completeness += change;

            player.ghosts[id] = (byte)completeness;
        }
    }
}
