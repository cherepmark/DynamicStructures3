# Результаты экспериментов

Отчёт сформирован автоматически 2026-10-27 17:04:13 UTC.
Все числа ниже — фактические результаты запуска программы, а не пример-заглушка.

## Условия эксперимента

| Параметр | Значение |
|---|---|
| Дата и время (UTC) | 2026-10-27 17:04:13 |
| ОС | Microsoft Windows 10.0.26200 |
| Архитектура ОС / процесса | X64 / X64 |
| Платформа .NET | .NET 9.0.9 |
| Версия среды | 9.0.9 |
| Конфигурация сборки | Release |
| Логических процессоров | 6 |
| Частота Stopwatch | 10000000 тиков/с |
| Доступная память (GC) | 15.92 ГиБ |
| Повторов на измерение | 5 |
| Прогревочных запусков | 1 |

## Наборы входных данных

| Сценарий | Семейство | Операций | Состав | Файл |
|---|---|---:|---|---|
| stack-example | Stack | 8 | push/enqueue=3, pop/dequeue=1, peek/top=1, isEmpty=2, print=1, total=8 | stack_example.txt |
| stack-short | Stack | 1000 | push/enqueue=300, pop/dequeue=300, peek/top=200, isEmpty=200, print=0, total=1000 | stack_short.txt |
| stack-medium | Stack | 10000 | push/enqueue=4000, pop/dequeue=2000, peek/top=2000, isEmpty=2000, print=0, total=10000 | stack_medium.txt |
| stack-large | Stack | 100000 | push/enqueue=50000, pop/dequeue=20000, peek/top=20000, isEmpty=10000, print=0, total=100000 | stack_large.txt |
| stack-push-heavy | Stack | 100000 | push/enqueue=90000, pop/dequeue=0, peek/top=0, isEmpty=10000, print=0, total=100000 | stack_push_heavy.txt |
| stack-pop-heavy | Stack | 100000 | push/enqueue=50000, pop/dequeue=50000, peek/top=0, isEmpty=0, print=0, total=100000 | stack_pop_heavy.txt |
| stack-peek-heavy | Stack | 100000 | push/enqueue=40000, pop/dequeue=10000, peek/top=40000, isEmpty=10000, print=0, total=100000 | stack_peek_heavy.txt |
| stack-mixed | Stack | 100000 | push/enqueue=49999, pop/dequeue=20000, peek/top=20000, isEmpty=10000, print=1, total=100000 | stack_mixed.txt |
| queue-example | Queue | 8 | push/enqueue=3, pop/dequeue=1, peek/top=1, isEmpty=2, print=1, total=8 | queue_example.txt |
| queue-short | Queue | 1000 | push/enqueue=600, pop/dequeue=300, peek/top=100, isEmpty=0, print=0, total=1000 | queue_short.txt |
| queue-medium | Queue | 10000 | push/enqueue=4000, pop/dequeue=2000, peek/top=2000, isEmpty=2000, print=0, total=10000 | queue_medium.txt |
| queue-large | Queue | 40000 | push/enqueue=30000, pop/dequeue=10000, peek/top=0, isEmpty=0, print=0, total=40000 | queue_large.txt |
| queue-remove-heavy | Queue | 40000 | push/enqueue=20000, pop/dequeue=20000, peek/top=0, isEmpty=0, print=0, total=40000 | queue_remove_heavy.txt |
| queue-enqueue-heavy | Queue | 40000 | push/enqueue=38000, pop/dequeue=0, peek/top=1000, isEmpty=1000, print=0, total=40000 | queue_enqueue_heavy.txt |
| queue-peek-heavy | Queue | 40000 | push/enqueue=10000, pop/dequeue=1000, peek/top=25000, isEmpty=4000, print=0, total=40000 | queue_peek_heavy.txt |
| queue-mixed | Queue | 40000 | push/enqueue=20000, pop/dequeue=10000, peek/top=5000, isEmpty=5000, print=0, total=40000 | queue_mixed.txt |

