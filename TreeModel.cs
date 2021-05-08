using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tree
{
    class TreeModel
    {
        private int _TreeLayers;
        private int _InputSampleSize;
        private int _nx;
        private DataHolder _dh;
 
        public TreeModel(int TreeLayers, int InputSampleSize, int nx, DataHolder dh)
        {
            _TreeLayers = TreeLayers;
            _InputSampleSize = InputSampleSize;
            _nx = nx;
            _dh = dh;
        }

        private class TreeHolder
        {
            private IFormatter formatter = new BinaryFormatter();
            MemoryStream _stream = null;

            public void SerializeTree(Unod Root)
            {
                if (null != _stream)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                _stream = new MemoryStream();
                formatter.Serialize(_stream, Root);
            }

            public Unod DeserializeTree()
            {
                if (null == _stream) return null;
                _stream.Seek(0, SeekOrigin.Begin);
                Unod obj = (Unod)formatter.Deserialize(_stream);
                return obj;
            }
        }

        private double[] GetMinMax(double[] minmax, int[] group)
        {
            double[] rtn = new double[group.Length];
            int i = 0;
            foreach (int n in group)
            {
                rtn[i] = minmax[n];
                ++i;
            }
            return rtn;
        }

        private int[] GetBlocks(int[] NX, int[] group)
        {
            int[] rtn = new int[group.Length];
            int i = 0;
            foreach (int n in group)
            {
                rtn[i] = NX[n];
                ++i;
            }
            return rtn;
        }

        private void InitializeEntries(int InputSampleSize, int nTreeLayers, int[] NX)
        {
            int nInputs = _dh.GetNumberOfInputs();
            double targetMax = _dh.GetTargetMax();
            double targetMin = _dh.GetTargetMin();
            double[] xmin = _dh.GetInputMin();
            double[] xmax = _dh.GetInputMax();     

            List<int[]> groups = Static.GetRandomGroups(nInputs, InputSampleSize, nTreeLayers);
            int index = 0;
            foreach (Unod unod in UHolder.Entry)
            {
                double[] min = GetMinMax(xmin, groups[index]);
                double[] max = GetMinMax(xmax, groups[index]);
                int[] nx = GetBlocks(NX, groups[index]);

                //foreach (int k in groups[index])
                //{
                //    Console.Write("{0} ", k);
                //}
                //Console.WriteLine();

                unod._u = Static.InitializeU(InputSampleSize, nx, min, max, targetMin, targetMax, groups[index]);
                ++index;
            }
            groups.Clear();
        }

        private double EstimateTreeAccuracy(Unod Root, DataType dataType)
        {
            double error = 0.0;
            double min = double.MaxValue;
            double max = double.MinValue;
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            int cnt = 0;
            for (int i = 0; i < _dh._target.Count; ++i)
            {
                if (_dh._dt[i] != dataType) continue;
                double z = _dh._target[i];
                double model = Root.GetResult(_dh._inputs[i]);
                x.Add(model);
                y.Add(z);
                error += (z - model) * (z - model);
                if (z < min) min = z;
                if (z > max) max = z;
                ++cnt;
            }
            double pearson = Static.PearsonCorrelation(x.ToArray(), y.ToArray());
            error /= (double)(cnt);
            error = Math.Sqrt(error);
            error /= (max - min);
            if (DataType.TEST == dataType)
            {
                Console.WriteLine("Relative error for tree {0:0.0000}, pearson {1:0.0000}", error, pearson);
            }
            return error;
        }

        private double EstimateTreeAccuracyQuantized(Unod Root, DataType dataType)
        {
            int right = 0;
            int total = 0;
            for (int i = 0; i < _dh._target.Count; ++i)
            {
                if (_dh._dt[i] != dataType) continue;
                double[] inputs = _dh._inputs[i];
                double z = _dh._target[i];
                double model = Root.GetResult(inputs);
                if (z >= 0.5 && model >= 0.5) ++right;
                if (z < 0.5 && model < 0.5) ++right;
                ++total;
            }
            double ratio = (double)(right);
            ratio /= (double)(total);
            Console.WriteLine("Total records: {0}, correct results: {1}, ratio: {2:0.0000}", total, right, ratio);
            return ratio;
        }

        private void Identification(Unod Root, int Steps, TreeHolder th, double mu)
        {
            EstimateTreeAccuracy(Root, DataType.SELECT);
            double minError = double.MaxValue;
            for (int step = 0; step < Steps; ++step)
            {
                for (int i = 0; i < _dh._target.Count; ++i)
                {
                    if (_dh._dt[i] != DataType.TRAIN) continue;
                    double[] inputs = _dh._inputs[i];
                    double z = _dh._target[i];
                    double model = Root.GetResult(inputs);
                    double delta = z - model;
                    Root.FindDeltas(delta);
                    Root.Update(inputs, mu);
                }
                double error = EstimateTreeAccuracy(Root, DataType.SELECT);
                if (error < minError)
                {
                    th.SerializeTree(Root);
                    minError = error;
                }
            }
        }

        private void Run(int steps, double mu)
        {
            //Build the tree
            UHolder.Entry.Clear();
            Unod Root = new Unod(0, _TreeLayers);
            InitializeEntries(_InputSampleSize, _TreeLayers, _dh._NUMBER_OF_BLOCKS);
            Root.InitializeU(2, _nx, 0.0, 0.0, _dh.GetTargetMin(), _dh.GetTargetMax());
            //Tree is initialized

            //Identification
            DateTime start = DateTime.Now;
            TreeHolder th = new TreeHolder();
            Identification(Root, steps, th, mu);
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;

            //End result, the model is tested on validation data
            Unod BestTree = th.DeserializeTree();

            EstimateTreeAccuracy(BestTree, DataType.TEST);
        }

        private void RunQuantized(int steps, double mu)
        {
            //Build the tree
            UHolder.Entry.Clear();
            Unod Root = new Unod(0, _TreeLayers);
            InitializeEntries(_InputSampleSize, _TreeLayers, _dh._NUMBER_OF_BLOCKS);
            Root.InitializeU(2, _nx, 0.0, 0.0, _dh.GetTargetMin(), _dh.GetTargetMax());
            //Tree is initialized

            //Identification
            DateTime start = DateTime.Now;
            TreeHolder th = new TreeHolder();
            Identification(Root, steps, th, mu);
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;

            //End result, the model is tested on validation data
            Unod BestTree = th.DeserializeTree();
            EstimateTreeAccuracyQuantized(BestTree, DataType.TEST);
        }

        public void ExecuteMainFlow(int steps, double mu)
        {
            for (int i = 0; i < 10; ++i)
            {
                Run(steps, mu);
            }
        }

        public void ExecuteMainFlowQuantized(int steps, double mu)
        {
            for (int i = 0; i < 10; ++i)
            {
                RunQuantized(steps, mu);
            }
        }
    }
}
