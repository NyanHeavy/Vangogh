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

Funcionalidades
---
### Use POST ou GET facilmente
```csharp
.POST(Url)
```
```csharp
.GET(Url)
```
### Cabeçalhos Customizados
Defina cabeçalhos como necessario
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
Suporta quantos precisar
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
### Corpo da Requisição
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
### Metodos de Conexão
```csharp
.UseIEnumerator()
```
ou
```csharp
.UseAsync()
```
⚠ O método de conexão assíncrona ainda não foi implementado ⚠
```csharp
Vangogh.Instance()
.GET(Url)
.useIEnumerator()
.WithErrorEndAction((error) => { })
.WithGetResultAction((response) => { })
.Init();
```
### Tentativas de Reconexão
Define o número máximo de tentativas antes de encerrar
```csharp
.SetAttempts(int)
```
Você também pode definir o tempo entre as tentativas
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
### Eventos
```csharp
//Chamado quando o processo é iniciado
.WithStartAction(() => { })

//Chamado quando o processo encontra um erro
.WithErrorAction((error) => { })

//Chamado quando o processo encontra um erro e encerra
.WithErrorEndAction((error) => { })

//Chamado quando o processo é concluido e com a resposta
.WithGetResultAction((response) => { })
```

Também:
- Suporta nativamente os metodos GET e POST, PATCH e DELETE no futuro.
- Cabeçalhos customizados.
- Eventos dinâmicos

### Plataformas Suportadas
- Executável ✔
- Android ✔
- iOS (maybe)
- WebGl ✔

Uso
----
Clone o projeto. Abra Vangogh/Assets no Unity ou importe o UnityPackage para seu projeto existente
ou
Copie o script Vangogh.cs e cole no seu projeto.

Creditos
----
 ▶ Baseado na estrutura de Davinci por [Shamsdev](https://github.com/shamsdev/davinci)

Licença
----
**Vangogh** está disponível sob a licença **MIT**. Veja o arquivo LICENSE para mais informações.
