using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gameplay;

[CreateAssetMenu(fileName = "Shape", menuName = "Shape", order = 2)]
public class Shape : ScriptableObject
{
    public enum Ownership : byte
    {
        FRIENDLY,
        ENEMY,
        NONE,
        DOESNT_MATTER
    }


    private sbyte requiredProgress;

    public Vector2Int footprint;
    public Ownership[] ownershipSequence;
    public byte[] markerSequence;
    private struct Composition : IEqualityComparer<Composition>
    {
        public bool Equals(Composition a, Composition b)
        {
            return a.ownerships.GetLength(0) == b.ownerships.GetLength(0) &&
            a.ownerships.GetLength(1) == b.ownerships.GetLength(1) &&
            a.ownerships.Cast<Ownership>().SequenceEqual(b.ownerships.Cast<Ownership>()) &&
            a.markers.GetLength(0) == b.markers.GetLength(0) &&
            a.markers.GetLength(1) == b.markers.GetLength(1) &&
            a.markers.Cast<byte>().SequenceEqual(b.markers.Cast<byte>());
        }
        public int GetHashCode(Composition x)
        {
            return 23 * ownerships.GetHashCode() + markers.GetHashCode();
        }
        public Composition(int size_x, int size_y)
        {
            ownerships = new Ownership[size_x, size_y];
            markers = new byte[size_x, size_y];
            size = new Vector2Int(size_x, size_y);
        }
        public void SetPair(int x, int y, Ownership ownership, byte marker)
        {
            ownerships[x, y] = ownership;
            markers[x, y] = marker;
        }
        public readonly Vector2Int size;
        public Ownership[,] ownerships;
        byte[,] markers;
    }
    private Composition[] compositions;
    private void Compose()
    {
        compositions = new Composition[4];
        for (int i = 0; i < 4; i++)
        {
            compositions[i] = i % 2 == 0 ? new Composition(footprint.x, footprint.y) : new Composition(footprint.y, footprint.x);
        }

        int written_position = 0;
        for (int y = 0; y < footprint.y; y++)
        {
            for (int x = 0; x < footprint.x; x++)
            {
                Ownership ownership = ownershipSequence[written_position];
                byte marker = markerSequence[written_position++];

                compositions[0].SetPair(x, y, ownership, marker);
                compositions[1].SetPair(footprint.y - 1 - y, x, ownership, marker);
                compositions[2].SetPair(footprint.x - 1 - x, footprint.y - 1 - y, ownership, marker);
                compositions[3].SetPair(y, footprint.x - 1 - 1, ownership, marker);
            }
        }

        compositions = compositions.Distinct().ToArray();

        requiredProgress = (sbyte)ownershipSequence.Count(x => x == Ownership.FRIENDLY || x == Ownership.ENEMY);
    }
    public static void ComposeAll()
    {
        foreach (Structure structure in Board.board.structurePrefabs)
            foreach (Shape shape in structure.shapes)
                shape.Compose();
    }

