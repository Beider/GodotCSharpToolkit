using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

/// <summary>
/// Does node stuff, also thanks ChatGPT for making util methods easy
/// </summary>
namespace GodotCSharpToolkit.Misc
{
    public static class NodeUtils
    {
        public static void MoveNodeBelow(Node nodeToMove, Node targetNode)
        {
            if (nodeToMove == null || targetNode == null)
            {
                Logger.Error("Invalid nodes provided.");
                return;
            }

            Node parent = targetNode.GetParent();

            if (parent == null)
            {
                Logger.Error("Target node has no parent.");
                return;
            }

            parent.RemoveChild(nodeToMove);
            int targetIndex = parent.GetChildren().IndexOf(targetNode);
            parent.AddChild(nodeToMove);
            parent.MoveChild(nodeToMove, targetIndex + 1);
        }

        public static void MoveNodeAbove(Node nodeToMove, Node targetNode)
        {
            if (nodeToMove == null || targetNode == null)
            {
                Logger.Error("Invalid nodes provided.");
                return;
            }

            Node parent = targetNode.GetParent();

            if (parent == null)
            {
                Logger.Error("Target node has no parent.");
                return;
            }

            parent.RemoveChild(nodeToMove);
            int targetIndex = parent.GetChildren().IndexOf(targetNode);
            parent.AddChild(nodeToMove);
            parent.MoveChild(nodeToMove, targetIndex);
        }

        public static Node GetSiblingNodeAbove(Node currentNode)
        {
            Node parent = currentNode.GetParent();
            if (parent == null)
            {
                Logger.Error("Current node has no parent.");
                return null;
            }

            int currentIndex = parent.GetChildren().IndexOf(currentNode);
            if (currentIndex > 0)
            {
                return parent.GetChild(currentIndex - 1);
            }
            else
            {
                Logger.Warning("Current node is the first node below the parent.");
                return null;
            }
        }

        public static Node GetSiblingNodeBelow(Node currentNode)
        {
            Node parent = currentNode.GetParent();
            if (parent == null)
            {
                Logger.Error("Current node has no parent.");
                return null;
            }

            int currentIndex = parent.GetChildren().IndexOf(currentNode);
            int childCount = parent.GetChildCount();
            if (currentIndex < childCount - 1)
            {
                return parent.GetChild(currentIndex + 1);
            }
            else
            {
                Logger.Warning("Current node is the last node below the parent.");
                return null;
            }
        }
    }
}