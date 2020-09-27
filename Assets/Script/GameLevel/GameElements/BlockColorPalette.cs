using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MainLevel.TetrisElements
{
    public static class BlockColorPalette
    {
        private static readonly Dictionary<string, Color> _colorPalette =
            new Dictionary<string, Color>
            {
                {"cian", new Color(.7f, .9f, 1f)}, {"green", new Color(.7f, 1f, .5f)},
                {"yellow", new Color(1f, .9f, .5f)}, {"pink", new Color(1f, .5f, 1f)}
            };

        public static Color GetColor(string name) => _colorPalette[name];

        public static (string, Color) GetRandomNameAndColor()
        {
            var kvp = _colorPalette.ElementAt(Random.Range(0, _colorPalette.Count));
            return (kvp.Key, kvp.Value);
        }
    }
}