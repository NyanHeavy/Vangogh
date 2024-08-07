# Vangogh
 Sistema de requisições de rede simples para **Unity**

```csharp
Vangogh.Instance()
.GET(Url)
.useIEnumerator()
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```

Features
---
### Use POST ou GET facilmente
```csharp
.POST(Url)
```
```csharp
.GET(Url)
```
### Cabeçalhos Customizados
Define the headers you need
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
Clone the project. Open Vangogh/Assets in unity or import the UnityPackage to your existing project
OR
Copy Vangogh.cs and past in your project.

THIRDPARTY
----
 ▶ Based on Davinci structure by [Shamsdev](https://github.com/shamsdev/davinci)

License
----
**Vangogh** is available under the **MIT** license. See the LICENSE file for more info.
