using Newtonsoft.Json;
using System.Collections.Generic;

namespace PipeHow.DungeonGenerator.RandomDungeon.Dungeondraft
{
    internal class DungeondraftMap
    {
        [JsonProperty("header")]
        internal DungeondraftHeader Header { get; set; }
        [JsonProperty("world")]
        internal DungeondraftWorld World { get; set; }
    }

    internal class DungeondraftWorld
    {
        [JsonProperty("format")]
        internal int Format { get; set; }
        [JsonProperty("width")]
        internal int Width { get; set; }
        [JsonProperty("height")]
        internal int Height { get; set; }
        [JsonProperty("next_node_id")]
        internal string NextNodeId { get; set; }
        [JsonProperty("next_prefab_id")]
        internal string NextPrefabId { get; set; }
        [JsonProperty("msi")]
        internal DungeondraftWorldMSI MSI { get; set; }
        [JsonProperty("grid")]
        internal DungeondraftWorldGrid Grid { get; set; }
        [JsonProperty("embedded")]
        internal object Embedded { get; set; }
        [JsonProperty("levels")]
        internal DungeondraftWorldLevelContainer Levels { get; set; }
    }

    internal class DungeondraftWorldLevelContainer
    {
        [JsonProperty("0")]
        internal DungeondraftWorldLevel Zero { get; set; }
    }

    internal class DungeondraftWorldLevel
    {
        [JsonProperty("label")]
        internal string Label { get; set; }
        [JsonProperty("environment")]
        internal DungeondraftWorldLevelEnvironment Environment { get; set; }
        [JsonProperty("layers")]
        internal DungeondraftWorldLevelLayers Layers { get; set; }
        [JsonProperty("shapes")]
        internal DungeondraftWorldLevelShapes Shapes { get; set; }
        [JsonProperty("tiles")]
        internal DungeondraftWorldLevelTiles Tiles { get; set; }
        [JsonProperty("patterns")]
        internal List<object> Patterns { get; set; }
        [JsonProperty("walls")]
        internal List<DungeondraftWorldLevelWall> Walls { get; set; }
        [JsonProperty("portals")]
        internal List<object> Portals { get; set; }
        [JsonProperty("cave")]
        internal DungeondraftWorldLevelCave Cave { get; set; }
        [JsonProperty("terrain")]
        internal DungeondraftWorldLevelTerrain Terrain { get; set; }
        [JsonProperty("water")]
        internal object Water { get; set; }
        [JsonProperty("materials")]
        internal object Materials { get; set; }
        [JsonProperty("paths")]
        internal List<object> Paths { get; set; }
        [JsonProperty("objects")]
        internal List<object> Objects { get; set; }
        [JsonProperty("lights")]
        internal List<object> Lights { get; set; }
        [JsonProperty("roofs")]
        internal DungeondraftWorldLevelRoofs Roofs { get; set; }
        [JsonProperty("texts")]
        internal List<object> Texts { get; set; }
    }

    internal class DungeondraftWorldLevelRoofs
    {
        [JsonProperty("shade")]
        internal bool Shade { get; set; }
        [JsonProperty("shade_contrast")]
        internal float ShadeContrast { get; set; }
        [JsonProperty("sun_direction")]
        internal int SunDirection { get; set; }
        [JsonProperty("roofs")]
        internal List<object> Roofs { get; set; }
    }

    internal class DungeondraftWorldLevelTerrain
    {
        [JsonProperty("enabled")]
        internal bool Enabled { get; set; }
        [JsonProperty("expand_slots")]
        internal bool ExpandSlots { get; set; }
        [JsonProperty("texture_1")]
        internal string Texture1 { get; set; }
        [JsonProperty("texture_2")]
        internal string Texture2 { get; set; }
        [JsonProperty("texture_3")]
        internal string Texture3 { get; set; }
        [JsonProperty("texture_4")]
        internal string Texture4 { get; set; }
        [JsonProperty("splat")]
        internal string Splat { get; set; }
    }

    internal class DungeondraftWorldLevelCave
    {
        [JsonProperty("bitmap")]
        internal string Bitmap { get; set; }
        [JsonProperty("ground_color")]
        internal string GroundColor { get; set; }
        [JsonProperty("wall_color")]
        internal string WallColor { get; set; }
        [JsonProperty("entrance_bitmap")]
        internal string EntranceBitmap { get; set; }
    }

    internal class DungeondraftWorldLevelWall
    {
        [JsonProperty("points")]
        internal string Points { get; set; }
        [JsonProperty("texture")]
        internal string Texture { get; set; }
        [JsonProperty("color")]
        internal string Color { get; set; }
        [JsonProperty("loop")]
        internal bool Loop { get; set; }
        [JsonProperty("type")]
        internal int Type { get; set; }
        [JsonProperty("joint")]
        internal int Joint { get; set; }
        [JsonProperty("normalize_uv")]
        internal bool NormalizeUV { get; set; }
        [JsonProperty("shadow")]
        internal bool Shadow { get; set; }
        [JsonProperty("node_id")]
        internal string NodeId { get; set; }
        [JsonProperty("portals")]
        internal List<object> Portals { get; set; }
    }

