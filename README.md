# Простой движок для Bpmn

## BpmnDotNet
При разработки данного продукта вдохновлялись продуктами Camunda [Camunda](https://github.com/camunda) <br>
Какие требования реализовывает:

1. Минимизирует затраты на вызовы блоков bpmn. Реализация Handler bpmn и движка вызовов в одном сервисе (сетевой вызов обработчиков не
   поддерживается).
2. При реализации Handler bpmn придерживаться .net стандартов построение кода (каждый обработчик в отдельном классе).
3. Поддержка bpmn схем сделанных [Camunda-modeler v7](https://github.com/camunda)
4. Каждый Handler запускается асинхронно.  
5. Реализован мониторинг выполняемых процессов.

## Докуметация:

1. [BPMN нотация.](./Documents/BpmnNanation.md)
2. [Инструкция по использованию](./Documents/Developer.md)
3. [BpmnDotNet.ElasticClient](./BpmnDotNet/BpmnDotNet.ElasticClient/READMI.md)
4. [BpmnDotNet.Arm.Web](./BpmnDotNet/BpmnDotNet.Arm.Web/README.MD)

## Кому это подойдет:
1. Небольшой проект в котором не требуются распределенные вызовы обработчиков.
2. Кому достаточно только базовых блоков для реализации бизнес процесса.

## Визуальная часть мониторинга процессов.
1. [UI представления](./Documents/UiMonitoring.md)