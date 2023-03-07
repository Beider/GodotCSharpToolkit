using Godot;
using System;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;
using Newtonsoft.Json;

public abstract class JsonDefWithName : IJsonDefWithName
{
    public event Action<JsonDefWithName> OnStatusChange = delegate { };

    [JsonIgnore]
    public JsonDefWithName Original;

    public abstract string GetUniqueId();

    public abstract string GetName();

    [JsonIgnore]
    public bool IsNew { get; set; } = false;

    [JsonIgnore]
    private bool _IsModified = false;
    [JsonIgnore]
    public bool IsModified
    {
        get
        {
            return _IsModified;
        }
        set
        {
            _IsModified = value;
            OnStatusChange(this);
        }
    }

    [JsonIgnore]
    private bool _IsInvalid = false;
    [JsonIgnore]
    public bool IsInvalid
    {
        get
        {
            return _IsInvalid;
        }
        set
        {
            _IsInvalid = value;
            OnStatusChange(this);
        }
    }

    [JsonIgnore]
    private bool _IsTaggedForDelete = false;
    [JsonIgnore]
    public bool IsTaggedForDelete
    {
        get
        {
            return _IsTaggedForDelete;
        }
        set
        {
            _IsTaggedForDelete = value;
            if (_IsTaggedForDelete)
            {
                _IsModified = true;
            }
            OnStatusChange(this);
        }
    }

    [JsonIgnore]
    public string SourceFile;
    public void SetSource(string file)
    {
        SourceFile = file;
    }

    public JsonDefWithName Clone(Type objectType)
    {
        var clone = (JsonDefWithName)this.CloneJsonObject(objectType);
        clone.SourceFile = SourceFile;
        clone.Original = this;
        return clone;
    }

    public string GetSource()
    {
        return SourceFile;
    }

    /// <summary>
    /// Check if this is loaded from the local drive or not
    /// </summary>
    public bool IsLocal()
    {
        return !FileUtils.IsGodotPath(SourceFile);
    }

    public string GetCategory()
    {
        var filename = SourceFile.GetFile();
        return filename.Replace($".{SourceFile.Extension()}", "");
    }
}