using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Syslog.StructuredData.Tests
{
    [TestClass]
    public class CollectionFormatterTests
    {
        [TestMethod]
        public void StructuredChildArray()
        {
            var child1 = new ArrayList() {1, "A"};

            var list1 = new ArrayList() {"x", child1};
            var obj2 = new TestObject2() {M = "y", N = child1};
            var dict3 = new Dictionary<string, object>() {{"P", "z"}, {"Q", child1}};

            var properties = new Dictionary<string, object>() {{"a", list1}, {"@b", obj2}, {"c", dict3},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"('x','System.Collections.ArrayList')\" b=\"{M='y' N=(1,'A')}\" c=\"(P='z' Q=(1,'A'))\"]");
        }

        [TestMethod()]
        public void StructuredChildDictionary()
        {
            var child1 = new Dictionary<string, object>() {{"X", "A"}, {"Y", 1}};

            var list1 = new ArrayList() {"x", child1};
            var obj2 = new TestObject2() {M = "y", N = child1};

            var properties = new Dictionary<string, object>() {{"a", list1}, {"@b", obj2},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(
                "[- a=\"('x','System.Collections.Generic.Dictionary`2[System.String,System.Object\\]')\" b=\"{M='y' N='System.Collections.Generic.Dictionary`2[System.String,System.Object\\]'}\"]");
        }


        [TestMethod()]
        public void StructuredRecursiveDictionaryShouldStop()
        {
            var child1 = new Dictionary<string, object>() {{"Y", 1}};
            var dictionary1 = new Dictionary<string, object>() {{"X", 2}, {"Z", child1}};
            child1.Add("Q", dictionary1);

            var properties = new Dictionary<string, object>() {{"a", dictionary1},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"(X=2 Z='System.Collections.Generic.Dictionary`2[System.String,System.Object\\]')\"]");
        }

        [TestMethod()]
        public void StructuredRecursiveListShouldStop()
        {
            var child1 = new ArrayList() {1, "A"};
            var list1 = new ArrayList() {2, child1};
            child1.Add(list1);

            var properties = new Dictionary<string, object>() {{"a", list1},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"(2,'System.Collections.ArrayList')\"]");
        }


        private class TestObject2
        {
            public object M { get; set; }

            public object N { get; set; }
        }
    }
}
