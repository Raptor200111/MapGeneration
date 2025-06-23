using Godot;
using System;

[Tool]
public static class JsonConfigIO
{
    public static bool Save(string filePath, Variant data)
    {
        string dir = filePath.GetBaseDir();
        if (!DirAccess.DirExistsAbsolute(dir))
            DirAccess.MakeDirRecursiveAbsolute(dir);

        string json = Json.Stringify(data, "\t");

        using var f = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (f == null)
        {
            GD.PrintErr($"[JsonConfigIO] No se pudo abrir {filePath}.");
            return false;
        }
        f.StoreString(json);

#if TOOLS
        var fs = EditorInterface.Singleton?.GetResourceFilesystem();
        if (fs != null)
        {
            if (!fs.IsScanning())
                fs.CallDeferred("update_file", filePath);
            else
            {
                void OnScanFinished()
                {
                    fs.UpdateFile(filePath);
                    fs.FilesystemChanged -= OnScanFinished;
                }
                fs.FilesystemChanged += OnScanFinished;
            }
        }
#endif
        return true;
    }

    public static Variant Load(string filePath)
    {
        if (!FileAccess.FileExists(filePath))
            return default;

        string raw = FileAccess.GetFileAsString(filePath);
        Variant v = Json.ParseString(raw);

        if (v.VariantType == Variant.Type.Nil)
            GD.PrintErr($"[JsonConfigIO] JSON inválido en '{filePath}'.");

        return v;
    }

    private static EditorFileDialog _dialog;
    private static bool _pendingSave;
    private static Func<Variant> _getter;
    private static Action<Variant> _setter;

    public static void ShowSaveDialog(Func<Variant> getter)
    {
        _pendingSave = true;
        _getter = getter;
        _setter = null;
        PrepareDialog(EditorFileDialog.FileModeEnum.SaveFile);
    }

    public static void ShowLoadDialog(Action<Variant> setter)
    {
        _pendingSave = false;
        _getter = null;
        _setter = setter;
        PrepareDialog(EditorFileDialog.FileModeEnum.OpenFile);
    }

    private static void PrepareDialog(EditorFileDialog.FileModeEnum mode)
    {
        if (_dialog == null)
        {
            _dialog = new EditorFileDialog
            {
                Access = EditorFileDialog.AccessEnum.Filesystem,
                Filters = new[] { "*.json ; JSON config files" }
            };
            _dialog.FileSelected += OnPathChosen;

            EditorInterface.Singleton.GetBaseControl().AddChild(_dialog);
        }
        _dialog.FileMode = mode;
        _dialog.CurrentPath = "res://";
        _dialog.PopupCentered();
    }

    private static void OnPathChosen(string path)
    {
        if (!path.EndsWith(".json"))
            path += ".json";

        if (_pendingSave && _getter != null)
        {
            Save(path, _getter.Invoke());
        }
        else if (!_pendingSave && _setter != null)
        {
            Variant v = Load(path);
            if (v.VariantType != Variant.Type.Nil)
                _setter.Invoke(v);
        }
    }
}
