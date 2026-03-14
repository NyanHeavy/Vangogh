# Vangogh - Simple WebRequest System for Unity

**Vangogh** is a lightweight and fluent HTTP request helper for Unity built on top of `UnityWebRequest`.  
It focuses on **minimal syntax**, **chainable configuration**, and **simple async workflows** using coroutines.

Project by **NyanHeavy Studios**  
Inspired by **Davinci** by Shamsdev.

## Features

- Simple **fluent builder syntax**
- Built on Unity's native `UnityWebRequest`
- **GET / POST** support
- **Custom headers**
- **Automatic retry system**
- **Single-instance request protection**
- Optional **logging**
- **Callback-based results**
- **Coroutine-based async execution**
- **No external dependencies**

## Installation

1. Copy `Vangogh.cs` into your Unity project (recommended folder):

```
Assets/
  Scripts/
    Vangogh/
      Vangogh.cs
```

2. Unity will compile automatically.

No additional setup is required.

## Basic Usage

### Simple GET

```csharp
using NyanHeavyStudios.Vangogh;

Vangogh.GET("https://api.example.com/data")
    .OnResult(res =>
    {
        Debug.Log(res.result);
    })
    .Init();
```

### Simple POST

```csharp
Vangogh.POST("https://api.example.com/login")
    .SetBody("{\"user\":\"admin\",\"password\":\"123\"}")
    .OnResult(res =>
    {
        Debug.Log(res.result);
    })
    .Init();
```

## Full Example

```csharp
Vangogh.POST("https://api.example.com/data")
    .SetBody("{\"value\":10}")
    .SetContentType("application/json")
    .AddHeader("Authorization", "Bearer TOKEN")
    .SetAttempts(3, 1f)
    .SetLog(true)
    .UseSingleInstance()
    .OnStart(() => Debug.Log("Request started"))
    .OnSuccess(() => Debug.Log("Success"))
    .OnError(() => Debug.Log("Error"))
    .OnEnd(() => Debug.Log("Finished"))
    .OnResult(res =>
    {
        Debug.Log(res.code);
        Debug.Log(res.result);
    })
    .Init();
```

## API Overview

### Request Creation

```csharp
Vangogh.GET(url)
Vangogh.POST(url)
```

### Configuration

| Method | Description |
|------|-------------|
| `SetBody(string)` | Defines request body |
| `SetContentType(string)` | Sets Content-Type header |
| `AddHeader(key,value)` | Adds custom header |
| `SetAttempts(count, delay)` | Retry system |
| `UseSingleInstance()` | Prevent duplicate calls |
| `SetLog(bool)` | Enable logging |

### Events

| Event | Description |
|------|-------------|
| `OnStart()` | Called when request begins |
| `OnSuccess()` | Called on HTTP success |
| `OnError()` | Called when request fails |
| `OnEnd()` | Always called at end |
| `OnResult()` | Returns response data |

### Response Object

```csharp
public class VangoghResponse
{
    public long code;
    public string result;
}
```

## Example Response Handling

```csharp
.OnResult(res =>
{
    if(res.code == 200)
        Debug.Log(res.result);
})
```

## Notes

- `UseSingleInstance()` cancels previous active requests with the same URL.
- `SetAttempts()` retries on connection failure.
- All requests run internally via a persistent `DontDestroyOnLoad` manager.

## Supporting Platforms
- Standalone Builds ✔
- Android ✔
- iOS (Why not?)
- WebGl ✔

## Credits

Vangogh structure based on:
- [Davinci Image Downloader](https://github.com/shamsdev/davinci) by ShamsDev

----
## License
**Vangogh** is available under the **MIT** license. See the LICENSE file for more info.
