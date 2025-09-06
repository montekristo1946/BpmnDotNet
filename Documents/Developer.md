# Инструкция по использованию

## Описания кода реализации:

1. Пример кода [Sample.ConsoleApp](../BpmnDotNet/Sample.ConsoleApp/README.MD)
2. Для реализации обработчика блока определяем и регистрируем в контейнере IBpmnHandler
3. Регистрируем IElasticClient
4. Через весь процесс идет IContextBpmnProcess
5. Для реализации маршрутизации заполняем IExclusiveGateWay
6. Отправка сообщений ( ReceiveTask) IBpmnClient.SendMessage 
7. Для реализации получения сообщений (ReceiveTask) в контексте реализуем IMessageReceiveTask 
8. С точки зрения реализация IBpmnHandler для блоков ServiceTask, SendTask, SubProcess - одинаковая. Различие только в
   начертании на схеме. 
9. Все блоки на схеме можно определить как IBpmnHandler и реализовать функционал (кроме flow). 
10. При выбрасывании Exception весь процесс останавливается (Exception.Message фиксируется elastic). 
11. При подключении ElasticClient записывает состояние процесса в Elastic (для дальнейшей визуализации).


