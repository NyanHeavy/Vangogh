using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Vangogh - Simple API WebRequest system for Unity
/// v3.3 (Ultra-Lean Syntax Edition)
/// Developed by NyanHeavy Studios : https://github.com/NyanHeavy/Vangogh
/// Based on Davinci structure by Shamsdev : https://github.com/shamsdev/davinci
/// </summary>

namespace NyanHeavyStudios.Vangogh
{
    public class Vangogh : MonoBehaviour
    {
        private static Vangogh _instance;
        private static readonly Dictionary<string, Coroutine> ActiveRequests = new();

        #region Shortcut Methods
        /// <summary> Shortcut to start a GET request immediately. </summary>
        /// <param name="url">The target URL.</param>
        public static RequestBuilder GET(string url)
        {
            if (_instance == null)
            {
                GameObject go = new("Vangogh");
                _instance = go.AddComponent<Vangogh>();
                DontDestroyOnLoad(go);
            }
            return new RequestBuilder(_instance).GET(url);
        }

        /// <summary> Shortcut to start a POST request immediately. </summary>
        /// <param name="url">The target URL.</param>
        public static RequestBuilder POST(string url)
        {
            if (_instance == null)
            {
                GameObject go = new("Vangogh");
                _instance = go.AddComponent<Vangogh>();
                DontDestroyOnLoad(go);
            }
            return new RequestBuilder(_instance).POST(url);
        }
        #endregion

        /// <summary> Starts a new request configuration builder manually. </summary>
        public RequestBuilder Request() => new(this);

        #region Internal Utils
        internal void Log(string message, LogType logType, bool enabled)
        {
            if (!enabled) return;
            string prefix = "<color=#4CAF50>[Vangogh]</color>";
            switch (logType)
            {
                case LogType.Error: Debug.LogError($"{prefix} {message}"); break;
                case LogType.Warning: Debug.LogWarning($"{prefix} {message}"); break;
                default: Debug.Log($"{prefix} {message}"); break;
            }
        }

        internal string CreateMD5(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
            return sb.ToString();
        }

        #endregion

        /// <summary> Helper class to configure and execute individual web requests. </summary>
        public class RequestBuilder
        {
            private readonly Vangogh _manager;
            private string _url;
            private ConnectionMethod _method;
            private string _body;
            private WWWForm _form;
            private bool _isJson = false;
            private string _contentType = "application/json";
            private bool _logEnabled;
            private bool _singleInstance;
            private int _retries;
            private float _delay = 1f;
            private bool _useCache = false;
            private readonly List<CustomHeaders> _localHeaders = new();

            private UnityAction _onStart, _onSuccess, _onError, _onEnd;
            private UnityAction<VangoghResponse> _onResult;

            public RequestBuilder(Vangogh manager) => _manager = manager;

            /// <summary> Sets the request method to GET and defines the target URL. </summary>
            public RequestBuilder GET(string url) { _url = url; _method = ConnectionMethod.get; return this; }

            /// <summary> Sets the request method to POST and defines the target URL. </summary>
            public RequestBuilder POST(string url) { _url = url; _method = ConnectionMethod.post; return this; }

            /// <summary> Sets the form body for the request (WWWForm). </summary>
            public RequestBuilder SetBody(WWWForm form) { _form = form; _isJson = false; return this; }

            /// <summary> Sets the json body for request </summary>
            public RequestBuilder SetJsonBody(string json) { _body = json; _isJson = true; _contentType = "application/json"; return this; }

            /// <summary> Simple set for pure string body </summary>
            /// <param name="raw"> string </param> <param name="contentType"> request content type </param> <returns></returns>
            public RequestBuilder SetRawBody(string raw, string contentType) { _body = raw; _isJson = true; _contentType = contentType; return this; }

            /// <summary> Sets the Content-Type header. Default is "application/json". </summary>
            public RequestBuilder SetContentType(string type) { _contentType = type; return this; }

            /// <summary> Enables or disables console logging for this specific request. </summary>
            public RequestBuilder SetLog(bool enable) { _logEnabled = enable; return this; }

