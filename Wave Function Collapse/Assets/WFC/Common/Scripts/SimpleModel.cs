using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public class SimpleModel : Model
    {
        private List<Color[]> tiles;
        private List<string> tilenames;
        private int tilesize;
        private bool blackBackground;
        private readonly WFCParamsSimple _wfcParamsSimple;

        private SimpleTileData TileData => _wfcParamsSimple.TileData ;
        
        public SimpleModel(WFCParamsSimple wfcParamsSimple, Subset subset, int width, int height, bool periodic, bool blackBackground, WFCParams.Heuristic heuristicInstance) : base(1,  width, height, periodic, heuristicInstance)
        {
            _wfcParamsSimple = wfcParamsSimple;
            this.blackBackground = blackBackground;

            tilesize = TileData.Size;
            var unique = TileData.Unique;

            Color[] TileF(Func<int, int, Color> f)
            {
                var result = new Color[tilesize * tilesize];
                for (var y = 0; y < tilesize; y++)
                {
                    for (var x = 0; x < tilesize; x++)
                    {
                        result[x + y * tilesize] = f(x, y);
                    }
                }

                return result;
            }
            
            Color[] Rotate(IReadOnlyList<Color> array) => TileF((x, y) => array[tilesize - 1 - y + x * tilesize]);
            Color[] Reflect(IReadOnlyList<Color> array) => TileF((x, y) => array[tilesize - 1 - x + y * tilesize]);

            tiles = new List<Color[]>();
            tilenames = new List<string>();

            var weightList = new List<double>();
            var action = new List<int[]>();
            var firstOccurence = new Dictionary<string, int>();
            //TODO get rid of this list
            var subsetNames = subset.tiles.Select(tile => tile.Name).ToList();

            foreach (var tile in TileData.Tiles)
            {
                var tileName = tile.Name;
                //TODO Replace this with better stuff
                if(! string.IsNullOrEmpty(subsetName) && !subsetNames.Contains(tileName)) {continue;}

                var (a, b) = tile.GetFunctions();
                var cardinality = tile.GetCardinality();

                T = action.Count;
                firstOccurence.Add(tileName, T);

                var map = new int[cardinality][];
                for (var t = 0; t < cardinality; t++)
                {
                    map[t] = new int[8];

                    map[t][0] = t;
                    map[t][1] = a(t);
                    map[t][2] = a(a(t));
                    map[t][3] = a(a(a(t)));
                    map[t][4] = b(t);
                    map[t][5] = b(a(t));
                    map[t][6] = b(a(a(t)));
                    map[t][7] = b(a(a(a(t))));

                    for (int s = 0; s < 8; s++)
                    {
                        map[t][s] += T;
                    }
                    
                    action.Add(map[t]);
                }

                if (unique)
                {
                    for (var t = 0; t < cardinality; t++)
                    {
                        tiles.Add(TileF((x, y) => tile.sprite.texture.GetPixel(x, y)));
                        tilenames.Add($"{tileName} {t}");
                    }
                }
                else
                {
                    tiles.Add(TileF((x, y) => tile.sprite.texture.GetPixel(x, y)));
                    tilenames.Add($"{tileName} 0");

                    for (int t = 1; t < cardinality; t++)
                    {
                        if(t <= 3) { tiles.Add(Rotate(tiles[T + t - 1]));}
                        if(t >= 4) { tiles.Add(Reflect(tiles[T + t - 4]));}
                        tilenames.Add($"{tileName} {t}");
                    }
                }

                for (int t = 0; t < cardinality; t++)
                {
                    weightList.Add(tile.weight);
                }
            }

            T = action.Count;
            Weights = weightList.ToArray();
            Propagator = new int[MagicConstant][][];
            var densePropagator = new bool [MagicConstant][][];
            for (int d = 0; d < MagicConstant; d++)
            {
                densePropagator[d] = new bool[T][];
                Propagator[d] = new int[T][];
                for (int t = 0; t < T; t++)
                {
                    densePropagator[d][t] = new bool[T];
                }
            }


            foreach (var neighbor in TileData.Neighbors)
            {
                var left = neighbor.left;
                var right = neighbor.right;
                
                if(! string.IsNullOrEmpty(subsetName) && (!subsetNames.Contains(left.tile.Name) || !subsetNames.Contains(right.tile.Name))) {continue;}

                var L = action[firstOccurence[left.tile.Name]][left.id];
                var D = action[L][1];
                var R = action[firstOccurence[right.tile.Name]][right.id];
                var U = action[R][1];
                
                densePropagator[0][R][L] = true;
                densePropagator[0][action[R][6]][action[L][6]] = true;
                densePropagator[0][action[L][4]][action[R][4]] = true;
                densePropagator[0][action[L][2]][action[R][2]] = true;

                densePropagator[1][U][D] = true;
                densePropagator[1][action[D][6]][action[U][6]] = true;
                densePropagator[1][action[U][4]][action[D][4]] = true;
                densePropagator[1][action[D][2]][action[U][2]] = true;
            }

            for (int t2 = 0; t2 < T; t2++)
            {
                for (int t1 = 0; t1 < T; t1++)
                {
                    densePropagator[2][t2][t1] = densePropagator[0][t1][t2];
                    densePropagator[3][t2][t1] = densePropagator[1][t1][t2];
                }
            }
            
            var sparsePropagator = new List<int>[MagicConstant][];
            for (int d = 0; d < MagicConstant; d++)
            {
                sparsePropagator[d] = new List<int>[T];
                for (int t = 0; t < T; t++)
                {
                    sparsePropagator[d][t] = new List<int>();
                }
            }

            for (int d = 0; d < MagicConstant; d++)
            {
                for (int t1 = 0; t1 < T; t1++)
                {
                    var sp = sparsePropagator[d][t1];
                    var tp = densePropagator[d][t1];

                    for (int t2 = 0; t2 < T; t2++)
                    {
                        if (tp[t2])
                        {
                            sp.Add(t2);
                        }
                    }

                    var ST = sp.Count;
                    if (ST == 0)
                    {
                        Debug.LogError($"ERROR: tile {tilenames[t1]} has no neighbors in direction {d}");
                    }

                    Propagator[d][t1] = new int[ST];
                    for (int st = 0; st < ST; st++)
                    {
                        Propagator[d][t1][st] = sp[st];
                    }
                }
            }
        }

        public override Texture2D GetGraphics()
        {
            var texture = new Texture2D(MX * tilesize, MY * tilesize);
            if (Observed[0] >= 0)
            {
                for (int x = 0; x < MX; x++)
                {
                    for (int y = 0; y < MY; y++)
                    {
                        var tile = tiles[Observed[x + y * MX]];
                        for (int yt = 0; yt < tilesize; yt++)
                        {
                            for (int xt = 0; xt < tilesize; xt++)
                            {
                                var c = tile[xt + yt * tilesize];
                                texture.SetPixel(x * tilesize + xt, y * tilesize + yt, c);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < MX; x++)
                {
                    for (int y = 0; y < MY; y++)
                    {
                        var a = Wave[x + y * MX];
                        var amount = (from b in a where b select 1).Sum();
                        var lambda = 1.0 / (from t in Enumerable.Range(0, T) where a[t] select Weights[t]).Sum();
                        
                        for (int yt = 0; yt < tilesize; yt++)
                        {
                            for (int xt = 0; xt < tilesize; xt++)
                            {
                                if (blackBackground && amount == T)
                                {
                                    texture.SetPixel(x * tilesize + xt, y * tilesize + yt, Color.black);
                                }
                                else
                                {
                                    double r = 0, g = 0, b = 0;
                                    for (int t = 0; t < T; t++)
                                    {
                                        if (a[t])
                                        {
                                            var c = tiles[t][xt + yt * tilesize];
                                            var coeff = Weights[t] * lambda;
                                            r += c.r * coeff;
                                            g += c.g * coeff;
                                            b += c.b * coeff;
                                        }
                                    }
                                    texture.SetPixel(x * tilesize + xt, y * tilesize + yt, new Color((float)r,(float)g,(float)b));
                                }
                            }
                        }
                    }
                }
            }

            return texture;
        }
    }
}