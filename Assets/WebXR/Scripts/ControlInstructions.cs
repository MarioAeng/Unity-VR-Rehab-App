using TMPro;
using UnityEngine;

public class ControlInstructions : MonoBehaviour
{
    public TextMeshProUGUI instructionsText;

    void Start()
    {
        instructionsText.text =
            "<b>Keyboard Controls</b>\n" +
            "W/S: Move Up/Down\n" +
            "A/D: Move Left/Right\n" +
            "Z/X: Grip In/Out\n" +
            "Left Ctrl/Right Ctrl: Trigger\n" +
            "Q/E: Flexion Raise\n" +
            "F/R: Abduction Raise\n" +
            "G/T: Arm Rotate\n" +
            "Space: Arm Press\n" +
            "B: Reset Pose";
    }
}
