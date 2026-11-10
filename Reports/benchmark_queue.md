# Очереди: результаты

Сформировано 2026-10-27 17:04:13 UTC; конфигурация Release; повторов 5.

## Наборы входных данных

| Сценарий | Семейство | Операций | Состав | Файл |
|---|---|---:|---|---|
| queue-example | Queue | 8 | push/enqueue=3, pop/dequeue=1, peek/top=1, isEmpty=2, print=1, total=8 | queue_example.txt |
| queue-short | Queue | 1000 | push/enqueue=600, pop/dequeue=300, peek/top=100, isEmpty=0, print=0, total=1000 | queue_short.txt |
| queue-medium | Queue | 10000 | push/enqueue=4000, pop/dequeue=2000, peek/top=2000, isEmpty=2000, print=0, total=10000 | queue_medium.txt |
| queue-large | Queue | 40000 | push/enqueue=30000, pop/dequeue=10000, peek/top=0, isEmpty=0, print=0, total=40000 | queue_large.txt |
| queue-remove-heavy | Queue | 40000 | push/enqueue=20000, pop/dequeue=20000, peek/top=0, isEmpty=0, print=0, total=40000 | queue_remove_heavy.txt |
| queue-enqueue-heavy | Queue | 40000 | push/enqueue=38000, pop/dequeue=0, peek/top=1000, isEmpty=1000, print=0, total=40000 | queue_enqueue_heavy.txt |
| queue-peek-heavy | Queue | 40000 | push/enqueue=10000, pop/dequeue=1000, peek/top=25000, isEmpty=4000, print=0, total=40000 | queue_peek_heavy.txt |
| queue-mixed | Queue | 40000 | push/enqueue=20000, pop/dequeue=10000, peek/top=5000, isEmpty=5000, print=0, total=40000 | queue_mixed.txt |

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

