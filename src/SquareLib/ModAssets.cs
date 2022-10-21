using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SquareLib
{
	public static class ModAssets
	{
		private static readonly TextureAtlas TileAtlas = Assets.GetTextureAtlas("tiles_solid");

		public static TextureAtlas GetCustomTileAtlas(string name, Assembly rootAssembly = null)
		{
			rootAssembly ??= Assembly.GetCallingAssembly();
			var path = Path.Combine(Path.GetDirectoryName(rootAssembly.Location)!, name + ".png");
			TextureAtlas textureAtlas = null;
			try
			{
				var data = File.ReadAllBytes(path);
				var tex = new Texture2D(2, 2);
				tex.LoadImage(data);
				textureAtlas = ScriptableObject.CreateInstance<TextureAtlas>();
				textureAtlas.texture = tex;
				textureAtlas.scaleFactor = TileAtlas.scaleFactor;
				textureAtlas.items = TileAtlas.items;
			}
			catch
			{
				Debug.LogError($"Could not load atlas image at path {path}");
			}

			return textureAtlas;
		}

		public static Sprite AddSpriteFromFile(string name, Assembly rootAssembly = null)
		{
			rootAssembly ??= Assembly.GetCallingAssembly();
			var texture = LoadTexture(name, null, rootAssembly);

			var sprite = Sprite.Create(
				texture,
				new Rect(0, 0, texture.width, texture.height),
				new Vector2(texture.width / 2f, texture.height / 2f)
			);

			Assets.Sprites.Add(name, sprite);
			return sprite;
		}

		public static Texture2D LoadTexture(string name, string folder = null, Assembly rootAssembly = null)
		{
			Texture2D texture = null;
			var directory = Path.Combine(
				Path.GetDirectoryName((rootAssembly ?? Assembly.GetExecutingAssembly()).Location)!,
				folder ?? "assets"
			);

			var texFile = Path.Combine(directory, $"{name}.png");

			try
			{
				var data = File.ReadAllBytes(texFile);
				texture = new Texture2D(1, 1);
				texture.LoadImage(data);
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not load texture at {texFile}");
				Debug.LogException(e);
			}

			return texture;
		}

		public static Sprite AddSpriteFromManifest(string manifest, Assembly rootAssembly = null)
		{
			rootAssembly ??= Assembly.GetCallingAssembly();
			var texture = LoadTextureInternal(rootAssembly, manifest);
			if (texture != null)
			{
				var sprite = Sprite.Create(
					texture,
					new Rect(0, 0, texture.width, texture.height),
					new Vector2(texture.width / 2f, texture.height / 2f)
				);

				Assets.Sprites.Add(manifest, sprite);
				return sprite;
			}

			return null;
		}

		public static Texture2D LoadTextureFromManifest(string manifest, Assembly rootAssembly = null)
		{
			rootAssembly ??= Assembly.GetCallingAssembly();
			return LoadTextureInternal(rootAssembly, manifest);
		}

		// the calling assembly should be determined at all the entry points and passed down
		private static Texture2D LoadTextureInternal(Assembly assembly, string manifest)
		{
			using var zeroStream = assembly.GetManifestResourceStream(manifest);
			using var memStream = new MemoryStream();
			if (zeroStream == null)
			{
				Debug.LogError($"Unable to load texture from assembly {assembly.FullName}, manifest {manifest}");
				return null;
			}

			var texture = new Texture2D(1, 1);
			zeroStream.CopyTo(memStream);
			texture.LoadImage(memStream.ToArray());
			return texture;
		}
	}
}
