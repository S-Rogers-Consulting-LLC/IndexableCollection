using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject {
    public static class Factory {
        private static Random TheRandomName = new Random(7);
        private static Random TheRandomDescription = new Random(77);
        private static Random TheRandomAverage = new Random(798);

        private static string[] TheNames = { "Name 1", "Name 2", "Name 3", "Name 4", "Name 5" };


        public static Collection<Item> CreateCollectionOtItems(int Count) { return CreateCollectionOfItems(Count, 0, 500); }

        public static Collection<Item> CreateCollectionOfItems(int Count, int NameRangeStart, int NameRangeEnd) {
            var timer = new HiPerformanceTimer();
            timer.Start();
            var items = new Collection<Item>();
            for (int number = 0; number < Count; number++)
                items.Add(CreateRandomItem(number, NameRangeStart, NameRangeEnd));
            timer.Stop();
            Console.WriteLine("Created a non indexed collection of " + Count + " Items in " + (Double)timer.ElapsedMilliseconds + " ms!");
            return items;
        }

        public static IndexableCollection<Item> CreateIndexableCollectionOtItemsWithIdAsPrimaryKey(int Count = 10) {
            var CollectionOfItems = CreateCollectionOtItems(Count);
            var timer = new HiPerformanceTimer();
            timer.Start();
            var indexableCollectionOfItems = Factory.CreateCollectionOtItems(0).ToIndexableCollection().BuildPrimaryKeyIndex(property => property.Id);
            foreach (var item in CollectionOfItems)
                indexableCollectionOfItems.Add(item);
            timer.Stop();
            Console.WriteLine("Created an indexed collection 'Using Add(..)' of " + CollectionOfItems.Count + " Items in " + (Double)timer.ElapsedMilliseconds + " ms!");          
            return indexableCollectionOfItems;
        }

        public static IndexableCollection<Item> CreateIndexableCollectionOtItems(Collection<Item> CollectionOfItems) {
            var timer = new HiPerformanceTimer();
            timer.Start();
            var indexableCollectionOfItems = CollectionOfItems.ToIndexableCollection().EnableAutoInexing();
            Console.WriteLine("Created an indexed collection of " + CollectionOfItems.Count + " Items in " + (Double)timer.ElapsedMilliseconds + " ms!");
            timer.Stop();
            return indexableCollectionOfItems;
        }

        public static IndexableReadOnlyCollection<Item> CreateIndexableCollectionOtItemsUsingAdd(Collection<Item> CollectionOfItems) {
            var timer = new HiPerformanceTimer();
            timer.Start();
            var indexableCollectionOfItems = Factory.CreateCollectionOtItems(0).ToIndexableCollection().EnableAutoInexing();
            foreach (var item in CollectionOfItems)
                indexableCollectionOfItems.Add(item);
            Console.WriteLine("Created an indexed collection 'Using Add(..)' of " + CollectionOfItems.Count + " Items in " + (Double)timer.ElapsedMilliseconds + " ms!");
            timer.Stop();
            return indexableCollectionOfItems;
        }



        private static Item CreateRandomItem(int Number, int NameRangeStart, int NameRangeEnd) {
            return new Item(
                                    Number.MakeGuid(),
                                    Number,
                                    TheRandomName.Next(NameRangeStart, NameRangeEnd).ToString(),
                                    TheNames[TheRandomDescription.Next(0, TheNames.Length - 1)],
                                    ((double)TheRandomAverage.Next(0, 40)) / 10.0
                            );
        }

        public static QueryPerformanceResult<Item> ExecuteQueryPerformanceResult(this IEnumerable<Item> Query) {
            var timer = new HiPerformanceTimer();
            timer.Start();
            var resultList = Query.ToList<Item>();
            timer.Stop();
            return new QueryPerformanceResult<Item>(resultList.Count(), timer.ElapsedMilliseconds, resultList);
        }

        public static QueryPerformanceResult<Item> ExecuteQueryPerformanceResultWithTrace(this IEnumerable<Item> Query) {
            var timer = new HiPerformanceTimer();
            timer.Start();
            var resultList = Query.ToList<Item>();
            timer.Stop();
            return new QueryPerformanceResult<Item>(resultList.Count(), timer.ElapsedMilliseconds, resultList);
        }

        public static Guid MakeGuid(this Int32 Int32) { return new Guid(Int32.ToString().PadLeft(32, '0')); }
    }

    public class QueryPerformanceResult<T> {
        public QueryPerformanceResult(Int64 Count, Decimal ElapsedMilliseconds, IEnumerable<T> Output) {
            this.Count = Count;
            this.ElapsedMilliseconds = ElapsedMilliseconds;
            this.Output = Output.ToArray();
        }

        public Int64 Count { get; private set; }
        public Decimal ElapsedMilliseconds { get; private set; }
        public T[] Output { get; private set; }
    }

    public class Item {
        public Item(
                    Guid Id,
                    Int32 Number,
                    String Name,
                    String Description,
                    Double Average
        ) {
            this.Id = Id;
            this.Number = Number;
            this.Name = Name;
            this.Description = Description;
            this.Average = Average;
        }

        public Guid Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public double Average { get; private set; }
    }

    public class ParentItem {
        public int Key;
        public string ParentCaption;
    }

    public class ChildItem {
        public int Key;
        public string ChildCaption;
    }
}
