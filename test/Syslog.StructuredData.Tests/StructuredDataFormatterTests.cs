using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Syslog.Tests
{
    [TestClass()]
    public class StructuredDataFormatterTests
    {
        [Flags]
        private enum TestEnum
        {
            None = 0,
            Flag1 = 1,
            Flag2 = 2,
            Flag3 = 4
        }

        [TestMethod()]
        public void StructuredArrayValues()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", new int[] {1, 2, 3}}, {"b", new List<string>() {"A", "B", "C"}},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"(1,2,3)\" b=\"('A','B','C')\"]", actual);
        }

        [TestMethod()]
        public void StructuredBasicStringValue()
        {
            var properties = new Dictionary<string, object>() {{"a", "A"},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"A\"]", actual);
        }

        [TestMethod()]
        public void StructuredByteArray()
        {
            var byteArray1 = new byte[] {0xa1, 0, 5, 255};
            var byteArray2 = new byte[0];
            var byteArray3 = new List<byte>() {1, 2};
            var properties =
                new Dictionary<string, object>() {{"a", byteArray1}, {"b", byteArray2}, {"c", byteArray3},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"0xa10005ff\" b=\"0x\" c=\"(0x01,0x02)\"]");
        }

        [TestMethod()]
        public void StructuredByteValue()
        {
            var properties = new Dictionary<string, object>() {{"a", (byte)0}, {"b", (byte)0xa1},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"0x00\" b=\"0xA1\"]", actual);
        }

        [TestMethod()]
        public void StructuredDateTimeOffsetValue()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", new DateTimeOffset(2001, 2, 3, 4, 5, 6, 7, TimeSpan.Zero)},
                {"b", new DateTimeOffset(2002, 2, 3, 4, 5, 6, 7, TimeSpan.FromHours(10))},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"2001-02-03T04:05:06.007000+00:00\" b=\"2002-02-03T04:05:06.007000+10:00\"]");
        }

        [TestMethod()]
        public void StructuredDateTimeValue()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", new DateTime(2001, 2, 3)}, {"b", new DateTime(2002, 2, 3, 4, 5, 6, 7, DateTimeKind.Utc)},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe("[- a=\"2001-02-03\" b=\"2002-02-03T04:05:06\"]");
        }

        [TestMethod()]
        public void StructuredEnumValue()
        {
            var properties = new Dictionary<string, object>() {{"a", (TestEnum)5},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"Flag1, Flag3\"]", actual);
        }

        [TestMethod()]
        public void StructuredEscapedStringValue()
        {
            var properties = new Dictionary<string, object>() {{"a", @"w=x\y""z"},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(@"[- a=""w=x\\y\""z""]");
        }

        [TestMethod()]
        public void StructuredEscapePropertyName()
        {
            var properties = new Dictionary<string, object>() {{"a b=c", 1},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a_b_x3D_c=\"1\"]", actual);
        }

        [TestMethod()]
        public void StructuredGuidValue()
        {
            var properties =
                new Dictionary<string, object>() {{"a", new Guid("12345678-abcd-4321-8765-ba9876543210")},};

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"12345678-abcd-4321-8765-ba9876543210\"]", actual);
        }

        [TestMethod()]
        public void StructuredNullableOtherValue()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", (bool?)true},
                {"b", (bool?)null},
                {"c", (DateTime?)new DateTime(2001, 2, 3, 4, 5, 6, 8, DateTimeKind.Unspecified)},
                {"d", (DateTimeOffset?)new DateTimeOffset(2002, 3, 4, 5, 6, 7, 8, TimeSpan.FromHours(10))},
                {"e", (TimeSpan?)new TimeSpan(1, 2, 3, 4, 5)},
                {"f", (Guid?)new Guid("12345678-abcd-4321-8765-ba9876543210")},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            actual.ShouldBe(
                "[- a=\"true\" b=\"null\" c=\"2001-02-03T04:05:06\" d=\"2002-03-04T05:06:07.008000+10:00\" e=\"1.02:03:04.0050000\" f=\"12345678-abcd-4321-8765-ba9876543210\"]");
        }

        [TestMethod()]
        public void StructuredNullableValue()
        {
            int? b = -2;
            int? k = null;
            var properties = new Dictionary<string, object>()
            {
                {"a", (short?)-1},
                {"b", b},
                {"c", (long?)-3},
                {"d", (ushort?)4},
                {"e", (uint?)5},
                {"f", (ulong?)6},
                {"g", (sbyte?)7},
                {"h", (float?)8.1},
                {"i", (double?)9.2},
                {"j", (decimal?)10.3},
                {"k", k},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Console.WriteLine(actual);
            Assert.AreEqual(
                "[- a=\"-1\" b=\"-2\" c=\"-3\" d=\"4\" e=\"5\" f=\"6\" g=\"7\" h=\"8.1\" i=\"9.2\" j=\"10.3\" k=\"null\"]",
                actual);
        }

        [TestMethod()]
        public void StructuredPrimitiveValue()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", (short)-1},
                {"b", (int)-2},
                {"c", (long)-3},
                {"d", (ushort)4},
                {"e", (uint)5},
                {"f", (ulong)6},
                {"g", (sbyte)7},
                {"h", (float)8.1},
                {"i", (double)9.2},
                {"j", (decimal)10.3},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual(
                "[- a=\"-1\" b=\"-2\" c=\"-3\" d=\"4\" e=\"5\" f=\"6\" g=\"7\" h=\"8.1\" i=\"9.2\" j=\"10.3\"]",
                actual);
        }

        [TestMethod()]
        public void StructuredTimeSpanValue()
        {
            var properties = new Dictionary<string, object>()
            {
                {"a", new TimeSpan(0, 0, 0, 4, 5)}, {"b", TimeSpan.Zero - new TimeSpan(1, 2, 3, 4, 5)},
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("[- a=\"00:00:04.0050000\" b=\"-1.02:03:04.0050000\"]", actual);
        }
    }
}
