# Vangogh
 Simple WebRequest system for **Unity**! [PT-BR](https://github.com/NyanHeavy/Vangogh/blob/main/README_PTBR.md)

```csharp
Vangogh.Instance()
.GET(Url)
.UseIEnumerator()
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```

Features
---
### Use POST or GET easily
```csharp
.POST(Url)
```
```csharp
.GET(Url)
```
### Custom Headers
```csharp
.SetHeader("Header", "Value")
```
```csharp
Vangogh.Instance()
.GET(Url)
.UseIEnumerator()
.SetHeader("Authorization", "value")
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
Supports as many headers as you need
```csharp
Vangogh.Instance()
.GET(Url)
.UseIEnumerator()
.SetHeader("Authorization", "value")
.SetHeader("Content-Type", "value")
.SetHeader("Any-Header-Name", "value")
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
### Easy debugging
```csharp
.SetEnableLog()
```
### Request Body
```csharp
.SetBody("value")
```
```csharp
Vangogh.Instance()
.GET(Url)
.UseIEnumerator()
.SetBody("some-value")
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
### Connection Method
```csharp
.UseIEnumerator()
```
or
```csharp
.UseAsync()
```
⚠ Asynchronous connection method has not yet been implemented ⚠
```csharp
Vangogh.Instance()
.GET(Url)
.useIEnumerator()
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
### Reconnection Attempts
Sets the maximum number of attempts before terminating
```csharp
.SetAttempts(int)
```
You can also set the time between attempts
```csharp
.SetAttemptsDelay(float)
```
```csharp
Vangogh.Instance()
.GET(Url)
.useIEnumerator()
.SetAttempts(3)
.SetAttemptsDelay(5f)
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
### Control Instances
Using single instance will terminating existing process with current url
```csharp
.UseSingleInstance()
```
### Actions
```csharp
//Called when the process is started
.WithStartAction(() => { })

//Called when the process encounters an error
.WithErrorAction((error) => { })

//Called when the process encounters an error and finalize
.WithErrorEndAction((error) => { })

//Called when the process is done with results
.WithGetResultAction((response) => { })
```

Also:
- Supports native GET & POST, PATCH and DELETE in future
- Custom Headers.
- Request Body.

### Supporting Platforms
- Standalone Builds ✔
- Android ✔
- iOS (maybe)
- WebGl ✔

Usage
----
Clone the project. Open Vangogh/Assets in unity or import the UnityPackage to your existing project or copy Vangogh.cs and past in your project.
Add namespace in you script:
```csharp
using NyanHeavyStudios.Vangogh;
```

THIRDPARTY
----
 ▶ Based on Davinci structure by [Shamsdev](https://github.com/shamsdev/davinci)

License
----
**Vangogh** is available under the **MIT** license. See the LICENSE file for more info.
