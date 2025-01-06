using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Vangogh - Simple API WebRequest system for Unity
/// v 2.0
/// Developed by NyanHeavy Studios : https://github.com/NyanHeavy/Vangogh
/// Based on Davinci structure by Shamsdev : https://github.com/shamsdev/davinci
/// </summary>

namespace NyanHeavyStudios.Vangogh
{
    public class Vangogh : MonoBehaviour
    {
        private bool enableLog = false;
        private readonly VangoghResponse result = new();
        [SerializeField] private string url = null;

        private UnityAction onStartAction, onSuccessAction, onEndAction;

        private UnityAction onErrorAction;
        private UnityAction<VangoghResponse> onGetResult;

        private static readonly Dictionary<string, Vangogh> underProcessVangogs = new();

        private string uniqueHash;
        private int maxRetrys = 0;
        private int currentRetry;
        private float retryDelay = 1f;

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
        /// Show or hide logs in console.
        /// </summary>
        /// <param name="enablelog">'true' for show logs in console.</param>
        /// <returns></returns>
        public Vangogh SetEnableLog()
        {
            this.enableLog = true;

            if (enableLog)
            {
                SendLog("Logging enabled", LogType.Log);
            }

            return this;
        }

        /// <summary>
        /// Set url for GET method.
        /// </summary>
        /// <param name="url">GET data Url</param>
        /// <returns></returns>
        public Vangogh GET(string url)
        {
            this.method = ConnectionMethod.get;

            if (enableLog)
            {
                SendLog("GET Url set: " + url, LogType.Log);
            }

            this.url = url;
            return this;
        }

        /// <summary>
        /// Set url for POST method.
        /// </summary>
        /// <param name="url">POST data Url</param>
        /// <returns></returns>
        public Vangogh POST(string url)
        {
            this.method = ConnectionMethod.post;

            if (enableLog)
            {
                SendLog("POST Url set: " + url, LogType.Log);
            }

            this.url = url;
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
                SendLog("Using single instance", LogType.Log);
            }

            return this;
        }

        /// <summary>
        /// Set action to invoke when Vangogh start process.
        /// </summary>
        /// <param name="action">UnityAction function</param>
        /// <returns></returns>
        public Vangogh WithStartAction(UnityAction action)
        {
            this.onStartAction = action;

            if (enableLog)
            {
                SendLog("On start action set : " + action.ToString(), LogType.Log);
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
                SendLog("On success action set : " + action.ToString(), LogType.Log);
            }

            return this;
        }

        /// <summary>
        /// Set action to invoke when Vangogh end request process => regardless of the outcome.
        /// </summary>
        /// <param name="action">UnityAction function</param>
        /// <returns></returns>
        public Vangogh WithEndAction(UnityAction action)
        {
            this.onEndAction = action;

            if (enableLog)
            {
                SendLog("On end action set : " + action.ToString(), LogType.Log);
            }

            return this;
        }

        /// <summary>
        /// Set action to invoke when Vangogh get error request.
        /// </summary>
        /// <param name="action">UnityAction function</param>
        /// <returns></returns>
        public Vangogh WithErrorAction(UnityAction action)
        {
            this.onErrorAction = action;

            if (enableLog)
            {
                SendLog("On error action set : " + action.ToString(), LogType.Log);
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
                SendLog("On get result action set : " + action.ToString(), LogType.Log);
            }

            return this;
        }

        /// <summary>
        /// Set max attempts to instance reconnect
        /// </summary>
        /// <param name="retry">Int of attempts</param>
        /// <returns></returns>
        public Vangogh SetAttempts(int retry)
        {
            this.maxRetrys = retry;

            if (enableLog)
            {
                SendLog("Retry connection set to : " + retry, LogType.Log);
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
                SendLog("Attempts delay connection set to : " + delay, LogType.Log);
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
                SendLog($"Header set : [{custom.headerType}]:[{custom.headerValue}]", LogType.Log);
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
                SendLog($"ContentType set : {type}", LogType.Log);
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
                SendLog($"Content Body set : {contentBody}", LogType.Log);
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
                Error("Url has not been set. Use 'GET' or 'POST' function to set url.");
                return;
            }

            uniqueHash = CreateMD5(url);

            if (url.StartsWith("http://")) //Default unity project rejects request with non secure url.
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
                Error("Url is not correct : " + ex);
                return;
            }

            if (enableLog)
            {
                SendLog("Start Process", LogType.Log);
            }

