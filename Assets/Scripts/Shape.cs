using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gameplay;

[CreateAssetMenu(fileName = "Shape", menuName = "Shape", order = 1)]
public class Shape : ScriptableObject
{
    public enum Ownership : byte
    {
        FRIENDLY,
        ENEMY,
        NONE,
        DOESNT_MATTER
    }
    public Vector2Int footprint;
    public Ownership[] ownershipSequence;
    public byte[] markerSequence;
    struct CompositionMarker
    {
        public CompositionMarker(Ownership requirement, byte marker)
        {
            ownership_requirement = requirement;
            marker_group = marker;
        }
        Ownership ownership_requirement;
        byte marker_group;

        public Ownership Ownership_requirement
        {
            get
            {
                return ownership_requirement;
            }
        }

        public byte Marker_group
        {
            get
            {
                return marker_group;
            }
        }
    }
    CompositionMarker[][,] variations;
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

        Tile.OnTileOwnershipChange += UpdateGhostTile;
    }
    void CalculateVariations()
    {
        //0: L => R, B => T
        //1: B => T, R => L
        //2: R => L, T => B
        //3: T => B, L => R
        CompositionMarker[][,] shapes = new CompositionMarker[4][,];
        for (int i = 0; i < 4; i++)
        {
            shapes[i] = i % 2 == 0 ? new CompositionMarker[footprint.x, footprint.y] : new CompositionMarker[footprint.y, footprint.x];
        }

        int written_position = 0;
        for (int y = 0; y < footprint.y; y++)
        {
            for (int x = 0; x < footprint.x; x++)
            {
                CompositionMarker marker = new CompositionMarker(ownershipSequence[written_position], markerSequence[written_position++]);

                shapes[0][x, y] = marker;
                shapes[1][footprint.y - 1 - y, x] = marker;
                shapes[2][footprint.x - 1 - x, footprint.y - 1 - y] = marker;
                shapes[3][y, footprint.x - 1 - x] = marker;
            }
        }

        List<CompositionMarker[,]> variations = new List<CompositionMarker[,]>();
        for (int i = 0; i < 4; i++)
        {
            CompositionMarker[,] shape = shapes[i];
            Debug.Log(shape.VisualizationString(x => " " + x.Ownership_requirement.ToString().First() + " "));
            if (!variations.Any(x => x.GetLength(0) == shape.GetLength(0) && x.GetLength(1) == shape.GetLength(1) && x.Cast<CompositionMarker>().SequenceEqual(shape.Cast<CompositionMarker>())))
                variations.Add(shape);
        }

        this.variations = variations.ToArray();
    }

    void CalculateRequiredCompleteness()
    {
        startingCompleteness = (byte)ownershipSequence.Count(x => x == Ownership.NONE);
        requiredCompleteness = (byte)ownershipSequence.Count(x => x != Ownership.DOESNT_MATTER);
    }

    public static void InitializeGhosts()
    {
        AllocateGhostIdentificationPartitions();
        AllocateAndPartitionGhostCompletenesses();
        AllocateGhostTileHooks();

        int[,] hook_count = new int[Board.board.size.x, Board.board.size.y];
        uint ghost_count = 0;

        foreach (Structure structure in Board.board.structurePrefabs)
        {
            foreach (Shape shape in structure.shapes)
            {
                foreach (CompositionMarker[,] variation in shape.variations)
                {
                    Vector2Int bounds = new Vector2Int(Board.board.size.x - variation.GetLength(0), Board.board.size.y - variation.GetLength(1));
                    for (int board_y = 0; board_y <= bounds.y; board_y++)
                    {
                        for (int board_x = 0; board_x <= bounds.x; board_x++)
                        {
                            for (int structure_x = 0; structure_x < variation.GetLength(0); structure_x++)
                            {
                                for (int structure_y = 0; structure_y < variation.GetLength(1); structure_y++)
                                {
                                    Vector2Int pos = new Vector2Int(board_x + structure_x, board_y + structure_y);
                                    ghost_tiles[pos.x, pos.y][hook_count[pos.x, pos.y]++] = new GhostHook(ghost_count, variation[structure_x, structure_y].Ownership_requirement);
                                }
                            }

                            ghost_completenesses[Player.player_on_turn][ghost_count] = shape.startingCompleteness;
                            ghost_completenesses[Player.player_on_turn.opponent][ghost_count++] = shape.startingCompleteness;
                        }
                    }
                }
            }
        }
    }

    static void AllocateGhostTileHooks()
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
                foreach (CompositionMarker[,] variation in shape.variations)
                {
                    int xsize = variation.GetLength(0);
                    int ysize = variation.GetLength(1);

                    for (int x = 0; x < Board.board.size.x; x++)
                    {
                        for (int y = 0; y < Board.board.size.y; y++)
                        {
                            arraySizes[x, y] += (Mathf.Clamp(edgeDistances[x, y].x, 0, xsize) * Mathf.Clamp(edgeDistances[x, y].y, 0, ysize));
                        }
                    }
                }
            }
        }

        ghost_tiles = new GhostHook[Board.board.size.x, Board.board.size.y][];
        for (int x = 0; x < Board.board.size.x; x++)
        {
            for (int y = 0; y < Board.board.size.y; y++)
            {
                ghost_tiles[x, y] = new GhostHook[arraySizes[x, y]];
            }
        }
    }
    static void AllocateGhostIdentificationPartitions()
    {
        partition = new uint[3][,];
        partition[0] = new uint[2, Board.board.structurePrefabs.Length];
        partition[1] = new uint[2, Board.board.structurePrefabs.Sum(x => x.shapes.Length)];
        partition[2] = new uint[2, Board.board.structurePrefabs.Sum(x => x.shapes.Sum(y => y.variations.Length))];
    }
    static void AllocateAndPartitionGhostCompletenesses()
    {
        int ghostCount = 0;
        int structure_count = 0;
        int shape_count = 0;
        int variation_count = 0;

        foreach (Structure structure in Board.board.structurePrefabs)
        {
            partition[0][1, structure_count] = (uint)shape_count;

            foreach (Shape shape in structure.shapes)
            {
                partition[1][1, shape_count] = (uint)variation_count;

                foreach (CompositionMarker[,] variation in shape.variations)
                {
                    partition[2][1, variation_count] = (uint)ghostCount;

                    ghostCount += Mathf.Clamp((Board.board.size.x - variation.GetLength(0) + 1) * (Board.board.size.y - variation.GetLength(1) + 1), 0, int.MaxValue);

                    partition[2][0, variation_count++] = (uint)ghostCount;
                }

                partition[1][0, shape_count++] = (uint)ghostCount;
            }

            partition[0][0, structure_count++] = (uint)ghostCount;
        }

        ghost_completenesses = new Dictionary<Player, byte[]>();

        ghost_completenesses.Add(Player.player_on_turn, new byte[ghostCount]);
        ghost_completenesses.Add(Player.player_on_turn.opponent, new byte[ghostCount]);

        Debug.Log("Ghost count: " + ghostCount);
    }
    struct GhostHook
    {
        public GhostHook(uint ghost, Ownership requirement)
        {
            this.ghost = ghost;
            this.requirement = requirement;
        }
        public uint ghost;
        public Ownership requirement;
    }
    static GhostHook[,][] ghost_tiles;
    static Dictionary<Player, byte[]> ghost_completenesses;
    static uint[][,] partition;
    enum TileChange
    {
        TAKE,
        OVERTAKE,
        UNTAKE
    }
    static uint updated_id;
    public static void UpdateGhostTile(Tile tile, Player old_owner, Player new_owner)
    {
        TileChange change = old_owner != null ? (new_owner != null ? TileChange.OVERTAKE : TileChange.UNTAKE) : TileChange.TAKE;
        GhostHook[] hooks = ghost_tiles[tile.position.x, tile.position.y];

        for (int i = 0; i < hooks.Length; i++)
        {
            GhostHook hook = hooks[i];
            updated_id = hook.ghost;

            switch (hook.requirement)
            {
                case Ownership.FRIENDLY:
                    switch (change)
                    {
                        case TileChange.TAKE:
                            ChangeGhost(new_owner, 1);
                            break;
                        case TileChange.UNTAKE:
                            ChangeGhost(old_owner, -1);
                            break;
                        case TileChange.OVERTAKE:
                            ChangeGhost(old_owner, -1);
                            ChangeGhost(new_owner, 1);
                            break;
                    }
                    break;
                case Ownership.ENEMY:
                    switch (change)
                    {
                        case TileChange.TAKE:
                            ChangeGhost(new_owner.opponent, 1);
                            break;
                        case TileChange.UNTAKE:
                            ChangeGhost(old_owner.opponent, -1);
                            break;
                        case TileChange.OVERTAKE:
                            ChangeGhost(old_owner, 1);
                            ChangeGhost(new_owner, -1);
                            break;
                    }
                    break;
                case Ownership.NONE:
                    switch (change)
                    {
                        case TileChange.TAKE:
                            ChangeGhost(new_owner, -1);
                            ChangeGhost(new_owner.opponent, -1);
                            break;
                        case TileChange.UNTAKE:
                            ChangeGhost(old_owner, 1);
                            ChangeGhost(old_owner.opponent, 1);
                            break;
                    }
                    break;
            }
        }
    }

    static void ChangeGhost(Player player, int change)
    {
        int completeness = ghost_completenesses[player][updated_id];
        completeness += change;

        int[] category = Categorize(updated_id, partition);

        Structure structure = Board.board.structurePrefabs[category[0]];
        Shape shape = structure.shapes[category[1]];

        if (completeness == shape.requiredCompleteness)
        {
            Debug.Log("Completed " + structure.name + " type " + shape.name + " variation " + category[2] + " at " + category[3]);

            Structure new_structure = Instantiate(structure.gameObject).GetComponent<Structure>();
            new_structure.transform.SetParent(Board.board.transform);

            CompositionMarker[,] composition = shape.variations[category[2]];
            Vector2Int size = new Vector2Int(composition.GetLength(0), composition.GetLength(1));

            int position = category[3];
            int position_x_limit = Board.board.size.x - size.x + 1;

            bool[,] formation = new bool[size.x, size.y];
            int[,] markers = new int[size.x, size.y];
            Tile tile = Board.board[position % position_x_limit, position / position_x_limit];

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    formation[x, y] = composition[x, y].Ownership_requirement != Ownership.DOESNT_MATTER;
                    markers[x, y] = composition[x, y].Marker_group;
                }
            }

            new_structure.Form(tile, formation, markers, shape.markerSequence.Max());

            Board.board.structures.Add(updated_id, new_structure);
            Player.player_on_turn.structures.Add(new_structure);

            new_structure.owner = Player.player_on_turn;
        }
        else if (Board.board.structures.ContainsKey(updated_id))
        {
            Structure old_structure = Board.board.structures[updated_id];
            if (old_structure.owner == player)
            {
                Debug.Log("Destroyed " + structure.name + " type " + shape.name + " variation " + category[2] + " at " + category[3]);


                Board.board.structures.Remove(updated_id);
                old_structure.owner.structures.Remove(old_structure);
                old_structure.Deform();

                Destroy(old_structure.gameObject);
            }
        }

        ghost_completenesses[player][updated_id] = (byte)completeness;
    }

    static int[] Categorize(uint id, uint[][,] partition)
    {
        int[] result = new int[partition.Length + 1];
        int startingPoint = 0;

        for (int depth = 0; depth < partition.Length; depth++)
        {
            for (int i = startingPoint; i < partition[depth].GetLength(1); i++)
            {
                uint candidate = partition[depth][0, i];
                if (id < candidate)
                {
                    result[depth] = i - startingPoint;
                    startingPoint = (int)partition[depth][1, i];
                    break;
                }
            }
        }

        result[partition.Length] = (int)id - startingPoint;

        return result;
    }
}
