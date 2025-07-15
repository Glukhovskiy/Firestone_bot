# Firestone Bot

Автоматический бот для игры Firestone, созданный с использованием MelonLoader.

## Описание

Firestone Bot автоматизирует выполнение повседневных задач в игре Firestone, включая сбор ресурсов, выполнение миссий, исследования и другие игровые активности.

## Возможности

### Основные модули:
- **Guild Expeditions** - Автоматическое участие в гильдейских экспедициях
- **Map Missions** - Выполнение миссий на карте
- **Daily Tasks** - Выполнение ежедневных заданий
- **Meteorite Research** - Исследование метеоритов
- **Firestone Research** - Исследование огненных камней
- **Mystery Box** - Открытие загадочных коробок
- **Check In** - Ежедневная регистрация
- **Oracle Rituals** - Ритуалы оракула
- **Oracle's Gift** - Дары оракула
- **Oracle Blessings** - Благословения оракула
- **Upgrades** - Автоматические улучшения
- **Chests** - Открытие сундуков
- **Tanks** - Управление танками
- **Alchemy** - Алхимия
- **Engineer** - Инженерные задачи
- **Close Windows** - Закрытие окон

## Установка

1. Убедитесь, что у вас установлен MelonLoader для игры Firestone
2. Скопируйте `Firestone_bot.dll` в папку `Mods` игры
3. Запустите игру

## Управление

### Горячие клавиши:
- **F5** - Включить/выключить бота
- **F7** - Включить/выключить модуль Chests
- **F8** - Включить/выключить режим отладки

## Конфигурация

Настройки бота хранятся в файле `Mods/config.json`. Каждый модуль можно включить или выключить:

```
enabled=true
debugEnabled=true
guildExpeditions=true
mapMissions=false
dailyTasks=true
meteoriteResearch=true
firestoneResearch=true
mysteryBox=true
checkIn=true
oracleRituals=true
oraclesGift=true
oracleBlessings=true
upgrades=true
chests=true
tanks=false
alchemy=true
engineer=true
closeWindows=false
```

### Параметры конфигурации:
- `enabled` - Общее включение/выключение бота
- `debugEnabled` - Режим отладки
- Остальные параметры - включение/выключение конкретных модулей

## Принцип работы

Бот работает циклически, последовательно выполняя активные модули:
1. Guild Expeditions
2. Map Missions
3. Daily Tasks
4. Meteorite Research
5. Firestone Research
6. Mystery Box
7. Check In
8. Oracle Rituals
9. Oracle's Gift
10. Oracle Blessings
11. Upgrades
12. Chests
13. Tanks
14. Alchemy
15. Engineer
16. Close Windows

После завершения всех модулей цикл повторяется.

## Безопасность

- Бот использует только стандартные игровые интерфейсы
- Не модифицирует игровые данные напрямую
- Работает через симуляцию пользовательских действий

## Требования

- Игра Firestone
- MelonLoader
- .NET 6.0

## Версия

Текущая версия: **release-0.12**

## Поддержка

При возникновении проблем:
1. Проверьте логи MelonLoader
2. Убедитесь в корректности конфигурации
3. Перезапустите игру с ботом

## Отказ от ответственности

Использование бота может нарушать правила игры. Используйте на свой страх и риск.