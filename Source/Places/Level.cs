﻿using Tests.Places;
using Turnable.Utilities;

namespace Turnable.Places
{
    public class Level
    {
        public TileMap TileMap { get; set; }
        public Viewport Viewport { get; set; }

        public Level(string fullPath)
        {
            TileMap = new TileMap(fullPath);
            Viewport = new Viewport(16, 16);
        }
    }
}
