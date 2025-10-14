using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TextAsset dialogueFile;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;

    private Dictionary<string, List<string>> dialogueSections = new();
    private List<string> currentSection;
    private int index;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public bool IsDialogueActive => dialoguePanel.activeSelf;
    public string CurrentSection { get; private set; }

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (dialogueFile != null) LoadDialogue(dialogueFile);
    }

    void Update()
    {
        if (IsDialogueActive && Input.GetKeyDown(KeyCode.T))
        {
            OnSpacePressed();
        }
    }

    public void LoadDialogue(TextAsset textFile)
    {
        dialogueSections.Clear();
        string[] lines = textFile.text.Split('\n');
        string currentKey = "Default";
        dialogueSections[currentKey] = new List<string>();

        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentKey = line.Trim('[', ']');
                if (!dialogueSections.ContainsKey(currentKey))
                    dialogueSections[currentKey] = new List<string>();
            }
            else
            {
                dialogueSections[currentKey].Add(line);
            }
        }
    }

    public void StartSection(string sectionName)
    {
        if (!dialogueSections.ContainsKey(sectionName)) return;

        CurrentSection = sectionName;
        currentSection = dialogueSections[sectionName];
        index = 0;

        if (currentSection.Count > 0)
        {
            dialoguePanel.SetActive(true);
            ShowLine();
        }
    }

    private void ShowLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentSection[index]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void OnSpacePressed()
    {
        if (isTyping)
        {
            // Finish typing instantly
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentSection[index];
            isTyping = false;
        }
        else
        {
            // Move to next line
            index++;
            if (index < currentSection.Count)
            {
                ShowLine();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