## Стек на собственном связном списке

| Сценарий | Операций | Чтение+разбор, мс (среднее) | Выполнение, мс (среднее) | Медиана выполнения, мс | Мин/макс выполнения, мс | Полное время, мс (среднее) |
|---|---:|---:|---:|---:|---:|---:|
| stack-example | 8 | 0.1694 | 0.0557 | 0.0023 | 0.0021 / 0.2693 | 0.2257 |
| stack-short | 1000 | 0.2551 | 0.0686 | 0.0686 | 0.0646 / 0.0754 | 0.3240 |
| stack-medium | 10000 | 0.8920 | 0.6565 | 0.2110 | 0.1751 / 2.4694 | 1.5493 |
| stack-large | 100000 | 17.2880 | 11.7092 | 11.3551 | 10.7980 / 13.4964 | 29.0005 |
| stack-push-heavy | 100000 | 28.8285 | 2.5575 | 2.6221 | 2.2380 / 2.8372 | 31.3881 |
| stack-pop-heavy | 100000 | 14.9322 | 11.4756 | 11.0985 | 10.6074 / 12.3842 | 26.4104 |
| stack-peek-heavy | 100000 | 13.7890 | 1.5462 | 1.2664 | 1.1360 / 2.7268 | 15.3374 |
| stack-mixed | 100000 | 13.7040 | 11.8235 | 11.7511 | 11.1098 / 12.7338 | 25.5310 |

## Две реализации очереди

| Сценарий | Реализация | Операций | Чтение+разбор, мс (среднее) | Выполнение, мс (среднее) | Медиана выполнения, мс | Мин/макс выполнения, мс | Полное время, мс (среднее) |
|---|---|---:|---:|---:|---:|---:|---:|
| queue-example | ListBasedQueue | 8 | 0.1762 | 0.0097 | 0.0029 | 0.0029 / 0.0361 | 0.1861 |
| queue-example | StandardQueue | 8 | 0.1611 | 0.0074 | 0.0033 | 0.0030 / 0.0235 | 0.1688 |
| queue-short | ListBasedQueue | 1000 | 0.4104 | 0.0885 | 0.0849 | 0.0808 / 0.1022 | 0.4994 |
| queue-short | StandardQueue | 1000 | 0.3048 | 0.0769 | 0.0635 | 0.0577 / 0.1395 | 0.3826 |
| queue-medium | ListBasedQueue | 10000 | 0.8596 | 1.7216 | 1.5743 | 1.4949 / 2.3058 | 2.5822 |
| queue-medium | StandardQueue | 10000 | 0.9033 | 0.2930 | 0.2922 | 0.2124 / 0.4109 | 1.1975 |
| queue-large | ListBasedQueue | 40000 | 2.9214 | 59.5312 | 59.0545 | 58.2613 / 60.9678 | 62.4538 |
| queue-large | StandardQueue | 40000 | 3.6360 | 1.2706 | 0.6882 | 0.6248 / 3.5692 | 4.9081 |
| queue-remove-heavy | ListBasedQueue | 40000 | 2.5277 | 47.5991 | 46.9488 | 45.6016 / 51.4346 | 50.1284 |
| queue-remove-heavy | StandardQueue | 40000 | 2.3398 | 0.9160 | 0.5788 | 0.5477 / 1.4832 | 3.2569 |
| queue-enqueue-heavy | ListBasedQueue | 40000 | 3.0343 | 0.7959 | 0.7976 | 0.6953 / 0.8957 | 3.8316 |
| queue-enqueue-heavy | StandardQueue | 40000 | 2.9865 | 0.7717 | 0.7579 | 0.6204 / 0.9120 | 3.7596 |
| queue-peek-heavy | ListBasedQueue | 40000 | 2.3760 | 2.5369 | 2.4907 | 2.4155 / 2.6849 | 4.9142 |
| queue-peek-heavy | StandardQueue | 40000 | 2.2454 | 0.5257 | 0.4968 | 0.4849 / 0.5836 | 2.7720 |
| queue-mixed | ListBasedQueue | 40000 | 2.5716 | 36.1140 | 35.9586 | 34.9388 / 37.4289 | 38.6866 |
| queue-mixed | StandardQueue | 40000 | 2.3296 | 0.5452 | 0.5183 | 0.5018 / 0.6014 | 2.8758 |

