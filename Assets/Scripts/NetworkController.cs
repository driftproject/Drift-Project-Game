using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    public static NetworkController instance;

    public string response;

    public void GetRequest(string url)
    {
        StartCoroutine(GetRequestIE(url));
    }

    public void PostRequest(string url, string username, string password)
    {
        StartCoroutine(PostRequestIE(url, username, password));
    }

    IEnumerator GetRequestIE(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(webRequest.downloadHandler.text);
                    break;
            }

            response = webRequest.downloadHandler.text;
        }
    }

    IEnumerator PostRequestIE(string url, string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
        }
    }
}