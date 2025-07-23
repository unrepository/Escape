using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using Cinenic.Components;
using Cinenic.Renderer;
using Cinenic.Renderer.Resources;
using Cinenic.Resources;
using NLog;
using Silk.NET.Assimp;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Ai = Silk.NET.Assimp.Assimp;
using AiScene = Silk.NET.Assimp.Scene;
using AiNode = Silk.NET.Assimp.Node;
using AiMesh = Silk.NET.Assimp.Mesh;
using AiFace = Silk.NET.Assimp.Face;
using AiMaterial = Silk.NET.Assimp.Material;
using AiTextureType = Silk.NET.Assimp.TextureType;
using AiTextureMapMode = Silk.NET.Assimp.TextureMapMode;
using AiTextureWrapMode = Silk.NET.Assimp.TextureWrapMode;
using Buffer = System.Buffer;
using Material = Cinenic.Renderer.Material;
using Mesh = Cinenic.Renderer.Mesh;
using Texture = Cinenic.Renderer.Texture;

namespace Cinenic.Extensions.Assimp {
	
	public unsafe static class AssimpSceneLoader {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly Ai _ai = Ai.GetApi();
		
		public static AssimpScene Load(IPlatform platform, string path, uint? flags = null) {
			flags ??= (uint)
			(
				PostProcessSteps.GenerateSmoothNormals
				| PostProcessSteps.JoinIdenticalVertices
				| PostProcessSteps.Triangulate
				| PostProcessSteps.FixInFacingNormals
				| PostProcessSteps.CalculateTangentSpace
				| PostProcessSteps.LimitBoneWeights
				| PostProcessSteps.PreTransformVertices
				| PostProcessSteps.OptimizeMeshes
			);

			var aiScene = _ai.ImportFile(path, flags.Value);
			return Load(platform, aiScene, Path.GetDirectoryName(path));
		}
		
		public static AssimpScene Load(IPlatform platform, AiScene* aiScene, string baseDirectory) {
			if(aiScene is null || aiScene->MFlags == (uint) SceneFlags.Incomplete || aiScene->MRootNode is null) {
				throw new PlatformException($"Could not load scene: {_ai.GetErrorStringS()}");
			}

			var scene = new AssimpScene();

			_ProcessNode(platform, ref scene, null, aiScene->MRootNode, aiScene, baseDirectory);
			_ai.ReleaseImport(aiScene);

			return scene;
		}

