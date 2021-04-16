using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tree
{
    class Program
	{
        static void Main(string[] args)
        {
            DataHolder dh = new DataHolder();

            double mu = 0.05; //regularization

            Console.WriteLine("Test of analytical data");
            dh.BuildFormulaData();
            TreeModel tm1 = new TreeModel(4, 3, 10, dh);
            tm1.ExecuteMainFlow(100, mu);

            Console.WriteLine("\nModelling of physical system");
            dh.BuildAirfoilData();
            TreeModel tm2 = new TreeModel(7, 4, 2, dh);
            tm2.ExecuteMainFlow(100, mu);

            Console.WriteLine("\nModelling of biological system");
            dh.BuildMClassData();
            TreeModel tm3 = new TreeModel(3, 8, 5, dh);
            tm3.ExecuteMainFlowQuantized(100, mu);

            Console.WriteLine("\nModelling of social system");
            dh.BuildBankChurnData();
            TreeModel tm4 = new TreeModel(3, 7, 4, dh);
            tm4.ExecuteMainFlowQuantized(100, mu);
        }
    }
}


