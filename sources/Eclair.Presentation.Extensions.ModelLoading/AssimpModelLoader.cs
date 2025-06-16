using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Eclair.Renderer;
using Eclair.Presentation.Drawing;
using NLog;
using NLog.Fluent;
using Silk.NET.Assimp;
using Silk.NET.Core;
using AiScene = Silk.NET.Assimp.Scene;
using AiNode = Silk.NET.Assimp.Node;
using AiMesh = Silk.NET.Assimp.Mesh;
using AiFace = Silk.NET.Assimp.Face;
using AiMaterial = Silk.NET.Assimp.Material;
using AiTextureType = Silk.NET.Assimp.TextureType;
using Material = Eclair.Renderer.Material;
using Mesh = Eclair.Renderer.Mesh;
using PrimitiveType = Silk.NET.OpenGL.PrimitiveType;
using Texture = Eclair.Renderer.Texture;

namespace Eclair.Presentation.Extensions.ModelLoading {
	
	public class AssimpModelLoader {
		
		private static readonly Assimp _ai = Assimp.GetApi();
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public static Model Load(IPlatform platform, string name, string path)
			=> Create(platform, name, path: path);
		
		public static Model Load(IPlatform platform, string name, byte[] data)
			=> Create(platform, name, data: data);
		
		private unsafe static Model Create(
			IPlatform platform,
			string name,
			byte[]? data = null,
			string? path = null,
			uint flags =
				(uint) (PostProcessSteps.GenerateSmoothNormals 
						| PostProcessSteps.JoinIdenticalVertices 
						| PostProcessSteps.Triangulate 
						| PostProcessSteps.FixInFacingNormals
						| PostProcessSteps.CalculateTangentSpace 
						| PostProcessSteps.LimitBoneWeights 
						| PostProcessSteps.PreTransformVertices 
						| PostProcessSteps.OptimizeMeshes)
		) {
			Debug.Assert(data != null || path != null);
			_logger.Debug("Creating model {Name}", name);

			var scene = path != null
				? _ai.ImportFile(path, flags)
				: _ai.ImportFileFromMemory(data, (uint) data.Length, flags, (byte*) null);
			
			if(scene == null || scene->MFlags == (uint) SceneFlags.Incomplete || scene->MRootNode == null) {
				throw new PlatformException("Failed to load model:" + _ai.GetErrorStringS());
			}
			
			_logger.Debug("Begin processing");
			var sw = Stopwatch.StartNew();

			var model = new Model {
				Name = name
			};
			
			var materials = GetMaterials(scene);
			ProcessNode(platform, ref model, materials, scene, scene->MMaterials, scene->MRootNode);
			
			_ai.FreeScene(scene);
			
			sw.Stop();
			_logger.Debug("Finished in {Milliseconds}ms", sw.ElapsedMilliseconds);
			return model;
		}

		private unsafe static List<Material> GetMaterials(AiScene* aiScene) {
			var materials = new List<Material>();
			
			for(int i = 0; i < aiScene->MNumMaterials; i++) {
				var material = new Material();
				ProcessMaterial(ref material, aiScene->MMaterials[i]);
				materials.Add(material);
			}

			return materials;
		}

		private unsafe static void ProcessNode(
			IPlatform platform,
			ref Model model,
			IList<Material> materials,
			AiScene* aiScene,
			AiMaterial** aiMaterials,
			AiNode* aiNode
		) {
			for(int i = 0; i < aiNode->MNumMeshes; i++) {
				ProcessMesh(platform, ref model, materials, aiScene, aiMaterials, aiScene->MMeshes[aiNode->MMeshes[i]]);
			}

			for(int i = 0; i < aiNode->MNumChildren; i++) {
				ProcessNode(platform, ref model, materials, aiScene, aiMaterials, aiNode->MChildren[i]);
			}
		}

