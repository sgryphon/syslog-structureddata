using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shouldly;

namespace Syslog.Test
{
    [TestClass]
    public class FormatTests
    {
        [TestMethod()]
        public void FormatIdAndParameters()
        {
            var id = "a";
            var parameters = new Dictionary<string, object> {["a:b"] = 1, ["y:c"] = "x"};

            var actual = StructuredData.Format(id, parameters);

            actual.ShouldBe("[a a:b=\"1\" y:c=\"x\"]");
        }

        [TestMethod()]
        public void FormatScopeValue()
        {
            var scope = new StringBuilder("café\tz");

            var actual = StructuredData.Format(string.Empty, new Dictionary<string, object>{["scope"] = scope});

            // Calls ToString() and encodes the control code (but keeps the UTF8)
            actual.ShouldBe("[- scope=\"café\\x09z\"]");
        }
        
        [TestMethod()]
        public void FormatSingleValue()
        {
            var scope = new StringBuilder("café\tz");

            var actual = StructuredData.Format("scope", scope);

            // Calls ToString() and encodes the control code (but keeps the UTF8)
            actual.ShouldBe("[- scope=\"café\\x09z\"]");
        }

        [TestMethod()]
        public void FormatEncoding()
        {
            var value = new StringBuilder("café\tz");
            var id = "x" + value.ToString();
            var name = "y" + value.ToString();

            var actual = StructuredData.Format(id, name, value);

            // Calls ToString() and encodes the control code (but keeps the UTF8)
            actual.ShouldBe("[xcaf_xE9__x09_z ycaf_xE9__x09_z=\"café\\x09z\"]");
        }

        [TestMethod()]
        public void FormatListWithSdId()
        {
            var list = new Dictionary<string, object> {["SD-ID"] = "a", ["a:b"] = 1, ["y:c"] = "x"};

            var actual = StructuredData.Format(list);

            actual.ShouldBe("[a b=\"1\" y:c=\"x\"]");
        }

        [TestMethod()]
        public void FormatListWithSdIdCaseInsensitive()
        {
            var list = new Dictionary<string, object> {["sd-id"] = "a", ["a:b"] = 1, ["y:c"] = "x"};

            var actual = StructuredData.Format(list);

            actual.ShouldBe("[a b=\"1\" y:c=\"x\"]");
        }

        [TestMethod()]
        public void FormatListWithoutSdId()
        {
            var list = new Dictionary<string, object> {["a:b"] = 1, ["y:c"] = "x"};

            var actual = StructuredData.Format(list);

            actual.ShouldBe("[- a:b=\"1\" y:c=\"x\"]");
        }
        
        [TestMethod()]
        public void FormatCustomInterface()
        {
            var mockStructuredData = Substitute.For<IStructuredData>();
            mockStructuredData.Id.Returns("a");
            mockStructuredData.Parameters.Returns(new Dictionary<string, object> {["a:b"] = 1, ["y:c"] = "x"});
            
            var actual = StructuredData.Format(mockStructuredData);

            actual.ShouldBe("[a a:b=\"1\" y:c=\"x\"]");
        }

        [TestMethod()]
        public void CreateFromListWithoutSdId()
        {
            var list = new Dictionary<string, object> {["a:b"] = 1, ["y:c"] = "x"};

            var actual = StructuredData.From(list);

            actual.Id.ShouldBe(string.Empty);
            actual.Parameters.Count.ShouldBe(2);
            var first = actual.Parameters.First();
            first.Key.ShouldBe("a:b");
            first.Value.ShouldBe(1);
        }
    }
}
