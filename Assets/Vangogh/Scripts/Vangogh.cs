using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Vangogh - Simple Networking request
/// v 1.0
/// Developed by NyanHeavy Studios : https://nyanheavy.app
/// Based on Davinci structure by Shamsdev : https://github.com/shamsdev/davinci
/// </summary>

public class Vangogh : MonoBehaviour
{
    private bool enableLog = false;

    private readonly VangoghResponse result = new();
    [SerializeField] private string url = null;

    private UnityAction onStartAction, onSuccessAction, onEndAction;

    private UnityAction<int> onDownloadProgressChange;
    private UnityAction<string> onErrorAction;
    private UnityAction<string> onErrorEnd;
    private UnityAction<VangoghResponse> onGetResult;

    private static readonly Dictionary<string, Vangogh> underProcessVangogs = new();

    private string uniqueHash;
    private int progress;
    private int maxRetrys = 0;
    private int currentRetry;
    private float retryDelay = 1f;

    private bool useienumerator;
    private bool useasync;
    private bool single;
    private string contenttype;
    private string contentbody;
    private ConnectionMethod method;

    private readonly List<CustomHeaders> customHeaders = new();


    /// <summary>
    /// Get instance of vangogh class
    /// </summary>
    public static Vangogh Instance()
    {
        return new GameObject("Vangogh").AddComponent<Vangogh>();
    }

    /// <summary>
    /// Set url for GET data download.
    /// </summary>
    /// <param name="url">GET data Url</param>
    /// <returns></returns>
    public Vangogh GET(string url)
    {
        this.method = ConnectionMethod.get;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Url set : " + url);
        }

