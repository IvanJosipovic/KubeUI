using System;
using System.Collections.Generic;
using Xunit;
using KubeUI.Core;
using KubeUI.Schema;

namespace KubeUI.Tests
{
    public class Extension
    {
        private class TestObject
        {
            public List<string> testnull { get; set; }
            public List<string> testnull2 { get; set; } = new List<string>();
            public List<string> test { get; set; } = new List<string>() { "test" };

            public Dictionary<string, string> dictnull { get; set; }
            public Dictionary<string, string> dictnull2 { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> dict { get; set; } = new Dictionary<string, string>() { { "13", "3" } };

            public TestObject2 obj2 { get; set; } = new TestObject2();

            public string testString { get; set; }
            public int? testInt { get; set; }
        }

        private class TestObject2
        {
            public List<string> testnull { get; set; }
            public List<string> testnull2 { get; set; } = new List<string>();
            public List<string> test { get; set; } = new List<string>() { "test" };

            public Dictionary<string, string> dictnull { get; set; }
            public Dictionary<string, string> dictnull2 { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> dict { get; set; } = new Dictionary<string, string>() { { "13", "3" } };

            public TestObject3 obj3 { get; set; } = new TestObject3();

            public string testString { get; set; }
            public int? testInt { get; set; }
        }

        private class TestObject3
        {
            public List<string> testnull { get; set; }
            public List<string> testnull2 { get; set; } = new List<string>();
            public List<string> test { get; set; } = new List<string>() { "test" };

            public Dictionary<string, string> dictnull { get; set; }
            public Dictionary<string, string> dictnull2 { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> dict { get; set; } = new Dictionary<string, string>() { { "13", "3" } };

            public TestNullObject obj4 { get; set; } = new TestNullObject();

            public string testString { get; set; }
            public int? testInt { get; set; }
        }

        private class TestNullObject
        {
            public List<string> testnull { get; set; }
            public List<string> testnull2 { get; set; } = new List<string>();

            public Dictionary<string, string> dictnull { get; set; }
            public Dictionary<string, string> dictnull2 { get; set; } = new Dictionary<string, string>();

            public string testString { get; set; }
            public int? testInt { get; set; }
        }

        private class TestCol
        {
            public List<TestNullObject> testnull { get; set; } = new List<TestNullObject>() { new TestNullObject() };

            public List<TestNullObject> testsingle { get; set; } = new List<TestNullObject>() { new TestNullObject(), new TestNullObject() {testString = "test" }, new TestNullObject() };
        }

        [Fact]
        public void GutObjectTest()
        {
            var testObject = new TestObject();

            testObject.GutObject();

            Assert.Null(testObject.testnull);
            Assert.Null(testObject.testnull2);
            Assert.NotNull(testObject.test);

            Assert.Null(testObject.dictnull);
            Assert.Null(testObject.dictnull2);
            Assert.NotNull(testObject.dict);
        }

        [Fact]
        public void GutObjectChildTest()
        {
            var testObject = new TestObject();

            testObject.GutObject();

            Assert.Null(testObject.obj2.testnull);
            Assert.Null(testObject.obj2.testnull2);
            Assert.NotNull(testObject.obj2.test);

            Assert.Null(testObject.obj2.dictnull);
            Assert.Null(testObject.obj2.dictnull2);
            Assert.NotNull(testObject.obj2.dict);
        }

        [Fact]
        public void GutObjectChildNullTest()
        {
            var testObject = new TestObject();

            testObject.GutObject();

            Assert.Null(testObject.obj2.obj3.obj4);
        }

        [Fact]
        public void GutObjectCollNullTest()
        {
            var testObject = new TestCol();

            testObject.GutObject();

            Assert.Null(testObject.testnull);

            Assert.NotNull(testObject.testsingle);
            Assert.True(testObject.testsingle.Count == 1);
        }
    }
}
