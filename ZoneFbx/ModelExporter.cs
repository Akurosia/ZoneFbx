using ZoneFbx.Fbx;
using ZoneFbx.Processor;

namespace ZoneFbx
{
    internal class ModelExporter
    {
        private readonly string modelPath;
        private readonly string outputPath;
        private readonly string sceneName;
        private readonly Lumina.GameData data;
        private readonly ZoneExporter.Options options;

        private readonly ContextManager ContextManager;
        private readonly TextureProcessor textureProcessor;
        private readonly MaterialProcessor materialProcessor;
        private readonly ModelProcessor modelProcessor;
        private readonly FbxExporter fbxExporter;

        private IntPtr contextManager { get; set; }

        public ModelExporter(string gamePath, string modelPath, string outputPath, ZoneExporter.Options options)
        {
            this.modelPath = modelPath;
            this.options = options;

            var modelName = Path.GetFileNameWithoutExtension(modelPath);
            sceneName = options.variantId == 1 ? modelName : $"{modelName}_v{options.variantId:D4}";
            this.outputPath = Path.Combine(outputPath, sceneName) + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(this.outputPath);

            Console.WriteLine("Initializing...");

            contextManager = ContextManager.Create();
            ContextManager.CreateManager(contextManager);
            ContextManager.CreateScene(contextManager, sceneName);

            fbxExporter = new(contextManager);

            try
            {
                data = new Lumina.GameData(gamePath);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Error: Game path directory is not valid!\n");
                throw new Exception("game path directory is not valid");
            }

            ContextManager = new ContextManager();
            textureProcessor = new(data, contextManager, options, this.outputPath, sceneName);
            materialProcessor = new(data, contextManager, options, textureProcessor, this.outputPath);
            modelProcessor = new(data, contextManager, options, ContextManager, materialProcessor);

            if (!exportModel())
            {
                Console.WriteLine("ZoneFbx has run into an error. Please open an issue on the GitHub repo with details about this error.");
                return;
            }
            Console.WriteLine("Model export finished.");
        }

        private bool exportModel()
        {
            Console.WriteLine("Processing model and textures...");
            var model = modelProcessor.LoadModel(modelPath, options.variantId);
            if (model == null)
            {
                Console.WriteLine($"Failed to load model {modelPath}.");
                return false;
            }

            var rootNode = Scene.GetRootNode(contextManager);
            var modelNode = Node.Create(contextManager, sceneName);
            var hasChildren = modelProcessor.ProcessModel(model, modelNode);
            if (!hasChildren)
            {
                hasChildren = modelProcessor.ProcessModelWithoutTexture(model, modelNode);
            }

            if (!hasChildren)
            {
                Console.WriteLine($"Failed to process model {modelPath}.");
                return false;
            }

            Node.AddChild(rootNode, modelNode);

            Console.WriteLine("Saving scene...");
            var outputFilePath = $"{this.outputPath}{sceneName}.fbx";
            if (!fbxExporter.Export(outputFilePath))
            {
                Console.WriteLine("Failed to save scene.");
                return false;
            }

            if (options.enableJsonExport || options.enableMTMap) materialProcessor.ExportJsonTextureMap();

            Console.WriteLine($"Done! Model exported to {outputFilePath}");
            return true;
        }

        ~ModelExporter()
        {
            if (contextManager != IntPtr.Zero)
            {
                ContextManager.DestroyManager(contextManager);
                ContextManager.Destroy(contextManager);
            }
            ContextManager.CppVectorCleanup();
        }
    }
}
