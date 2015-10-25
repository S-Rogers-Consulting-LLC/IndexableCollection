using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject {
    [TestClass]
    public class UnitTestWhere {
        [TestMethod]
        public void TestWhereOnStringSingleExersizerByString() {
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 40, 1));
        }

        [TestMethod]
        public void TestWhereOnStringMultipleStresserByString() {
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 5));

            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 40));
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 41));
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 42));
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 43));
            Assert.IsTrue(IsIndexedWhereFasterByString(1000000, 44));
        }

        [TestMethod]
        public void TestWhereOnStringSingleExersizerByGuid() {
            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 40, 1));
        }

        [TestMethod]
        public void TestWhereOnStringMultipleStresserByGuid() {
            Assert.IsTrue(IsIndexedWhereFasterByGuid(10000000, 5));

            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 40));
            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 41));
            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 42));
            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 43));
            Assert.IsTrue(IsIndexedWhereFasterByGuid(1000000, 44));
        }

        [TestMethod]
        public void TestOrderByWhereOnStringSingleExersizerByStringUsingAdd() {
            Assert.IsTrue(IsIndexedOrderByWhereFasterByStringUsingAdd(1000000, 40, 5));
        }

        #region Support Code
        private Boolean IsIndexedWhereFasterByString(Int32 CollectionSize = 10000000, Int32 ItmeName = 42, Int32 Iterations = 10) {
            var collectionOfItems = Factory.CreateCollectionOtItems(CollectionSize);
            var indexableCollectionOfItems = Factory.CreateIndexableCollectionOtItems(collectionOfItems).DisableAutoInexing()
                                                                                                                            .BuildIndex(property => property.Name)
                                                                                                                            .BuildIndex(property => property.Average)
                                                                                                                            .BuildIndex(property => property.Number);

            var queryPerformanceResult1 = RunWhereTestForIndexedByString(indexableCollectionOfItems, ItmeName);
            Console.WriteLine("[First Touch; Caching up Indexes] Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");

            var iterationElapsedMillisecondsSum = (Decimal)0;
            for (var i = 0; i < Iterations; i++) {
                queryPerformanceResult1 = RunWhereTestForIndexedByString(indexableCollectionOfItems, ItmeName);
                Console.WriteLine("Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");
                iterationElapsedMillisecondsSum += queryPerformanceResult1.ElapsedMilliseconds;
            }
            var iterationElapsedMillisecondsAverage = iterationElapsedMillisecondsSum / Iterations;


            var queryPerformanceResult2 = RunWhereTestForNotIndexedByString(collectionOfItems, ItmeName);
            Console.WriteLine("Non Indexed Query of '" + queryPerformanceResult2.Count + "' Items occurred in " + queryPerformanceResult2.ElapsedMilliseconds + " ms!");

            var speedup = (queryPerformanceResult2.ElapsedMilliseconds / iterationElapsedMillisecondsAverage);
            Console.WriteLine("Indexes speed-up execution " + speedup + " times.");
            Console.WriteLine();
            return speedup >= 1;
        }

        private Boolean IsIndexedWhereFasterByGuid(Int32 CollectionSize = 10000000, Int32 ItmeName = 42, Int32 Iterations = 10) {
            var collectionOfItems = Factory.CreateCollectionOtItems(CollectionSize);
            var indexableCollectionOfItems = Factory.CreateIndexableCollectionOtItems(collectionOfItems);

            var queryPerformanceResult1 = RunWhereTestForIndexedByGuid(indexableCollectionOfItems, ItmeName);
            Console.WriteLine("[First Touch; Caching up Indexes] Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");

            var iterationElapsedMillisecondsSum = (Decimal)0;
            for (var i = 0; i < Iterations; i++) {
                queryPerformanceResult1 = RunWhereTestForIndexedByGuid(indexableCollectionOfItems, ItmeName);
                Console.WriteLine("Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");
                iterationElapsedMillisecondsSum += queryPerformanceResult1.ElapsedMilliseconds;
            }
            var iterationElapsedMillisecondsAverage = iterationElapsedMillisecondsSum / Iterations;


            var queryPerformanceResult2 = RunWhereTestForNotIndexedByGuid(collectionOfItems, ItmeName);
            Console.WriteLine("Non Indexed Query of '" + queryPerformanceResult2.Count + "' Items occurred in " + queryPerformanceResult2.ElapsedMilliseconds + " ms!");

            var speedup = (queryPerformanceResult2.ElapsedMilliseconds / iterationElapsedMillisecondsAverage);
            Console.WriteLine("Indexes speed-up execution " + speedup + " times.");
            Console.WriteLine();
            return speedup >= 1;
        }

        private Boolean IsIndexedOrderByWhereFasterByStringUsingAdd(Int32 CollectionSize = 10000000, Int32 ItmeName = 42, Int32 Iterations = 10) {
            var collectionOfItems = Factory.CreateCollectionOtItems(CollectionSize);
            var indexableCollectionOfItems = Factory.CreateIndexableCollectionOtItemsUsingAdd(collectionOfItems);

            var queryPerformanceResult1 = RunOrderdByWhereTestForIndexedByString(indexableCollectionOfItems, ItmeName);
            Console.WriteLine("[First Touch; Caching up Indexes] Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");

            var iterationElapsedMillisecondsSum = (Decimal)0;
            for (var i = 0; i < Iterations; i++) {
                queryPerformanceResult1 = RunOrderdByWhereTestForIndexedByString(indexableCollectionOfItems, ItmeName);
                Console.WriteLine("Indexed Query of '" + queryPerformanceResult1.Count + "' Items occurred in " + queryPerformanceResult1.ElapsedMilliseconds + " ms!");
                iterationElapsedMillisecondsSum += queryPerformanceResult1.ElapsedMilliseconds;
            }
            var iterationElapsedMillisecondsAverage = iterationElapsedMillisecondsSum / Iterations;

            var queryPerformanceResult2 = RunOrderdByWhereTestForNotIndexedByString(collectionOfItems, ItmeName);
            Console.WriteLine("Non Indexed Query of '" + queryPerformanceResult2.Count + "' Items occurred in " + queryPerformanceResult2.ElapsedMilliseconds + " ms!");

            var speedup = (queryPerformanceResult2.ElapsedMilliseconds / iterationElapsedMillisecondsAverage);
            Console.WriteLine("Indexes speed-up execution " + speedup + " times.");
            Console.WriteLine();
            return speedup >= 1;
        }


        private QueryPerformanceResult<Item> RunWhereTestForIndexedByString(IndexableReadOnlyCollection<Item> IndexableReadOnlyCollection, Int32 ItemName) {
            return (
                    from item in IndexableReadOnlyCollection
                    where item.Name == ItemName.ToString()
                    select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunWhereTestForIndexedByString(IndexableCollection<Item> IndexableCollectionItem, Int32 ItemName) {
            return (
                from item in IndexableCollectionItem
                where item.Name == ItemName.ToString()
                select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunWhereTestForNotIndexedByString(Collection<Item> CollectionItem, Int32 ItemName) {
            return (
                from item in CollectionItem
                where item.Name == ItemName.ToString()
                select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunWhereTestForIndexedByGuid(IndexableCollection<Item> IndexableCollectionItem, Int32 Id) {
            return (
                    from item in IndexableCollectionItem
                    where item.Id == Id.MakeGuid()
                    select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunWhereTestForNotIndexedByGuid(Collection<Item> CollectionItem, Int32 Id) {
            return (
                    from item in CollectionItem
                    where item.Id == Id.MakeGuid()
                    select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunOrderdByWhereTestForNotIndexedByString(Collection<Item> CollectionItem, Int32 ItemName) {
            return (
                from item in CollectionItem
                where item.Name == ItemName.ToString()
                orderby item.Average ascending
                select item
            ).ExecuteQueryPerformanceResult();
        }

        private QueryPerformanceResult<Item> RunOrderdByWhereTestForIndexedByString(IndexableReadOnlyCollection<Item> IndexableReadOnlyCollection, Int32 ItemName) {
            return (
                from item in IndexableReadOnlyCollection
                where item.Name == ItemName.ToString()
                orderby item.Average ascending
                select item
            ).ExecuteQueryPerformanceResultWithTrace();
        }
     
        #endregion
    }
}
