using Godot;
using System;

[Tool]
public partial class ResourcePicker : PanelContainer
{
    [Export] private NodePath AddonResourcePickerPath;
    [Export] private NodePath SpinBoxPath;

    private EditorResourcePicker _editorResourcePicker;
    private SpinBox _spinBox;

    public override void _Ready()
    {
        _editorResourcePicker = GetNode<EditorResourcePicker>(AddonResourcePickerPath);
        _spinBox = GetNode<SpinBox>(SpinBoxPath);
    }

    public void Delete()
    {
        this.QueueFree();
    }

    public string GetResourcePath()
    {
        return _editorResourcePicker.EditedResource.ResourcePath;
    }

    public void SetResourcePath(string path)
    {
        _editorResourcePicker.EditedResource = ResourceLoader.Load(path);
    }

    public int GetProbability()
    {
        return (int)_spinBox.Value;
    }

    public void SetProbability(int probability)
    {
        _spinBox.Value = probability;
    }
}