		private unsafe static void ProcessMesh(
			IPlatform platform,
			ref Model model,
			IList<Material> materials,
			AiScene* aiScene,
			AiMaterial** aiMaterials,
			AiMesh* aiMesh
		) {
			var vertices = new Vertex[aiMesh->MNumVertices];
			var indices = new List<uint>();

		#region Vertices
			for(int i = 0; i < aiMesh->MNumVertices; i++) {
				var vertex = new Vertex {
					Position = aiMesh->MVertices[i]
				};

				if(aiMesh->MNormals != null) {
					vertex.Normal = aiMesh->MNormals[i];
				}

				if(aiMesh->MTextureCoords[0] != null) {
					var texCoords = aiMesh->MTextureCoords[0][i];
					vertex.UV = new(texCoords.X, texCoords.Y);
				}

				vertices[i] = vertex;
			}
		#endregion

		#region Indices
			for(int i = 0; i < aiMesh->MNumFaces; i++) {
				var face = aiMesh->MFaces[i];

				for(int j = 0; j < face.MNumIndices; j++) {
					indices.Add(face.MIndices[j]);
				}
			}
		#endregion
			
			var mesh = new Mesh {
				Vertices = vertices,
				Indices = indices.ToArray(),
				Material = materials[(int) aiMesh->MMaterialIndex]
			};

			model.Meshes.Add(mesh);

			uint texCount = ProcessTextures(platform, ref model, ref mesh, aiScene, aiMaterials[(int) aiMesh->MMaterialIndex], AiTextureType.Diffuse);
			texCount += ProcessTextures(platform, ref model, ref mesh, aiScene, aiMaterials[(int) aiMesh->MMaterialIndex], AiTextureType.Normals);
			texCount += ProcessTextures(platform, ref model, ref mesh, aiScene, aiMaterials[(int) aiMesh->MMaterialIndex], AiTextureType.DiffuseRoughness);
			texCount += ProcessTextures(platform, ref model, ref mesh, aiScene, aiMaterials[(int) aiMesh->MMaterialIndex], AiTextureType.Metalness);

			if(texCount == 0) {
				_logger.Warn("{ModelName}: Mesh {MeshIndex} has no textures", model.Name, model.Meshes.Count - 1);
			}
		}

		private unsafe static void ProcessMaterial(
			ref Material material,
			AiMaterial* aiMaterial
		) {
			if(aiMaterial == null) return;

			_ai.GetMaterialColor(aiMaterial, Assimp.MatkeyColorDiffuse,
			                    0, 0, ref material.Albedo);

			uint max = 1;

			float shininess = 0;
			float reflectivity = 0;
			
			_ai.GetMaterialFloatArray(aiMaterial, Assimp.MaterialShadingModel,
			                         0, 0, ref shininess, ref max);
			_ai.GetMaterialFloatArray(aiMaterial, Assimp.MaterialReflectivity,
			                         0, 0, ref reflectivity, ref max);

			material.Roughness = 1.0f - Math.Clamp(shininess / 256.0f, 0, 1);
			material.Metallic = reflectivity;
		}

		private unsafe static uint ProcessTextures(
			IPlatform platform,
			ref Model model,
			ref Mesh mesh,
			AiScene* aiScene, 
			AiMaterial* aiMaterial,
			AiTextureType textureType
		) {
			uint texCount = _ai.GetMaterialTextureCount(aiMaterial, textureType);

			if(texCount == 0) {
				return 0;
			}
			
			if(texCount > 1) {
				_logger.Warn("{ModelName}: More than 1 texture for {TextureType}. This will (probably) not work!", model.Name, textureType);
			}
			
			for(uint i = 0; i < texCount; i++) {
				AssimpString path;
				_ai.GetMaterialTexture(aiMaterial, textureType, i, &path,
				                      null, null, null, null, null, null);

				Texture? texture = null;
				
				if(aiScene->MNumTextures > 0 && path.Data[0] == '*') {
					int textureId = int.Parse((string) path.AsString.Replace("*", ""));
					var aiTexture = aiScene->MTextures[textureId];

					if(aiTexture->MHeight == 0) {
						var data = new Span<byte>(aiTexture->PcData, (int) aiTexture->MWidth);
						texture = Texture.Create(platform, data.ToArray());
					}
				} else {
					_logger.Debug("{ModelName}: Material texture path: {Path}", model.Name, path.AsString);

					string? dirPath = Path.GetDirectoryName(model.Name);
					if(dirPath == null) throw new ArgumentException(nameof(model.Name));

					string filePath = Path.Combine(dirPath, Uri.UnescapeDataString(path.AsString));

					using(var stream = new FileStream(filePath, FileMode.Open)) {
						texture = Texture.Create(platform, stream);
					}
				}

				if(texture == null) {
					_logger.Warn("{ModelName}: Could not load texture for material", model.Name);
					continue;
				}

				var trueType = textureType switch {
					AiTextureType.Diffuse => Material.TextureType.Diffuse,
					AiTextureType.DiffuseRoughness => Material.TextureType.Roughness,
					AiTextureType.Normals => Material.TextureType.Normal,
					AiTextureType.Metalness => Material.TextureType.Metallic,
					_ => Material.TextureType.None
				};

				if(trueType == Material.TextureType.None) {
					_logger.Warn("{ModelName} Unknown material texture type: {TextureType}", model.Name, trueType);
					continue;
				}
				
				mesh.Material.UseTextures |= trueType;
				mesh.Textures = mesh.Textures.Append(texture).ToArray();
			}

			return texCount;
		}
	}
}
