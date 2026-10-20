using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CoffeeApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoffeeApp.Tests
{
    [TestClass]
    public class CoffeeTests
    {
        private const string ValidLine = "AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,250";

        private Coffee coffee;
        private int savedDefaultTemperature;
        private TextWriter originalOut;
        private StringWriter consoleOutput;
        private CultureInfo originalCulture;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            int _ = Coffee.Counter;
        }

        [ClassCleanup]
        public static void ClassTeardown()
        {
            Coffee.DefaultTemperatureC = 75;
        }

        [TestInitialize]
        public void Setup()
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            originalOut = Console.Out;
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            savedDefaultTemperature = Coffee.DefaultTemperatureC;
            Coffee.DefaultTemperatureC = 75;

            coffee = new Coffee("AB1234", "Ranok Blend", CoffeeType.Latte, 85.50,
                                new DateTime(2024, 3, 15), false, 250);
        }

        [TestCleanup]
        public void Cleanup()
        {
            coffee = null;
            Coffee.DefaultTemperatureC = savedDefaultTemperature;
            Console.SetOut(originalOut);
            consoleOutput.Dispose();
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }

        [TestMethod]
        public void Constructor_FullParameters_SetsAllProperties()
        {
            DateTime date = new DateTime(2024, 3, 15);

            Coffee actual = new Coffee("CD5678", "Milk Dream", CoffeeType.Mocha, 60.0, date, true, 300);

            Assert.AreEqual("CD5678", actual.OrderCode);
            Assert.AreEqual("Milk Dream", actual.Name);
            Assert.AreEqual(CoffeeType.Mocha, actual.Type);
            Assert.AreEqual(60.0, actual.Price, 1e-9);
            Assert.AreEqual(date, actual.RoastDate);
            Assert.IsTrue(actual.IsDecaf);
            Assert.AreEqual(300, actual.VolumeMl);
            Assert.AreEqual(75, actual.TemperatureC);
            Assert.AreEqual("грн", actual.Currency);
        }

        [TestMethod]
        public void Constructor_TwoParameters_SetsDefaultValues()
        {
            string code = "AA1111";
            string name = "Classic";

            Coffee actual = new Coffee(code, name);

            Assert.AreEqual(CoffeeType.Espresso, actual.Type);
            Assert.AreEqual(50.0, actual.Price, 1e-9);
            Assert.AreEqual(DateTime.Today, actual.RoastDate);
            Assert.IsFalse(actual.IsDecaf);
            Assert.AreEqual(200, actual.VolumeMl);
        }

        [TestMethod]
        public void Constructor_ThreeParameters_SetsTypeAndDefaultPrice()
        {
            CoffeeType type = CoffeeType.Cappuccino;

            Coffee actual = new Coffee("BB2222", "Foam", type);

            Assert.AreEqual(type, actual.Type);
            Assert.AreEqual(50.0, actual.Price, 1e-9);
            Assert.AreEqual(200, actual.VolumeMl);
        }

        [TestMethod]
        public void Constructor_FourParameters_SetsPriceAndDefaultVolume()
        {
            double price = 99.99;

            Coffee actual = new Coffee("CC3333", "Strong", CoffeeType.Americano, price);

            Assert.AreEqual(price, actual.Price, 1e-9);
            Assert.AreEqual(CoffeeType.Americano, actual.Type);
            Assert.AreEqual(DateTime.Today, actual.RoastDate);
        }

        [TestMethod]
        public void Constructor_Default_SetsOnlyDefaultTemperature_CounterNotChanged()
        {
            int counterBefore = Coffee.Counter;

            Coffee actual = new Coffee();

            Assert.IsNull(actual.OrderCode);
            Assert.IsNull(actual.Name);
            Assert.AreEqual(Coffee.DefaultTemperatureC, actual.TemperatureC);
            Assert.AreEqual(counterBefore, Coffee.Counter);
        }

        [TestMethod]
        public void Constructor_ValidData_IncrementsCounterByOne()
        {
            int expected = Coffee.Counter + 1;

            new Coffee("DD4444", "Counter Test");
            int actual = Coffee.Counter;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_InvalidData_ThrowsAndCounterNotChanged()
        {
            int expected = Coffee.Counter;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new Coffee("EE5555", "Bad Price", CoffeeType.Latte, -10.0));
            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        public void Constructor_UsesStaticDefaultTemperature()
        {
            Coffee.DefaultTemperatureC = 60;

            Coffee actual = new Coffee("FF6666", "Warm");

            Assert.AreEqual(60, actual.TemperatureC);
        }

        [TestMethod]
        [DataRow("AB1234")]
        [DataRow("123456")]
        [DataRow("abcdef")]
        [DataRow("Кава12")]
        public void OrderCode_SetValidValue_ValueIsSaved(string code)
        {
            coffee.OrderCode = code;

            Assert.AreEqual(code, coffee.OrderCode);
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        public void OrderCode_SetNullOrEmpty_ThrowsArgumentNullException(string code)
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => coffee.OrderCode = code);
        }

        [TestMethod]
        [DataRow("AB12")]
        [DataRow("AB12345")]
        [DataRow("AB12#4")]
        [DataRow("AB 123")]
        public void OrderCode_SetInvalidValue_ThrowsArgumentException_ValueNotChanged(string code)
        {
            string expected = coffee.OrderCode;

            ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() => coffee.OrderCode = code);

            StringAssert.StartsWith(ex.Message, "Error:");
            Assert.AreEqual(expected, coffee.OrderCode);
        }

        [TestMethod]
        [DataRow("Ra")]
        [DataRow("Ranok Blend")]
        [DataRow("abcdefghijklmnopqrstuvwxyz1234")]
        public void Name_SetValidValue_ValueIsSaved(string name)
        {
            coffee.Name = name;

            Assert.AreEqual(name, coffee.Name);
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Name_SetNullOrWhiteSpace_ThrowsArgumentNullException(string name)
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => coffee.Name = name);
        }

        [TestMethod]
        [DataRow("R")]
        [DataRow("abcdefghijklmnopqrstuvwxyz12345")]
        public void Name_SetWrongLength_ThrowsArgumentOutOfRangeException(string name)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.Name = name);
        }

        [TestMethod]
        public void Name_SetValueWithComma_ThrowsArgumentException()
        {
            string name = "Ranok,Blend";

            Assert.ThrowsExactly<ArgumentException>(() => coffee.Name = name);
        }

        [TestMethod]
        [DataRow(CoffeeType.Espresso)]
        [DataRow(CoffeeType.Americano)]
        [DataRow(CoffeeType.Cappuccino)]
        [DataRow(CoffeeType.Latte)]
        [DataRow(CoffeeType.Mocha)]
        [DataRow(CoffeeType.FlatWhite)]
        public void Type_SetDefinedValue_ValueIsSaved(CoffeeType type)
        {
            coffee.Type = type;

            Assert.AreEqual(type, coffee.Type);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(6)]
        [DataRow(99)]
        public void Type_SetUndefinedValue_ThrowsArgumentException(int value)
        {
            CoffeeType type = (CoffeeType)value;

            Assert.ThrowsExactly<ArgumentException>(() => coffee.Type = type);
            Assert.AreEqual(CoffeeType.Latte, coffee.Type);
        }

        [TestMethod]
        [DataRow(0.01)]
        [DataRow(85.5)]
        [DataRow(500.0)]
        public void Price_SetValueInRange_ValueIsSaved(double price)
        {
            coffee.Price = price;

            Assert.AreEqual(price, coffee.Price, 1e-9);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-100.0)]
        [DataRow(500.01)]
        [DataRow(600.0)]
        public void Price_SetValueOutOfRange_ThrowsAndValueNotChanged(double price)
        {
            double expected = coffee.Price;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.Price = price);
            Assert.AreEqual(expected, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void RoastDate_SetMinimalDate_ValueIsSaved()
        {
            DateTime date = new DateTime(2020, 1, 1);

            coffee.RoastDate = date;

            Assert.AreEqual(date, coffee.RoastDate);
        }

        [TestMethod]
        public void RoastDate_SetToday_ValueIsSaved()
        {
            DateTime date = DateTime.Today;

            coffee.RoastDate = date;

            Assert.AreEqual(date, coffee.RoastDate);
        }

        [TestMethod]
        public void RoastDate_SetDateBefore2020_ThrowsArgumentOutOfRangeException()
        {
            DateTime date = new DateTime(2019, 12, 31);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.RoastDate = date);
        }

        [TestMethod]
        public void RoastDate_SetFutureDate_ThrowsArgumentOutOfRangeException()
        {
            DateTime date = DateTime.Today.AddDays(1);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.RoastDate = date);
            Assert.AreEqual(new DateTime(2024, 3, 15), coffee.RoastDate);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void IsDecaf_SetValue_ValueIsSaved(bool value)
        {
            coffee.IsDecaf = value;

            Assert.AreEqual(value, coffee.IsDecaf);
        }

        [TestMethod]
        [DataRow(30)]
        [DataRow(250)]
        [DataRow(1000)]
        public void VolumeMl_SetValueInRange_ValueIsSaved(int volume)
        {
            coffee.VolumeMl = volume;

            Assert.AreEqual(volume, coffee.VolumeMl);
        }

        [TestMethod]
        [DataRow(29)]
        [DataRow(0)]
        [DataRow(1001)]
        public void VolumeMl_SetValueOutOfRange_ThrowsArgumentOutOfRangeException(int volume)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.VolumeMl = volume);
            Assert.AreEqual(250, coffee.VolumeMl);
        }

        [TestMethod]
        public void PricePer100Ml_Price85_50Volume250_Returns34_20()
        {
            double expected = 34.20;

            double actual = coffee.PricePer100Ml;

            Assert.AreEqual(expected, actual, 1e-9);
        }

        [TestMethod]
        [DataRow(20, 20)]
        [DataRow(60, 60)]
        [DataRow(95, 95)]
        [DataRow(19, 75)]
        [DataRow(96, 75)]
        [DataRow(-5, 75)]
        public void DefaultTemperatureC_SetValue_SavesValueOrResetsTo75(int value, int expected)
        {
            Coffee.DefaultTemperatureC = value;
            int actual = Coffee.DefaultTemperatureC;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("AB1234", true)]
        [DataRow("Кава12", true)]
        [DataRow("", true)]
        [DataRow("AB-123", false)]
        [DataRow("AB 123", false)]
        public void IsLettersOrDigits_ReturnsExpected(string text, bool expected)
        {
            bool actual = Coffee.IsLettersOrDigits(text);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("AB1234", true)]
        [DataRow("ab12cd", true)]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow("AB123", false)]
        [DataRow("AB12345", false)]
        [DataRow("AB12#4", false)]
        public void IsValidOrderCode_ReturnsExpected(string code, bool expected)
        {
            bool actual = Coffee.IsValidOrderCode(code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(0, 32.0)]
        [DataRow(100, 212.0)]
        [DataRow(23, 73.4)]
        [DataRow(-40, -40.0)]
        public void CelsiusToFahrenheit_ReturnsExpected(int celsius, double expected)
        {
            double actual = Coffee.CelsiusToFahrenheit(celsius);

            Assert.AreEqual(expected, actual, 1e-9);
        }

        [TestMethod]
        [DataRow(34.2, 250, 85.5)]
        [DataRow(20.0, 100, 20.0)]
        [DataRow(40.0, 30, 12.0)]
        public void CalculatePrice_ReturnsExpected(double pricePer100Ml, int volume, double expected)
        {
            double actual = Coffee.CalculatePrice(pricePer100Ml, volume);

            Assert.AreEqual(expected, actual, 1e-9);
        }

        [TestMethod]
        public void DecreaseCounter_PositiveCount_DecreasesCounter()
        {
            new Coffee("GG7777", "One");
            new Coffee("GG8888", "Two");
            int expected = Coffee.Counter - 2;

            Coffee.DecreaseCounter(2);

            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        public void DecreaseCounter_CountGreaterThanCounter_CounterBecomesZero()
        {
            int count = Coffee.Counter + 100;

            Coffee.DecreaseCounter(count);

            Assert.AreEqual(0, Coffee.Counter);
        }

        [TestMethod]
        public void DecreaseCounter_NegativeCount_ThrowsAndCounterNotChanged()
        {
            int expected = Coffee.Counter;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Coffee.DecreaseCounter(-5));
            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        public void Parse_ValidString_ReturnsObjectWithCorrectProperties()
        {
            Coffee actual = Coffee.Parse(ValidLine);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(Coffee));
            Assert.AreEqual("AB1234", actual.OrderCode);
            Assert.AreEqual("Ranok Blend", actual.Name);
            Assert.AreEqual(CoffeeType.Latte, actual.Type);
            Assert.AreEqual(85.50, actual.Price, 1e-9);
            Assert.AreEqual(new DateTime(2024, 3, 15), actual.RoastDate);
            Assert.IsFalse(actual.IsDecaf);
            Assert.AreEqual(250, actual.VolumeMl);
        }

        [TestMethod]
        public void Parse_ValidString_IncrementsCounter()
        {
            int expected = Coffee.Counter + 1;

            Coffee.Parse(ValidLine);

            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        public void Parse_StringWithSpacesAndLowerCaseType_IsTrimmedAndParsed()
        {
            string s = " AB1234 , Ranok Blend , latte , 85.50 , 15.03.2024 , Так , 250 ";

            Coffee actual = Coffee.Parse(s);

            Assert.AreEqual("AB1234", actual.OrderCode);
            Assert.AreEqual("Ranok Blend", actual.Name);
            Assert.AreEqual(CoffeeType.Latte, actual.Type);
            Assert.IsTrue(actual.IsDecaf);
        }

        [TestMethod]
        [DataRow("Так", true)]
        [DataRow("так", true)]
        [DataRow("yes", true)]
        [DataRow("TRUE", true)]
        [DataRow("Ні", false)]
        [DataRow("no", false)]
        [DataRow("false", false)]
        public void Parse_DecafVariants_ParsedCorrectly(string decaf, bool expected)
        {
            string s = $"AB1234,Ranok Blend,Latte,85.50,15.03.2024,{decaf},250";

            Coffee actual = Coffee.Parse(s);

            Assert.AreEqual(expected, actual.IsDecaf);
        }

        [TestMethod]
        public void Parse_TwoCallsWithSameString_ReturnDifferentObjects()
        {
            Coffee first = Coffee.Parse(ValidLine);
            Coffee second = Coffee.Parse(ValidLine);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(first.ToString(), second.ToString());
        }

        [TestMethod]
        public void Parse_ResultOfToString_RestoresEqualObject()
        {
            string s = coffee.ToString();

            Coffee actual = Coffee.Parse(s);

            Assert.AreEqual(s, actual.ToString());
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        public void Parse_NullOrEmpty_ThrowsArgumentNullException(string s)
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Coffee.Parse(s));
        }

        [TestMethod]
        [DataRow("250")]
        [DataRow("AB1234,Ranok Blend,Latte")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,250,extra")]
        [DataRow("AB1234,Ranok Blend,Latte,85,50,15.03.2024,Ні,250")]
        public void Parse_WrongNumberOfValues_ThrowsFormatException(string s)
        {
            FormatException ex = Assert.ThrowsExactly<FormatException>(() => Coffee.Parse(s));

            StringAssert.Contains(ex.Message, "очікується 7 значень");
        }

        [TestMethod]
        [DataRow("AB1234,Ranok Blend,Frappe,85.50,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,1,85.50,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50a,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.13.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,2024-03-15,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Можливо,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,250a")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,250.5")]
        public void Parse_InvalidFormatOfValue_ThrowsFormatException(string s)
        {
            Assert.ThrowsExactly<FormatException>(() => Coffee.Parse(s));
        }

        [TestMethod]
        [DataRow("AB1234,R,Latte,85.50,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,-100,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,600,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2019,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,20")]
        [DataRow("AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,1500")]
        public void Parse_ValueOutOfRange_ThrowsArgumentOutOfRangeException(string s)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Coffee.Parse(s));
        }

        [TestMethod]
        [DataRow(",Ranok Blend,Latte,85.50,15.03.2024,Ні,250")]
        [DataRow("AB1234,,Latte,85.50,15.03.2024,Ні,250")]
        public void Parse_EmptyCodeOrName_ThrowsArgumentNullException(string s)
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Coffee.Parse(s));
        }

        [TestMethod]
        [DataRow("AB12,Ranok Blend,Latte,85.50,15.03.2024,Ні,250")]
        [DataRow("AB12#4,Ranok Blend,Latte,85.50,15.03.2024,Ні,250")]
        public void Parse_InvalidOrderCode_ThrowsArgumentException(string s)
        {
            Assert.Throws<ArgumentException>(() => Coffee.Parse(s));
        }

        [TestMethod]
        public void Parse_InvalidString_CounterNotChanged()
        {
            int expected = Coffee.Counter;

            Assert.Throws<Exception>(() => Coffee.Parse("AB1234,Ranok Blend,Latte,600,15.03.2024,Ні,250"));

            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        public void TryParse_ValidString_ReturnsTrueAndObject()
        {
            bool result = Coffee.TryParse(ValidLine, out Coffee actual);

            Assert.IsTrue(result);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ValidLine, actual.ToString());
        }

        [TestMethod]
        public void TryParse_ValidString_IncrementsCounter()
        {
            int expected = Coffee.Counter + 1;

            Coffee.TryParse(ValidLine, out _);

            Assert.AreEqual(expected, Coffee.Counter);
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("250")]
        [DataRow("AB1234,Ranok Blend,Frappe,85.50,15.03.2024,Ні,250")]
        [DataRow("AB12#4,Ranok Blend,Latte,85.50,15.03.2024,Ні,250")]
        [DataRow("AB1234,Ranok Blend,Latte,600,15.03.2024,Ні,250")]
        public void TryParse_InvalidString_ReturnsFalseNullAndCounterNotChanged(string s)
        {
            int expectedCounter = Coffee.Counter;

            bool result = Coffee.TryParse(s, out Coffee actual);

            Assert.IsFalse(result);
            Assert.IsNull(actual);
            Assert.AreEqual(expectedCounter, Coffee.Counter);
            StringAssert.Contains(consoleOutput.ToString(), "Error:");
        }

        [TestMethod]
        public void TryParse_WrongNumberOfValues_PrintsFormatMessage()
        {
            string s = "AB1234,Ranok Blend,Latte";

            Coffee.TryParse(s, out _);
            string output = consoleOutput.ToString();

            StringAssert.Contains(output, "невірний формат рядка");
        }

        [TestMethod]
        public void Reheat_NoParameters_IncreasesTemperatureBy10()
        {
            int expected = 85;

            coffee.Reheat();

            Assert.AreEqual(expected, coffee.TemperatureC);
            StringAssert.Contains(consoleOutput.ToString(), "[Виклик Reheat()]");
        }

        [TestMethod]
        [DataRow(5, 80)]
        [DataRow(20, 95)]
        [DataRow(50, 95)]
        [DataRow(-60, 20)]
        public void Reheat_WithDegrees_ChangesTemperatureWithinLimits(int degrees, int expected)
        {
            coffee.Reheat(degrees);

            Assert.AreEqual(expected, coffee.TemperatureC);
        }

        [TestMethod]
        public void Reheat_CalledTwice_TemperatureNotAbove95()
        {
            coffee.Reheat(15);
            coffee.Reheat(15);

            Assert.AreEqual(95, coffee.TemperatureC);
            Assert.IsTrue(coffee.TemperatureC <= 95);
        }

        [TestMethod]
        [DataRow(10, 65)]
        [DataRow(55, 20)]
        [DataRow(100, 20)]
        [DataRow(-30, 95)]
        public void Cool_WithDegrees_ChangesTemperatureWithinLimits(int degrees, int expected)
        {
            coffee.Cool(degrees);

            Assert.AreEqual(expected, coffee.TemperatureC);
        }

        [TestMethod]
        public void Cool_AfterReheat_ReturnsToInitialTemperature()
        {
            int expected = coffee.TemperatureC;

            coffee.Reheat(10);
            coffee.Cool(10);

            Assert.AreEqual(expected, coffee.TemperatureC);
        }

        [TestMethod]
        [DataRow(10.0, 76.95)]
        [DataRow(50.0, 42.75)]
        [DataRow(99.0, 0.855)]
        public void ApplyDiscount_ValidPercent_DecreasesPrice(double percent, double expected)
        {
            coffee.ApplyDiscount(percent);

            Assert.AreEqual(expected, coffee.Price, 1e-9);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-10.0)]
        [DataRow(100.0)]
        [DataRow(150.0)]
        public void ApplyDiscount_InvalidPercent_ThrowsAndPriceNotChanged(double percent)
        {
            double expected = coffee.Price;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.ApplyDiscount(percent));
            Assert.AreEqual(expected, coffee.Price, 1e-9);
        }

        [TestMethod]
        [DataRow(10.0, 30.0, 76.95)]
        [DataRow(90.0, 30.0, 30.0)]
        [DataRow(50.0, 100.0, 85.5)]
        public void ApplyDiscount_WithMinPrice_ReturnsExpectedPrice(double percent, double minPrice, double expected)
        {
            coffee.ApplyDiscount(percent, minPrice);

            Assert.AreEqual(expected, coffee.Price, 1e-9);
        }

        [TestMethod]
        [DataRow(-5.0)]
        [DataRow(100.0)]
        public void ApplyDiscount_WithMinPrice_InvalidPercent_Throws(double percent)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => coffee.ApplyDiscount(percent, 30.0));
            Assert.AreEqual(85.5, coffee.Price, 1e-9);
        }

        [TestMethod]
        [DataRow(50, 300)]
        [DataRow(750, 1000)]
        [DataRow(800, 1000)]
        [DataRow(-300, 30)]
        public void AddVolume_ChangesVolumeWithinLimits(int ml, int expected)
        {
            coffee.AddVolume(ml);

            Assert.AreEqual(expected, coffee.VolumeMl);
        }

        [TestMethod]
        public void AddVolume_PriceIsNotChanged()
        {
            double expected = coffee.Price;

            coffee.AddVolume(100);

            Assert.AreEqual(expected, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void AddVolume_IncreasePriceTrue_PriceIncreasesProportionally()
        {
            double expectedPrice = 85.5 * 300 / 250;

            coffee.AddVolume(50, true);

            Assert.AreEqual(300, coffee.VolumeMl);
            Assert.AreEqual(expectedPrice, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void AddVolume_IncreasePriceFalse_PriceNotChanged()
        {
            double expectedPrice = coffee.Price;

            coffee.AddVolume(50, false);

            Assert.AreEqual(300, coffee.VolumeMl);
            Assert.AreEqual(expectedPrice, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void AddVolume_IncreasePriceTrue_PriceNotAbove500()
        {
            coffee.Price = 400;

            coffee.AddVolume(750, true);

            Assert.AreEqual(1000, coffee.VolumeMl);
            Assert.AreEqual(500.0, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void AddVolume_IncreasePriceTrue_NegativeMl_PriceDecreases()
        {
            coffee.AddVolume(-125, true);

            Assert.AreEqual(125, coffee.VolumeMl);
            Assert.AreEqual(42.75, coffee.Price, 1e-9);
        }

        [TestMethod]
        public void ToString_ReturnsStringInParseFormat()
        {
            string actual = coffee.ToString();

            Assert.AreEqual(ValidLine, actual);
        }

        [TestMethod]
        public void ToString_Decaf_ContainsTak_AndSevenParts()
        {
            coffee.IsDecaf = true;
            string[] expected = { "AB1234", "Ranok Blend", "Latte", "85.50", "15.03.2024", "Так", "250" };

            string[] actual = coffee.ToString().Split(',');

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToTableRow_ReturnsFormattedRow()
        {
            string expected = "AB1234  Ranok Blend         Latte       85.50     15.03.2024    Ні     250     75";

            string actual = coffee.ToTableRow();

            Assert.AreEqual(expected, actual);
            StringAssert.StartsWith(actual, "AB1234");
            StringAssert.EndsWith(actual, "75");
        }

        [TestMethod]
        public void ToTableRow_IsNotEqualToToString()
        {
            string row = coffee.ToTableRow();
            string str = coffee.ToString();

            Assert.AreNotEqual(str, row);
        }
    }
}