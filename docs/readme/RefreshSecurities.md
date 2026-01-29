# Большие шаги
- [x] Сервис Worker запускает задачу
- [x] Задача отправляет в RabbitMQ сообщение DataSecuritiesRefreshKey
- [x] Сообщение получает сервис Data
- [ ] В обработчике запускается фоновая задача, которая
    - [ ] получает данные от ISS.MOEX
    - [ ] конвертирует их в entity
    - [ ] сохраняет в БД

# Средние шаги
- [ ] **Сервис Worker запускает задачу**
    - [ ] Проверяем запущена ли уже такая задача
        - [ ] в Redis ищем значение с ключом WorkerSecuritiesRefreshStatusRedisKey
        - [x] формат:
            - string MessageId
            - string CorrelationId
            - datetime CreatedAt
            - datetime UpdatedAt
            - string Message
            - string ErrorMessage
            - RefreshStatus {None, IsScheduled, IsRunning, IsCompleted, IsFailed}
        - [ ] если нет значения, то задача не запущена и переходим к следующему шагу
        - [ ] при завершении задачи в Redis записывается значение с RefreshStatus IN (IsCompleted, IsFailed) и временем жизни 5 минут (делаем паузу между запусками)
- [ ] **Задача отправляет в RabbitMQ сообщение DataSecuritiesRefreshKey**
    - [ ] Создается сообщение, указывается CorrelationId (либо берется из WorkerSecuritiesRefreshStatusRedisKey, либо создается новый)
- [ ] **Сообщение получает сервис Data**
    - [x] Настроить CorrelationId
    - [x] Получаем CorrelationId
- [ ] **В обработчике запускается фоновая задача, которая**
    - [ ] отправляем в RabbitMQ сообщение TelegramStartKey
    - [ ] получает данные от ISS.MOEX (MoexClient)
    - [ ] конвертирует их в entity (Converters)
    - [ ] сохраняет в БД (DataService)
    - [ ] в случае ошибки отправялем в RabbitMQ сообщение TelegramErrorKey
    - [ ] в случае успеха отправялем в RabbitMQ сообщение TelegramCompleteKey

# Мелкие шаги
Correlation ID