            onStartAction?.Invoke();

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
                StartCoroutine(VangoghGET());
            }
            else if (method == ConnectionMethod.post)
            {
                VangoghPOST();
            }
        }

        private async void VangoghPOST()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                    {
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
                            result.Code = (long)response.StatusCode;

                            if (response.Content != null)
                            {
                                HttpContent content = response.Content;
                                result.Result = await content.ReadAsStringAsync();
                            }

                            ReturnHttpResponse();
                            underProcessVangogs.Remove(uniqueHash);
                        }
                        else
                        {
                            onErrorAction?.Invoke();
                            CheckResponse(result.Code, result.Result);
                        }
                    }
                }
                catch (HttpRequestException exception)
                {
                    if (maxRetrys == 0)
                    {
                        SendLog($"Error : {exception.Message}", LogType.Error);
                        Finish();
                    }
                    else
                    {
                        if (currentRetry < maxRetrys)
                        {
                            currentRetry++;
                            onErrorAction?.Invoke();
                            SendLog($"[Attempt {currentRetry}/{maxRetrys}] {exception.Message}", LogType.Warning);
                            StartCoroutine(WaitForRetrys(ConnectionMethod.post));
                        }
                        else
                        {
                            onErrorAction?.Invoke();
                            SendLog($"Maximum attempts reached : {exception.Message}", LogType.Error);
                            Finish();
                        }
                    }
                }
                catch (Exception exception)
                {
                    SendLog($"Unknow POST error : {exception.Message}", LogType.Error);
                    Finish();
                }
            }
        }

        private IEnumerator VangoghGET()
        {
            if (enableLog)
            {
                SendLog("Request started", LogType.Log);
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

            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
            {
                if (maxRetrys == 0)
                {
                    CheckResponse(www.responseCode, www.error);
                    www.Dispose();
                }
                else
                {
                    if (currentRetry < maxRetrys)
                    {
                        currentRetry++;
                        SendLog($"[Attempt {currentRetry}/{maxRetrys}] {www.error}", LogType.Warning);
                        www.Dispose();
                        StartCoroutine(WaitForRetrys(ConnectionMethod.get));
                    }
                    else
                    {
                        SendLog($"Maximum attempts reached", LogType.Error);
                        CheckResponse(www.responseCode, www.error);
                        www.Dispose();
                    }
                }
            }
            else if (www.result == UnityWebRequest.Result.Success)
            {
                result.Code = www.responseCode;
                result.Result = www.downloadHandler.text;
                www.Dispose();
                www = null;
                onSuccessAction?.Invoke();

                ReturnHttpResponse();
                underProcessVangogs.Remove(uniqueHash);
            }
            else
            {
                onErrorAction?.Invoke();
                CheckResponse(www.responseCode, www.error);
                www.Dispose();
                onEndAction?.Invoke();
            }
        }

        private void CheckResponse(long httpStatusCode, string error)
        {
            switch (httpStatusCode)
            {
                case 400:
                    SendLog($"[{httpStatusCode}] Bad Request : {error}", LogType.Error);
                    break;
                case 401:
                    SendLog($"[{httpStatusCode}] Unauthorized : {error}", LogType.Error);
                    break;
                case 403:
                    SendLog($"[{httpStatusCode}] Forbidden : {error}", LogType.Error);
                    break;
                case 404:
                    SendLog($"[{httpStatusCode}] Not Found : {error}", LogType.Error);
                    break;
                case 429:
                    SendLog($"[{httpStatusCode}] Too Many Requests : {error}", LogType.Error);
                    break;
                case 500:
                    SendLog($"[{httpStatusCode}] Internal Server Error : {error}", LogType.Error);
                    break;
                case 502:
                    SendLog($"[{httpStatusCode}] Bad Gateway : {error}", LogType.Error);
                    break;
                default:
                    SendLog($"[{httpStatusCode}] Unknow Error : {error}", LogType.Error);
                    break;
            }

            Finish();
        }

        IEnumerator WaitForRetrys(ConnectionMethod method)
        {
            SendLog($"[Waiting for next Attempt] : {retryDelay}s", LogType.Warning);
            yield return new WaitForSeconds(retryDelay);

            if (method == ConnectionMethod.get)
            {
                StopAllCoroutines();
                StartCoroutine(VangoghGET());
            }
            else if (method == ConnectionMethod.post)
            {
                StopAllCoroutines();
                VangoghPOST();
            }
        }

        private void Error(string message)
        {
            if (enableLog)
            {
                SendLog(message, LogType.Error);
            }

            Finish();
        }

        private void ReturnHttpResponse()
        {
            StopAllCoroutines();

            if (enableLog)
            {
                SendLog("Getting result", LogType.Log);
            }

            if (result.Code == 200)
            {
                onGetResult?.Invoke(result);
                Finish();
            }
            else
            {
                onErrorAction?.Invoke();
                SendLog($"Wrong response code : {result.Code}", LogType.Error);
                Finish();
            }
        }

        public static string CreateMD5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private void SendLog(string message, LogType logtype)
        {
            string log = "";

            if (Application.isEditor)
            {
                log = logtype switch
                {
                    LogType.Error => $"<color=red>[Vangogh][Error] {message}</color>",
                    LogType.Log => $"<color=green>[Vangogh][Debug] {message}</color>",
                    LogType.Warning => $"<color=yellow>[Vangogh][Warning] {message}</color>",
                    _ => $"[Vangogh] : {message}",
                };
            }
            else
            {
                log = logtype switch
                {
                    LogType.Error => $"[Vangogh][Error] {message}",
                    LogType.Log => $"[Vangogh][Debug] {message}",
                    LogType.Warning => $"[Vangogh][Warning] {message}",
                    _ => $"[Vangogh] : {message}",
                };
            }

            Debug.Log(log);
        }

        private void Finish()
        {
            underProcessVangogs.Remove(uniqueHash);

            if (enableLog)
            {
                SendLog("Operation has been finished", LogType.Log);
            }

            onEndAction?.Invoke();
            Invoke(nameof(Destroyer), 0.5f);
        }

        public void Destroyer()
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    public class VangoghResponse
    {
        public long Code;
        public string Result;
    }

    [Serializable]
    public class CustomHeaders
    {
        public string headerType;
        public string headerValue;
    }

    public enum ConnectionMethod { get, post, patch, delete }
}