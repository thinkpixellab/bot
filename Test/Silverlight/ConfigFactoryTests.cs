using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test
{
    [TestClass]
    public class ConfigFactoryTests : SilverlightTest
    {
        [TestMethod]
        public void BasicRoundTrip()
        {
            var factory = ConfigFactory<ConfigFactoryTestClass>.Instance;

            var instance = factory.CreateDefaultInstance();

            var roundTripInstance = testRoundTrip(factory, instance);

            Assert.AreEqual(roundTripInstance.IntProperty, ConfigFactoryTestClass.DefaultIntValue);
            Assert.AreEqual(roundTripInstance.StringProperty, ConfigFactoryTestClass.DefaultStringValue);
            Assert.AreEqual(roundTripInstance.UriProperty, new Uri(ConfigFactoryTestClass.DefaultUriValue));
        }

        [TestMethod]
        public void TweakedRoundTrip()
        {
            var values = new Dictionary<string, object>();

            var intValue = Util.Rnd.Next(500);
            values["IntProperty"] = intValue;

            var stringValue = DateTime.Now.ToLongDateString();
            values["StringProperty"] = stringValue;

            var uriValue = new Uri("http://testCrazy");
            values["UriProperty"] = uriValue;

            var factory = ConfigFactory<ConfigFactoryTestClass>.Instance;
            var instance = factory.CreateInstance(values);

            var roundTrip = testRoundTrip(factory, instance);

            Assert.AreEqual(intValue, roundTrip.IntProperty);
            Assert.AreEqual(stringValue, roundTrip.StringProperty);
            Assert.AreEqual(uriValue, roundTrip.UriProperty);
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