        this.url = url;
        return this;
    }

    /// <summary>
    /// Set url for POST data upload.
    /// </summary>
    /// <param name="url">POST data Url</param>
    /// <returns></returns>
    public Vangogh POST(string url)
    {
        this.method = ConnectionMethod.post;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Url set : " + url);
        }

        this.url = url;
        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh start request process.
    /// </summary>
    /// <param name="action">UnityAction function</param>
    /// <returns></returns>
    public Vangogh WithStartAction(UnityAction action)
    {
        this.onStartAction = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On start action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Use IEnumerator method as request system
    /// </summary>
    public Vangogh UseIEnumerator()
    {
        this.useienumerator = true;
        this.useasync = false;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Using IEnumerator");
        }

        return this;
    }

    /// <summary>
    /// Use Async method as downloader system
    /// </summary>
    public Vangogh UseAsync()
    {
        this.useasync = true;
        this.useienumerator = false;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Using async");
        }

        return this;
    }

    /// <summary>
    /// Use single instance, reload stop other instance with same url
    /// </summary>
    public Vangogh UseSingleInstance()
    {
        this.single = true;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Using single instance");
        }

        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh get success request process.
    /// </summary>
    /// <param name="action">UnityAction function</param>
    /// <returns></returns>
    public Vangogh WithSuccessAction(UnityAction action)
    {
        this.onSuccessAction = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On success action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh end request process.
    /// </summary>
    /// <param name="action">UnityAction function</param>
    /// <returns></returns>
    public Vangogh WithEndAction(UnityAction action)
    {
        this.onEndAction = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On end action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh get error request.
    /// </summary>
    /// <param name="action">UnityAction function</param>
    /// <returns></returns>
    public Vangogh WithErrorAction(UnityAction<string> action)
    {
        this.onErrorAction = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On error action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh get error request and finalize.
    /// </summary>
    /// <param name="action">UnityAction function</param>
    /// <returns></returns>
    public Vangogh WithErrorEndAction(UnityAction<string> action)
    {
        this.onErrorEnd = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On error end action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set action(int) to invoke when Vangogh is downloading request (0-1).
    /// </summary>
    /// <param name="action">UnityAction function(int)</param>
    /// <returns></returns>
    public Vangogh WithDownloadProgressChangedAction(UnityAction<int> action)
    {
        this.onDownloadProgressChange = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On download progress changed action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set action to invoke when Vangogh get request results.
    /// </summary>
    /// <param name="action">UnityAction function(VangoghResponse)</param>
    /// <returns></returns>
    public Vangogh WithGetResultAction(UnityAction<VangoghResponse> action)
    {
        this.onGetResult = action;

        if (enableLog)
        {
            Debug.Log("[Vangogh] On get result action set : " + action);
        }

        return this;
    }

    /// <summary>
    /// Set max attempts to instance reconnect
    /// </summary>
    /// <param name="retry">Int of attempts</param>
    /// <returns></returns>
    public Vangogh MaxAttempts(int retry)
    {
        this.maxRetrys = retry;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Retry connection set to : " + retry);
        }

        return this;
    }

    /// <summary>
    /// Set delay between request attempts
    /// </summary>
    /// <param name="delay">Delay between attempts</param>
    /// <returns></returns>
    public Vangogh SetAttemptsDelay(float delay)
    {
        this.retryDelay = delay;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Attempts delay connection set to : " + delay);
        }

        return this;
    }

    /// <summary>
    /// Set custom header of request
    /// </summary>
    /// <param name="type">Type of header, ex: Authorization, Content-Type...</param>
    /// <param name= "value" >Value of header type</param>
    /// <returns></returns>
    public Vangogh SetHeader(string type, string value)
    {
        CustomHeaders custom = new()
        {
            headerType = type,
            headerValue = value
        };

        this.customHeaders.Add(custom);

        if (enableLog)
        {
            Debug.Log($"[Vangogh] Header set: [{custom.headerType}]:[{custom.headerValue}]");
        }

        return this;
    }

    /// <summary>
    /// Set ContentType to send with POST request
    /// </summary>
    /// <param name="type">Type of Content, use with POST</param>
    /// <returns></returns>
    public Vangogh SetContentType(string type)
    {
        this.contenttype = type;

        if (enableLog)
        {
            Debug.Log($"[Vangogh] ContentType set: {type}");
        }

        return this;
    }

    /// <summary>
    /// Set custom body of request
    /// </summary>
    /// <param name="contentBody">Body of request, use with POST</param>
    /// <returns></returns>
    public Vangogh SetBody(string contentBody)
    {
        this.contentbody = contentBody;

        if (enableLog)
        {
            Debug.Log($"[Vangogh] Content Body set: {contentBody}");
        }

        return this;
    }

    /// <summary>
    /// Show or hide logs in console.
    /// </summary>
    /// <param name="enablelog">'true' for show logs in console.</param>
    /// <returns></returns>
    public Vangogh SetEnableLog(bool enableLog)
    {
        this.enableLog = enableLog;

        if (enableLog)
        {
            Debug.Log("[Vangogh] Logging enabled : " + enableLog);
        }

        return this;
    }

    /// <summary>
    /// Start vangogh process.
    /// </summary>
    public void Init()
    {
        if (url == null)
        {
            Error("[Vangogh] Url has not been set. Use 'load' funtion to set image url.");
            return;
        }

        if (url.StartsWith("http://")) //Default project rejects request with non secure url
        {
            url = url.Replace("http://", "https://");
        }

        try
        {
            Uri uri = new(url); //convert string to url format
            this.url = uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            Error("[Vangogh] Url is not correct : " + ex);
            return;
        }

        if (method == ConnectionMethod.get)
        {
            if (useasync == false && useienumerator == false)
            {
                Error("[Vangogh] loader type has not been set. Use 'usingIEnumerator or usingAsync' function to set loader type.");
                return;
            }
        }

        if (useasync == true && useienumerator == true)
        {
            Error("[Vangogh] Two or more loader types has been set. Please use just one.");
            return;
        }

        if (enableLog)
        {
            Debug.Log("[Vangogh] Start Working.");
        }

        if (onStartAction != null)
        {
            onStartAction?.Invoke();
        }

        uniqueHash = CreateMD5(url);

        if (underProcessVangogs.ContainsKey(uniqueHash))
        {
            Vangogh sameProcess = underProcessVangogs[uniqueHash];

            if (single)
            {
                sameProcess.Finish();
                AddDownloadProcess();
            }
            else
            {
                sameProcess.onSuccessAction += () =>
                {
                    if (onSuccessAction != null)
                    {
                        onSuccessAction?.Invoke();
                    }

                    ReturnHttpResponse();
                };
            }
        }
        else
        {
            AddDownloadProcess();
        }
    }

    private void AddDownloadProcess()
    {
        underProcessVangogs.Add(uniqueHash, this);
        StopAllCoroutines();

        if (method == ConnectionMethod.get)
        {
            if (useienumerator)
            {
                StartCoroutine(VangoghGET());
            }
            else if (useasync)
            {
                //Request using async methods will be implemented in future
            }
        }
        else if (method == ConnectionMethod.post)
        {
            StartCoroutine(VangoghPOST());
        }
    }

    private IEnumerator VangoghPOST()
    {
        UnityWebRequest request = new(url, "POST");
        byte[] bodyRaw = new UTF8Encoding().GetBytes(contentbody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (customHeaders.Count > 0)
        {
            for (int i = 0; i < customHeaders.Count; i++)
            {
                request.SetRequestHeader(customHeaders[i].headerType, customHeaders[i].headerValue);
            }
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            if (maxRetrys == 0)
            {
                //onErrorAction?.Invoke("[Vangogh] " + www.error);
                CheckResponse(request.responseCode, request.error);
                //onErrorEnd?.Invoke("[Vangogh] " + www.error);
                request.Dispose();
                Finish();
            }
            else
            {
                if (currentRetry < maxRetrys)
                {
                    currentRetry++;
                    onErrorAction?.Invoke("[Vangogh] [Attempt " + currentRetry + "/" + maxRetrys + "] " + request.error);
                    request.Dispose();
                    StartCoroutine(WaitForRetrys(ConnectionMethod.post));
                }
                else
                {
                    onErrorAction?.Invoke("[Vangogh] Max retrys reached > " + request.error);
                    //onErrorEnd?.Invoke("[Vangogh] [MAXRETRYS]");
                    CheckResponse(request.responseCode, request.error);
                    request.Dispose();
                    Finish();
                }
            }
        }
        else if (request.result == UnityWebRequest.Result.Success)
        {
            result.responseCode = request.responseCode;
            result.responseResult = request.downloadHandler.text;
            request.Dispose();

            if (onSuccessAction != null)
            {
                onSuccessAction?.Invoke();
            }

            ReturnHttpResponse();
            underProcessVangogs.Remove(uniqueHash);
        }
        else
        {
            onErrorAction?.Invoke(request.error);
            onErrorEnd?.Invoke("[UNKNOWRESULT]");
            request.Dispose();
            onEndAction?.Invoke();
        }
    }

    private async void VangoghAsyncPOST()
    {
        Debug.Log("[Vangogh] DownloaderPOST");

        using var httpClient = new HttpClient();
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"), url);
            if (customHeaders.Count > 0)
            {
                for (int i = 0; i < customHeaders.Count; i++)
                {
                    request.Headers.TryAddWithoutValidation(customHeaders[i].headerType, customHeaders[i].headerValue);
                }
            }

            request.Content = new StringContent(contentbody);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contenttype);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                result.responseCode = (long)response.StatusCode;

                if (response.Content != null)
                {
                    HttpContent content = response.Content;
                    result.responseResult = await content.ReadAsStringAsync();
                }

                ReturnHttpResponse();
                Debug.Log("[Vangogh] after return httpresponse");
                underProcessVangogs.Remove(uniqueHash);
            }
            else
            {
                onErrorAction?.Invoke("[Vangogh] " + response.StatusCode);
                onErrorEnd?.Invoke("[Vangogh] " + response.Content);
                Debug.Log("[Vangogh] is not success");
                Finish();
            }
        }
        catch (HttpRequestException ex)
        {
            if (maxRetrys == 0)
            {
                onErrorAction?.Invoke(ex.Message);
                onErrorEnd?.Invoke(ex.Message);
                Debug.Log("[Vangogh] exception");
                Finish();
            }
            else
            {
                if (currentRetry < maxRetrys)
                {
                    currentRetry++;
                    onErrorAction?.Invoke("[Vangogh] [Attempt " + currentRetry + "/" + maxRetrys + "] " + ex.Message);
                    StartCoroutine(WaitForRetrys(ConnectionMethod.post));
                }
                else
                {
                    onErrorAction?.Invoke("[Vangogh] Max retrys reached > " + ex.Message);
                    onErrorEnd?.Invoke("[Vangogh] [MAXRETRYS]");
                    Finish();
                }
            }
        }
        catch (Exception ex)
        {
            onErrorEnd?.Invoke("[Vangogh] [POST ERROR]" + ex);
            Debug.Log("[Vangogh] post exception: " + ex);
            Finish();
        }
    }

    private IEnumerator VangoghGET()
    {
        if (enableLog)
        {
            Debug.Log("[Vangogh] Download started.");
        }

        UnityWebRequest www = UnityWebRequest.Get(url);

        if (customHeaders.Count > 0)
        {
            for (int i = 0; i < customHeaders.Count; i++)
            {
                www.SetRequestHeader(customHeaders[i].headerType, customHeaders[i].headerValue);
            }
        }

        yield return www.SendWebRequest();

        while (!www.isDone)
        {
            progress = (int)(www.downloadProgress * 100f);
            onDownloadProgressChange?.Invoke(progress);

            if (enableLog)
            {
                Debug.Log("[Vangogh] Downloading progress : " + progress + "%");
            }

            yield return null;
        }

        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
        {
            if (maxRetrys == 0)
            {
                //onErrorAction?.Invoke("[Vangogh] " + www.error);
                CheckResponse(www.responseCode, www.error);
                //onErrorEnd?.Invoke("[Vangogh] " + www.error);
                www.Dispose();
                Finish();
            }
            else
            {
                if (currentRetry < maxRetrys)
                {
                    currentRetry++;
                    onErrorAction?.Invoke("[Vangogh] [Attempt " + currentRetry + "/" + maxRetrys + "] " + www.error);
                    www.Dispose();
                    StartCoroutine(WaitForRetrys(ConnectionMethod.get));
                }
                else
                {
                    onErrorAction?.Invoke("[Vangogh] Max retrys reached > " + www.error);
                    //onErrorEnd?.Invoke("[Vangogh] [MAXRETRYS]");
                    CheckResponse(www.responseCode, www.error);
                    www.Dispose();
                    Finish();
                }
            }
        }
        else if (www.result == UnityWebRequest.Result.Success)
        {
            result.responseCode = www.responseCode;
            result.responseResult = www.downloadHandler.text;
            www.Dispose();
            www = null;

            if (onSuccessAction != null)
            {
                onSuccessAction?.Invoke();
            }

            ReturnHttpResponse();
            underProcessVangogs.Remove(uniqueHash);
        }
        else
        {
            onErrorAction?.Invoke(www.error);
            onErrorEnd?.Invoke("[UNKNOWRESULT]");
            www.Dispose();
            onEndAction?.Invoke();
        }
    }

    private void CheckResponse(long httpStatusCode, string error)
    {
        switch (httpStatusCode)
        {
            case 400:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Bad Request: {error}");
                break;
            case 401:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Unauthorized: {error}");
                break;
            case 403:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Forbidden: {error}");
                break;
            case 404:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Not Found: {error}");
                break;
            case 429:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Too Many Requests: {error}");
                break;
            case 500:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Internal Server Error: {error}");
                break;
            case 502:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Bad Gateway: {error}");
                break;
            default:
                onErrorAction?.Invoke($"[Vangogh] [{httpStatusCode}] Unknow Error: {error}");
                break;
        }

        Finish();
    }

    IEnumerator WaitForRetrys(ConnectionMethod method)
    {
        yield return new WaitForSeconds(retryDelay);

        if (method == ConnectionMethod.get)
        {
            if (useienumerator)
            {
                StartCoroutine(VangoghGET());
            }
            else if (useasync)
            {
                //Downloading using async methods will be implemented in future
            }
        }
        else if (method == ConnectionMethod.post)
        {
            StartCoroutine(VangoghPOST());
        }
    }

    private void Error(string message)
    {
        if (enableLog)
        {
            Debug.LogError("[Vangogh] Error : " + message);
        }

        if (onErrorAction != null)
        {
            onErrorAction.Invoke(message);
        }

        else Finish();
    }

    private void Finish()
    {
        underProcessVangogs.Remove(uniqueHash);

        if (enableLog)
        {
            Debug.Log("[Vangogh] Operation has been finished");
        }

        if (onEndAction != null)
        {
            onEndAction?.Invoke();
        }

        Invoke(nameof(Destroyer), 0.5f);
    }

    public void Destroyer()
    {
        Destroy(gameObject);
    }

    private void ReturnHttpResponse()
    {
        progress = 100;
        if (onDownloadProgressChange != null)
        {
            onDownloadProgressChange?.Invoke(progress);
        }

        if (enableLog)
        {
            Debug.Log("[Vangogh] Downloading progress : " + progress + "%");
        }

        StopAllCoroutines();

        if (enableLog)
        {
            Debug.Log("[Vangogh] Get resulting!");
        }

        if (result.responseCode == 200)
        {
            onGetResult?.Invoke(result);
            Finish();
        }
        else
        {
            onErrorAction?.Invoke("[Vangogh] Wrong reponse code > " + result.responseCode);
            onErrorEnd?.Invoke($"[RESPONSE{result.responseResult}]");
            Finish();
        }
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
    }
}

[Serializable]
public class VangoghResponse
{
    public long responseCode;
    public string responseResult;
}

[Serializable]
public class CustomHeaders
{
    public string headerType;
    public string headerValue;
}

public enum ConnectionMethod { get, post, patch, delete }
