{
	description = "nix build environment";
	
	inputs = {
		nixpkgs.url = "github:nixos/nixpkgs/nixpkgs-unstable";
	};

	outputs = { self, nixpkgs }:
		let
			universal = function:
				nixpkgs.lib.genAttrs [
					"x86_64-linux"
					"aarch64-linux"
				] (system: function nixpkgs.legacyPackages.${system});
		in {
			devShell = universal (pkgs: 
				(pkgs.mkShell rec {
					name = "dotnet";

					libs = with pkgs; [
						freetype
						libGL
						xorg.libX11
						xorg.libXrandr
						xorg.libXcursor
						xorg.libXi
						xorg.libXinerama
						xorg.libXtst
						xorg.libXxf86vm
						libxkbcommon
						udev
						glib
						fontconfig
						gtk3
						cairo
						pango
						harfbuzz
						atk
						gobject-introspection
						gdk-pixbuf
						#wayland
						glfw
						vulkan-headers
						vulkan-loader
						stdenv.cc.cc.lib
						opencl-headers
						ocl-icd
						vulkan-validation-layers
					];
					
					LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath libs;
					dotnet = pkgs.dotnetCorePackages.sdk_9_0_3xx;

					nativeBuildInputs = with pkgs; [
						dotnet

						# debugging
						renderdoc

						# shader compiler
						shaderc

						vulkan-tools

						gcc
						cmake
						ninja
						gnumake
					];

					buildInputs = with pkgs; [
						libs
					];
					
					shellHook = ''
						export DOTNET_ROOT="${dotnet}/share/dotnet"
						export VULKAN_SDK="${pkgs.vulkan-headers}"
						export VK_LAYER_PATH="${pkgs.vulkan-validation-layers}/share/vulkan/explicit_layer.d"
					'';
				}));
		};
}
