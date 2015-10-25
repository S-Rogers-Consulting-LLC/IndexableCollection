using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnitTestProject {
    [TestClass]
    public class UnitTestJoin {
        [TestMethod]
        public void TestJoinOnStringSingleExersizerByString() {
            Assert.IsTrue(IsIndexedJoinFasterByString(10, 1000000, 1));
        }

        [TestMethod]
        public void TestJoinOnStringMultipleStresserByString() {
            Assert.IsTrue(IsIndexedJoinFasterByString(1, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(5, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(10, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(20, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(30, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(40, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByString(50, 1000000));
        }

        [TestMethod]
        public void TestJoinOnStringSingleExersizerByGuid() {
            Assert.IsTrue(IsIndexedJoinFasterByGuid(10, 1000000, 1));
        }

        [TestMethod]
        public void TestJoinOnStringMultipleStresserByGuid() {
            Assert.IsTrue(IsIndexedJoinFasterByGuid(1, 1000000));
            Assert.IsTrue(IsIndexedJoinFasterByGuid(5, 1000000));
        }

        #region Support Code
        private Boolean IsIndexedJoinFasterByString(int ItemCountA = 10, int ItemCountB = 1000000, Int32 Iterations = 10) {
            int nameCount = Convert.ToInt32(Math.Max((double)ItemCountA, (double)ItemCountB) * .02);
            if (nameCount == 0)
                nameCount = 1;

            var collectionOfItemsA = Factory.CreateCollectionOfItems(ItemCountA, 0, nameCount);
            var collectionOfItemsB = Factory.CreateCollectionOfItems(ItemCountB, 0, nameCount);

            var indexableCollectionOfItemsA = Factory.CreateIndexableCollectionOtItems(collectionOfItemsA);
            var indexableCollectionOfItemsB = Factory.CreateIndexableCollectionOtItems(collectionOfItemsB);

            var queryPerformanceResult1 = RunJoinTestForIndexedByString(indexableCollectionOfItemsA, indexableCollectionOfItemsB);
            Console.WriteLine("[First Touch; Caching up Indexes] Join Query of '" + queryPerformanceResult1.Count + "' Indexed Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");

            var iterationElapsedMillisecondsSum = (Decimal)0;
            for (var i = 0; i < Iterations; i++) {
                queryPerformanceResult1 = RunJoinTestForIndexedByString(indexableCollectionOfItemsA, indexableCollectionOfItemsB);
                Console.WriteLine("Join Query of '" + queryPerformanceResult1.Count + "' Indexed Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");
                iterationElapsedMillisecondsSum += queryPerformanceResult1.ElapsedMilliseconds;
            }
            var iterationElapsedMillisecondsAverage = iterationElapsedMillisecondsSum / Iterations;

            var queryPerformanceResult2 = RunJoinTestForNotIndexedByString(collectionOfItemsA, collectionOfItemsB);
            Console.WriteLine("Join Query of '" + queryPerformanceResult2.Count + "' Non Indexed Items occurred in " + queryPerformanceResult2.ElapsedMilliseconds + " ms!");

            var speedup = (queryPerformanceResult2.ElapsedMilliseconds / iterationElapsedMillisecondsAverage);
            Console.WriteLine("Indexes speed-up execution " + speedup + " times.");
            Console.WriteLine();
            return speedup >= 1;
        }

        private Boolean IsIndexedJoinFasterByGuid(int ItemCountA = 10, int ItemCountB = 1000000, Int32 Iterations = 10) {
            int nameCount = Convert.ToInt32(Math.Max((double)ItemCountA, (double)ItemCountB) * .02);
            if (nameCount == 0)
                nameCount = 1;

            var collectionOfItemsA = Factory.CreateCollectionOfItems(ItemCountA, 0, nameCount);
            var collectionOfItemsB = Factory.CreateCollectionOfItems(ItemCountB, 0, nameCount);

            var indexableCollectionOfItemsA = Factory.CreateIndexableCollectionOtItems(collectionOfItemsA);
            var indexableCollectionOfItemsB = Factory.CreateIndexableCollectionOtItems(collectionOfItemsB);

            var queryPerformanceResult1 = RunJoinTestForIndexedByGuid(indexableCollectionOfItemsA, indexableCollectionOfItemsB);
            Console.WriteLine("[First Touch; Caching up Indexes] Join Query of '" + queryPerformanceResult1.Count + "' Indexed Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");

            var iterationElapsedMillisecondsSum = (Decimal)0;
            for (var i = 0; i < Iterations; i++) {
                queryPerformanceResult1 = RunJoinTestForIndexedByGuid(indexableCollectionOfItemsA, indexableCollectionOfItemsB);
                Console.WriteLine("Join Query of '" + queryPerformanceResult1.Count + "' Indexed Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");
                iterationElapsedMillisecondsSum += queryPerformanceResult1.ElapsedMilliseconds;
            }
            var iterationElapsedMillisecondsAverage = iterationElapsedMillisecondsSum / Iterations;

            var queryPerformanceResult2 = RunJoinTestForNotIndexedByGuid(collectionOfItemsA, collectionOfItemsB);
            Console.WriteLine("Join Query of '" + queryPerformanceResult2.Count + "' Non Indexed Items occurred in " + queryPerformanceResult2.ElapsedMilliseconds + " ms!");

            var speedup = (queryPerformanceResult2.ElapsedMilliseconds / iterationElapsedMillisecondsAverage);
            Console.WriteLine("Indexes speed-up execution " + speedup + " times.");
            Console.WriteLine();
            return speedup >= 1;
        }


        private QueryPerformanceResult<Item> RunJoinTestForIndexedByString(IndexableCollection<Item> IndexableCollectionOfItemsA, IndexableCollection<Item> IndexableCollectionOfItemsB) {
            return (
                from a in IndexableCollectionOfItemsA
                join b in IndexableCollectionOfItemsB on a.Name equals b.Name
                select new Item(Guid.Empty, 0, a.Name, b.Description, 0)
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunJoinTestForNotIndexedByString(Collection<Item> CollectionOfItemsA, Collection<Item> CollectionOfItemsB) {
            return (
                from a in CollectionOfItemsA
                join b in CollectionOfItemsB on a.Name equals b.Name
                select new Item(Guid.Empty, 0, a.Name, b.Description, 0)
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunJoinTestForIndexedByGuid(IndexableCollection<Item> IndexableCollectionOfItemsA, IndexableCollection<Item> IndexableCollectionOfItemsB) {
            return (
                from a in IndexableCollectionOfItemsA
                join b in IndexableCollectionOfItemsB on a.Id equals b.Id
                select new Item(Guid.Empty, 0, a.Name, b.Description, 0)
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunJoinTestForNotIndexedByGuid(Collection<Item> CollectionOfItemsA, Collection<Item> CollectionOfItemsB) {
            return (
                from a in CollectionOfItemsA
                join b in CollectionOfItemsB on a.Id equals b.Id
                select new Item(Guid.Empty, 0, a.Name, b.Description, 0)
            ).ExecuteQueryPerformanceResult();
        }
        #endregion
    }
}
