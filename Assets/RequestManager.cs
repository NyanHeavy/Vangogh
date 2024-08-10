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
        .UseIEnumerator()
        .WithStartAction(() => { Debug.Log("Start request..."); })
        .WithErrorEndAction((error) => { Debug.Log($"Error: {error}"); })
        .WithGetResultAction((response) => { GetResponse(response); })
        .Init();
    }

    private void GetResponse(VangoghResponse response)
    {
        Debug.Log($"Response Code: {response.Code} | Result: {response.Result}");
    }
}
