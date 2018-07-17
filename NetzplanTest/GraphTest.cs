using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netzplan;

namespace NetzplanTest
{
    [TestClass]
    public class GraphTest
    {
        [TestMethod]
        public void TestCreateNodes()
        {
            string[] csvLines = new string[]
            {
                "1;Vorgang 1;4;-",
                "2;Vorgang 2;8;1",
                "3;Vorgang 3;2;1",
                "4;Vorgang 4;9;1",
                "5;Vorgang 5;7;2,3",
                "6;Vorgang 6;1;4",
                "7;Vorgang 7;2;6",
                "8;Vorgang 8;3;7",
                "9;Vorgang 9;7;5,7",
                "10;Vorgang 10;5;8,9"
            };

            //PrivateObject graph = new PrivateObject(typeof(Graph), new object[] { "title", csvLines });
            PrivateObject graph = new PrivateObject(typeof(Netzplan.Netzplan));
            Dictionary<string, Subtask> actualNodes = (Dictionary<string, Subtask>)graph.Invoke("CreateNodes", csvLines);

            Subtask node1 = new Subtask("1", "Vorgang 1", 4, new List<Subtask>());
            Subtask node2 = new Subtask("2", "Vorgang 2", 8, new List<Subtask>() { node1 });
            Subtask node3 = new Subtask("3", "Vorgang 3", 2, new List<Subtask>() { node1 });
            Subtask node4 = new Subtask("4", "Vorgang 4", 9, new List<Subtask>() { node1 });
            Subtask node5 = new Subtask("5", "Vorgang 5", 7, new List<Subtask>() { node2, node3 });
            Subtask node6 = new Subtask("6", "Vorgang 6", 1, new List<Subtask>() { node4 });
            Subtask node7 = new Subtask("7", "Vorgang 7", 2, new List<Subtask>() { node6 });
            Subtask node8 = new Subtask("8", "Vorgang 8", 3, new List<Subtask>() { node7 });
            Subtask node9 = new Subtask("9", "Vorgang 9", 7, new List<Subtask>() { node5, node7 });
            Subtask node10 = new Subtask("10", "Vorgang 10", 5, new List<Subtask>() { node8, node9 });

            Dictionary<string, Subtask> expectedNodes = new Dictionary<string, Subtask>
            {
                { "1", node1 },
                { "2", node2 },
                { "3", node3 },
                { "4", node4 },
                { "5", node5 },
                { "6", node6 },
                { "7", node7 },
                { "8", node8 },
                { "9", node9 },
                { "10", node10 }
            };

            Assert.AreEqual(expectedNodes, actualNodes);
        }
    }
}


//string[] csvLines = new string[]
//            {
//                "1;Vorgang 1;4;-",
//                "2;Vorgang 2;8;1",
//                "3;Vorgang 3;2;1",
//                "4;Vorgang 4;9;1",
//                "5;Vorgang 5;7;2,3",
//                "6;Vorgang 6;1;4",
//                "7;Vorgang 7;2;6",
//                "8;Vorgang 8;3;7",
//                "9;Vorgang 9;7;5,7",
//                "10;Vorgang 10;5;8,9"
//            };