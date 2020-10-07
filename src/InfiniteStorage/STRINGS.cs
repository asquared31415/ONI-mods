namespace InfiniteStorage
{
    public static class STRINGS
    {
        public static class BUILDINGS
        {
            public static class PREFABS
            {
                public static class INFSTORAGE_INFINITEITEMSTORAGE
                {
                    public static LocString NAME = "Infinite Storage Unit";

                    // This one goes on the bottom
                    public static LocString DESC =
                        "Just as before, they asked \"What if we could go smaller?\", ignoring the laws of physics.\n" +
                        "And once again, they discovered that these \"laws\" of physics can be thought of as mere suggestions.";

                    // This one goes on the top
                    public static LocString EFFECT =
                        "Stores an infinite amount of the solid resources of your choosing.";
                }

                public static class INFSTORAGE_INFINITELIQUIDSTORAGE
                {
                    public static LocString NAME = "Infinite Liquid Storage Unit";

                    // This one goes on the bottom
                    public static LocString DESC =
                        "Just as before, they asked \"What if we could go smaller?\", ignoring the laws of physics.\n" +
                        "And once again, they discovered that these \"laws\" of physics can be thought of as mere suggestions.";

                    // This one goes on the top
                    public static LocString EFFECT =
                        "Stores an infinite amount of the liquid resources of your choosing.";
                }

                public static class INFSTORAGE_INFINITEGASSTORAGE
                {
                    public static LocString NAME = "Infinite Gas Storage Unit";

                    // This one goes on the bottom
                    public static LocString DESC =
                        "Just as before, they asked \"What if we could go smaller?\", ignoring the laws of physics.\n" +
                        "And once again, they discovered that these \"laws\" of physics can be thought of as mere suggestions.";

                    // This one goes on the top
                    public static LocString EFFECT =
                        "Stores an infinite amount of the gaseous resources of your choosing.";
                }
            }
        }

        public static class UI
        {
            public static class SHOWHIDE_CONTENTS
            {
                public static LocString SHOW = "Show Contents";
                public static LocString HIDE = "Hide Contents";

                public static LocString SHOW_TOOLTIP =
                    $"Show the contents of the {BUILDINGS.PREFABS.INFSTORAGE_INFINITEITEMSTORAGE.NAME}.\n<b><color=#FF0000>Warning! MAY LAG!</color></b>";

                public static LocString HIDE_TOOLTIP =
                    $"Hide the contents of the {BUILDINGS.PREFABS.INFSTORAGE_INFINITEITEMSTORAGE.NAME}.";
            }
        }
    }
}
