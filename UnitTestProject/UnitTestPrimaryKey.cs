using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnitTestProject {
    [TestClass]
    public class UnitTestPrimaryKey {
        [TestMethod]
        public void TestPrimaryKeyByString() {
            var indexableCollectionOfItemsA = Factory.CreateIndexableCollectionOtItemsWithIdAsPrimaryKey(100000);
            var indexableCollectionOfItemsB = Factory.CreateIndexableCollectionOtItemsWithIdAsPrimaryKey(1);
            try {
                foreach (var item in indexableCollectionOfItemsB)
                    indexableCollectionOfItemsA.Add(item);

                throw new Exception("Failed to deect duplicate key insertion.");
            } catch (Exception exception) {
                Console.WriteLine("Exception.Message: " + exception.Message);
                Assert.IsTrue(true);
            }
        }

        #region Support Code
    
        #endregion
    }
}
