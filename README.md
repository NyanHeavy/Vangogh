# Vangogh
 Simple Networking request system for **Unity**

```csharp
Vangogh.Instance()
   .GET(Url)
   .WithErrorEndAction((error) => { Debug.Log(error); })
   .WithGetResultAction((response) => { Debug.Log(response.responseResult); })
   .Init();
```

Also:
- Supports native GET & POST, PATCH and DELETE in future
- Custom Headers.
- Request Body.

### Supporting Platforms
- Standalone Builds
- Android
- iOS
- WebGl

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
