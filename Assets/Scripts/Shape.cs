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
        public byte[,] markers;
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
                compositions[3].SetPair(y, footprint.x - 1 - x, ownership, marker);
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

    public static class Ghosts
    {
        static Dictionary<Player, sbyte[]> player_progress;
        static Tile[,][] tiles;

        struct Tile
        {
            public Tile(uint progress_record, Ownership requirement)
            {
                this.progress_record = progress_record;
                this.requirement = requirement;
            }
            public readonly uint progress_record;
            public readonly Ownership requirement;
            public void OnOwnershipMovement(Player player, bool moving_out)
            {
                if (requirement != Ownership.DOESNT_MATTER && ((requirement == Ownership.NONE) == (player == null)))
                {
                    if (requirement == Ownership.NONE)
                    {
                        foreach (Player p in Player.players)
                        {
                            if (moving_out)
                                --player_progress[p][progress_record];
                            else
                                ++player_progress[p][progress_record];
                        }
                    }
                    else
                    {
                        if (requirement == Ownership.ENEMY) player = player.opponent;
                        if (moving_out)
                            --player_progress[player][progress_record];
                        else
                            ++player_progress[player][progress_record];
                    }
                }
            }
            public static void OnAnyOwnershipChange(Gameplay.Tile tile, Player old_owner, Player new_owner)
            {
                Classification[] classifications = new Classification[tiles[tile.position.x, tile.position.y].Length];

                for (int i = 0; i < classifications.Length; i++)
                    classifications[i] = new Classification(tiles[tile.position.x, tile.position.y][i].progress_record);

                int? deformed_classification = null;
                for (int i = 0; i < classifications.Length; i++)
                {
                    tiles[tile.position.x, tile.position.y][i].OnOwnershipMovement(old_owner, true);
                    tiles[tile.position.x, tile.position.y][i].OnOwnershipMovement(new_owner, false);

                    if (classifications[i].deforming) deformed_classification = i;
                }

                if (deformed_classification != null)
                    classifications[(int)deformed_classification].Deform();


                int formed_classification = 0;
                Player forming_player = null;
                for (int i = 0; i < classifications.Length; i++)
                {
                    Player candidate = classifications[i].forming_player;
                    if (candidate != null)
                        if (forming_player != null)
                            return;
                        else
                        {
                            forming_player = candidate;
                            formed_classification = i;
                        }
                }

                if (forming_player != null)
                    classifications[formed_classification].FormFor(forming_player);
            }
        }
        static uint[][,] catalog;

        struct Classification
        {
            public Classification(uint progress_record)
            {
                this.progress_record = progress_record;
                structure = null;
                shape = null;
                composition = new Composition();
                position = Vector2Int.zero;

                uint starting_position = 0;
                for (int i = 0; i < 3; i++)
                {
                    uint[,] entry = catalog[i];

                    uint left_bound = starting_position;
                    uint right_bound = (uint)(entry.GetLength(1) - 1);

                    int index = 0;
                    while (left_bound <= right_bound)
                    {
                        uint median = (left_bound + right_bound) / 2;

                        uint lower_value = median == 0 ? 0 : entry[0, median - 1];
                        uint upper_value = entry[0, median];

                        if (progress_record >= lower_value && progress_record < upper_value)
                        {
                            index = (int)median;
                            break;
                        }
                        else if (progress_record > lower_value)
                        {
                            left_bound = median + 1;
                        }
                        else
                        {
                            right_bound = median - 1;
                        }
                    }


                    int local_index = index - (int)starting_position;
                    starting_position = entry[1, index];

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

                            int position_index = (int)(progress_record - starting_position);
                            int position_x_limit = Board.board.size.x - composition.size.x + 1;
                            position = new Vector2Int(position_index % position_x_limit, position_index / position_x_limit);
                            break;
                    }
                }
            }
            public readonly uint progress_record;
            public readonly Structure structure;
            public readonly Shape shape;
            public readonly Composition composition;
            public readonly Vector2Int position;
            public bool deforming
            {
                get
                {
                    return Board.board.structures.ContainsKey(progress_record) && !IsCompleteFor(Board.board.structures[progress_record].owner);
                }
            }

            public Player forming_player
            {
                get
                {
                    if (!Board.board.structures.ContainsKey(progress_record))
                    {
                        foreach (Player player in Player.players)
                        {
                            if (IsCompleteFor(player)) return player;
                        }
                    }

                    return null;
                }
            }
            bool IsCompleteFor(Player player)
            {
                return player_progress[player][progress_record] == shape.requiredProgress;
            }

            public void FormFor(Player player)
            {
                IsolateFor(player);
                Debug.Log("Formed " + structure.name + " at " + position);

                Structure new_structure = Instantiate(structure.gameObject).GetComponent<Structure>();
                new_structure.transform.SetParent(Board.board.transform);

                bool[,] formation = new bool[composition.size.x, composition.size.y];
                int[,] markers = new int[composition.size.x, composition.size.y];

                for (int x = 0; x < composition.size.x; x++)
                {
                    for (int y = 0; y < composition.size.y; y++)
                    {
                        formation[x, y] = composition.ownerships[x, y] != Ownership.DOESNT_MATTER;
                        markers[x, y] = composition.markers[x, y];
                    }
                }

                new_structure.Form(Board.board[position.x, position.y], formation, markers, shape.markerSequence.Max());

                Board.board.structures.Add(progress_record, new_structure);
                Player.player_on_turn.structures.Add(new_structure);

                new_structure.owner = Player.player_on_turn;
            }

            public void Deform()
            {
                Debug.Log("Deformed " + structure.name + " at " + position);
                Structure deformed_structure = Board.board.structures[progress_record];
                IsolateFor(deformed_structure.owner, reverse: true);

                deformed_structure.owner.structures.Remove(deformed_structure);
                Board.board.structures.Remove(progress_record);
                deformed_structure.Deform();

                Destroy(deformed_structure.gameObject);
            }

            void IsolateFor(Player player, bool reverse = false)
            {
                for (int x = 0; x < composition.size.x; x++)
                {
                    for (int y = 0; y < composition.size.y; y++)
                    {
                        Vector2Int global = position + new Vector2Int(x, y);
                        Ownership structure_composition_requirement = composition.ownerships[x, y];
                        Player structure_tile_owner = structure_composition_requirement == Ownership.FRIENDLY ? player : (structure_composition_requirement == Ownership.ENEMY ? player.opponent : null);

                        for (int i = 0; i < tiles[global.x, global.y].Length; i++)
                            if (tiles[global.x, global.y][i].progress_record != progress_record)
                                tiles[global.x, global.y][i].OnOwnershipMovement(structure_tile_owner, !reverse);
                    }
                }
            }
        }

        public static void Add()
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


            player_progress = new Dictionary<Player, sbyte[]>();
            foreach (Player player in Player.players)
            {
                player_progress.Add(player, new sbyte[ghost_progress_count]);
            }

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
                    {
                        int xsize = Mathf.Clamp(composition.size.x, 0, Board.board.center.x);
                        int ysize = Mathf.Clamp(composition.size.y, 0, Board.board.center.y);
                        for (int x = 0; x < Board.board.size.x; x++)
                            for (int y = 0; y < Board.board.size.y; y++)
                                ghost_count[x, y] += (Mathf.Clamp(tile_edge_distances[x, y].x, 0, xsize) * Mathf.Clamp(tile_edge_distances[x, y].y, 0, ysize));
                    }
            tiles = new Tile[Board.board.size.x, Board.board.size.y][];


            Debug.Log("Ghost count: " + ghost_count.Cast<int>().Sum());

            for (int x = 0; x < Board.board.size.x; x++)
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    tiles[x, y] = new Tile[ghost_count[x, y]];
                    ghost_count[x, y] = 0;
                }



            uint progress_record = 0;

            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    foreach (Composition composition in shape.compositions)
                    {
                        Vector2Int bounds = new Vector2Int(Board.board.size.x - composition.size.x, Board.board.size.y - composition.size.y);
                        for (int board_y = 0; board_y <= bounds.y; board_y++)
                        {
                            for (int board_x = 0; board_x <= bounds.x; board_x++)
                            {
                                for (int composition_x = 0; composition_x < composition.size.x; composition_x++)
                                {
                                    for (int composition_y = 0; composition_y < composition.size.y; composition_y++)
                                    {
                                        Vector2Int pos = new Vector2Int(board_x + composition_x, board_y + composition_y);
                                        tiles[pos.x, pos.y][ghost_count[pos.x, pos.y]++] = new Tile(progress_record, composition.ownerships[composition_x, composition_y]);
                                    }
                                }

                                progress_record++;
                            }
                        }
                    }
                }
            }

            Gameplay.Tile.OnTileOwnershipChange -= Tile.OnAnyOwnershipChange;
            Gameplay.Tile.OnTileOwnershipChange += Tile.OnAnyOwnershipChange;
        }
    }
}