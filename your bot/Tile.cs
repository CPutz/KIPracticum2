using System;
namespace Ants {
	public enum Tile { Ant, Enemy, Food, Dead, Land, Water, Unseen, Hill }

    public static class TileExtensions {

        public static QTile ToQTile(this Tile self) {
            switch (self) {
                case Tile.Ant:
                    return QTile.Friend;

                case Tile.Food:
                    return QTile.Food;

                default:
                    return QTile.None;
            }
        }
    }
}

