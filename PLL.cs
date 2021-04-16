﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tree
{
    [Serializable]
    class PLL
    {
        private int _points;
        private double[] _x;
        private double[] _y;
        private double _deltax;
 
        public PLL(int points, double xmin, double xmax)
        {
            Initialize(points, xmin, xmax);
        }

        public void SetConstant(double c)
        {
            for (int i = 0; i < _points; ++i)
            {
                _y[i] = c;
            }
        }

        public void SetLinear(double ymin, double ymax)
        {
            double deltay = (ymax - ymin) / (_points - 1);
            for (int i = 0; i < _points; ++i)
            {
                _y[i] = ymin + deltay * i;
            }
        }

        public void SetRandom(double ymin, double ymax)
        {
            Random rnd = new Random();
            for (int i = 0; i < _points; ++i)
            {
                _y[i] = (rnd.Next() % 100) / 100.0 * (ymax - ymin) + ymin;
            }
            for (int i = 0; i < _points - 1; ++i)
            {
                _y[i] = (_y[i] + _y[i + 1]) / 2.0;
            }
        }

        public double GetDerrivative(double x)
        {
            int low = (int)((x - _x[0]) / _deltax);
            if (low < 0) low = 0;
            if (low > _x.Length - 2) low = _x.Length - 2;
            return (_y[low + 1] - _y[low]) / _deltax;
        }

        private void Initialize(int points, double xmin, double xmax)
        {
            if (points < 2)
            {
                Console.WriteLine("Number of blocks is too low");
                Environment.Exit(0);
            }
            if (xmin >= xmax)
            {
                Console.WriteLine("The limits are invalid {0:0.000}, {1:0.0000}", xmin, xmax);
                Environment.Exit(0);
            }
            _points = points;
            _x = new double[_points];
            _y = new double[_points];
            _deltax = (xmax - xmin) / (_points - 1);
            _x[0] = xmin;
            _y[0] = 0.0;
            for (int i = 1; i < _x.Length; ++i)
            {
                _x[i] = _x[i - 1] + _deltax;
                _y[i] = 0.0;
            }
        }

        public void Resize(int N)
        {
            double[] tmp = new double[N];
            double deltax = (_x[_x.Length - 1] - _x[0]) / (N - 1);
            double min = _x[0];
            for (int i = 0; i < N; ++i)
            {
                double x = min + i * deltax;
                tmp[i] = GetFunction(x);
            }

            _x = new double[N];
            _y = new double[N];
            _deltax = deltax;
            for (int i = 0; i < _x.Length; ++i)
            {
                _x[i] = min + i * _deltax;
                _y[i] = tmp[i];
            }
            _points = N;
        }


        private void ChangeFieldOfDefinition(bool isBottom)
        {
            if (isBottom)
            {
                _points += 1;
                double[] tmpX = new double[_points];
                double[] tmpY = new double[_points];
                tmpX[0] = _x[0] - _deltax;
                tmpY[0] = 0.0;
                for (int i = 1; i < _points; ++i)
                {
                    tmpX[i] = _x[i - 1];
                    tmpY[i] = _y[i - 1];
                }
                _x = new double[_points];
                _y = new double[_points];
                for (int i = 0; i < _points; ++i)
                {
                    _x[i] = tmpX[i];
                    _y[i] = tmpY[i];
                }
            }
            else
            {
                _points += 1;
                double[] tmpX = new double[_points];
                double[] tmpY = new double[_points];
                tmpX[_points - 1] = _x[_x.Length - 1] + _deltax;
                tmpY[_points - 1] = 0.0;
                for (int i = 0; i < _points - 1; ++i)
                {
                    tmpX[i] = _x[i];
                    tmpY[i] = _y[i];
                }
                _x = new double[_points];
                _y = new double[_points];
                for (int i = 0; i < _points; ++i)
                {
                    _x[i] = tmpX[i];
                    _y[i] = tmpY[i];
                }
            }
        }

        public void Update(double x, double delta, double mu)
        {
            if (x < _x[0])
            {
                ChangeFieldOfDefinition(true);
            }

            if (x > _x[_x.Length - 1])
            {
                ChangeFieldOfDefinition(false);
            }

            int left = (int)((x - _x[0]) / _deltax);
            if (left < 0) left = 0;
            if (left >= _y.Length - 1)
            {
                _y[_y.Length - 1] += delta * mu;
                return;
            }

            double leftx = x - _x[left];
            double rightx = _x[left + 1] - x;
            _y[left + 1] += delta * leftx / _deltax * mu;
            _y[left] += delta * rightx / _deltax * mu;
        }

        public double GetFunction(double x)
        {
            if (x < _x[0])
            {
                double derrivative = (_y[1] - _y[0]) / _deltax;
                return _y[1] - derrivative * (_x[1] - x);
            }
            if (x > _x[_x.Length - 1])
            {
                double derrivative = (_y[_y.Length - 1] - _y[_y.Length - 2]) / _deltax;
                return _y[_y.Length - 2] + derrivative * (x - _x[_x.Length - 2]);
            }
            int left = (int)((x - _x[0]) / _deltax);
            if (left < 0) left = 0;
            if (left >= _x.Length - 1) return _y[_x.Length - 1];
            double leftx = x - _x[left];
            return (_y[left + 1] - _y[left]) / _deltax * leftx + _y[left];
        }

        public void ShowData()
        {
            for (int i = 0; i < _x.Length; ++i)
            {
                Console.WriteLine("{0:0.0000} {1:0.0000}", _x[i], _y[i]);
            }
        }

        public double GetY(int pos)
        {
            if (pos > _y.Length - 1) pos = _y.Length - 1;
            if (pos < 0) pos = 0;
            return _y[pos];
        }

        public int GetNumberOfPoints()
        {
            return _points;
        }

        public double GetXmin()
        {
            return _x[0];
        }

        public double GetXmax()
        {
            return _x[_x.Length - 1];
        }

        public double[] GetAllFunc()
        {
            return _y;
        }

        public double[] GetAllArgs()
        {
            return _x;
        }

        public double GetDeltax()
        {
            return _deltax;
        }
    }
}
