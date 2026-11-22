using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarterAssets;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Exercises the saved tutorial scene in Play Mode without changing its assets.</summary>
[InitializeOnLoad]
public static class TutorialFlowChecks
{
    private const string runKey = "TutorialFlowChecks.Run";
    private const string scenePath = "Assets/Scenes/Official levels/Tutorial Level.unity";
    private static IEnumerator routine;
    private static readonly List<string> results = new List<string>();
    private static readonly List<string> runtimeErrors = new List<string>();

    static TutorialFlowChecks()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Tools/Tutorial/Run Flow Checks")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().path != scenePath)
            throw new InvalidOperationException("Open Tutorial Level before running its checks.");
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        SessionState.SetBool(runKey, true);
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(runKey, false))
        {
            results.Clear();
            runtimeErrors.Clear();
            Directory.CreateDirectory("Temp/TutorialChecks");
            Application.logMessageReceived += OnLog;
            routine = CheckFlow();
            EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= Tick;
            routine = null;
            SessionState.SetBool(runKey, false);
        }
    }

    private static void Tick()
    {
        try
        {
            if (routine != null && routine.MoveNext()) return;
            Finish("PASS");
        }
        catch (Exception exception)
        {
            results.Add(exception.ToString());
            Finish("FAIL");
        }
    }

    private static void Finish(string outcome)
    {
        Application.logMessageReceived -= OnLog;
        EditorApplication.update -= Tick;
        routine = null;
        SessionState.SetBool(runKey, false);
        Directory.CreateDirectory("Temp/TutorialChecks");
        File.WriteAllLines("Temp/TutorialChecks/results.txt", new[] { outcome }.Concat(results));
        Debug.Log("Tutorial flow checks: " + outcome);
        EditorApplication.isPlaying = false;
    }

    private static IEnumerator CheckFlow()
    {
        // Run both final outcomes and both destination buttons on fresh scene instances.
        for (int run = 0; run < 2; run++)
        {
            Tutorial_SM tutorial = null;
            float deadline = Time.realtimeSinceStartup + 15f;
            while (tutorial == null || tutorial.ActiveMosquito == null)
            {
                Require(Time.realtimeSinceStartup < deadline, "Tutorial did not initialize.");
                tutorial = UnityEngine.Object.FindFirstObjectByType<Tutorial_SM>();
                yield return null;
            }
            SerializedObject setup = new SerializedObject(tutorial);
            GameObject player = Reference<GameObject>(setup, "player");
            MosquitoManager[] managers = UnityEngine.Object.FindObjectsByType<MosquitoManager>(FindObjectsSortMode.None);
            Check(managers.Sum(m => m.ActiveMosquitoCount) == 0, "Room 1 has no spawned mosquitoes.");
            Vector3 initial = tutorial.ActiveMosquito.transform.position;
            float until = Time.time + 0.6f;
            while (Time.time < until) yield return null;
            Check(Vector3.Distance(initial, tutorial.ActiveMosquito.transform.position) < 0.01f, "Placed mosquito remains stationary.");
            if (run == 0) ScreenCapture.CaptureScreenshot("Temp/TutorialChecks/Room1.png");
            tutorial.NextStage(1);
            Check(tutorial.CurrentStageIndex == 0, "Cannot advance before completing a room.");

            for (int stage = 0; stage < 4; stage++)
            {
                deadline = Time.realtimeSinceStartup + 15f;
                while (tutorial.ActiveMosquito == null)
                {
                    Require(Time.realtimeSinceStartup < deadline, "No mosquito spawned in room " + (stage + 1));
                    yield return null;
                }
                Check(tutorial.CurrentStageIndex == stage, "Entered room " + (stage + 1));
                TMP_Text objective = Reference<TMP_Text>(setup, "objectiveText");
                Check(objective.text.Contains("ROOM " + (stage + 1)), "Room objective is visible.");
                if (stage > 0)
                {
                    Check(managers.Sum(m => m.ActiveMosquitoCount) == 1, "Exactly one spawned mosquito is active.");
                    initial = tutorial.ActiveMosquito.transform.position;
                    until = Time.time + 0.5f;
                    while (Time.time < until) yield return null;
                    Check(tutorial.ActiveMosquito != null && Vector3.Distance(initial, tutorial.ActiveMosquito.transform.position) > 0.02f, "Mosquito flies toward its target.");

                    if (run == 0)
                    {
                        TargetHealth target = setup.FindProperty("targets").GetArrayElementAtIndex(stage).objectReferenceValue as TargetHealth;
                        MosquitoAgent failedMosquito = tutorial.ActiveMosquito;
                        MoveInto(failedMosquito, target.GetComponent<Collider>());
                        deadline = Time.realtimeSinceStartup + 3f;
                        while (failedMosquito.gameObject.activeInHierarchy)
                        {
                            Require(Time.realtimeSinceStartup < deadline, "Target trigger did not remove the mosquito.");
                            yield return null;
                        }
                        Check(!tutorial.IsRoomComplete, "Target arrival fails the attempt without completing the room.");
                        Check(Reference<TMP_Text>(setup, "feedbackText").text.Contains("reached the target"), "Failure feedback explains the retry.");
                        if (stage == 1) ScreenCapture.CaptureScreenshot("Temp/TutorialChecks/Retry.png");
                        deadline = Time.realtimeSinceStartup + 15f;
                        while (tutorial.ActiveMosquito == null)
                        {
                            Require(Time.realtimeSinceStartup < deadline, "Failed attempt did not respawn.");
                            yield return null;
                        }
                        Check(managers.Sum(m => m.ActiveMosquitoCount) == 1, "Retry spawns only one mosquito.");
                        Check(!tutorial.ActiveMosquito.GetComponent<MosquitoHealth>().IsDead, "Reused mosquito has full live health.");
                    }
                }

                MosquitoAgent mosquito = tutorial.ActiveMosquito;
                if (stage == 3 && run == 0)
                {
                    Zapper zapper = GameObject.Find("Room4").GetComponentInChildren<Zapper>();
                    Require(zapper != null, "Final room has no zapper.");
                    MoveInto(mosquito, zapper.GetComponent<Collider>() ?? zapper.GetComponentInChildren<Collider>());
                }
                else
                {
                    Window window = setup.FindProperty("windows").GetArrayElementAtIndex(stage).objectReferenceValue as Window;
                    MoveInto(mosquito, window.GetComponent<Collider>());
                }

                deadline = Time.realtimeSinceStartup + 10f;
                while (!tutorial.IsRoomComplete)
                {
                    Require(Time.realtimeSinceStartup < deadline, "Room " + (stage + 1) + " did not complete through its trigger.");
                    yield return null;
                }
                Check(!mosquito.gameObject.activeInHierarchy, "Successful mosquito is deactivated.");
                Check(!player.GetComponent<FirstPersonController>().enabled && !player.GetComponent<PlayerInteraction>().enabled, "Completion stops player movement and item interaction.");
                Check(Cursor.lockState == CursorLockMode.None, "Completion unlocks the cursor.");
                Check(Reference<GameObject>(setup, "mainMenuButton").activeSelf == (stage == 3), "Main Menu is offered only after the final room.");
                until = Time.time + 0.6f;
                while (Time.time < until) yield return null;
                Check(managers.Sum(m => m.ActiveMosquitoCount) == 0, "No respawn occurs after success.");
                if (stage == 3)
                {
                    ScreenCapture.CaptureScreenshot("Temp/TutorialChecks/Complete" + run + ".png");
                    until = Time.time + 0.25f;
                    while (Time.time < until) yield return null;
                }

                Button button = (setup.FindProperty("tutorialButtons").GetArrayElementAtIndex(stage).objectReferenceValue as GameObject).GetComponent<Button>();
                Canvas.ForceUpdateCanvases();
                if (stage < 3)
                {
                    Click(button);
                    Check(tutorial.CurrentStageIndex == stage + 1 && !tutorial.IsRoomComplete, "Next Room button advances exactly one room.");
                    Check(player.GetComponent<FirstPersonController>().enabled && player.GetComponent<PlayerInteraction>().enabled, "Next room restores player control.");
                }
                else
                {
                    string destination = run == 0 ? "MainMenuScene" : "FinalLevel";
                    Click(run == 0 ? Reference<GameObject>(setup, "mainMenuButton").GetComponent<Button>() : button);
                    deadline = Time.realtimeSinceStartup + 20f;
                    while (SceneManager.GetActiveScene().name != destination)
                    {
                        Require(Time.realtimeSinceStartup < deadline, "Destination button did not load " + destination);
                        yield return null;
                    }
                    Check(true, "Completion button loads " + destination);
                    Check(Cursor.lockState == (run == 0 ? CursorLockMode.None : CursorLockMode.Locked), "Destination has the correct cursor state.");
                }
            }
            if (run == 0)
            {
                SceneManager.LoadScene("Tutorial Level");
                yield return null;
            }
        }
        Check(runtimeErrors.Count == 0, "No runtime errors: " + string.Join("; ", runtimeErrors));
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            runtimeErrors.Add(message);
    }

    private static T Reference<T>(SerializedObject setup, string name) where T : UnityEngine.Object
        => setup.FindProperty(name).objectReferenceValue as T;

    private static void MoveInto(MosquitoAgent mosquito, Collider trigger)
    {
        Require(trigger != null && trigger.isTrigger, "Expected an enabled trigger collider.");
        mosquito.ResetAgent();
        mosquito.transform.position = trigger.bounds.center;
        Physics.SyncTransforms();
    }

    private static void Click(Button button)
    {
        Require(button.isActiveAndEnabled && button.interactable, "Button is unavailable.");
        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, button.GetComponent<RectTransform>().position);
        PointerEventData pointer = new PointerEventData(EventSystem.current) { position = center, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count > 0 && (hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform)), "Button is covered or unreachable through the UI raycaster.");
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    private static void Check(bool condition, string message)
    {
        Require(condition, message);
        results.Add("PASS: " + message);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