    internal class DungeondraftWorldLevelTiles
    {
        [JsonProperty("cells")]
        internal string Cells { get; set; }
        [JsonProperty("colors")]
        internal List<string> Colors { get; set; }
    }

    internal class DungeondraftWorldLevelShapes
    {
        [JsonProperty("polygons")]
        internal List<string> Polygons { get; set; }
        [JsonProperty("walls")]
        internal List<int> Walls { get; set; }
    }

    internal class DungeondraftWorldLevelLayers
    {
        [JsonProperty("-400")]
        internal string MinusFourHundred { get; set; }
        [JsonProperty("-100")]
        internal string MinusOneHundred { get; set; }
        [JsonProperty("100")]
        internal string OneHundred { get; set; }
        [JsonProperty("200")]
        internal string TwoHundred { get; set; }
        [JsonProperty("300")]
        internal string ThreeHundred { get; set; }
        [JsonProperty("400")]
        internal string FourHundred { get; set; }
        [JsonProperty("700")]
        internal string SevenHundred { get; set; }
        [JsonProperty("900")]
        internal string NineHundred { get; set; }
    }

    internal class DungeondraftWorldLevelEnvironment
    {
        [JsonProperty("baked_lighting")]
        internal bool BakedLighting { get; set; }
        [JsonProperty("ambient_light")]
        internal string AmbientLight { get; set; }
    }

    internal class DungeondraftWorldGrid
    {
        [JsonProperty("color")]
        internal string Color { get; set; }
    }

    internal class DungeondraftWorldMSI
    {
        [JsonProperty("offset_map_size")]
        internal int OffsetMapSize { get; set; }
        [JsonProperty("max_offset_distance")]
        internal float MaxOffsetDistance { get; set; }
        [JsonProperty("cell_size")]
        internal int CellSize { get; set; }
        [JsonProperty("seed")]
        internal string Seed { get; set; }
    }

    internal class DungeondraftHeader
    {
        [JsonProperty("creation_build")]
        internal string CreationBuild { get; set; }
        [JsonProperty("creation_date")]
        internal DungeondraftHeaderDate CreationDate { get; set; }
        [JsonProperty("uses_default_assets")]
        internal bool UsesDefaultAssets { get; set; }
        [JsonProperty("asset_manifest")]
        internal List<object> AssetManifest { get; set; }
        [JsonProperty("editor_state")]
        internal DungeonDraftHeaderEditorState EditorState { get; set; }
    }

    internal class DungeonDraftHeaderEditorState
    {
        [JsonProperty("current_level")]
        internal int CurrentLevel { get; set; }
        [JsonProperty("camera_position")]
        internal string CameraPosition { get; set; }
        [JsonProperty("camera_zoom")]
        internal int CameraZoom { get; set; }
        [JsonProperty("guide_position")]
        internal string GuidePosition { get; set; }
        [JsonProperty("trace_image")]
        internal string TraceImage { get; set; }
        [JsonProperty("color_palettes")]
        internal DungeondraftHeaderColorPalettes ColorPalettes { get; set; }
        [JsonProperty("object_tags_memory")]
        internal DungeondraftHeaderTagsMemory ObjectTagsMemory { get; set; }
        [JsonProperty("scatter_tags_memory")]
        internal DungeondraftHeaderTagsMemory ScatterTagsMemory { get; set; }
        [JsonProperty("object_library_memory")]
        internal object ObjectLibraryMemory { get; set; }
        [JsonProperty("scatter_library_memory")]
        internal object ScatterLibraryMemory { get; set; }
    }

    internal class DungeondraftHeaderTagsMemory
    {
        [JsonProperty("set")]
        internal int Set { get; set; }
        [JsonProperty("tags")]
        internal List<object> Tags { get; set; }
    }

    internal class DungeondraftHeaderColorPalettes
    {
        [JsonProperty("object_custom_colors")]
        internal List<string> ObjectCustomColors { get; set; }
        [JsonProperty("scatter_custom_colors")]
        internal List<string> ScatterCustomColors { get; set; }
        [JsonProperty("light_colors")]
        internal List<string> LightColors { get; set; }
        [JsonProperty("grid_colors")]
        internal List<string> GridColors { get; set; }
        [JsonProperty("deep_water_colors")]
        internal List<string> DeepWaterColors { get; set; }
        [JsonProperty("shallow_water_colors")]
        internal List<string> ShallowWaterColors { get; set; }
        [JsonProperty("cave_ground_colors")]
        internal List<string> CaveGroundColors { get; set; }
        [JsonProperty("cave_wall_colors")]
        internal List<string> CaveWallColors { get; set; }
    }

    internal class DungeondraftHeaderDate
    {
        [JsonProperty("year")]
        internal int Year { get; set; }
        [JsonProperty("month")]
        internal int Month { get; set; }
        [JsonProperty("day")]
        internal int Day { get; set; }
        [JsonProperty("weekday")]
        internal int Weekday { get; set; }
        [JsonProperty("dst")]
        internal bool DST { get; set; }
        [JsonProperty("hour")]
        internal int Hour { get; set; }
        [JsonProperty("minute")]
        internal int Minute { get; set; }
        [JsonProperty("second")]
        internal int Second { get; set; }
    }
}
