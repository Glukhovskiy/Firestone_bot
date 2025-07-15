# Настройка проекта

## Переменные окружения

Для корректной работы проекта необходимо настроить переменную окружения:

### Windows (Steam)
```
FirestoneGamePath=C:\Program Files (x86)\Steam\steamapps\common\Firestone
```

### Windows (Epic Games)
```
FirestoneGamePath=C:\Program Files (x86)\Epic Games\FirestoneOnlineIdleRPG
```

## Установка переменной окружения

1. Откройте "Переменные среды" в Windows
2. Добавьте новую переменную `FirestoneGamePath`
3. Укажите путь к папке с игрой
4. Перезапустите Visual Studio

## Альтернативный способ

Создайте файл `Directory.Build.props` в корне проекта:

```xml
<Project>
  <PropertyGroup>
    <FirestoneGamePath>C:\Program Files (x86)\Steam\steamapps\common\Firestone</FirestoneGamePath>
  </PropertyGroup>
</Project>
```