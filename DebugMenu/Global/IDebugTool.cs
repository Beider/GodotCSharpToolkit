
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
/// <para>Node instance = scene.Instance();</para>
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
/// <para>I have included my own code at the bottom of this file so you can see how it could work. This code will obviously
/// not work as you will miss the classes and JSON dependencies.</para>
/// </summary>
public interface IDebugTool
{
    /// <summary>
    /// If you need to run in the background add yourself as a node
    /// </summary>
    void Initialize();
}

/*

    /// <summary>
    /// Simple tool that scans for new RecordableEvents on startup.
    /// If any are found it is added to our JSON file that lists all RecordableEvents.
    /// We do this to ensure each RecordableEvent has a consistent unique ID.
    /// </summary>
    public class DebugToolEvents : IDebugTool
    {
        private List<Type> NewEvents = new List<Type>();

        private EventIdJsonFile FileContent;

        /// <summary>
        /// Called by DebugMenu
        /// </summary>
        public void Initialize()
        {
            LoadExistingEvents();
            CheckEvents();
            SaveEvents();
        }

        /// <summary>
        /// Get all existing registered events from JSON
        /// </summary>
        private void LoadExistingEvents()
        {
            FileContent = new EventIdJsonFile();
            string fileContent = Utils.LoadTextFile(Constants.RELATIVE_JSON_EVENT_IDS_PATH + Constants.EVENT_ID_FILE_NAME);
            if (!fileContent.IsNullOrEmpty())
            {
                FileContent = (EventIdJsonFile)Utils.FromJson(fileContent, typeof(EventIdJsonFile));
            }
            else
            {
                FileContent = new EventIdJsonFile();
                FileContent.Values = new List<EventIdJsonDef>();
            }
        }

        /// <summary>
        /// Check all events in assembly to see if they exist in JSON
        /// </summary>
        private void CheckEvents()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && typeof(RecordableEvent).IsAssignableFrom(type))
                {
                    CheckEventExists(type);
                }
            }
        }

        /// <summary>
        /// check if the individual event type exists in JSON, if not add it to NewEvents list
        /// </summary>
        /// <param name="type">The type to check</param>
        private void CheckEventExists(Type type)
        {
            foreach (var value in FileContent.Values)
            {
                if (value.ClassName.Equals(type.Name))
                {
                    return;
                }
            }
            NewEvents.Add(type);
        }

        /// <summary>
        /// Save events if any new events were found
        /// </summary>
        private void SaveEvents()
        {
            GD.Print($"- {NewEvents.Count} new events found");
            if (NewEvents.Count == 0)
            {
                return;
            }

            byte currentId = GetMaxId();

            foreach (Type evnt in NewEvents)
            {
                currentId++;
                GD.Print($"- Added Event ({currentId}): {evnt.Name}");
                var eventId = new EventIdJsonDef();
                eventId.Id = currentId;
                eventId.ClassName = evnt.Name;
                FileContent.Values.Add(eventId);
            }

            string json = Utils.ToJson(FileContent);
            System.IO.File.WriteAllText(Constants.EVENT_ID_FULL_PATH, json);
        }

        /// <summary>
        /// Get max ID currently in use
        /// </summary>
        private byte GetMaxId()
        {
            byte id = 0;
            foreach (var value in FileContent.Values)
            {
                if (value.Id > id)
                {
                    id = value.Id;
                }
            }
            return id;
        }

    }

*/