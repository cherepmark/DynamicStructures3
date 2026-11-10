# Стек: результаты

Сформировано 2026-10-27 17:04:13 UTC; конфигурация Release; повторов 5.

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

