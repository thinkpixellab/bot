using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;
using PixelLab.Test.Helpers;

namespace PixelLab.Test
{
    [TestClass]
    public class ConfigFactoryTests : SilverlightTest
    {
        private readonly ConfigFactory<ConfigFactoryTestClass> _factory = ConfigFactory<ConfigFactoryTestClass>.Instance;

        [TestMethod]
        public void BasicRoundTrip()
        {
            var instance = _factory.CreateDefaultInstance();

            var roundTripInstance = testRoundTrip(_factory, instance);

            Assert.AreEqual(roundTripInstance.IntProperty, ConfigFactoryTestClass.DefaultIntValue);
            Assert.AreEqual(roundTripInstance.StringProperty, ConfigFactoryTestClass.DefaultStringValue);
            Assert.AreEqual(roundTripInstance.UriProperty, new Uri(ConfigFactoryTestClass.DefaultUriValue));
        }

        [TestMethod]
        public void TweakedRoundTrip()
        {
            var intValue = Util.Rnd.Next(500);
            var stringValue = DateTime.Now.ToLongDateString();
            var uriValue = new Uri("http://testCrazy");

            var values = new Dictionary<string, object>(){
                { "IntProperty", intValue},
                { "StringProperty", stringValue},
                { "UriProperty", uriValue}
            };

            var instance = _factory.CreateInstance(values);

            var roundTrip = testRoundTrip(_factory, instance);

            Assert.AreEqual(intValue, roundTrip.IntProperty);
            Assert.AreEqual(stringValue, roundTrip.StringProperty);
            Assert.AreEqual(uriValue, roundTrip.UriProperty);
        }

        [TestMethod]
        public void TestMissingProperty()
        {
            var intValue = Util.Rnd.Next(500);
            var uriValue = new Uri("http://testCrazy");

            var values = new Dictionary<string, object>(){
                { "IntProperty", intValue},
                { "UriProperty", uriValue}
            };

            var argEx = AssertPlus.ExceptionThrown<ArgumentException>(() => _factory.CreateInstance(values));
            Assert.IsTrue(argEx.Message.StartsWith("missing required key StringProperty"));
        }

        private T testRoundTrip<T>(ConfigFactory<T> factory, T instance)
        {
            var factoryDefaultValues = factory.GetValues(instance);

            var roundTripInstance = factory.CreateInstance(factoryDefaultValues);

            Assert.IsTrue(factory.Equals(instance, roundTripInstance));
            Assert.AreEqual(factory.GetHashCode(instance), factory.GetHashCode(roundTripInstance));

            return roundTripInstance;
        }
    }

    public class ConfigFactoryTestClass
    {
        public const string DefaultStringValue = "A cool test string";
        public const int DefaultIntValue = 42;
        public const string DefaultUriValue = "http://thinkpixellab.com";

        public ConfigFactoryTestClass(string stringProperty, Uri uriProperty, int intProperty)
        {
            this.StringProperty = stringProperty;
            this.UriProperty = uriProperty;
            this.IntProperty = intProperty;
        }

        [DefaultValue(DefaultIntValue)]
        public int IntProperty { get; private set; }

        [DefaultValue(DefaultStringValue)]
        public string StringProperty { get; private set; }

        [DefaultValue(DefaultUriValue)]
        public Uri UriProperty { get; private set; }

    }
}
