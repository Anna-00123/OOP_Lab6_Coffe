using CoffeeApp;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CoffeeApp.Tests
{
    [TestClass]
    public class ProgramTests
    {
        private const int MaxCount = 5;

        private List<Coffee> coffees;
        private Coffee latte;
        private Coffee mocha;
        private Coffee espresso;
        private TextWriter originalOut;
        private StringWriter consoleOutput;
        private CultureInfo originalCulture;

        [TestInitialize]
        public void Setup()
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            originalOut = Console.Out;
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            latte = new Coffee("AB1234", "Ranok Blend", CoffeeType.Latte, 85.50,
                               new DateTime(2024, 3, 15), false, 250);
            mocha = new Coffee("CD5678", "Milk Dream", CoffeeType.Mocha, 60.00,
                               new DateTime(2024, 5, 1), true, 300);
            espresso = new Coffee("EF9012", "Dark Night", CoffeeType.Latte, 40.00,
                                  new DateTime(2023, 11, 20), false, 30);

            coffees = new List<Coffee> { latte, mocha, espresso };
        }

        [TestCleanup]
        public void Cleanup()
        {
            coffees = null;
            Console.SetOut(originalOut);
            consoleOutput.Dispose();
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }

        private static Coffee NewCoffee(string code)
        {
            return new Coffee(code, "Test Coffee", CoffeeType.Americano, 50.0,
                              new DateTime(2024, 1, 1), false, 200);
        }

        [TestMethod]
        public void AddCoffee_NewCoffee_ReturnsTrueAndAddsToEnd()
        {
            Coffee item = NewCoffee("ZZ0001");

            bool result = Program.AddCoffee(coffees, item, MaxCount);

            Assert.IsTrue(result);
            Assert.AreEqual(4, coffees.Count);
            Assert.AreSame(item, coffees[3]);
        }

        [TestMethod]
        public void AddCoffee_ListIsFull_ReturnsFalseAndListNotChanged()
        {
            Program.AddCoffee(coffees, NewCoffee("ZZ0001"), MaxCount);
            Program.AddCoffee(coffees, NewCoffee("ZZ0002"), MaxCount);
            Coffee extra = NewCoffee("ZZ0003");

            bool result = Program.AddCoffee(coffees, extra, MaxCount);

            Assert.IsFalse(result);
            Assert.AreEqual(MaxCount, coffees.Count);
            CollectionAssert.DoesNotContain(coffees, extra);
        }

        [TestMethod]
        [DataRow("AB1234")]
        [DataRow("ab1234")]
        public void AddCoffee_DuplicateCode_ThrowsArgumentException(string code)
        {
            Coffee duplicate = NewCoffee(code);

            ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(
                () => Program.AddCoffee(coffees, duplicate, MaxCount));

            StringAssert.Contains(ex.Message, "вже є у списку");
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        public void AddCoffee_NullCoffee_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Program.AddCoffee(coffees, null, MaxCount));
        }

        [TestMethod]
        public void AddCoffee_NullList_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Program.AddCoffee(null, NewCoffee("ZZ0001"), MaxCount));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-3)]
        public void AddCoffee_InvalidMaxCount_ThrowsArgumentOutOfRangeException(int max)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => Program.AddCoffee(coffees, NewCoffee("ZZ0001"), max));
        }

        [TestMethod]
        public void AddCoffeeRange_EnoughSpace_AddsAllInOrder()
        {
            List<Coffee> items = new List<Coffee> { NewCoffee("ZZ0001"), NewCoffee("ZZ0002") };

            int added = Program.AddCoffeeRange(coffees, items, MaxCount);

            Assert.AreEqual(2, added);
            Assert.AreEqual(5, coffees.Count);
            Assert.AreEqual("ZZ0001", coffees[3].OrderCode);
            Assert.AreEqual("ZZ0002", coffees[4].OrderCode);
        }

        [TestMethod]
        public void AddCoffeeRange_NotEnoughSpace_ReturnsZeroAndNothingAdded()
        {
            List<Coffee> items = new List<Coffee> { NewCoffee("ZZ0001"), NewCoffee("ZZ0002"), NewCoffee("ZZ0003") };

            int added = Program.AddCoffeeRange(coffees, items, MaxCount);

            Assert.AreEqual(0, added);
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        public void AddCoffeeRange_DuplicateInsideItems_ThrowsArgumentException()
        {
            List<Coffee> items = new List<Coffee> { NewCoffee("ZZ0001"), NewCoffee("zz0001") };

            Assert.ThrowsExactly<ArgumentException>(() => Program.AddCoffeeRange(coffees, items, MaxCount));
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        public void AddCoffeeRange_ItemsContainNull_ThrowsArgumentNullException()
        {
            List<Coffee> items = new List<Coffee> { NewCoffee("ZZ0001"), null };

            Assert.ThrowsExactly<ArgumentNullException>(() => Program.AddCoffeeRange(coffees, items, MaxCount));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(3)]
        public void InsertCoffee_ValidIndex_InsertsAtPosition(int index)
        {
            Coffee item = NewCoffee("ZZ0001");

            bool result = Program.InsertCoffee(coffees, index, item, MaxCount);

            Assert.IsTrue(result);
            Assert.AreSame(item, coffees[index]);
            Assert.AreEqual(4, coffees.Count);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(4)]
        public void InsertCoffee_InvalidIndex_ThrowsArgumentOutOfRangeException(int index)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => Program.InsertCoffee(coffees, index, NewCoffee("ZZ0001"), MaxCount));
        }

        [TestMethod]
        public void InsertCoffee_ListIsFull_ReturnsFalse()
        {
            bool result = Program.InsertCoffee(coffees, 0, NewCoffee("ZZ0001"), 3);

            Assert.IsFalse(result);
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        [DataRow("AB1234", true)]
        [DataRow("cd5678", true)]
        [DataRow("  EF9012 ", true)]
        [DataRow("XX0000", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        public void ContainsCode_ReturnsExpected(string code, bool expected)
        {
            bool actual = Program.ContainsCode(coffees, code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("CD5678")]
        [DataRow("cd5678")]
        [DataRow(" CD5678 ")]
        public void FindByCode_ExistingCode_ReturnsSameObject(string code)
        {
            Coffee actual = Program.FindByCode(coffees, code);

            Assert.IsNotNull(actual);
            Assert.AreSame(mocha, actual);
        }

        [TestMethod]
        [DataRow("XX0000")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow((string)null)]
        public void FindByCode_UnknownOrEmptyCode_ReturnsNull(string code)
        {
            Coffee actual = Program.FindByCode(coffees, code);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [DataRow("AB1234", 0)]
        [DataRow("EF9012", 2)]
        [DataRow("XX0000", -1)]
        [DataRow(null, -1)]
        public void FindIndexByCode_ReturnsExpectedIndex(string code, int expected)
        {
            int actual = Program.FindIndexByCode(coffees, code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FindByCode_EmptyList_ReturnsNull()
        {
            List<Coffee> empty = new List<Coffee>();

            Coffee actual = Program.FindByCode(empty, "AB1234");

            Assert.IsNull(actual);
        }

        [TestMethod]
        [DataRow("Ranok", 1)]
        [DataRow("dream", 1)]
        [DataRow("a", 3)]
        [DataRow("Cappuccino", 0)]
        public void FindByName_ReturnsExpectedCount(string part, int expected)
        {
            List<Coffee> result = Program.FindByName(coffees, part);

            Assert.AreEqual(expected, result.Count);
        }

        [TestMethod]
        public void FindByName_ResultContainsFoundObject_OriginalListNotChanged()
        {
            List<Coffee> result = Program.FindByName(coffees, "Night");

            CollectionAssert.Contains(result, espresso);
            Assert.AreEqual(3, coffees.Count);
            Assert.AreNotSame(coffees, result);
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("   ")]
        public void FindByName_EmptyText_ThrowsArgumentNullException(string part)
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Program.FindByName(coffees, part));
        }

        [TestMethod]
        public void FindByType_Latte_ReturnsTwoInOriginalOrder()
        {
            List<Coffee> expected = new List<Coffee> { latte, espresso };

            List<Coffee> actual = Program.FindByType(coffees, CoffeeType.Latte);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FindByType_NoSuchType_ReturnsEmptyList()
        {
            List<Coffee> actual = Program.FindByType(coffees, CoffeeType.FlatWhite);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(6)]
        public void FindByType_UndefinedType_ThrowsArgumentException(int value)
        {
            Assert.ThrowsExactly<ArgumentException>(() => Program.FindByType(coffees, (CoffeeType)value));
        }

        [TestMethod]
        [DataRow(0.0, 500.0, 3)]
        [DataRow(40.0, 60.0, 2)]
        [DataRow(60.0, 60.0, 1)]
        [DataRow(86.0, 100.0, 0)]
        public void FindByPriceRange_ReturnsExpectedCount(double min, double max, int expected)
        {
            List<Coffee> result = Program.FindByPriceRange(coffees, min, max);

            Assert.AreEqual(expected, result.Count);
        }

        [TestMethod]
        public void FindByPriceRange_MinGreaterThanMax_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() => Program.FindByPriceRange(coffees, 100, 50));
        }

        [TestMethod]
        [DataRow(-1.0, 50.0)]
        [DataRow(10.0, -5.0)]
        public void FindByPriceRange_NegativePrice_ThrowsArgumentOutOfRangeException(double min, double max)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Program.FindByPriceRange(coffees, min, max));
        }

        [TestMethod]
        public void FindMostExpensive_ReturnsCoffeeWithMaxPrice()
        {
            Coffee actual = Program.FindMostExpensive(coffees);

            Assert.AreSame(latte, actual);
        }

        [TestMethod]
        public void FindMostExpensive_EmptyList_ReturnsNull()
        {
            Coffee actual = Program.FindMostExpensive(new List<Coffee>());

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void RemoveAtNumber_ValidNumber_RemovesAndDecreasesCounter()
        {
            int expectedCounter = Coffee.Counter - 1;

            bool result = Program.RemoveAtNumber(coffees, 2);

            Assert.IsTrue(result);
            Assert.AreEqual(2, coffees.Count);
            CollectionAssert.DoesNotContain(coffees, mocha);
            Assert.AreEqual(expectedCounter, Coffee.Counter);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(4)]
        [DataRow(100)]
        public void RemoveAtNumber_InvalidNumber_ReturnsFalseAndListNotChanged(int number)
        {
            int expectedCounter = Coffee.Counter;

            bool result = Program.RemoveAtNumber(coffees, number);

            Assert.IsFalse(result);
            Assert.AreEqual(3, coffees.Count);
            Assert.AreEqual(expectedCounter, Coffee.Counter);
        }

        [TestMethod]
        [DataRow("AB1234")]
        [DataRow("ab1234")]
        public void RemoveByCode_ExistingCode_RemovesObject(string code)
        {
            int expectedCounter = Coffee.Counter - 1;

            bool result = Program.RemoveByCode(coffees, code);

            Assert.IsTrue(result);
            Assert.IsFalse(Program.ContainsCode(coffees, "AB1234"));
            Assert.AreEqual(2, coffees.Count);
            Assert.AreEqual(expectedCounter, Coffee.Counter);
        }

        [TestMethod]
        [DataRow("XX0000")]
        [DataRow("")]
        [DataRow((string)null)]
        public void RemoveByCode_UnknownCode_ReturnsFalse(string code)
        {
            bool result = Program.RemoveByCode(coffees, code);

            Assert.IsFalse(result);
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        [DataRow(CoffeeType.Latte, 2, 1)]
        [DataRow(CoffeeType.Mocha, 1, 2)]
        [DataRow(CoffeeType.Espresso, 0, 3)]
        public void RemoveAllByType_ReturnsRemovedCount(CoffeeType type, int expectedRemoved, int expectedLeft)
        {
            int removed = Program.RemoveAllByType(coffees, type);

            Assert.AreEqual(expectedRemoved, removed);
            Assert.AreEqual(expectedLeft, coffees.Count);
            Assert.IsFalse(coffees.Exists(c => c.Type == type));
        }

        [TestMethod]
        public void RemoveAllByType_UndefinedType_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() => Program.RemoveAllByType(coffees, (CoffeeType)42));
            Assert.AreEqual(3, coffees.Count);
        }

        [TestMethod]
        public void ClearAll_RemovesAllAndReturnsCount()
        {
            int removed = Program.ClearAll(coffees);

            Assert.AreEqual(3, removed);
            Assert.AreEqual(0, coffees.Count);
        }

        [TestMethod]
        public void ClearAll_EmptyList_ReturnsZero()
        {
            List<Coffee> empty = new List<Coffee>();

            int removed = Program.ClearAll(empty);

            Assert.AreEqual(0, removed);
        }

        [TestMethod]
        public void SortByPrice_Ascending_OrdersFromCheapest()
        {
            List<Coffee> expected = new List<Coffee> { espresso, mocha, latte };

            Program.SortByPrice(coffees, false);

            CollectionAssert.AreEqual(expected, coffees);
        }

        [TestMethod]
        public void SortByPrice_Descending_OrdersFromMostExpensive()
        {
            List<Coffee> expected = new List<Coffee> { latte, mocha, espresso };

            Program.SortByPrice(coffees, true);

            CollectionAssert.AreEqual(expected, coffees);
        }

        [TestMethod]
        public void SortByName_OrdersAlphabetically()
        {
            string[] expected = { "Dark Night", "Milk Dream", "Ranok Blend" };

            Program.SortByName(coffees);

            CollectionAssert.AreEqual(expected, coffees.ConvertAll(c => c.Name));
        }

        [TestMethod]
        public void CreateRandomCoffee_ReturnsValidObjectWithUniqueCode()
        {
            Random rnd = new Random(12345);

            Coffee actual = Program.CreateRandomCoffee(coffees, rnd);

            Assert.IsNotNull(actual);
            Assert.IsTrue(Coffee.IsValidOrderCode(actual.OrderCode));
            Assert.IsFalse(Program.ContainsCode(coffees, actual.OrderCode));
            Assert.IsTrue(actual.Price > 0 && actual.Price <= 500);
            Assert.IsTrue(actual.VolumeMl >= 30 && actual.VolumeMl <= 1000);
        }

        [TestMethod]
        public void CreateRandomCoffee_TenObjects_AllCanBeAdded()
        {
            Random rnd = new Random(1);
            List<Coffee> list = new List<Coffee>();

            for (int i = 0; i < 10; i++)
                Program.AddCoffee(list, Program.CreateRandomCoffee(list, rnd), 10);

            Assert.AreEqual(10, list.Count);
        }

        [TestMethod]
        public void CreateRandomCoffee_NullRandom_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Program.CreateRandomCoffee(coffees, null));
        }

        [TestMethod]
        [DataRow("5", 0, 7, true, 5)]
        [DataRow(" 0 ", 0, 7, true, 0)]
        [DataRow("7", 0, 7, true, 7)]
        [DataRow("8", 0, 7, false, 0)]
        [DataRow("-1", 0, 7, false, 0)]
        [DataRow("abc", 0, 7, false, 0)]
        [DataRow("2.5", 0, 7, false, 0)]
        [DataRow("", 0, 7, false, 0)]
        [DataRow(null, 0, 7, false, 0)]
        [DataRow("99999999999", 0, 7, false, 0)]
        public void TryParseInt_ReturnsExpected(string input, int min, int max, bool expectedResult, int expectedValue)
        {
            bool result = Program.TryParseInt(input, min, max, out int value);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [DataRow("85.5", true, 85.5)]
        [DataRow("85,5", true, 85.5)]
        [DataRow(" -10 ", true, -10.0)]
        [DataRow("abc", false, 0.0)]
        [DataRow("", false, 0.0)]
        [DataRow(null, false, 0.0)]
        [DataRow("NaN", false, 0.0)]
        public void TryParseDouble_ReturnsExpected(string input, bool expectedResult, double expectedValue)
        {
            bool result = Program.TryParseDouble(input, out double value);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedValue, value, 1e-9);
        }

        [TestMethod]
        [DataRow("1", CoffeeType.Espresso)]
        [DataRow("6", CoffeeType.FlatWhite)]
        [DataRow("latte", CoffeeType.Latte)]
        [DataRow(" Mocha ", CoffeeType.Mocha)]
        public void TryParseCoffeeType_ValidInput_ReturnsTrueAndType(string input, CoffeeType expected)
        {
            bool result = Program.TryParseCoffeeType(input, out CoffeeType actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("7")]
        [DataRow("Frappe")]
        [DataRow("")]
        [DataRow((string)null)]
        public void TryParseCoffeeType_InvalidInput_ReturnsFalse(string input)
        {
            bool result = Program.TryParseCoffeeType(input, out _);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CollectionMethods_DoNotWriteToConsole()
        {
            Program.AddCoffee(coffees, NewCoffee("ZZ0001"), MaxCount);
            Program.FindByName(coffees, "Dream");
            Program.FindByType(coffees, CoffeeType.Latte);
            Program.SortByPrice(coffees, false);
            Program.RemoveByCode(coffees, "ZZ0001");

            string output = consoleOutput.ToString();

            Assert.AreEqual(string.Empty, output);
        }
    }
}