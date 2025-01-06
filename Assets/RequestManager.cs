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
        Vangogh.Instance()
        .GET("https://catfact.ninja/fact")
        .SetAttemptsDelay(2)
        .SetAttempts(3)
        .WithStartAction(() => { OnStartProcess(); })
        .WithGetResultAction((response) => { GetResponse(response); })
        .Init();
    }

    private void OnStartProcess()
    {
        Debug.Log("Initiating request...");
    }

    private void GetResponse(VangoghResponse response)
    {
        Debug.Log($"Response Code: {response.Code} | Result: {response.Result}");
    }
}