            /// <summary> Configures retry logic on connection failure. </summary>
            public RequestBuilder SetAttempts(int count, float delay = 1f) { _retries = count; _delay = delay; return this; }

            /// <summary> Configure to use cache to response faster. </summary>
            public RequestBuilder UseCache(bool usecache) { _useCache = usecache; return this; }

            /// <summary> If called, any existing request to the same URL will be cancelled before starting this one. </summary>
            public RequestBuilder UseSingleInstance() { _singleInstance = true; return this; }

            /// <summary> Adds a custom header to this specific request. </summary>
            public RequestBuilder AddHeader(string key, string value) { _localHeaders.Add(new CustomHeaders { headerType = key, headerValue = value }); return this; }

            /// <summary> Action triggered when the request starts. </summary>
            public RequestBuilder OnStart(UnityAction a) { _onStart = a; return this; }

            /// <summary> Action triggered only on HTTP success (200-299). </summary>
            public RequestBuilder OnSuccess(UnityAction a) { _onSuccess = a; return this; }

            /// <summary> Action triggered on connection or protocol error. </summary>
            public RequestBuilder OnError(UnityAction a) { _onError = a; return this; }

            /// <summary> Action triggered when the process finishes, regardless of the result. </summary>
            public RequestBuilder OnEnd(UnityAction a) { _onEnd = a; return this; }

            /// <summary> Action triggered on success, providing the response code and data string. </summary>
            public RequestBuilder OnResult(UnityAction<VangoghResponse> a) { _onResult = a; return this; }

            /// <summary> Finalizes configuration and executes the request through the Manager. </summary>
            public void Init()
            {
                if (string.IsNullOrEmpty(_url)) return;
                string hash = _manager.CreateMD5(_url);

                if (_singleInstance && ActiveRequests.ContainsKey(hash))
                {
                    if (ActiveRequests[hash] != null) _manager.StopCoroutine(ActiveRequests[hash]);
                    ActiveRequests.Remove(hash);
                }

                Coroutine c = _manager.StartCoroutine(ExecuteRoutine(hash));
                ActiveRequests[hash] = c;
            }

            private IEnumerator ExecuteRoutine(string hash)
            {
                _onStart?.Invoke();
                int currentRetry = 0;
                bool completed = false;

                while (currentRetry <= _retries && !completed)
                {
                    UnityWebRequest www = new();

                    if (_method == ConnectionMethod.get)
                    {
                        www = UnityWebRequest.Get(_url);
                    }
                    else
                    {
                        if (_isJson)
                        {
                            byte[] bodyRaw = Encoding.UTF8.GetBytes(_body ?? "");
                            www = new UnityWebRequest(_url, "POST")
                            {
                                uploadHandler = new UploadHandlerRaw(bodyRaw),
                                downloadHandler = new DownloadHandlerBuffer()
                            };

                            www.SetRequestHeader("Content-Type", _contentType);
                        }
                        else
                        {
                            www = UnityWebRequest.Post(_url, _form);
                        }
                    }

                    foreach (var h in _localHeaders) www.SetRequestHeader(h.headerType, h.headerValue);
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        var res = new VangoghResponse { code = www.responseCode, result = www.downloadHandler.text };
                        _onSuccess?.Invoke();
                        _onResult?.Invoke(res);
                        completed = true;
                    }
                    else
                    {
                        currentRetry++;
                        if (currentRetry > _retries)
                        {
                            _manager.Log($"Request failed: {www.error}", LogType.Error, _logEnabled);
                            _onError?.Invoke();
                        }
                        else yield return new WaitForSeconds(_delay);
                    }
                }

                _onEnd?.Invoke();
                if (ActiveRequests.ContainsKey(hash)) ActiveRequests.Remove(hash);
            }
        }
    }

    [Serializable] public class VangoghResponse { public long code; public string result; }
    [Serializable] public class CustomHeaders { public string headerType; public string headerValue; }
    public enum ConnectionMethod { get, post, patch, delete }
}
