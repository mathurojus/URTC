using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

#region Data Classes

[System.Serializable]
public class CollaborationRequest
{
    public string project_name;
    public string user_email;
    public string project_description;
    public string token; // used only for collaborators
}

[System.Serializable]
public class CollaborationResponse
{
    public bool success;
    public string message;
    public string project_id;
    public string repo_url;
    public string token;
}

#endregion

public class URTC_Panel : EditorWindow
{
    private enum PanelMode { Owner, Collaborator }
    private PanelMode currentMode = PanelMode.Owner;

    // Common fields
    private string serverURL = "http://0.0.0.0:8080";
    private string userEmail = "";
    private bool isLoading = false;
    private string statusMessage = "";

    // Owner fields
    private string projectName = "";
    private string projectDescription = "";
    private string projectPath = "";
    private string token = "";
    private string currentProjectID = "";
    private string currentRepoURL = "";
    private string collaboratorEmail = "";

    // Collaborator fields
    private string joinToken = "";

    [MenuItem("Window/URTC Panel")]
    public static void ShowWindow()
    {
        URTC_Panel window = GetWindow<URTC_Panel>();
        window.titleContent = new GUIContent("URTC Collaboration");
        window.Show();
    }

    private void OnEnable()
    {
        projectName = Application.productName;
        projectPath = Application.dataPath.Replace("/Assets", "");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("URTC Collaboration Panel", EditorStyles.boldLabel);
        GUILayout.Space(10);

        currentMode = (PanelMode)GUILayout.Toolbar((int)currentMode, new string[] { "Owner", "Collaborator" });
        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { textColor = statusMessage.StartsWith("Error") ? Color.red : Color.green }
            };
            GUILayout.Label(statusMessage, style);
            GUILayout.Space(10);
        }

        switch (currentMode)
        {
            case PanelMode.Owner:
                DrawOwnerPanel();
                break;
            case PanelMode.Collaborator:
                DrawCollaboratorPanel();
                break;
        }

        GUILayout.Space(20);
        if (!string.IsNullOrEmpty(currentRepoURL) && GUILayout.Button("Open Repository"))
        {
            Application.OpenURL(currentRepoURL);
        }
    }

    #region Owner Panel

    private void DrawOwnerPanel()
    {
        GUILayout.Label("Start New Collaboration", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Server URL", serverURL);
        userEmail = EditorGUILayout.TextField("Your Email", userEmail);
        projectName = EditorGUILayout.TextField("Project Name", projectName);
        projectDescription = EditorGUILayout.TextField("Description (optional)", projectDescription);

        GUI.enabled = !isLoading && !string.IsNullOrEmpty(userEmail);
        if (GUILayout.Button(isLoading ? "Creating..." : "Start Collaboration"))
        {
            StartCollaboration();
        }
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(currentProjectID))
        {
            GUILayout.Space(15);
            GUILayout.Label("Project Details", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Project ID", currentProjectID);
            EditorGUILayout.LabelField("Repository URL", currentRepoURL);
            EditorGUILayout.LabelField("Join Token", token);

            GUILayout.Space(10);
            collaboratorEmail = EditorGUILayout.TextField("Add Collaborator Email", collaboratorEmail);
            if (GUILayout.Button("Add Collaborator"))
            {
                statusMessage = "Collaborator invite functionality coming soon!";
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Push Changes to GitHub"))
            {
                statusMessage = "Pushing changes...";
                StartSimulatedPush();
                Debug.Log("Simulating Git push from Unity project...");
            }
        }
    }

    #endregion

    #region Collaborator Panel

    private void DrawCollaboratorPanel()
    {
        GUILayout.Label("Join Existing Collaboration", EditorStyles.boldLabel);
        userEmail = EditorGUILayout.TextField("Your Email", userEmail);
        joinToken = EditorGUILayout.TextField("Join Token", joinToken);

        GUI.enabled = !isLoading && !string.IsNullOrEmpty(joinToken);
        if (GUILayout.Button(isLoading ? "Joining..." : "Join Collaboration"))
        {
            JoinCollaboration();
        }
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(currentRepoURL))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Connected Repository", currentRepoURL);
            if (GUILayout.Button("Pull Latest Changes"))
            {
                statusMessage = "Pulling latest changes...";
                StartSimulatedPull();
                Debug.Log("Simulating Git pull...");
            }
        }
    }

    #endregion

    #region API Calls

    private void StartCollaboration()
    {
        CollaborationRequest req = new CollaborationRequest
        {
            project_name = projectName,
            user_email = userEmail,
            project_description = projectDescription
        };

        string jsonData = JsonUtility.ToJson(req);
        StartRequestCoroutine(serverURL + "/api/start-collaboration", jsonData, isJoin: false);
    }

    private void JoinCollaboration()
    {
        CollaborationRequest req = new CollaborationRequest
        {
            user_email = userEmail,
            token = joinToken
        };

        string jsonData = JsonUtility.ToJson(req);
        StartRequestCoroutine(serverURL + "/api/join-collaboration", jsonData, isJoin: true);
    }

    private void StartRequestCoroutine(string url, string jsonData, bool isJoin)
    {
        EditorCoroutineUtility.StartCoroutine(SendCollaborationRequest(url, jsonData, isJoin));
    }

    private IEnumerator SendCollaborationRequest(string url, string jsonData, bool isJoin)
    {
        isLoading = true;
        Repaint();

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 30;

            yield return www.SendWebRequest();
            isLoading = false;

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusMessage = $"Error: {www.error}";
            }
            else
            {
                string responseText = www.downloadHandler.text;
                CollaborationResponse response = JsonUtility.FromJson<CollaborationResponse>(responseText);

                if (response.success)
                {
                    statusMessage = response.message;
                    currentRepoURL = response.repo_url;
                    currentProjectID = response.project_id;
                    token = response.token;
                }
                else
                {
                    statusMessage = "Error: " + response.message;
                }
            }

            Repaint();
        }
    }

    #endregion

    #region Simulated Git Actions

    private void StartSimulatedPush()
    {
        EditorCoroutineUtility.StartCoroutine(SimulateGitAction("Push"));
    }

    private void StartSimulatedPull()
    {
        EditorCoroutineUtility.StartCoroutine(SimulateGitAction("Pull"));
    }

    private IEnumerator SimulateGitAction(string action)
    {
        isLoading = true;
        statusMessage = $"{action} in progress...";
        Repaint();

        float progress = 0f;
        
        float stepDelay = 0.2f;
        double nextTime = EditorApplication.timeSinceStartup;

        while (progress < 1f)
        {
            if (EditorApplication.timeSinceStartup >= nextTime)
            {
                progress += 0.1f;
                EditorUtility.DisplayProgressBar(
                    $"{action} to GitHub",
                    $"{action}ing changes... ({(int)(progress * 100)}%)",
                    progress
                );
                nextTime = EditorApplication.timeSinceStartup + stepDelay;
            }

            yield return null; // Let the Editor update between frames
        }


        EditorUtility.ClearProgressBar();
        isLoading = false;

        if (action == "Push")
        {
            statusMessage = "Changes successfully pushed to GitHub (simulated).";
            Debug.Log($"[URTC] {action} simulation complete at {DateTime.Now}");
        }
        else
        {
            statusMessage = "Latest changes pulled from GitHub (simulated).";
            Debug.Log($"[URTC] {action} simulation complete at {DateTime.Now}");
        }

        Repaint();
    }

    #endregion

    private void OnDestroy()
    {
        EditorCoroutineUtility.StopAllCoroutines();
        EditorUtility.ClearProgressBar();
    }
}