## Сравнение очередей на одинаковых файлах

| Сценарий | Операций | ListBasedQueue, мс | StandardQueue, мс | Отношение ListBased / Standard |
|---|---:|---:|---:|---:|
| queue-example | 8 | 0.0097 | 0.0074 | 1.31× |
| queue-short | 1000 | 0.0885 | 0.0769 | 1.15× |
| queue-medium | 10000 | 1.7216 | 0.2930 | 5.88× |
| queue-large | 40000 | 59.5312 | 1.2706 | 46.85× |
| queue-remove-heavy | 40000 | 47.5991 | 0.9160 | 51.96× |
| queue-enqueue-heavy | 40000 | 0.7959 | 0.7717 | 1.03× |
| queue-peek-heavy | 40000 | 2.5369 | 0.5257 | 4.83× |
| queue-mixed | 40000 | 36.1140 | 0.5452 | 66.24× |

## Анализ результатов

- На сценарии `queue-remove-heavy` (массовое удаление из начала большой очереди) ListBasedQueue медленнее StandardQueue примерно в 51.96 раза. Это качественно согласуется с Dequeue = List<T>.RemoveAt(0), требующим сдвига O(n) элементов.
- На сценарии `queue-enqueue-heavy` отношение составляет примерно 1.03×: при преобладании Enqueue обе реализации близки, поскольку добавление в конец List<T> амортизированно O(1).
- На сценарии `queue-peek-heavy` отношение примерно 4.83×: просмотр начала у List<T> индексируется за O(1), поэтому различия меньше, чем при массовом RemoveAt(0).
- Для `queue-example` (8 операций) отношение 1.31× не следует трактовать как преимущество реализации: абсолютные времена порядка сотых долей миллисекунды, и на таком масштабе доминируют накладные расходы и шум.
- Для стека самый большой сценарий `stack-large` (100000 операций) выполнялся в среднем 11.7092 мс. Операции Push/Pop/Top/IsEmpty у связанного стека имеют O(1), поэтому суммарное время ожидаемо линейно по числу команд; один сценарий сам по себе не доказывает асимптотику.
- Абсолютные значения зависят от машины, JIT, сборщика мусора и фоновой нагрузки. По одному измерению нельзя делать вывод об асимптотике; ниже сопоставлены теория и несколько повторов.

## Теоретические оценки сложности

- Стек на односвязном списке: Push, Pop, Top, IsEmpty — O(1); память O(n).
- ListBasedQueue: Enqueue — амортизированно O(1), Dequeue — O(n) из-за RemoveAt(0), Peek — O(1); память O(n).
- StandardQueue: Enqueue, Dequeue, Peek — амортизированно O(1); память O(n).
- Вычисление постфиксного выражения: O(m) времени и O(m) памяти в худшем случае, где m — число токенов.
- Перевод инфиксной записи сортировочной станцией: O(n) времени и O(n) памяти.
- Разворот списка, перенос конца/начала, удаление/вставка по известной позиции — O(n) из-за поиска; сами операции со ссылками O(1).

## Ограничения методики

- Время чтения включает чтение файла и разбор команд; время выполнения — только операции над структурой.
- Вывод в консоль исключён из измеряемого участка: результаты операций не форматируются (observer = null).
- Перед каждым измерением вызывается сборка мусора; это уменьшает перенос состояния между запусками, но не устраняет влияние ОС.
- Для каждой пары реализаций очереди используется один и тот же файл сценария.
- Замеры сделаны на одной машине и не являются универсальным бенчмарком.

