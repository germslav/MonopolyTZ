Название
Консольное приложение учета личных финансов с поддержкой импорта двух CSV файлов: wallets.csv и transactions.csv.

Запуск
dotnet run 2025-09 wallets.csv transactions.csv
Аргументы
[0] целевой месяц форматы yyyy-MM или yyyy/MM или MM.yyyy. Если пропущен берется текущий месяц.
[1] путь к wallets.csv. По умолчанию wallets.csv.
[2] путь к transactions.csv. По умолчанию transactions.csv.

Файлы CSV
wallets.csv
Колонки: WalletID, Name, Currency, InitialBalance
Разделитель автоматически определяется из первой строки. Поддерживаются табуляция, точка с запятой, запятая.
Пример
WalletID,Name,Currency,InitialBalance
w1,Main,RUB,15000
w2,Savings,USD,100

transactions.csv
Колонки: WalletID, TransactionID, Date, Value, Type, Description
Type принимает значения Income или Expense. Допускаются русские аналоги Доход и Расход.
Value всегда неотрицательное число. Знак задается типом.
Дата поддерживает форматы
yyyy-MM-dd
dd.MM.yyyy
yyyy/MM/dd
dd/MM/yyyy
yyyy-MM-dd HH:mm:ss
dd.MM.yyyy HH:mm:ss
Также допускается общий разбор DateTime.TryParse для других локалей.
Числа парсятся через InvariantCulture и ru-RU. Дополнительно выполняется подстановка запятой на точку при необходимости.
Пример
WalletID,TransactionID,Date,Value,Type,Description
w1,t1,2025-09-01,5000,Income,Salary advance
w1,t2,2025-09-02,700,Expense,Groceries
w1,t3,2025-09-10,2000,Expense,Rent part
w2,t4,2025-09-05,50,Income,Transfer
w2,t5,2025-09-12,120,Expense,Gadgets

Логика обработки
Баланс кошелька равен InitialBalance плюс сумма доходов минус сумма расходов.
Транзакции применяются по каждому кошельку отдельно в порядке возрастания даты.
Расход отклоняется если его сумма превышает текущий баланс кошелька на момент применения.
Транзакции с неизвестным WalletID отклоняются.
Отчет не сводится в единую валюту. Все агрегаты считаются и печатаются отдельно по каждому кошельку.

Вывод
Report for YYYY-MM
Блок Grouped by type per wallet for YYYY-MM
Для каждого кошелька печатаются суммы по типам Income и Expense и список строк за выбранный месяц.
Блок Top N expenses per wallet for YYYY-MM
Топ N расходов для каждого кошелька за выбранный месяц.
Блок Rejected transactions while applying CSV
Список отклоненных строк с указанием причины.