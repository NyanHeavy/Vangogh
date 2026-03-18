using System.Collections;
using System.Collections.Generic;
using NyanHeavyStudios.Vangogh;
using UnityEngine;

public class RequestManager : MonoBehaviour
{
    void Start()
    {
        MakeRequest();
    }

    private void MakeRequest()
    {
        Vangogh
        .GET("https://catfact.ninja/fact")
        .SetAttempts(3, 1f)
        .OnStart(() => { OnStartProcess(); })
        .OnResult((res) => { GetResponse(res); })
        .Init();
    }

    private void OnStartProcess()
    {
        Debug.Log("Initiating request...");
    }

    private void GetResponse(VangoghResponse response)
    {
        Debug.Log($"Response Code: {response.code} | Result: {response.result}");
    }
}
