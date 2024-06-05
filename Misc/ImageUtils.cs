using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;
using GodotCSharpToolkit.Extensions;

/// <summary>
/// The file utils in this class work on both godot paths (res:// or user://) and absolute file paths.
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class ImageUtils
    {
        private static Dictionary<string, ImageTexture> _ImageCache = new Dictionary<string, ImageTexture>();

        // We ignore cache by default since it tends to screw up sometimes when reloading a lot
        public static Texture2D LoadTexture(string fullPath, bool useCache = true)
        {
            Texture2D texture = null;
            if (FileUtils.IsGodotPath(fullPath))
            {

                texture = ResourceLoader.Load(fullPath) as Texture2D;
                if (!GodotObject.IsInstanceValid(texture))
                {
                    texture = ResourceLoader.Load(fullPath, "", ResourceLoader.CacheMode.Ignore) as Texture2D;
                }
                if (texture == null)
                {
                    Logger.Error($"Could not load internal image '{fullPath}'");
                }
            }
            else
            {
                texture = LoadExternal(fullPath, useCache);
            }
            return texture;
        }

        public static void ClearExternalCache()
        {
            _ImageCache.Clear();
        }

        public static ImageTexture LoadExternal(string fullPath, bool useCache)
        {
            if (fullPath.IsNullOrEmpty())
            {
                Logger.Error($"Could not load external image path is empty");
                return null;
            }
            if (useCache && _ImageCache.ContainsKey(fullPath))
            {
                return _ImageCache[fullPath];
            }
            var image = new Image();
            var error = image.Load(fullPath);
            if (error != 0)
            {
                Logger.Error($"Could not load external image '{fullPath}': {error.ToString()}");
            }
            ImageTexture imageTexture = ImageTexture.CreateFromImage(image);
            if (useCache)
            {
                _ImageCache[fullPath] = imageTexture;
            }
            image = null;
            return imageTexture;
        }
    }
}