using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace WFC
{
    public abstract class Model
    {
        public bool[][] Wave { get; private set; }
        public int[][][] Propagator { get; set; }
        private int[][][] _compatible;
        public int[] Observed { get; private set; }
        private (int, int)[] _stack;
        private int _stackSize;
        private int _observedSoFar;
        
        private double[] _weightLogWeights;
        private double[] _distribution;
        private int[] _sumsOfOnes;
        private double _sumOfWeights;
        private double _sumOfWeightLogWeights;
        private double _startingEntropy;
        private double[] _sumsOfWeights;
        private double[] _sumsOfWeightLogWeights;
        private double[] _entropies;

        protected int MX { get; }
        protected int MY { get; }
        private int N;
        private bool _periodic;
        private WFCParams.Heuristic _heuristic;
        
        private static int[] dx = { -1, 0, 1, 0 };
        private static int[] dy = { 0, 1, 0, -1 };
        private static int[] opposite = { 2, 3, 0, 1 };

        protected const int MagicConstant = 4;
        private bool Initialized => Wave != null;
        protected double[] Weights { get; set; }
        protected int T { get; set; }
        public abstract Texture2D GetGraphics();
        
        protected Model(int N, int width, int height, bool periodic, WFCParams.Heuristic heuristicInstance)
        {
            this.N = N;
            MX = width;
            MY = height;
            _periodic = periodic;
            _heuristic = heuristicInstance;
        }
        
        public bool Run(int seed, int limit)
        {
            if (!Initialized)
            {
                Init();
            }

            Clear();
            var random = new Random(seed);

            for (var l = 0; l < limit || limit < 0; l++)
            {
                var node = GetNodeWithTheMinimumEntropy(random);
                Debug.Log($"Selected Node is {node}");
                if (node >= 0)
                {
                    Observe(node, random);
                    var success = Propagate();
                    if (!success) return false;
                }
                else
                {
                    for (var i = 0; i < Wave.Length; i++)
                    {
                        for (var t = 0; t < this.T; t++)
                        {
                            if (!Wave[i][t]) continue;
                            Observed[i] = t; break;
                        }
                    }
                    return true;
                }
            }

            return true;
        }

        private bool Propagate()
        {
            while (_stackSize > 0)
            {
                var (i1, t1) = _stack[_stackSize - 1];
                _stackSize--;

                var x1 = i1 % MX;
                var y1 = i1 / MX;

                for (var d = 0; d < 4; d++)
                {
                    var x2 = x1 + dx[d];
                    var y2 = y1 + dy[d];
                    if (!_periodic && (x2 < 0 || y2 < 0 || x2 + N > MX || y2 + N > MY)) continue;

                    if (x2 < 0) x2 += MX;
                    else if (x2 >= MX) x2 -= MX;
                    if (y2 < 0) y2 += MY;
                    else if (y2 >= MY) y2 -= MY;

                    var i2 = x2 + y2 * MX;
                    var p = Propagator[d][t1];
                    var compat = _compatible[i2];

                    foreach (var t2 in p)
                    {
                        var comp = compat[t2];

                        comp[d]--;
                        if (comp[d] == 0) Ban(i2, t2);
                    }
                }
            }

            return _sumsOfOnes[0] > 0;
        }

        private void Observe(int node, Random random)
        {
            var domain = Wave[node];
            for (var t = 0; t < T; t++)
            {
                var isValueAllowed = domain[t];
                _distribution[t] = isValueAllowed ? Weights[t] : 0.0;
            }

            var r = _distribution.Random(random.NextDouble());
            for (var t = 0; t < T; t++)
            {
                var isValueAllowed = domain[t];
                var indexIsR = (t == r);
                if (isValueAllowed != indexIsR)
                {
                    Ban(node, t);
                }
            }
        }

        private void Ban(int node, int t)
        {
            Wave[node][t] = false;
            var comp = _compatible[node][t];
            for (var i = 0; i < MagicConstant; i++)
            {
                comp[i] = 0;
            }

            _stack[_stackSize] = (node, t);
            _stackSize++;

            _sumsOfOnes[node]--;
            _sumsOfWeights[node] -= Weights[t];
            _sumsOfWeightLogWeights[node] -= _weightLogWeights[t];

            var sum = _sumsOfWeights[node];
            _entropies[node] = Math.Log(sum) - _sumsOfWeightLogWeights[node] / sum;
        }

        private int GetNodeWithTheMinimumEntropy(Random random)
        {
            if (_heuristic == WFCParams.Heuristic.Scanline)
            {
                return ScanLines();
            }

            var min = double.MaxValue;
            var argMin = -1;
            for (var i = 0; i < Wave.Length; i++)
            {
                if(IsOffLimits(i)) {continue;}

                var remainingValues = _sumsOfOnes[i];
                var entropy = _heuristic == WFCParams.Heuristic.Entropy ? _entropies[i] : remainingValues;
                if (remainingValues <= 1 || !(entropy <= min)) {continue;}
                var noise = 1E-6 * random.NextDouble();
                if (!(entropy + noise < min)) {continue;}
                min = entropy + noise;
                argMin = i;
            }

            return argMin;
        }

        private int ScanLines()
        {
            for (var i = _observedSoFar; i < Wave.Length; i++)
            {
                if (IsOffLimits(i))
                {
                    continue;
                }

                if (_sumsOfOnes[i] > 1)
                {
                    _observedSoFar = i + 1;
                    return i;
                }
            }

            return -1;
        }

        private bool IsOffLimits(int i)
        {
            return !_periodic && (i % MX + N > MX || i / MX + N > MY);
        }

        private void Init()
        {
            Wave = new bool[MX * MY][];
            _compatible = new int[Wave.Length][][];
            for (var i = 0; i < Wave.Length; i++)
            {
                Wave[i] = new bool[T];
                _compatible[i] = new int[T][];
                for (var t = 0; t < this.T; t++) _compatible[i][t] = new int[MagicConstant];
            }
            
            _distribution = new double[T];
            Observed = new int[MX * MY];

            _weightLogWeights = new double[T];
            _sumOfWeights = 0;
            _sumOfWeightLogWeights = 0;
            
            for (var t = 0; t < this.T; t++)
            {
                _weightLogWeights[t] = Weights[t] * Math.Log(Weights[t]);
                _sumOfWeights += Weights[t];
                _sumOfWeightLogWeights += _weightLogWeights[t];
            }
            
            _startingEntropy = Math.Log(_sumOfWeights) - _sumOfWeightLogWeights / _sumOfWeights;

            _sumsOfOnes = new int[MX * MY];
            _sumsOfWeights = new double[MX * MY];
            _sumsOfWeightLogWeights = new double[MX * MY];
            _entropies = new double[MX * MY];

            _stack = new (int, int)[Wave.Length * T];
            _stackSize = 0;

        }
        
       

        private void Clear()
        {
            for (var i = 0; i < Wave.Length; i++)
            {
                for (var j = 0; j < T; j++)
                {
                    Wave[i][j] = true;
                    for (var k = 0; k < MagicConstant; k++)
                    {
                        _compatible[i][j][k] = Propagator[opposite[k]][j].Length;
                    }
                }

                _sumsOfOnes[i] = Weights.Length;
                _sumsOfWeights[i] = _sumOfWeights;
                _sumsOfWeightLogWeights[i] = _sumOfWeightLogWeights;
                _entropies[i] = _startingEntropy;
                Observed[i] = -1;
            }

            _observedSoFar = 0;
        }

        
    }
}

