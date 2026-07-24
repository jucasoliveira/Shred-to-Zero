using UnityEditor;
using UnityEngine;

namespace ShredToZero.EditorTools
{
    /// <summary>
    /// Applies pixel-art import settings automatically to every texture under
    /// Assets/Art/Sprites. Unity calls this on import, so dropping a PNG into that
    /// folder gives you crisp, unfiltered, uncompressed sprites with no manual
    /// Inspector fiddling — and it stays correct for every future asset.
    ///
    /// Only runs on first import (when the asset has no meta yet), so any deliberate
    /// per-asset tweaks you make later are preserved.
    /// </summary>
    public class PixelArtImportSettings : AssetPostprocessor
    {
        private const string PixelArtFolder = "Assets/Art/Sprites";
        private const int PixelsPerUnit = 100;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(PixelArtFolder)) return;

            var importer = (TextureImporter)assetImporter;

            // Respect manual changes: only set things up on the very first import.
            if (!string.IsNullOrEmpty(importer.userData)) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;      // crisp pixels, no blur
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            importer.userData = "pixelart";
        }
    }
}
