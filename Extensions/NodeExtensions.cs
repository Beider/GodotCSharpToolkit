using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Extensions
{
    public static class NodeExtensions
    {
        #region Networking

        /// <summary>
        /// Is the game session the server?
        /// </summary>
        /// <returns>Returns true if we are or if network is inactive, false if not</returns>
        public static bool IsServer(this Node instance)
        {
            if (!IsNetworkActive(instance))
            {
                return true;
            }
            return GetPeerId(instance) == GetServerId(instance);
        }

        /// <summary>
        /// Check if we are a client
        /// </summary>
        /// <returns>True if we are, false if not or if network is not active</returns>
        public static bool IsClient(this Node instance)
        {
            return IsNetworkActive(instance) && !IsServer(instance);
        }

        /// <summary>
        /// Is the game session started in non-standalone?
        /// </summary>
        /// <returns>True if network is active, false if not</returns>
        public static bool IsNetworkActive(this Node instance)
        {
            SceneTree Tree = instance.GetTree();
            return Tree != null && Tree.HasNetworkPeer();
        }

        /// <summary>
        /// Get the PeerId of the server
        /// </summary>
        /// <returns>The PeerID of the server</returns>
        public static int GetServerId(this Node instance)
        {
            return instance.GetNetworkMaster();
        }

        /// <summary>
        /// Gets the peer ID from the game session, 1 for server or standalone
        /// We use same for server and standalone so a server could in theory be started mid game session
        /// without resetting owner on all objects from 0 to 1
        /// </summary>
        /// <returns>The peer id</returns>
        public static int GetPeerId(this Node instance)
        {
            SceneTree Tree = instance.GetTree();
            if (Tree != null && Tree.HasNetworkPeer())
            {
                return Tree.GetNetworkUniqueId();
            }

            return 1;
        }

        #endregion

        #region General Methods
        ///
        /// Clear the children of a node
        ///
        public static void ClearChildren(this Node instance)
        {
            foreach (Node n in instance.GetChildren())
            {
                instance.RemoveChild(n);
                n.QueueFree();
            }
        }


        /// <summary>
        /// Loads a scene from the given path or default on error
        /// </summary>
        /// <param name="path">The path to the scene</param>
        /// <typeparam name="T">The type you expect back, should inherit from node</typeparam>
        /// <returns>The scene or the default value of T (usually null) if not found</returns>
        public static T LoadSceneFromPath<T>(this Node node, string path) where T : Node
        {
            PackedScene pScene = ResourceLoader.Load(path) as PackedScene;
            if (pScene == null)
            {
                GD.Print($"Could not load scene {path}");
                return default(T);
            }

            return (T)pScene.Instance();
        }

        #endregion
    }
}