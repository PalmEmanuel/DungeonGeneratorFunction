using Newtonsoft.Json;
using PipeHow.DungeonGenerator.RandomDungeon.Dungeondraft;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PipeHow.DungeonMastery.RandomDungeon.Dungeondraft
{
    public static class DungeondraftExtensions
    {
        public static string ToDungeondraftMap(this IDungeon dungeon)
        {
            DungeondraftMap dungeondraftMap = new DungeondraftMap();

            var allCorners = dungeon.Map.SelectMany(r => r).Where(t => dungeon.IsCorner(t) || t.TileType == TileType.WallCross);

            // Create a list of lists to sort corners outlining the map in the right order, to allow for proper wall vector creation in Dungeondraft
            List<List<ITile>> cornerLists = new List<List<ITile>> {
                // Each list will contain one connected room or section
                new List<ITile>
                {
                    // Start from a top left corner, which one doesn't matter
                    allCorners.First(t => t.TileType == TileType.WallCornerUpperLeft)
                }
            };

            // Start searching to the right
            Func<ITile, ITile> searchMethod = dungeon.RightOf;
            bool clockwise = true;

            // Loop until we found all corners on map
            bool foundAllCorners = false;
            while (!foundAllCorners)
            {
                var currentCornerList = cornerLists.Last();
                var lastCorner = currentCornerList.Last();

                // Search clockwise
                switch (lastCorner.TileType)
                {
                    case TileType.WallCross:
                        // If the wall we found is a cross, keep searching but invert the next direction change
                        clockwise = !clockwise;
                        break;
                    case TileType.WallCornerUpperRight:
                        searchMethod = clockwise ? (Func <ITile, ITile>)dungeon.Below : dungeon.LeftOf;
                        break;
                    case TileType.WallCornerUpperLeft:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.RightOf : dungeon.Below;
                        break;
                    case TileType.WallCornerLowerRight:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.LeftOf : dungeon.Above;
                        break;
                    case TileType.WallCornerLowerLeft:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.Above : dungeon.RightOf;
                        break;
                    case TileType.WallCornerInnerUpperRight:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.LeftOf : dungeon.Below;
                        break;
                    case TileType.WallCornerInnerUpperLeft:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.Below : dungeon.RightOf;
                        break;
                    case TileType.WallCornerInnerLowerRight:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.Above : dungeon.LeftOf;
                        break;
                    case TileType.WallCornerInnerLowerLeft:
                        searchMethod = clockwise ? (Func<ITile, ITile>)dungeon.RightOf : dungeon.Above;
                        break;
                    default:
                        throw new ArgumentException("Not a valid corner type!");
                }

                // Loop through walls until we found the next corner to add to list
                var currentTile = lastCorner;
                bool foundNextCorner = false;
                while (!foundNextCorner)
                {
                    currentTile = searchMethod.Invoke(currentTile);
                    foundNextCorner = dungeon.IsCorner(currentTile) || currentTile.TileType == TileType.WallCross;
                }

                // Add the corner if it's not in the list
                // or if it's a central wall cross corner and it's only in the list once
                if (!currentCornerList.Any(t => t.Id == currentTile.Id) || currentTile.TileType == TileType.WallCross && currentCornerList.Count(t => t.Id == currentTile.Id) == 1)
                {
                    currentCornerList.Add(currentTile);
                } // If the corner is in the list, we found all corners of the current room or section
                else
                {
                    // Compare amount of corners in lists to see if we found them all
                    var foundCornerTiles = cornerLists.SelectMany(l => l.Select(t => t));
                    foundAllCorners = !allCorners.Any(c => !foundCornerTiles.Any(t => t.Id == c.Id));
                    if (!foundAllCorners)
                    {
                        // If we didn't find all corners, pick from all corners a corner which is not in the ordered corner list
                        // This corner is a corner of a section not connected to the previous room, or we would have found it
                        cornerLists.Add(new List<ITile> {
                            allCorners.First(t => !cornerLists.SelectMany(l => l.Select(t => t)).Any(c => c.Id == t.Id))
                        });
                        clockwise = true;
                    }
                }
            }
            
            // Create a list of each room's corner positions in the correct order for vector drawing
            List<string> polygonStringList = cornerLists.Select(list => string.Format("PoolVector2Array( {0} )", string.Join(", ", list.Select(t => $"{t.X * 256}, {t.Y * 256}")))).ToList();

            DateTime now = DateTime.Now;
            return JsonConvert.SerializeObject(new DungeondraftMap
            {
                Header = new DungeondraftHeader
                {
                    CreationBuild = "Generated by DungeonMastery @PalmEmanuel",
                    CreationDate = new DungeondraftHeaderDate
                    {
                        Year = now.Year,
                        Month = now.Month,
                        Day = now.Day,
                        Weekday = (int)now.DayOfWeek,
                        DST = false,
                        Hour = now.Hour,
                        Minute = now.Minute,
                        Second = now.Second
                    },
                    UsesDefaultAssets = true,
                    AssetManifest = new List<object>(),
                    EditorState = new DungeonDraftHeaderEditorState
                    {
                        CurrentLevel = 0,
                        CameraPosition = "Vector2( 6317.93, 4037.73 )",
                        CameraZoom = 8,
                        GuidePosition = "null",
                        TraceImage = null,
                        ColorPalettes = new DungeondraftHeaderColorPalettes
                        {
                            ObjectCustomColors = new List<string>
                            {
                                "ff6b3834",
                                "ffac584c",
                                "ff885848",
                                "ffc0866c",
                                "ff8d6d58",
                                "fff3a768",
                                "ff685848",
                                "ff9c8868",
                                "ffae9254",
                                "ffd8c888",
                                "ff888868",
                                "ffaab478",
                                "ff92aa58",
                                "ff87a868",
                                "ff679865",
                                "ff789868",
                                "ff546d56",
                                "ff68887c",
                                "ff667878",
                                "ff809dab",
                                "ff61788d",
                                "ff535869",
                                "ff786878",
                                "ff886878",
                                "ff905868",
                                "ff994858",
                                "ffd8d8d8",
                                "ff8a8a8a",
                                "ff585858",
                                "ff282828"
                            },
                            ScatterCustomColors = new List<string>
                            {
                                "ff6b3834",
                                "ffac584c",
                                "ff885848",
                                "ffc0866c",
                                "ff8d6d58",
                                "fff3a768",
                                "ff685848",
                                "ff9c8868",
                                "ffae9254",
                                "ffd8c888",
                                "ff888868",
                                "ffaab478",
                                "ff92aa58",
                                "ff87a868",
                                "ff679865",
                                "ff789868",
                                "ff546d56",
                                "ff68887c",
                                "ff667878",
                                "ff809dab",
                                "ff61788d",
                                "ff535869",
                                "ff786878",
                                "ff886878",
                                "ff905868",
                                "ff994858",
                                "ffd8d8d8",
                                "ff8a8a8a",
                                "ff585858",
                                "ff282828"
                            },
                            LightColors = new List<string>
                            {
                                "ffeccd8b",
                                "ffeaefca",
                                "ff80beff",
                                "ffffad58",
                                "ff4dd569"
                            },
                            GridColors = new List<string>
                            {
                                "7fffffff",
                                "7fcccccc",
                                "7f333333",
                                "7f000000"
                            },
                            DeepWaterColors = new List<string>
                            {
                                "ff3aa19a",
                                "ff8bceb0",
                                "ffffcc55",
                                "ff3c8ab8"
                            },
                            ShallowWaterColors = new List<string>
                            {
                                "ff3ac3b2",
                                "ff8bceb0",
                                "ffcc5555",
                                "ff54c1da"
                            },
                            CaveGroundColors = new List<string>
                            {
                                "ff7f7e71"
                            },
                            CaveWallColors = new List<string>
                            {
                                "ff7f7e71"
                            }
                        },
                        ObjectTagsMemory = new DungeondraftHeaderTagsMemory
                        {
                            Set = 0,
                            Tags = new List<object>()
                        },
                        ScatterTagsMemory = new DungeondraftHeaderTagsMemory
                        {
                            Set = 0,
                            Tags = new List<object>()
                        },
                        ObjectLibraryMemory = null,
                        ScatterLibraryMemory = null
                    }
                },
                World = new DungeondraftWorld
                {
                    Format = 3,
                    Width = dungeon.Width,
                    Height = dungeon.Height,
                    NextNodeId = "23", // TODO: What does this represent?
                    NextPrefabId = "0",
                    MSI = new DungeondraftWorldMSI
                    {
                        OffsetMapSize = 512,
                        MaxOffsetDistance = 0.2f,
                        CellSize = 64,
                        Seed = "6098059e" // TODO: What does this seed do, and should we base it on something?
                    },
                    Grid = new DungeondraftWorldGrid
                    {
                        Color = "7f000000"
                    },
                    Embedded = new object(),
                    Levels = new DungeondraftWorldLevelContainer
                    {
                        Zero = new DungeondraftWorldLevel
                        {
                            Label = "Ground",
                            Environment = new DungeondraftWorldLevelEnvironment
                            {
                                BakedLighting = true,
                                AmbientLight = "ffffffff"
                            },
                            Layers = new DungeondraftWorldLevelLayers
                            {
                                MinusFourHundred = "Below Ground",
                                MinusOneHundred = "Below Water",
                                OneHundred = "User Layer 1",
                                TwoHundred = "User Layer 2",
                                ThreeHundred = "User Layer 3",
                                FourHundred = "User Layer 4",
                                SevenHundred = "Above Walls",
                                NineHundred = "Above Roofs"
                            },
                            Shapes = new DungeondraftWorldLevelShapes
                            {
                                Polygons = polygonStringList,
                                Walls = cornerLists.Select(list => list.Count() / 2).ToList() // TODO: May only be correct for the first index?
                            },
                            Tiles = new DungeondraftWorldLevelTiles
                            {
                                Cells = string.Format("PoolIntArray( {0} )", string.Join(", ", dungeon.Map.SelectMany(r => r.Select(t => t.TileType == TileType.Floor ? 1 : -1)))),
                                Colors = dungeon.Map.SelectMany(r => r.Select(t => t.TileType == TileType.Floor ? "ffd0d0d0" : "ffffffff")).ToList() // TODO: Floor does not appear where it should
                            },
                            Patterns = new List<object>(),
                            Walls = cornerLists.Select(list =>
                            {
                                return new DungeondraftWorldLevelWall
                                {
                                    Points = string.Format("PoolVector2Array( {0} )", string.Join(", ", list.Select(t => $"{t.X * 256}, {t.Y * 256}"))),
                                    Texture = "res://textures/walls/stone.png",
                                    Color = "ff605f58",
                                    Loop = true,
                                    Type = 0,
                                    Joint = 1,
                                    NormalizeUV = true,
                                    Shadow = true,
                                    NodeId = $"{22 + cornerLists.IndexOf(list)}", // TODO: Not sure about this id
                                    Portals = new List<object>()
                                };
                            }).ToList(),
                            Portals = new List<object>(),
                            Cave = new DungeondraftWorldLevelCave
                            {
                                Bitmap = string.Format("PoolByteArray( {0} )", string.Join(", ", Enumerable.Repeat("0", dungeon.Width * dungeon.Height * 2 + dungeon.Width * 3 + 2))), // I don't know why the formula looks like this
                                GroundColor = "ff7f7e71",
                                WallColor = "ff7f7e71",
                                EntranceBitmap = string.Format("PoolByteArray( {0} )", string.Join(", ", Enumerable.Repeat("0", dungeon.Width * dungeon.Height * 2 + dungeon.Width * 3 + 2))) // TODO: Check if it also works when width and height is different
                            },
                            Terrain = new DungeondraftWorldLevelTerrain
                            {
                                Enabled = true,
                                ExpandSlots = false,
                                Texture1 = "res://textures/terrain/terrain_dirt.png",
                                Texture2 = "res://textures/terrain/terrain_gravel.png",
                                Texture3 = "res://textures/terrain/terrain_sand.png",
                                Texture4 = "res://textures/terrain/terrain_snow.png",
                                Splat = string.Format("PoolByteArray( {0} )", string.Join(", ", Enumerable.Repeat("255, 0, 0, 0", dungeon.Width * dungeon.Height * 2 * 8)))
                            },
                            Water = new object(),
                            Materials = new object(),
                            Paths = new List<object>(),
                            Objects = new List<object>(),
                            Lights = new List<object>(),
                            Roofs = new DungeondraftWorldLevelRoofs
                            {
                                Shade = true,
                                ShadeContrast = 0.5f,
                                SunDirection = 45,
                                Roofs = new List<object>()
                            },
                            Texts = new List<object>()
                        }
                    }
                }
            }, Formatting.Indented);
        }
    }
}
