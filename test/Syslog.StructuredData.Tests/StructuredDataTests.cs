using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Syslog.Tests
{
    [TestClass]
    public class StructuredDataTests
    {
        [TestMethod()]
        public void StructuredAddShouldFailAfterInitialSetup()
        {
            // Arrange
            var data = new StructuredData();

            // Act
            data.Add("a", 1);
            var s = data.ToString();

            // Assert
            Should.Throw<InvalidOperationException>(() =>
            {
                data.Add("b", 2);
            });
            s.ShouldBe("[- a=\"1\"]");
        }

        [TestMethod()]
        public void StructuredDictionaryInitializerSyntaxAdd()
        {
            var data = new StructuredData {{"a", 1},};

            var actual = data.ToString();

            actual.ShouldBe("[- a=\"1\"]");
        }


        [TestMethod()]
        public void StructuredDictionaryInitializerSyntaxIndexer()
        {
            var data = new StructuredData {["a"] = 1,};

            var actual = data.ToString();

            actual.ShouldBe("[- a=\"1\"]");
        }

        [TestMethod()]
        public void StructuredIdOnlyToEnumerable()
        {
            var id = "a";
            IStructuredData data = new StructuredData(id, new KeyValuePair<string, object>[0]);

            var actual = data.ToList();

            actual.Count.ShouldBe(1);
            actual[0].Key.ShouldBe("SD-ID");
            actual[0].Value.ShouldBe("a");
        }

        [TestMethod()]
        public void StructuredIdOnlyToString()
        {
            var id = "a";
            IStructuredData data = new StructuredData(id, new KeyValuePair<string, object>[0]);

            var actual = data.ToString();

            actual.ShouldBe("[a]");
        }

        [TestMethod()]
        public void StructuredIndexerShouldFailAfterInitialSetup()
        {
            var data = new StructuredData();

            data["a"] = 1;
            var s = data.ToString();

            Should.Throw<InvalidOperationException>(() =>
            {
                data["b"] = 2;
            });
            s.ShouldBe("[- a=\"1\"]");
        }

        [TestMethod()]
        public void StructuredParametersOnlyToEnumerable()
        {
            var parameters = new Dictionary<string, object>() {{"b", "B"}};
            IStructuredData data = new StructuredData(parameters);

            var actual = data.ToList();

            actual.Count.ShouldBe(1);
            actual[0].Key.ShouldBe("b");
            actual[0].Value.ShouldBe("B");
        }

        [TestMethod()]
        public void StructuredParametersOnlyToString()
        {
            var properties = new Dictionary<string, object>() {{"a", 1},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"1\"]");
        }

        [TestMethod()]
        public void StructuredToEnumerable()
        {
            // Arrange
            var id = "a";
            var parameters = new Dictionary<string, object>() {{"b", "B"}};
            IStructuredData data = new StructuredData(id, parameters);

            // Act
            var actual = data.ToList();

            // Assert
            actual.Count.ShouldBe(2);
            actual[0].Key.ShouldBe("a:b");
            actual[0].Value.ShouldBe("B");
            actual[1].Key.ShouldBe("SD-ID");
            actual[1].Value.ShouldBe("a");
        }

        [TestMethod()]
        public void StructuredToString()
        {
            // Arrange
            var id = "a";
            var parameters = new Dictionary<string, object>() {{"b", "B"}};
            IStructuredData data = new StructuredData(id, parameters);

            // Act
            var actual = data.ToString();

            actual.ShouldBe("[a b=\"B\"]");
        }
    }
}
