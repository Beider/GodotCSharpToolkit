
namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// Debug tools are executed as the game starts
    /// You can put all kinds of things in here that you only want to do in a developer environment.
    /// 
    /// <para>EXAMPLE 01:</para>
    /// <para>You can add new nodes to the tree and such during Initialize(). For instance if you have a UI
    /// you only want to be added to the game when you launch the game in debug mode then make it an
    /// IDebugTool and during Initialize() simply add yourself to the tree as the child of something.</para>
    /// 
    /// <para>Just note that the IDebugTool is instanced from the script, that means that if you attach the script
    /// to the root object of a scene the content of that scene will not be loaded. If you wish to load an entire scene
    /// only in debug mode then make the root script a IDebugTool and during Initialize() use:</para>
    /// 
    /// <para>var scene = ResourceLoader.Load("scene path") as PackedScene</para>
    /// <para>Node instance = scene.Instantiate();</para>
    /// <para>someNode.Addchild(instance);</para>
    /// 
    /// <para>EXAMPLE 02:</para>
    /// <para>As a second example in my own game I scan to see if I added new message types to my message system,
    /// if I have I will update a JSON file in my project directory with a unique ID for this new message type.</para>
    /// 
    /// <para>This is so I don't have to manually maintain a list of IDs, I don't want to resolve this simply by looping
    /// over types since order is not guaranteed and even sorting by name means that messages will change ID between
    /// different versions of the game.</para>
    /// 
    /// <para>For an example see DebugToolEvents located in the event system</para>
    /// </summary>
    public interface IDebugTool
    {
        /// <summary>
        /// If you need to run in the background add yourself as a node
        /// </summary>
        void Initialize();
    }
}