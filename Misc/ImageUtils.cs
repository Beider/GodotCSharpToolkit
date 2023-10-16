using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

/// <summary>
/// The file utils in this class work on both godot paths (res:// or user://) and absolute file paths.
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class ImageUtils
    {
        public static Texture LoadTexture(string fullPath)
        {
            Texture texture = null;
            if (FileUtils.IsGodotPath(fullPath))
            {
                texture = ResourceLoader.Load(fullPath) as Texture;
                if (texture == null)
                {
                    Logger.Error($"Could not load internal image '{fullPath}'");
                }
            }
            else
            {
                texture = LoadExternal(fullPath);
            }
            return texture;
        }

        public static ImageTexture LoadExternal(string fullPath)
        {
            var image = new Image();
            var error = image.Load(fullPath);
            if (error != 0)
            {
                Logger.Error($"Could not load external image '{fullPath}': {error.ToString()}");
            }
            ImageTexture imageTexture = new ImageTexture();
            imageTexture.CreateFromImage(image);
            image = null;
            return imageTexture;
        }
    }
}