using UnityEngine;

[System.Serializable]
public struct UIElementHighlighterBinding
{
    public KeyCode mainKey;
    public int mouseButton; // -1 means keyboard binding
    public bool ctrl;
    public bool shift;
    public bool alt;

    public bool IsMouse => mouseButton >= 0;

    public override string ToString()
    {
        string mods = "";
        if (ctrl) mods += "Ctrl + ";
        if (shift) mods += "Shift + ";
        if (alt) mods += "Alt + ";

        string main = IsMouse ? $"Mouse{mouseButton + 1}" : mainKey.ToString();
        return $"{mods}{main}";
    }
}