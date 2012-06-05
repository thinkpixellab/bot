using System;
using System.Globalization;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test
{
    [TestClass]
    public class EnumConverterTest : SilverlightTest
    {
        private readonly EnumConverter _converter = new EnumConverter();

        [TestMethod]
        public void TestEnumConverter()
        {
            var flipBool = "true->false,true";

            Test(flipBool, false, true);
            Test(flipBool, true, false);
            Test(flipBool, false, "true");
            Test(flipBool, true, "false");
            Test(flipBool, false, "TRUE");
            Test(flipBool, true, "FalsE");

            // string comparisons are case-insensitive
            Test(flipBool, "false", true);
            Test(flipBool, "true", false);
            Test(flipBool, "false", "true");
            Test(flipBool, "true", "false");
            Test(flipBool, "false", "TRUE");
            Test(flipBool, "true", "FalsE");


            var objectNullToBoolTest = "null->False,True";
            Test(objectNullToBoolTest, false, null);
            Test(objectNullToBoolTest, true, this);
            Test(objectNullToBoolTest, true, new Nullable<int>(4));

            var objectNullToEnumTest = "null->Saturday,Sunday";
            Test(objectNullToEnumTest, DayOfWeek.Saturday, null);
            Test(objectNullToEnumTest, DayOfWeek.Sunday, this);
            Test(objectNullToEnumTest, DayOfWeek.Sunday, new Nullable<int>(5));
        }

        private void Test<T>(string param, T targetValue, object input)
        {
            Assert.AreEqual(targetValue, _converter.Convert(input, typeof(T), param, CultureInfo.InvariantCulture));
        }
    }
}
