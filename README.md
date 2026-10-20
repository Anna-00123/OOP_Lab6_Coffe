# OOP_Lab6_Coffe — Generics. Collections

Практична робота № 6 з дисципліни «Об'єктно-орієнтоване програмування».
Предметна область — облік кавових напоїв (клас `Coffee`, перелік `CoffeeType`).

## Структура рішення

| Проєкт | Вміст |
|---|---|
| `OOP_Lab6_Coffe` | Консольний застосунок (.NET 10): `Coffee.cs` (клас `Coffee` і перелік `CoffeeType`), `Program.cs` (методи роботи з `List<Coffee>` і меню) |
| `OOP_Lab6_Coffe.Tests` | Проєкт MSTest: `CoffeeTests` (тести з Lab-5) і `ProgramTests` (тести методів роботи з колекцією) |

## Що змінено порівняно з Lab-5

Усі об'єкти зберігаються в колекції `List<Coffee>`. У класі `Program` виділено публічні статичні методи,
які **не використовують** `Console.ReadLine()` / `Console.WriteLine()` і отримують список як параметр.
Меню лише зчитує дані з клавіатури, викликає ці методи та виводить результат.

| Метод `Program` | Метод `List<T>` |
|---|---|
| `AddCoffee` | `Add`, `Exists` |
| `AddCoffeeRange` | `AddRange`, `FindIndex` |
| `InsertCoffee` | `Insert` |
| `ContainsCode` | `Exists` |
| `FindByCode` | `Find` |
| `FindIndexByCode` | `FindIndex` |
| `FindByName`, `FindByType`, `FindByPriceRange` | `FindAll` |
| `RemoveAtNumber` | `RemoveAt` |
| `RemoveByCode` | `Remove` |
| `RemoveAllByType` | `RemoveAll` |
| `ClearAll` | `Clear` |
| `SortByPrice`, `SortByName` | `Sort`, `Reverse` |
| `FindMostExpensive`, `CreateRandomCoffee` | прохід по списку |
| `TryParseInt`, `TryParseDouble`, `TryParseCoffeeType` | перевірка введення користувача |

Під час видалення об'єктів зі списку лічильник `Coffee.Counter` зменшується через `Coffee.DecreaseCounter`.

## Меню програми

1 – Додати напій (вручну / рядком / випадковий)
2 – Переглянути всі напої
3 – Знайти (за кодом, назвою, типом, діапазоном ціни, найдорожчий)
4 – Продемонструвати поведінку (Reheat, Cool, ApplyDiscount, AddVolume)
5 – Видалити (за номером, за кодом, усі певного типу, очистити список)
6 – Продемонструвати static-методи
7 – Сортувати список
0 – Вихід

Формат рядка для введення: `Код,Назва,Тип,Ціна,dd.MM.yyyy,Так/Ні,Об'єм`, наприклад
`AB1234,Ranok Blend,Latte,85.50,15.03.2024,Ні,250`.

## Тестування

- `CoffeeTests` — 71 тест-метод, 167 тестових випадків (з Lab-5);
- `ProgramTests` — 48 тест-методів, 106 тестових випадків: коректні, межові та некоректні значення
  (null, порожні рядки, дублікати кодів, переповнення списку, неіснуючі номери та коди, невідомі типи,
  некоректний діапазон цін, нечислове введення).

Разом: **273 тестові випадки, усі пройдено.**

## Запуск

- запуск програми: `dotnet run --project OOP_Lab6_Coffe`
- запуск тестів: `dotnet test`
- у Visual Studio: Тест → Виконати всі тести