		private static void _ProcessNode(
			IPlatform platform,
			ref AssimpScene scene,
			AssimpScene.Node? parent,
			AiNode* aiNode,
			AiScene* aiScene,
			string baseDirectory
		) {
			Matrix4x4.Decompose(
				aiNode->MTransformation,
				out var scale,
				out var rotation,
				out var translation
			);

			var node = new AssimpScene.Node {
				Transform = new Transform3D(translation, rotation, scale)
			};
			
			if(aiNode->MNumMeshes > 0) {
				var meshes = new List<Mesh>();
				
				for(int i = 0; i < aiNode->MNumMeshes; i++) {
					var mesh = aiScene->MMeshes[aiNode->MMeshes[i]];

				#region Mesh processing
					var vertices = new Vertex[mesh->MNumVertices];
					var indices = new List<uint>();

					for(int vi = 0; vi < vertices.Length; vi++) {
						vertices[vi].Position = mesh->MVertices[vi];
						vertices[vi].Normal = mesh->MNormals[vi];

						if(mesh->MTextureCoords[0] != null) {
							var uv = mesh->MTextureCoords[0][vi];
							vertices[vi].UV = new Vector2(uv.X, uv.Y);
						}
					}

					for(int j = 0; j < mesh->MNumFaces; j++) {
						var face = mesh->MFaces[j];

						for(int k = 0; k < face.MNumIndices; k++) {
							indices.Add(face.MIndices[k]);
						}
					}
				#endregion

				#region Material processing
					var mMat = aiScene->MMaterials[mesh->MMaterialIndex];
					var material = new Material();

					var wrapModeS = Texture.TextureWrapMode.Repeat;
					var wrapModeT = Texture.TextureWrapMode.Repeat;
					var minFilter = Texture.TextureFilter.Linear;
					var magFilter = Texture.TextureFilter.Linear;

					for(int j = 0; j < mMat->MNumProperties; j++) {
						var mProp = mMat->MProperties[j];

						// if(mProp->MKey.AsString.StartsWith("$tex")) {
						// 	// textures are handled later
						// 	continue;
						// }
						
						float* vF = mProp->MType == PropertyTypeInfo.Float ? (float*) mProp->MData : null;
						int* vI = mProp->MType == PropertyTypeInfo.Integer ? (int*) mProp->MData : null;
						string? vS = null;

						if(mProp->MType == PropertyTypeInfo.String) {
							byte[] buffer = new byte[mProp->MDataLength];

							fixed(byte* bufferPtr = buffer) {
								Buffer.MemoryCopy(mProp->MData, bufferPtr, buffer.Length, buffer.Length);
							}

							vS = Encoding.UTF8.GetString(buffer);
						}
						
						switch(mProp->MKey.AsString) {
							case "$clr.base":
								Debug.Assert(mProp->MDataLength == 16);
								
								material.AlbedoColor = Color.FromArgb(
									(byte) (*(vF + 3) * 255),
									(byte) (*vF * 255),
									(byte) (*(vF + 1) * 255),
									(byte) (*(vF + 2) * 255)
								);
								break;
							case "$mat.metallicFactor":
								Debug.Assert(mProp->MDataLength == 4);
								material.Metallic = *vF;
								break;
							case "$mat.roughnessFactor":
								material.Roughness = *vF;
								Debug.Assert(mProp->MDataLength == 4);
								break;
							case "$tex.file":
								break;
							case "$tex.mapmodeu":
							case "$tex.mapmodev":
								Debug.Assert(mProp->MDataLength == 4);
								var v = (AiTextureMapMode) (*(int*) mProp->MData);

								var wrapMode = v switch {
									AiTextureMapMode.Wrap => Texture.TextureWrapMode.Repeat,
									AiTextureMapMode.Clamp => Texture.TextureWrapMode.ClampToEdge,
									AiTextureMapMode.Mirror => Texture.TextureWrapMode.RepeatMirrored,
									AiTextureMapMode.Decal => throw new NotSupportedException(),
									_ => throw new NotSupportedException()
								};

								if(mProp->MKey.AsString.EndsWith('u')) {
									wrapModeS = wrapMode;
								} else {
									wrapModeT = wrapMode;
								}
								
								break;
							case "$mat.gltf.mappingFilter.min":
							case "$mat.gltf.mappingFilter.mag": // TODO
								if(Debugger.IsAttached) Debugger.Break();
								break;
							default:
								_logger.Warn($"Unhandled property: {mProp->MKey.AsString} (type={mProp->MType})");
								break;
						}
					}

					Ref<TextureResource>? LoadTexture(AiTextureType type) {
						AssimpString path;
						_ai.GetMaterialTexture(mMat, type, 0, &path, null, null, null, null, null, null);

						if(path.Length == 0) {
							_logger.Debug("Not loading texture type {Type} for material", type);
							return null;
						}
						
						var texture = new TextureResource();
						var importMeta = new TextureResource.Import {
							WrapMode = wrapModeS,
							Filter = magFilter
						};
						
						if(path.Data[0] == '*') {
							// texture is embedded
							int textureIndex = int.Parse(path.AsString.Replace("*", ""));
							var mTexture = aiScene->MTextures[textureIndex];

							if(mTexture->MWidth == 0) {
								throw new InvalidDataException("Texture has a size of 0");
							}

							var data = new Span<byte>(mTexture->PcData, (int) mTexture->MWidth);

							using var stream = new MemoryStream(data.ToArray());
							texture.Load(platform, null, stream, null, importMeta);
						} else {
							string fullPath = Path.Combine(baseDirectory, Uri.UnescapeDataString(path.AsString));
							
							using var stream = new FileStream(fullPath, FileMode.Open);
							texture.Load(platform, fullPath, stream, null, importMeta);
						}

						return new(texture);
					}

					material.AlbedoTexture = LoadTexture(AiTextureType.Diffuse);
					material.NormalTexture = LoadTexture(AiTextureType.Normals);
					material.RoughnessTexture = LoadTexture(AiTextureType.DiffuseRoughness);
					material.MetallicTexture = LoadTexture(AiTextureType.Metalness);
					
					_logger.Trace("Material construction complete");
					_logger.Trace(material);
				#endregion
					
					meshes.Add(new Mesh {
						Vertices = vertices,
						Indices = indices.ToArray(),
						Material = material
					});
					
					_logger.Trace("Mesh created");
				}

				var model = new Model {
					Meshes = meshes
				};

				node.Model = model;
			}
			
			if(parent is null) {
				scene.Nodes.Add(node);
			} else {
				parent.Children.Add(node);
			}

			for(int i = 0; i < aiNode->MNumChildren; i++) {
				_ProcessNode(platform, ref scene, node, aiNode->MChildren[i], aiScene, baseDirectory);
			}
		}
	}
}
