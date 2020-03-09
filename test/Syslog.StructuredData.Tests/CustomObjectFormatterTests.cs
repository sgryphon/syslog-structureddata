using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Syslog.Tests
{
    [TestClass]
    public class CustomObjectFormatterTests
    {
        [TestMethod()]
        public void DestructuredCustomObjectMessage()
        {
            var properties = new Dictionary<string, object>() {{"a", new TestObject() {X = 1.2, Y = 3.4}},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(@"[- a=""w=x\\y\""z""]");
        }

        [TestMethod()]
        public void DestructuredCustomObjectProperty()
        {
            var testObject = new TestObject() {X = 1.2, Y = 3.4};
            var properties = new Dictionary<string, object>() {{"@a", testObject},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(@"[- a=""{X=1.2 Y=3.4}""]");
        }

        [TestMethod()]
        public void StructuredCustomObjectValue()
        {
            var properties = new Dictionary<string, object>() {{"a", new TestObject()},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(@"[- a=""w=x\\y\""z""]");
        }

        private class TestObject
        {
            public double X { get; set; }

            public double Y { get; set; }

            public override string ToString()
            {
                return @"w=x\y""z";
            }
        }
    }
}