    struct Ghost
    {
        public uint progress_record;
        public Ownership requirement;
        public void OnPlayerMovement(Player player, bool moving_out, ref Dictionary<Player, uint> structureChanges)
        {
            OnPlayerMovement(player, moving_out);
            if (Board.board.structures.ContainsKey(progress_record))
            {
                structureChanges.Add(Board.board.structures[progress_record].owner, progress_record);
            }
            else
            {
                Structure structure;
                Shape shape;
                Composition composition;
                int position;

                LookupInCatalog(progress_record, out structure, out shape, out composition, out position);

                foreach (Player p in Player.players)
                {

                }
            }
        }
        public void OnPlayerMovement(Player player, bool moving_out)
        {
            if (requirement != Ownership.DOESNT_MATTER && ((requirement == Ownership.NONE) == (player == null)))
            {
                if (requirement == Ownership.NONE)
                {
                    foreach (Player p in Player.players)
                    {
                        if (moving_out)
                            ++ghost_progress[p][progress_record];
                        else
                            --ghost_progress[p][progress_record];
                    }
                }
                else
                {
                    if (requirement == Ownership.ENEMY) player = player.opponent;
                    if (moving_out)
                        --ghost_progress[player][progress_record];
                    else
                        ++ghost_progress[player][progress_record];
                }
            }
        }
    }
    static Ghost[,][] ghosts;
    static Dictionary<Player, sbyte[]> ghost_progress;
    public static void AddGhosts()
    {
        //Allocate ghost progress records and create catalog
        int ghost_progress_count = 0;
        int structure_count = 0;
        int shape_count = 0;
        int variation_count = 0;

        catalog = new uint[3][,];
        catalog[0] = new uint[2, Board.board.structurePrefabs.Length];
        catalog[1] = new uint[2, Board.board.structurePrefabs.Sum(x => x.shapes.Length)];
        catalog[2] = new uint[2, Board.board.structurePrefabs.Sum(x => x.shapes.Sum(y => y.compositions.Length))];

        foreach (Structure structure in Board.board.structurePrefabs)
        {
            catalog[0][1, structure_count] = (uint)shape_count;

            foreach (Shape shape in structure.shapes)
            {
                catalog[1][1, shape_count] = (uint)variation_count;

                foreach (Composition composition in shape.compositions)
                {
                    catalog[2][1, variation_count] = (uint)ghost_progress_count;

                    ghost_progress_count += Mathf.Clamp((Board.board.size.x - composition.size.x + 1) * (Board.board.size.y - composition.size.y + 1), 0, int.MaxValue);

                    catalog[2][0, variation_count++] = (uint)ghost_progress_count;
                }

                catalog[1][0, shape_count++] = (uint)ghost_progress_count;
            }

            catalog[0][0, structure_count++] = (uint)ghost_progress_count;
        }


        sbyte[] ghost_progress = new sbyte[ghost_progress_count];
        Debug.Log("Ghost progress count: " + ghost_progress_count);

        //Add the ghosts to the tiles
        int[,] ghost_count = new int[Board.board.size.x, Board.board.size.y];
        Vector2Int[,] tile_edge_distances = new Vector2Int[Board.board.size.x, Board.board.size.y];

        for (int x = 0; x < Board.board.size.x; x++)
            for (int y = 0; y < Board.board.size.y; y++)
                tile_edge_distances[x, y] = new Vector2Int((Mathf.Abs(Mathf.Abs(x - Board.board.center.x) - Board.board.center.x) + 1), (Mathf.Abs(Mathf.Abs(y - Board.board.center.y) - Board.board.center.y) + 1));

        foreach (Structure structure in Board.board.structurePrefabs)
            foreach (Shape shape in structure.shapes)
                foreach (Composition composition in shape.compositions)
                    for (int x = 0; x < Board.board.size.x; x++)
                        for (int y = 0; y < Board.board.size.y; y++)
                            ghost_count[x, y] += (Mathf.Clamp(tile_edge_distances[x, y].x, 0, composition.size.x) * Mathf.Clamp(tile_edge_distances[x, y].y, 0, composition.size.y));

        ghosts = new Ghost[Board.board.size.x, Board.board.size.y][];


        Debug.Log("Ghost count: " + ghost_count.Cast<int>().Sum());
        for (int x = 0; x < Board.board.size.x; x++)
            for (int y = 0; y < Board.board.size.y; y++)
            {
                ghosts[x, y] = new Ghost[ghost_count[x, y]];
                ghost_count[x, y] = 0;
            }



        uint progress_record = 0;

        foreach (Structure structure in Board.board.structurePrefabs)
            foreach (Shape shape in structure.shapes)
                foreach (Composition composition in shape.compositions)
                {
                    Vector2Int bounds = new Vector2Int(Board.board.size.x - composition.size.x, Board.board.size.y - composition.size.y);
                    for (int board_y = 0; board_y <= bounds.y; board_y++)
                        for (int board_x = 0; board_x <= bounds.x; board_x++)
                            for (int composition_x = 0; composition_x < composition.size.x; composition_x++)
                                for (int composition_y = 0; composition_y < composition.size.y; composition_y++)
                                {
                                    Vector2Int pos = new Vector2Int(board_x + composition_x, board_y + composition_y);
                                    Ghost ghost;

                                    ghost.progress_record = progress_record++;
                                    ghost.requirement = composition.ownerships[composition_x, composition_y];

                                    ghosts[pos.x, pos.y][ghost_count[pos.x, pos.y]++] = ghost;
                                }
                }


        Tile.OnTileOwnershipChange += OnTileOwnershipChange;
    }
    static void OnTileOwnershipChange(Tile tile, Player old_owner, Player new_owner)
    {
        Dictionary<Player, uint> potentialStructureChanges = new Dictionary<Player, uint>();

        for (int i = 0; i < ghosts[tile.position.x, tile.position.y].Length; i++)
        {
            ghosts[tile.position.x, tile.position.y][i].OnPlayerMovement(old_owner, true);
            ghosts[tile.position.x, tile.position.y][i].OnPlayerMovement(new_owner, false);
        }
    }
    static uint[][,] catalog;

    static void LookupInCatalog(uint progress_record, out Structure structure, out Shape shape, out Composition composition, out int position)
    {
        structure = null;
        shape = null;
        composition = new Composition();
        position = 0;

        uint starting_position = 0;
        for (int i = 0; i < 3; i++)
        {
            int jump_distance = (catalog[i].GetLength(0) - (int)starting_position + 1) / 2;
            int index = (int)starting_position + jump_distance;

            while (jump_distance != 0)
            {
                uint lower_value = index == 0 ? 0 : catalog[i][0, index - 1];
                uint upper_value = catalog[i][0, index];

                jump_distance *= (int)(Mathf.Sign(progress_record - lower_value) + Mathf.Sign(progress_record - upper_value)) / 2;
                jump_distance /= 2;
                index += jump_distance;
            }


            int local_index = index - (int)starting_position;

            starting_position = catalog[i][1, local_index];

            switch (i)
            {
                case 0:
                    structure = Board.board.structurePrefabs[local_index];
                    break;
                case 1:
                    shape = structure.shapes[local_index];
                    break;
                case 2:
                    composition = shape.compositions[local_index];
                    position = (int)(progress_record - starting_position);
                    break;
            }
        }
    }





}
