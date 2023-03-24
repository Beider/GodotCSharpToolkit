using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;
using Newtonsoft.Json;

public abstract class JsonDefWithName : IJsonDefWithName
{
    public event Action<JsonDefWithName> OnStatusChange = delegate { };

    /// <summary>
    /// Useful to drop your own values that you need for your editor or such
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, object> Metadata = null;

    [JsonIgnore]
    public JsonDefWithName Original;

    /// <summary>
    /// We assume this will be a GUID in the editors
    /// </summary>
    public abstract string GetUniqueId();

    /// <summary>
    /// We assume this will be a GUID in the editors
    /// </summary>
    public abstract void SetUniqueId(string uniqueId);
    public abstract string GetName();
    public abstract void SetName(string newName);

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
        return !FileUtils.IsResPath(SourceFile);
    }

    public string GetCategory()
    {
        var filename = SourceFile.GetFile();
        return filename.Replace($".{SourceFile.Extension()}", "");
    }

    #region Metadata
    public void AddMetadata(string key, object value)
    {
        if (Metadata == null)
        {
            Metadata = new Dictionary<string, object>();
        }
        if (Metadata.ContainsKey(key))
        {
            Metadata[key] = value;
        }
        else
        {
            Metadata.Add(key, value);
        }
    }

    public bool HasMetadata(string key)
    {
        if (Metadata == null) { return false; }
        return Metadata.ContainsKey(key);
    }

    public object GetMetadata(string key)
    {
        if (Metadata == null) { return null; }
        if (Metadata.ContainsKey(key))
        {
            return Metadata[key];
        }
        return null;
    }
    #endregion
}