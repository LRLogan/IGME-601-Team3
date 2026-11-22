using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Editor-only setup helper for the existing tutorial scene.</summary>
public static class TutorialSceneSetup
{
    private const string scenePath = "Assets/Scenes/Official levels/Tutorial Level.unity";
    private static readonly Color panelColor = new Color(0.06f, 0.09f, 0.12f, 0.94f);

    [MenuItem("Tools/Tutorial/Configure Scene %#F8")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().path != scenePath)
            throw new InvalidOperationException("Open Tutorial Level outside Play Mode first.");

        Tutorial_SM tutorial = UnityEngine.Object.FindFirstObjectByType<Tutorial_SM>();
        SerializedObject setup = new SerializedObject(tutorial);
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        Transform[] rooms = Enumerable.Range(1, 4).Select(i => roots.Single(r => r.name == "Room" + i).transform).ToArray();
        MosquitoAgent first = rooms[0].GetComponentInChildren<MosquitoAgent>(true);
        Require(first != null, "Room 1 needs its placed mosquito.");
        foreach (MosquitoAgent mosquito in UnityEngine.Object.FindObjectsByType<MosquitoAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            mosquito.gameObject.SetActive(mosquito == first);
        setup.FindProperty("firstRoomMosquito").objectReferenceValue = first;

        SerializedProperty targets = setup.FindProperty("targets");
        targets.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            MosquitoManager manager = rooms[i].GetComponentInChildren<MosquitoManager>(true);
            MosquitoSpawner spawner = setup.FindProperty("spawners").GetArrayElementAtIndex(i).objectReferenceValue as MosquitoSpawner;
            Require(manager != null && spawner != null, "Missing room manager or spawner.");
            SetReference(spawner, "mosquitoManager", manager);
            SerializedObject managerSetup = new SerializedObject(manager);
            managerSetup.FindProperty("initialPoolSize").intValue = i == 0 ? 0 : 1;
            managerSetup.ApplyModifiedPropertiesWithoutUndo();
            spawner.gameObject.SetActive(i != 0);
            TargetHealth target = manager.pointB != null ? manager.pointB.GetComponentInChildren<TargetHealth>(true) : null;
            if (i > 0) Require(target != null, "A spawned room needs a target with TargetHealth.");
            targets.GetArrayElementAtIndex(i).objectReferenceValue = target;
            if (target != null) SetReference(target, "mManager", manager);
            foreach (Window window in rooms[i].GetComponentsInChildren<Window>(true))
            {
                Rigidbody body = window.GetComponent<Rigidbody>();
                if (body != null) { body.isKinematic = true; body.useGravity = false; }
            }
        }
        foreach (MosquitoManager manager in roots.SelectMany(r => r.GetComponents<MosquitoManager>()))
            if (manager.pointB == null) manager.gameObject.SetActive(false);

        Canvas canvas = tutorial.GetComponent<Canvas>();
        canvas.transform.localScale = Vector3.one;
        canvas.sortingOrder = 10;
        TMP_FontAsset font = tutorial.GetComponentsInChildren<TMP_Text>(true).First().font;
        RectTransform objectivePanel = Panel("ObjectivePanel", canvas.transform, new Vector2(0.08f, 0.77f), new Vector2(0.92f, 0.97f));
        TMP_Text objective = Text("ObjectiveText", objectivePanel, font, 30, new Vector2(0, 0.32f), Vector2.one);
        objective.text = "ROOM 1 / 4 - LEARN TO PLACE ITEMS\nThis mosquito stays still so you can practise. Place a fan to push it out the window.";
        TMP_Text feedback = Text("FeedbackText", objectivePanel, font, 24, Vector2.zero, new Vector2(1, 0.32f));
        feedback.color = new Color(1, 0.78f, 0.35f);

        RectTransform completion = Panel("CompletionPanel", canvas.transform, new Vector2(0.24f, 0.3f), new Vector2(0.76f, 0.7f));
        completion.GetComponent<Image>().raycastTarget = true;
        TMP_Text completionText = Text("CompletionText", completion, font, 32, new Vector2(0, 0.48f), Vector2.one);
        completionText.alignment = TextAlignmentOptions.Center;

        for (int i = 0; i < 4; i++)
        {
            GameObject buttonObject = setup.FindProperty("tutorialButtons").GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(completion, false);
            rect.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.28f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(380, 64);
            rect.anchoredPosition = new Vector2(0, i == 3 ? 25 : 0);
            TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
            label.text = i == 3 ? "Play Final Level" : "Next Room";
            label.fontSize = 28;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20;
            label.fontSizeMax = 28;
            label.raycastTarget = false;
            buttonObject.SetActive(false);
        }

        GameObject finalButton = setup.FindProperty("tutorialButtons").GetArrayElementAtIndex(3).objectReferenceValue as GameObject;
        GameObject menuButton = UnityEngine.Object.Instantiate(finalButton, completion);
        menuButton.name = "TutorialMainMenuButton";
        menuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -55);
        menuButton.GetComponentInChildren<TMP_Text>(true).text = "Main Menu";
        Button menu = menuButton.GetComponent<Button>();
        while (menu.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(menu.onClick, 0);
        UnityEventTools.AddPersistentListener(menu.onClick, tutorial.ToMainMenu);

        RectTransform controlsPanel = Panel("ControlsPanel", canvas.transform, new Vector2(0.18f, 0.025f), new Vector2(0.82f, 0.095f));
        TMP_Text controls = Text("ControlsText", controlsPanel, font, 24, Vector2.zero, Vector2.one);
        controls.alignment = TextAlignmentOptions.Center;
        controls.text = "WASD: move   |   E: pick up / place   |   Scroll: rotate   |   Esc: pause";

        setup.FindProperty("objectiveText").objectReferenceValue = objective;
        setup.FindProperty("feedbackText").objectReferenceValue = feedback;
        setup.FindProperty("completionPanel").objectReferenceValue = completion.gameObject;
        setup.FindProperty("completionText").objectReferenceValue = completionText;
        setup.FindProperty("mainMenuButton").objectReferenceValue = menuButton;
        setup.FindProperty("pauseMenu").objectReferenceValue = UnityEngine.Object.FindFirstObjectByType<PauseMenuController>();
        setup.ApplyModifiedPropertiesWithoutUndo();
        completion.gameObject.SetActive(false);
        EventSystem[] events = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        Require(events.Length == 1 && events[0].GetComponent<InputSystemUIInputModule>() != null, "Expected one Input System EventSystem.");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        File.WriteAllText("Temp/TutorialSetup.txt", "Tutorial scene configured successfully.\n" + string.Join("\n", rooms.Select(r => r.name + ": " + string.Join(", ", r.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c is MosquitoManager || c is MosquitoSpawner || c is TargetHealth || c is MosquitoAgent).Select(c => c.name + " / " + c.GetType().Name)))));
        Debug.Log("Tutorial scene configured successfully.");
    }

    private static void SetReference(UnityEngine.Object owner, string field, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(owner);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        if (owner is Component component && PrefabUtility.IsPartOfPrefabInstance(component))
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }

    private static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = panelColor;
        panel.GetComponent<Image>().raycastTarget = false;
        return rect;
    }

    private static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, float size, Vector2 min, Vector2 max)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.rectTransform.SetParent(parent, false);
        text.rectTransform.anchorMin = min;
        text.rectTransform.anchorMax = max;
        text.rectTransform.offsetMin = new Vector2(24, 8);
        text.rectTransform.offsetMax = new Vector2(-24, -8);
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = string.Empty;
        return text;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
