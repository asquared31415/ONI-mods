using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SquareLib
{
	public static class ModAssets
	{
		private static readonly TextureAtlas TileAtlas = global::Assets.GetTextureAtlas("tiles_solid");

		public static TextureAtlas GetCustomTileAtlas(string name)
		{
			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), name + ".png");
			TextureAtlas textureAtlas = null;
			try
			{
				byte[] data = File.ReadAllBytes(path);
				var tex = new Texture2D(2, 2);
				tex.LoadImage(data);
				textureAtlas = ScriptableObject.CreateInstance<TextureAtlas>();
				textureAtlas.texture = tex;
				textureAtlas.vertexScale = TileAtlas.vertexScale;
				textureAtlas.items = TileAtlas.items;
			}
			catch
			{
				Debug.LogError($"[Item Permeable Tiles]: Could not load atlas image at path {path}");
			}

			return textureAtlas;
		}

		public static void AddSpriteFromFile(string name)
		{
			var texture = LoadTexture(name);

			var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2f, texture.height / 2f));
			Assets.Sprites.Add(name, sprite);
		}

		public static Texture2D LoadTexture(string name)
		{
			Texture2D texture = null;
			var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");

			var texFile = Path.Combine(directory, $"{name}.png");

			try
			{
				byte[] data = File.ReadAllBytes(texFile);
				texture = new Texture2D(1, 1);
				texture.LoadImage(data);
			}
			catch(Exception e)
			{
				Debug.LogError($"Could not load texture at assets/{texFile}.png");
				Debug.LogException(e);
			}

			return texture;
		}
	}
}
