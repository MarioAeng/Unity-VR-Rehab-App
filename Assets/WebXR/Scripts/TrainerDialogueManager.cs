using UnityEngine;
using TMPro;
using System.Collections;

namespace WebXR.Scripts
{
    public class TrainerDialogueManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI dialogueText;

        [Header("Dialogue Settings")]
        [TextArea(2, 5)] public string[] dialogueLines;
        public float textDisplayTime = 3f;

        protected int currentLineIndex = 0;
        protected bool isSpeaking = false;

        protected virtual void Start()
        {
            if (dialogueText == null)
            {
                Debug.LogError("TrainerDialogueManager: No TextMeshProUGUI assigned.");
                return;
            }

            StartCoroutine(DisplayNextLine());
        }

        protected virtual IEnumerator DisplayNextLine()
        {
            while (currentLineIndex < dialogueLines.Length)
            {
                isSpeaking = true;
                dialogueText.text = dialogueLines[currentLineIndex];
                yield return new WaitForSeconds(textDisplayTime);

                OnLineFinished(dialogueLines[currentLineIndex]);

                currentLineIndex++;
                isSpeaking = false;

                yield return new WaitUntil(() => CanContinue());
            }

            OnDialogueComplete();
        }

        protected virtual void OnLineFinished(string line) { }

        protected virtual bool CanContinue() => true;

        protected virtual void OnDialogueComplete()
        {
            dialogueText.text = "";
            Debug.Log("Trainer dialogue completed.");
        }
    }
}