using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[System.Serializable]
public class StartCollaborationRequest
{
    public string project_name;
    public string user_email
            public string user_token;  // Email-based auto-generated token;
}

[System.Serializable]
public class StartCollaborationResponse
{
    public bool success;
    public string message;
    public string project_id;
    public string repo_url;
}

public class URTC_Panel : EditorWindow
{
    private string userEmail = "";
    private string serverURL = "http://0.0.0.0:8080";
    private bool isLoading = false;
    private string statusMessage = "";
    private string currentProjectID = "";
    private string currentRepoURL = "";
    private string collaboratorEmail = "";

    [MenuItem("Window/URTC Panel")]
    public static void ShowWindow()
    {
        URTC_Panel window = GetWindow<URTC_Panel>();
        window.titleContent = new GUIContent("URTC Panel");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Initialize a URTC Repository", EditorStyles.boldLabel);
        
        if (!string.IsNullOrEmpty(currentProjectID))
        {
            GUILayout.Space(10);
            GUILayout.Label("Current Project:", EditorStyles.boldLabel);
            GUILayout.Label($"Project ID: {currentProjectID}");
            if (!string.IsNullOrEmpty(currentRepoURL))
            {
                GUILayout.Label($"Repository: {currentRepoURL}");
                if (GUILayout.Button("Open Repository"))
                {
                    Application.OpenURL(currentRepoURL);
                }
            }
            GUILayout.Space(10);
        }

        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUILayout.Space(5);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = statusMessage.Contains("Error") ? Color.red : Color.green;
            GUILayout.Label(statusMessage, style);
            GUILayout.Space(5);
        }

        if (string.IsNullOrEmpty(currentProjectID))
        {

            GUILayout.Label("Server URL:", EditorStyles.boldLabel);
            serverURL = EditorGUILayout.TextField("Server URL", serverURL);

            GUILayout.Space(10);

            GUILayout.Label("Status:", EditorStyles.boldLabel);
            statusMessage = EditorGUILayout.TextField("Status", statusMessage);

            GUILayout.Space(10);

            GUILayout.Label("User Email:", EditorStyles.boldLabel);
            userEmail = EditorGUILayout.TextField("Email", userEmail);

            GUILayout.Space(10);

            GUI.enabled = !isLoading && !string.IsNullOrEmpty(userEmail);
            if (GUILayout.Button(isLoading ? "Creating Repository..." : "Start Collaboration"))
            {
                StartCollaboration();
            }
            GUI.enabled = true;
        }
        GUILayout.Space(20);
        GUILayout.Label("Add Collaborators", EditorStyles.boldLabel);
        collaboratorEmail = EditorGUILayout.TextField("Collaborator Email", collaboratorEmail);
         
        if (GUILayout.Button("Add Collaborator"))
        {
            if (!string.IsNullOrEmpty(collaboratorEmail))
            {
                Debug.Log($"Adding collaborator: {collaboratorEmail}");
                statusMessage = "Collaborator functionality coming soon!";
            }
        }
    }

    private void StartCollaboration()
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            statusMessage = "Error: Please enter your email address";
            return;
        }

        isLoading = true;
        statusMessage = "Authenticating with GitHub...";

        string projectName = Application.productName;
        if (string.IsNullOrEmpty(projectName))
        {
            projectName = "UnityProject_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        StartCollaborationRequest request = new StartCollaborationRequest
        {
            project_name = projectName,
            user_email = userEmai,
                        user_token = GenerateEmailBasedToken(userEmail)l
        };

        string jsonData = JsonUtility.ToJson(request);

        // Start the HTTP request coroutine
        EditorCoroutineUtility.StartCoroutine(SendStartCollaborationRequest(jsonData));
    }

    private IEnumerator SendStartCollaborationRequest(string jsonData)
    {
        string url = serverURL + "/api/start-collaboration";

        // Add timeout to prevent hanging indefinitely
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            try
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                // Set a reasonable timeout (30 seconds)
                www.timeout = 30;

                yield return www.SendWebRequest();

                isLoading = false;

                // More comprehensive error checking
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    statusMessage = $"Connection Error: Cannot reach server at {serverURL}";
                    Debug.LogError($"Connection Error: {www.error ?? "Unknown connection error"}");
                }
                else if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    statusMessage = $"Server Error: HTTP {www.responseCode}";
                    Debug.LogError($"Protocol Error: HTTP {www.responseCode} - {www.error ?? "Unknown protocol error"}");
                }
                else if (www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    statusMessage = "Data Processing Error: Invalid response format";
                    Debug.LogError($"Data Processing Error: {www.error ?? "Unknown data processing error"}");
                }
                else if (www.result != UnityWebRequest.Result.Success)
                {
                    statusMessage = $"Request Failed: {www.result}";
                    Debug.LogError($"Request Failed: {www.result} - {www.error ?? "Unknown error"}");
                }
                else
                {
                    // Success case - but still need to validate response
                    string responseText = www.downloadHandler?.text ?? "";

                    if (string.IsNullOrEmpty(responseText))
                    {
                        statusMessage = "Error: Server returned empty response";
                        Debug.LogError("Server returned empty response");
                        Repaint();
                        yield break; // Exit the coroutine early
                    }

                    try
                    {
                        StartCollaborationResponse response = JsonUtility.FromJson<StartCollaborationResponse>(responseText);

                        if (response == null)
                        {
                            statusMessage = "Error: Could not parse server response";
                            Debug.LogError($"JSON parsing returned null. Response was: {responseText}");
                        }
                        else if (response.success)
                        {
                            statusMessage = "Success: " + (response.message ?? "Repository created successfully");
                            currentProjectID = response.project_id ?? "";
                            currentRepoURL = response.repo_url ?? "";

                            Debug.Log("Project created successfully!");
                            Debug.Log($"Project ID: {currentProjectID}");
                            Debug.Log($"Repository URL: {currentRepoURL}");
                        }
                        else
                        {
                            statusMessage = "Error: " + (response.message ?? "Unknown server error");
                            Debug.LogError($"Server reported failure: {response.message ?? "Unknown error"}");
                        }
                    }
                    catch (Exception e)
                    {
                        statusMessage = "Error: Invalid response format from server";
                        Debug.LogError($"JSON Parse Error: {e.Message}");
                        Debug.LogError($"Response that failed to parse: {responseText}");
                    }
                }
            
            
                // Generate email-based token for authentication
    private string GenerateEmailBasedToken(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "";
        
        // Generate a unique token based on email using SHA256
        using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(email + "URTC_SECRET_2025"));
            return System.Convert.ToBase64String(hashBytes).Substring(0, 16);
        }
    }}
            // catch (Exception e)
            // {
            //     // Catch any unexpected errors in the entire process
            //     isLoading = false;
            //     statusMessage = "Error: Unexpected error occurred";
            //     Debug.LogError($"Unexpected error in SendStartCollaborationRequest: {e.Message}");
            //     Debug.LogError($"Stack trace: {e.StackTrace}");
            // }
            finally
            {
                // Ensure UI gets updated regardless of what happens
                Repaint();
            }
        }
    }


    private void OnDestroy()
    {
        // Stop all coroutines when window is closed
        EditorCoroutineUtility.StopAllCoroutines();
    }
}
